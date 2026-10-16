import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',
    coverage: {
      provider: 'v8',
      include: ['src/**/*.ts'],
      // content.ts and popup.ts only wire chrome.* APIs to the tested modules.
      exclude: ['src/content.ts', 'src/popup.ts', 'src/**/*.test.ts'],
      thresholds: { lines: 75, functions: 75, branches: 75, statements: 75 },
    },
  },
});
