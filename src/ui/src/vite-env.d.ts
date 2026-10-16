/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Backend base URL. Defaults to the local backend (v1); Mode C points it at the hosted API. */
  readonly VITE_API_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
