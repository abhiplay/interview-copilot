import js from '@eslint/js';
import globals from 'globals';
import tseslint from 'typescript-eslint';

export default tseslint.config(
  { ignores: ['dist', 'coverage', 'web-ext-artifacts'] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ['**/*.{ts,mjs}'],
    languageOptions: { globals: { ...globals.browser, ...globals.webextensions, ...globals.node } },
  },
);
