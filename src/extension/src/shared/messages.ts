import type { AnswerEvent, SessionSummary } from './backend';

/** popup/content → background requests. */
export type Request =
  | { kind: 'sign-in'; username: string; password: string }
  | { kind: 'sign-out' }
  | { kind: 'status' };

export type StatusResponse = { signedIn: boolean; sessions: SessionSummary[]; error?: string };

/** Port name for answer streaming (content ↔ background). */
export const ANSWER_PORT = 'answer-stream';

/**
 * 'ping' is a keep-alive. Chrome terminates an MV3 service worker after ~30s of inactivity, and the
 * background's answer stream can stay silent for longer than that on a slow local model. Without the
 * ping the worker is killed mid-stream, the fetch dies, the server aborts the request and the answer
 * is lost. The content script (a page, which Chrome never idles out) pings so the worker stays alive.
 */
export type AnswerPortMessage =
  | { kind: 'ask'; sessionId: string; question: string }
  | { kind: 'ping' }
  | { kind: 'event'; event: AnswerEvent };

/** How often the content script pings the background worker to keep it alive. */
export const KEEPALIVE_INTERVAL_MS = 10_000;
