import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createOverlay } from './content/overlay';
import { renderStatus } from './popup/render';
import { BackendError, liveSessions, signIn, streamAnswer, toAnswerEvent, type AnswerEvent } from './shared/backend';
import { ANSWER_PORT, KEEPALIVE_INTERVAL_MS, type AnswerPortMessage } from './shared/messages';

const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status });

function sse(...events: [string, string][]): Response {
  return new Response(events.map(([e, d]) => `event: ${e}\r\ndata: ${d}\r\n\r\n`).join(''));
}

describe('backend client', () => {
  it('signs in and lists only live sessions with the bearer token', async () => {
    const fetchImpl = vi
      .fn()
      .mockResolvedValueOnce(json({ accessToken: 'tok' }))
      .mockResolvedValueOnce(json([{ id: 'a', title: 'A', status: 'Live' }, { id: 'b', title: 'B', status: 'Ended' }]));

    const token = await signIn('alice', 'secret-value', fetchImpl);
    const sessions = await liveSessions(token, fetchImpl);

    expect(token).toBe('tok');
    expect(sessions.map((s) => s.id)).toEqual(['a']);
    expect(fetchImpl.mock.calls[0]![0]).toBe('http://127.0.0.1:5480/api/auth/sign-in');
    expect(fetchImpl.mock.calls[1]![1].headers.Authorization).toBe('Bearer tok');
  });

  it('streams answer events across chunk boundaries', async () => {
    const events: AnswerEvent[] = [];
    const fetchImpl = vi.fn().mockResolvedValue(sse(['meta', '{"questionType":"Coding","version":1}'], ['delta', ' Hi'], ['unknown', 'x'], ['done', '']));

    await streamAnswer('tok', 's 1', 'Q?', (e) => events.push(e), fetchImpl);

    expect(fetchImpl.mock.calls[0]![0]).toBe('http://127.0.0.1:5480/api/sessions/s%201/answers');
    expect(events).toEqual([{ type: 'meta', questionType: 'Coding', version: 1 }, { type: 'delta', token: ' Hi' }, { type: 'done' }]);
  });

  it('parses error events', () => {
    expect(toAnswerEvent('event: error\ndata: Ollama down')).toEqual({ type: 'error', message: 'Ollama down' });
  });

  it.each([
    [() => Promise.reject(new TypeError('offline')), 'Desktop app not running'],
    [() => Promise.resolve(new Response(null, { status: 401 })), 'Sign in again'],
    [() => Promise.resolve(new Response(null, { status: 500 })), '500'],
  ])('explains failures', async (fetchImpl, message) => {
    await expect(liveSessions('t', fetchImpl)).rejects.toThrow(message);
  });

  it('fails on an empty stream', async () => {
    await expect(streamAnswer('t', 's', 'q', vi.fn(), () => Promise.resolve(new Response(null)))).rejects.toBeInstanceOf(BackendError);
  });
});

describe('overlay', () => {
  it('lists sessions, asks, and renders streamed events', () => {
    const onAsk = vi.fn();
    const onClose = vi.fn();
    const overlay = createOverlay(document, { onAsk, onClose });
    document.body.append(overlay.host);
    const root = overlay.host.shadowRoot!;

    overlay.setSessions([{ id: 's1', title: 'Acme', status: 'Live' }]);
    (root.querySelector('[data-question]') as HTMLInputElement).value = 'Why us?';
    root.querySelector('form')!.dispatchEvent(new Event('submit', { cancelable: true }));
    overlay.apply({ type: 'meta', questionType: 'Experience', version: 1 });
    overlay.apply({ type: 'delta', token: 'Because' });
    overlay.apply({ type: 'done' });
    (root.querySelector('[data-close]') as HTMLButtonElement).click();

    expect(onAsk).toHaveBeenCalledWith('s1', 'Why us?');
    expect(root.querySelector('[data-answer]')!.textContent).toBe('Because');
    expect(root.querySelector('[data-meta]')!.textContent).toBe('Experience · v1');
    expect((root.querySelector('.panel') as HTMLElement).style.opacity).toBe('0.85');
    expect(onClose).toHaveBeenCalled();
  });

  it('guides the user when there is no live session or question', () => {
    const onAsk = vi.fn();
    const overlay = createOverlay(document, { onAsk, onClose: vi.fn() }, 0.5);
    const root = overlay.host.shadowRoot!;
    const error = () => root.querySelector('[data-error]')!.textContent;

    overlay.setSessions([]);
    expect(error()).toContain('No live session');
    (root.querySelector('[data-question]') as HTMLInputElement).value = 'Q?';
    root.querySelector('form')!.dispatchEvent(new Event('submit', { cancelable: true }));
    expect(error()).toContain('Start a session');
    overlay.setSessions([], 'Sign in first');
    overlay.apply({ type: 'error', message: 'Ollama down' });

    expect(error()).toBe('Ollama down');
    expect(onAsk).not.toHaveBeenCalled();
  });
});

