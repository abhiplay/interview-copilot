// Builds the extension twice (Chromium + Firefox) from one codebase.
// Each entry is bundled as a classic IIFE script because MV3 content scripts can't be modules.
import { copyFile, mkdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { build } from 'vite';

const root = path.resolve(import.meta.dirname, '..');
const entries = ['background', 'content', 'popup'];
const base = JSON.parse(await readFile(path.join(root, 'manifest.base.json'), 'utf8'));

const targets = {
  chromium: { ...base, background: { service_worker: 'background.js' } },
  firefox: {
    ...base,
    background: { scripts: ['background.js'] },
    browser_specific_settings: { gecko: { id: 'interview-copilot@ukez15.github.io', strict_min_version: '128.0' } },
  },
};

for (const [target, manifest] of Object.entries(targets)) {
  const outDir = path.join(root, 'dist', target);
  for (const entry of entries) {
    await build({
      root,
      logLevel: 'warn',
      build: {
        outDir,
        emptyOutDir: false,
        lib: { entry: path.join(root, 'src', `${entry}.ts`), formats: ['iife'], name: entry, fileName: () => `${entry}.js` },
      },
    });
  }
  await mkdir(outDir, { recursive: true });
  await writeFile(path.join(outDir, 'manifest.json'), JSON.stringify(manifest, null, 2));
  await copyFile(path.join(root, 'src', 'popup.html'), path.join(outDir, 'popup.html'));
}
