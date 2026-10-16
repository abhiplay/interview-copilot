import { liveSessions, signIn, streamAnswer, BackendError } from './shared/backend';
import { ANSWER_PORT, type AnswerPortMessage, type Request, type StatusResponse } from './shared/messages';

/**
 * Background worker. Holds the sign-in token in session storage only (cleared when the browser
 * closes) and is the only part of the extension that talks to the local backend.
 */
const TOKEN_KEY = 'token';

async function token(): Promise<string | null> {
  const stored = await chrome.storage.session.get(TOKEN_KEY);
  return typeof stored[TOKEN_KEY] === 'string' ? (stored[TOKEN_KEY] as string) : null;
}

export async function handleRequest(request: Request): Promise<StatusResponse> {
  try {
    if (request.kind === 'sign-in') {
      await chrome.storage.session.set({ [TOKEN_KEY]: await signIn(request.username, request.password) });
    } else if (request.kind === 'sign-out') {
      await chrome.storage.session.remove(TOKEN_KEY);
    }
    const current = await token();
    return { signedIn: current !== null, sessions: current ? await liveSessions(current) : [] };
  } catch (error) {
    return { signedIn: (await token()) !== null, sessions: [], error: errorMessage(error) };
  }
}

export function handlePort(port: chrome.runtime.Port): void {
  if (port.name !== ANSWER_PORT) {
    return;
  }
  port.onMessage.addListener((message: AnswerPortMessage) => {
    if (message.kind !== 'ask') {
      return;
    }
    void (async () => {
      const send = (event: AnswerPortMessage) => port.postMessage(event);
      const current = await token();
      if (!current) {
        send({ kind: 'event', event: { type: 'error', message: 'Sign in from the extension popup first.' } });
        return;
      }
      try {
        await streamAnswer(current, message.sessionId, message.question, (event) => send({ kind: 'event', event }));
      } catch (error) {
        send({ kind: 'event', event: { type: 'error', message: errorMessage(error) } });
      }
    })();
  });
}

function errorMessage(error: unknown): string {
  return error instanceof BackendError ? error.message : 'Something went wrong.';
}

chrome.runtime.onMessage.addListener((request: Request, _sender, sendResponse) => {
  void handleRequest(request).then(sendResponse);
  return true; // async response
});
chrome.runtime.onConnect.addListener(handlePort);