describe('popup', () => {
  beforeEach(() => {
    document.body.innerHTML = '<form id="sign-in"></form><section id="signed-in"></section><p id="sessions"></p><p id="error"></p>';
  });

  it('shows sign-in when signed out and live sessions when signed in', () => {
    renderStatus(document, { signedIn: false, sessions: [], error: 'Desktop app not running' });
    expect(document.getElementById('sign-in')!.hidden).toBe(false);
    expect(document.getElementById('error')!.textContent).toBe('Desktop app not running');

    renderStatus(document, { signedIn: true, sessions: [{ id: 'a', title: 'Acme', status: 'Live' }] });
    expect(document.getElementById('signed-in')!.hidden).toBe(false);
    expect(document.getElementById('sessions')!.textContent).toContain('Live: Acme');

    renderStatus(document, { signedIn: true, sessions: [] });
    expect(document.getElementById('sessions')!.textContent).toContain('No live session');
  });
});

describe('background worker', () => {
  const store: Record<string, unknown> = {};
  let onMessage: (request: unknown, sender: unknown, respond: (value: unknown) => void) => boolean;

  beforeEach(async () => {
    vi.resetModules();
    for (const key of Object.keys(store)) delete store[key];
    vi.stubGlobal('chrome', {
      storage: {
        session: {
          get: vi.fn((key: string) => Promise.resolve({ [key]: store[key] })),
          set: vi.fn((values: Record<string, unknown>) => Promise.resolve(Object.assign(store, values))),
          remove: vi.fn((key: string) => Promise.resolve(delete store[key])),
        },
      },
      runtime: {
        onMessage: { addListener: vi.fn((fn) => (onMessage = fn)) },
        onConnect: { addListener: vi.fn() },
      },
    });
  });

  it('signs in, reports status and signs out', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn((url: string) => Promise.resolve(url.endsWith('/sign-in') ? json({ accessToken: 'tok' }) : json([{ id: 'a', title: 'A', status: 'Live' }]))),
    );
    const { handleRequest } = await import('./background');

    expect(await handleRequest({ kind: 'sign-in', username: 'u', password: 'p' })).toEqual({ signedIn: true, sessions: [{ id: 'a', title: 'A', status: 'Live' }] });
    expect(await handleRequest({ kind: 'sign-out' })).toEqual({ signedIn: false, sessions: [] });

    const respond = vi.fn();
    expect(onMessage({ kind: 'status' }, {}, respond)).toBe(true);
    await vi.waitFor(() => expect(respond).toHaveBeenCalledWith({ signedIn: false, sessions: [] }));
  });

  it('reports backend errors without crashing', async () => {
    vi.stubGlobal('fetch', vi.fn(() => Promise.reject(new TypeError('offline'))));
    const { handleRequest } = await import('./background');

    const status = await handleRequest({ kind: 'sign-in', username: 'u', password: 'p' });

    expect(status).toEqual({ signedIn: false, sessions: [], error: 'Desktop app not running. Open InterviewCopilot first.' });
  });

  it('streams answers over the port, or explains when not signed in', async () => {
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(sse(['delta', 'Hi'], ['done', '']))));
    const { handlePort } = await import('./background');
    const sent: AnswerPortMessage[] = [];
    let listener: (message: AnswerPortMessage) => void = () => undefined;
    const port = {
      name: ANSWER_PORT,
      postMessage: (m: AnswerPortMessage) => sent.push(m),
      onMessage: { addListener: (fn: typeof listener) => (listener = fn) },
    } as unknown as chrome.runtime.Port;

    handlePort({ name: 'other' } as chrome.runtime.Port);
    handlePort(port);
    listener({ kind: 'event', event: { type: 'done' } });
    listener({ kind: 'ask', sessionId: 's', question: 'Q?' });
    await vi.waitFor(() => expect(sent).toHaveLength(1));
    expect(sent[0]).toEqual({ kind: 'event', event: { type: 'error', message: 'Sign in from the extension popup first.' } });

    store.token = 'tok';
    listener({ kind: 'ask', sessionId: 's', question: 'Q?' });
    await vi.waitFor(() => expect(sent).toHaveLength(3));
    expect(sent.slice(1)).toEqual([
      { kind: 'event', event: { type: 'delta', token: 'Hi' } },
      { kind: 'event', event: { type: 'done' } },
    ]);

    vi.stubGlobal('fetch', vi.fn(() => Promise.reject(new Error('boom'))));
    listener({ kind: 'ask', sessionId: 's', question: 'Q?' });
    await vi.waitFor(() => expect(sent).toHaveLength(4));
    expect(sent[3]).toEqual({ kind: 'event', event: { type: 'error', message: 'Desktop app not running. Open InterviewCopilot first.' } });
  });

  it('ignores keep-alive pings instead of treating them as new questions', async () => {
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(sse(['done', '']))));
    const { handlePort } = await import('./background');
    const sent: AnswerPortMessage[] = [];
    let listener: (message: AnswerPortMessage) => void = () => undefined;
    const port = {
      name: ANSWER_PORT,
      postMessage: (m: AnswerPortMessage) => sent.push(m),
      onMessage: { addListener: (fn: typeof listener) => (listener = fn) },
    } as unknown as chrome.runtime.Port;

    handlePort(port);
    store.token = 'tok';
    listener({ kind: 'ping' });
    await new Promise((resolve) => setTimeout(resolve, 50));

    expect(sent).toHaveLength(0);
  });

  it('survives the page closing while an answer streams', async () => {
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(sse(['delta', 'Hi'], ['done', '']))));
    const { handlePort } = await import('./background');
    let listener: (message: AnswerPortMessage) => void = () => undefined;
    const port = {
      name: ANSWER_PORT,
      postMessage: () => {
        throw new Error('Attempting to use a disconnected port object');
      },
      onMessage: { addListener: (fn: typeof listener) => (listener = fn) },
    } as unknown as chrome.runtime.Port;

    const rejections: unknown[] = [];
    const onRejection = (reason: unknown) => rejections.push(reason);
    process.on('unhandledRejection', onRejection);

    handlePort(port);
    store.token = 'tok';
    listener({ kind: 'ask', sessionId: 's', question: 'Q?' });
    await new Promise((resolve) => setTimeout(resolve, 100));

    process.off('unhandledRejection', onRejection);
    expect(rejections).toHaveLength(0);
  });
});

