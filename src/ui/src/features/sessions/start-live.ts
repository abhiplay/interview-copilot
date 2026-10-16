import type { ApiClient } from '@/lib/api';
import type { Platform } from '@/platform';
import type { Session } from '@/types/api';

/**
 * Takes a session live and starts audio capture. Shared by the session page and the meeting prompt
 * (#16); #30 adds the BYOK key hand-off here, between going live and capturing.
 */
export async function startLiveSession(api: ApiClient, platform: Platform, sessionId: string): Promise<Session> {
  const started = await api.startSession(sessionId);
  await platform.startCapture(sessionId);
  return started;
}
