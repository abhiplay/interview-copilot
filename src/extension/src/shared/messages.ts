import type { AnswerEvent, SessionSummary } from './backend';

/** popup/content → background requests. */
export type Request =
  | { kind: 'sign-in'; username: string; password: string }
  | { kind: 'sign-out' }
  | { kind: 'status' };

export type StatusResponse = { signedIn: boolean; sessions: SessionSummary[]; error?: string };

/** Port name for answer streaming (content ↔ background). */
export const ANSWER_PORT = 'answer-stream';

export type AnswerPortMessage = { kind: 'ask'; sessionId: string; question: string } | { kind: 'event'; event: AnswerEvent };