describe('content script', () => {
  it('pings the background while the answer streams so the service worker is not idled out', async () => {
    vi.useFakeTimers();
    const posted: AnswerPortMessage[] = [];
    const port = {
      name: ANSWER_PORT,
      postMessage: (m: AnswerPortMessage) => posted.push(m),
      onMessage: { addListener: vi.fn() },
      disconnect: vi.fn(),
    };
    vi.resetModules();
    vi.stubGlobal('chrome', {
      runtime: {
        connect: vi.fn(() => port),
        sendMessage: vi.fn(() =>
          Promise.resolve({ signedIn: true, sessions: [{ id: 's1', title: 'Acme', status: 'Live' }] }),
        ),
      },
    });

    await import('./content');
    const root = document.querySelector('interview-copilot-overlay')!.shadowRoot!;
    await vi.waitFor(() => expect((root.querySelector('[data-session]') as HTMLSelectElement).value).toBe('s1'));

    (root.querySelector('[data-question]') as HTMLInputElement).value = 'Why us?';
    root.querySelector('form')!.dispatchEvent(new Event('submit', { cancelable: true }));

    expect(posted[0]).toEqual({ kind: 'ask', sessionId: 's1', question: 'Why us?' });

    // Chrome stops an idle MV3 service worker after ~30s; without a ping the stream dies.
    await vi.advanceTimersByTimeAsync(KEEPALIVE_INTERVAL_MS);
    expect(posted).toContainEqual({ kind: 'ping' });

    await vi.advanceTimersByTimeAsync(KEEPALIVE_INTERVAL_MS);
    expect(posted.filter((m) => m.kind === 'ping')).toHaveLength(2);

    vi.useRealTimers();
  });
});
