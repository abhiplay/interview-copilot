import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { ApiClient, DEFAULT_API_BASE_URL } from './api';
import { detectPlatform, type Platform } from '@/platform';

interface AppContextValue {
  api: ApiClient;
  platform: Platform;
  signedIn: boolean;
  /** Stores the token with the platform (shared with the overlay window) and marks the user signed in. */
  completeSignIn: (token: string) => Promise<void>;
}

const AppContext = createContext<AppContextValue | null>(null);

export function AppProvider({
  children,
  api: providedApi,
  platform: providedPlatform,
}: {
  children: ReactNode;
  api?: ApiClient;
  platform?: Platform;
}) {
  const api = useMemo(() => providedApi ?? new ApiClient(import.meta.env.VITE_API_BASE_URL ?? DEFAULT_API_BASE_URL), [providedApi]);
  const platform = useMemo(() => providedPlatform ?? detectPlatform(), [providedPlatform]);
  const [signedIn, setSignedIn] = useState(false);

  useEffect(() => {
    void platform.getAuthToken().then((token) => {
      if (token) {
        api.setToken(token);
        setSignedIn(true);
      }
    });
  }, [api, platform]);

  const value = useMemo<AppContextValue>(
    () => ({
      api,
      platform,
      signedIn,
      completeSignIn: async (token) => {
        api.setToken(token);
        await platform.setAuthToken(token);
        setSignedIn(true);
      },
    }),
    [api, platform, signedIn],
  );
  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useApp(): AppContextValue {
  const value = useContext(AppContext);
  if (!value) {
    throw new Error('useApp must be used inside <AppProvider>.');
  }
  return value;
}
