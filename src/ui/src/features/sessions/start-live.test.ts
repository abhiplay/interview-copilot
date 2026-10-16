import { describe, expect, it, vi } from 'vitest';
import { createWebPlatform } from '@/platform';
import { fakeApi, json } from '@/test/helpers';
import { startLiveSession } from './start-live';

describe('startLiveSession', () => {
  it('takes the session live, then starts capture', async () => {
    const { api } = fakeApi(() => json({ id: 's1', status: 'Live' }));
    const platform = createWebPlatform();
    const order: string[] = [];
    const startSession = vi.spyOn(api, 'startSession').mockImplementation(async (id) => {
      order.push(`start ${id}`);
      return { id, status: 'Live' } as Awaited<ReturnType<typeof api.startSession>>;
    });
    vi.spyOn(platform, 'startCapture').mockImplementation(async (id) => {
      order.push(`capture ${id}`);
      return { state: 'live', interviewer: true, mic: true, message: null };
    });

    const started = await startLiveSession(api, platform, 's1');

    expect(started.status).toBe('Live');
    expect(startSession).toHaveBeenCalledWith('s1');
    expect(order).toEqual(['start s1', 'capture s1']);
  });

  it('does not start capture when going live fails', async () => {
    const { api } = fakeApi(() => new Response(null, { status: 409 }));
    const platform = createWebPlatform();
    const capture = vi.spyOn(platform, 'startCapture');

    await expect(startLiveSession(api, platform, 's1')).rejects.toThrow();
    expect(capture).not.toHaveBeenCalled();
  });
});
