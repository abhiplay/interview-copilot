/** Talks to the local InterviewCopilot backend. Runs in the background worker (host permission, no CORS). */
export const BACKEND_URL = 'http://127.0.0.1:5480';

export interface SessionSummary {
  id: string;
  title: string;
  status: 'Draft' | 'Live' | 'Ended';
}

export type AnswerEvent =
  | { type: 'meta'; questionType: string; version: number }
  | { type: 'delta'; token: string }
  | { type: 'error'; message: string }
  | { type: 'done' };

type FetchLike = (input: string, init?: RequestInit) => Promise<Response>;

export class BackendError extends Error {}

export async function signIn(username: string, password: string, fetchImpl: FetchLike = fetch): Promise<string> {
  const response = await call(fetchImpl, '/api/auth/sign-in', null, {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  });
  return ((await response.json()) as { accessToken: string }).accessToken;
}

export async function liveSessions(token: string, fetchImpl: FetchLike = fetch): Promise<SessionSummary[]> {
  const response = await call(fetchImpl, '/api/sessions', token);
  return ((await response.json()) as SessionSummary[]).filter((s) => s.status === 'Live');
}

/** Streams an answer and reports each SSE event. */
export async function streamAnswer(
  token: string,
  sessionId: string,
  question: string,
  onEvent: (event: AnswerEvent) => void,
  fetchImpl: FetchLike = fetch,
): Promise<void> {
  const response = await call(fetchImpl, `/api/sessions/${encodeURIComponent(sessionId)}/answers`, token, {
    method: 'POST',
    body: JSON.stringify({ question }),
  });
  if (!response.body) {
    throw new BackendError('The answer stream was empty.');
  }

  const reader = response.body.pipeThrough(new TextDecoderStream()).getReader();
  let buffer = '';
  for (;;) {
    const { value, done } = await reader.read();
    if (done) {
      return;
    }
    buffer += value.replace(/\r\n/g, '\n');
    let boundary = buffer.indexOf('\n\n');
    while (boundary !== -1) {
      const event = toAnswerEvent(buffer.slice(0, boundary));
      buffer = buffer.slice(boundary + 2);
      if (event) {
        onEvent(event);
      }
      boundary = buffer.indexOf('\n\n');
    }
  }
}

export function toAnswerEvent(block: string): AnswerEvent | null {
  let name = '';
  const data: string[] = [];
  for (const line of block.split('\n')) {
    if (line.startsWith('event:')) {
      name = line.slice(6).trim();
    } else if (line.startsWith('data:')) {
      const value = line.slice(5);
      data.push(value.startsWith(' ') ? value.slice(1) : value);
    }
  }
  const payload = data.join('\n');
  switch (name) {
    case 'meta': {
      const meta = JSON.parse(payload) as { questionType: string; version: number };
      return { type: 'meta', questionType: meta.questionType, version: meta.version };
    }
    case 'delta':
      return { type: 'delta', token: payload };
    case 'error':
      return { type: 'error', message: payload };
    case 'done':
      return { type: 'done' };
    default:
      return null;
  }
}

async function call(fetchImpl: FetchLike, path: string, token: string | null, init: RequestInit = {}): Promise<Response> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  let response: Response;
  try {
    response = await fetchImpl(`${BACKEND_URL}${path}`, { ...init, headers });
  } catch {
    throw new BackendError('Desktop app not running. Open InterviewCopilot first.');
  }
  if (response.status === 401) {
    throw new BackendError('Sign in again from the extension popup.');
  }
  if (!response.ok) {
    throw new BackendError(`Request failed (${response.status}).`);
  }
  return response;
}
