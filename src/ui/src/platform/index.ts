/**
 * Everything the UI needs from its host. The desktop app provides it via the Electron preload
 * bridge; in a plain browser (Mode C) the web fallback is used. Never import electron in src/ui.
 */
export interface DesktopBridge {
  /** Held in the main process's memory only, so the overlay window shares the sign-in. */
  getAuthToken(): Promise<string | null>;
  setAuthToken(token: string | null): Promise<void>;
  getApiKey(provider: string): Promise<string | null>;
  setApiKey(provider: string, key: string): Promise<void>;
  deleteApiKey(provider: string): Promise<void>;
  setOverlayOpacity(opacity: number): Promise<void>;
  showOverlay(sessionId: string): Promise<void>;
  hideOverlay(): Promise<void>;
  getSettings(): Promise<DesktopSettings>;
  setSettings(settings: Partial<DesktopSettings>): Promise<DesktopSettings>;
}

export interface DesktopSettings {
  overlayOpacity: number;
  /** FR-16: off by default, so the app asks before it starts listening. */
  autoStartOnMeeting: boolean;
}

export interface Platform {
  readonly kind: 'desktop' | 'web';
  getAuthToken(): Promise<string | null>;
  setAuthToken(token: string | null): Promise<void>;
  getApiKey(provider: string): Promise<string | null>;
  setApiKey(provider: string, key: string): Promise<void>;
  deleteApiKey(provider: string): Promise<void>;
  setOverlayOpacity(opacity: number): Promise<void>;
  showOverlay(sessionId: string): Promise<void>;
  hideOverlay(): Promise<void>;
  getSettings(): Promise<DesktopSettings>;
  setSettings(settings: Partial<DesktopSettings>): Promise<DesktopSettings>;
}

declare global {
  interface Window {
    interviewCopilot?: DesktopBridge;
  }
}

export const DEFAULT_SETTINGS: DesktopSettings = { overlayOpacity: 0.85, autoStartOnMeeting: false };

/** Mode C placeholder: no keychain or overlay in a browser; keys are never put in web storage. */
export function createWebPlatform(): Platform {
  let settings = { ...DEFAULT_SETTINGS };
  let authToken: string | null = null;
  return {
    kind: 'web',
    getAuthToken: () => Promise.resolve(authToken),
    setAuthToken: (token) => {
      authToken = token;
      return Promise.resolve();
    },
    getApiKey: () => Promise.resolve(null),
    setApiKey: () => Promise.reject(new Error('Saving API keys needs the desktop app.')),
    deleteApiKey: () => Promise.resolve(),
    setOverlayOpacity: () => Promise.resolve(),
    showOverlay: () => Promise.resolve(),
    hideOverlay: () => Promise.resolve(),
    getSettings: () => Promise.resolve(settings),
    setSettings: (patch) => {
      settings = { ...settings, ...patch };
      return Promise.resolve(settings);
    },
  };
}

export function createDesktopPlatform(bridge: DesktopBridge): Platform {
  return {
    kind: 'desktop',
    getAuthToken: () => bridge.getAuthToken(),
    setAuthToken: (token) => bridge.setAuthToken(token),
    getApiKey: (provider) => bridge.getApiKey(provider),
    setApiKey: (provider, key) => bridge.setApiKey(provider, key),
    deleteApiKey: (provider) => bridge.deleteApiKey(provider),
    setOverlayOpacity: (opacity) => bridge.setOverlayOpacity(opacity),
    showOverlay: (sessionId) => bridge.showOverlay(sessionId),
    hideOverlay: () => bridge.hideOverlay(),
    getSettings: () => bridge.getSettings(),
    setSettings: (patch) => bridge.setSettings(patch),
  };
}

export function detectPlatform(win: Window = window): Platform {
  return win.interviewCopilot ? createDesktopPlatform(win.interviewCopilot) : createWebPlatform();
}
