import path from 'node:path';
import { pathToFileURL } from 'node:url';
import { app, BrowserWindow, dialog, ipcMain, net, protocol, safeStorage, type IpcMainInvokeEvent } from 'electron';
import { BackendProcess, packagedBackendPath, waitForHealthy } from './backend-process';
import { APP_HOST, APP_PROTOCOL, IpcChannels } from './ipc-channels';
import { KeyStore } from './key-store';
import { decide, detectMeetingApp, listProcessNames } from './meeting-detection';
import { clampOpacity, SettingsStore } from './settings-store';
import { createMainWindow, createOverlayWindow } from './windows';

const MEETING_POLL_MS = 5_000;

protocol.registerSchemesAsPrivileged([{ scheme: APP_PROTOCOL, privileges: { standard: true, secure: true, supportFetchAPI: true } }]);

let mainWindow: BrowserWindow | null = null;
let overlayWindow: BrowserWindow | null = null;
let authToken: string | null = null;
let lastMeeting: string | null = null;
let backend: BackendProcess | null = null;

const settings = SettingsStore.inDirectory(app.getPath('userData'));
const keys = KeyStore.inDirectory(safeStorage, app.getPath('userData'));

/** Only our own pages may call the bridge. */
function fromTrustedFrame(event: IpcMainInvokeEvent): boolean {
  const url = event.senderFrame?.url ?? '';
  return url.startsWith(`${APP_PROTOCOL}://${APP_HOST}/`) || (!!process.env.INTERVIEW_COPILOT_DEV_URL && url.startsWith(process.env.INTERVIEW_COPILOT_DEV_URL));
}

function handle(channel: string, listener: (...args: unknown[]) => unknown): void {
  ipcMain.handle(channel, (event, ...args) => {
    if (!fromTrustedFrame(event)) {
      throw new Error('Untrusted sender.');
    }
    return listener(...args);
  });
}

function registerIpc(): void {
  handle(IpcChannels.getAuthToken, () => authToken);
  handle(IpcChannels.setAuthToken, (token) => {
    authToken = typeof token === 'string' ? token : null;
  });
  handle(IpcChannels.getApiKey, (provider) => keys.get(String(provider)));
  handle(IpcChannels.setApiKey, (provider, key) => keys.set(String(provider), String(key)));
  handle(IpcChannels.deleteApiKey, (provider) => keys.delete(String(provider)));
  handle(IpcChannels.getSettings, () => settings.get());
  handle(IpcChannels.setSettings, (patch) => settings.update(patch));
  handle(IpcChannels.setOverlayOpacity, (opacity) => overlayWindow?.setOpacity(clampOpacity(opacity)));
  handle(IpcChannels.showOverlay, async (sessionId) => {
    overlayWindow?.close();
    overlayWindow = createOverlayWindow(String(sessionId), (await settings.get()).overlayOpacity);
    overlayWindow.on('closed', () => (overlayWindow = null));
  });
  handle(IpcChannels.hideOverlay, () => {
    overlayWindow?.close();
    overlayWindow = null;
  });
}

function serveUi(): void {
  const uiRoot = app.isPackaged ? path.join(process.resourcesPath, 'ui') : path.resolve(__dirname, '../../ui/dist');
  protocol.handle(APP_PROTOCOL, (request) => {
    const { pathname } = new URL(request.url);
    const filePath = path.normalize(path.join(uiRoot, decodeURIComponent(pathname)));
    if (!filePath.startsWith(uiRoot)) {
      return new Response('Not found', { status: 404 });
    }
    return net.fetch(pathToFileURL(filePath).toString());
  });
}

async function startBackend(): Promise<void> {
  if (app.isPackaged) {
    backend = new BackendProcess(packagedBackendPath(process.resourcesPath), (message) => dialog.showErrorBox('InterviewCopilot', message));
    backend.start();
  }
  if (!(await waitForHealthy())) {
    dialog.showErrorBox(
      'InterviewCopilot',
      app.isPackaged
        ? "The local backend didn't start. Check that port 5480 is free and restart the app."
        : 'Backend not reachable on http://127.0.0.1:5480. Start it with: pnpm dev:backend',
    );
  }
}

/** FR-16: ask before listening when a meeting app starts (unless auto-start is on). */
function watchForMeetings(): void {
  setInterval(() => {
    void (async () => {
      const meeting = detectMeetingApp(await listProcessNames().catch(() => []));
      const decision = decide(lastMeeting, meeting, (await settings.get()).autoStartOnMeeting);
      lastMeeting = meeting;
      if (decision === 'ignore' || !mainWindow) {
        return;
      }
      if (decision === 'ask') {
        const { response } = await dialog.showMessageBox(mainWindow, {
          type: 'question',
          buttons: ['Start InterviewCopilot', 'Not now'],
          defaultId: 0,
          cancelId: 1,
          message: `${meeting} meeting detected. Start InterviewCopilot?`,
          detail: 'It will listen to the meeting audio on this computer. Make sure transcription is allowed in this conversation.',
        });
        if (response !== 0) {
          return;
        }
      }
      mainWindow.show();
      mainWindow.focus();
    })();
  }, MEETING_POLL_MS);
}

void app.whenReady().then(async () => {
  serveUi();
  registerIpc();
  mainWindow = createMainWindow();
  mainWindow.on('closed', () => (mainWindow = null));
  await startBackend();
  watchForMeetings();
});

app.on('window-all-closed', () => {
  backend?.stop();
  app.quit();
});
