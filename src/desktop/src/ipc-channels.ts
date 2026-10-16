/** IPC channel names shared by main and preload. Keep in sync with src/ui/src/platform. */
export const IpcChannels = {
  getAuthToken: 'auth:get-token',
  setAuthToken: 'auth:set-token',
  getApiKey: 'keys:get',
  setApiKey: 'keys:set',
  deleteApiKey: 'keys:delete',
  setOverlayOpacity: 'overlay:set-opacity',
  showOverlay: 'overlay:show',
  hideOverlay: 'overlay:hide',
  getSettings: 'settings:get',
  setSettings: 'settings:set',
} as const;

export const BACKEND_URL = 'http://127.0.0.1:5480';
export const APP_PROTOCOL = 'app';
export const APP_HOST = 'interview-copilot';
