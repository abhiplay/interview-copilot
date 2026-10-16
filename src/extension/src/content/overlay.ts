import type { AnswerEvent, SessionSummary } from '../shared/backend';

export const DEFAULT_OPACITY = 0.85;

const STYLES = `
  :host { all: initial; }
  .panel { position: fixed; top: 16px; right: 16px; width: 380px; max-height: 70vh; overflow: auto; z-index: 2147483647;
    font: 14px/1.45 system-ui, sans-serif; color: #111; background: #fff; border: 1px solid #ddd; border-radius: 12px;
    box-shadow: 0 8px 30px rgba(0,0,0,.18); padding: 12px; display: grid; gap: 8px; }
  .row { display: flex; gap: 6px; }
  select, input { flex: 1; font: inherit; padding: 6px 8px; border: 1px solid #ccc; border-radius: 8px; }
  button { font: inherit; padding: 6px 10px; border: 0; border-radius: 8px; background: #3b4bc8; color: #fff; cursor: pointer; }
  button.ghost { background: transparent; color: #555; }
  .meta { font-size: 12px; color: #666; }
  .answer { white-space: pre-wrap; }
  .error { color: #b42318; }
  @media (prefers-color-scheme: dark) { .panel { color: #eee; background: #1d1f27; border-color: #333; } }
`;

export interface OverlayHandlers {
  onAsk: (sessionId: string, question: string) => void;
  onClose: () => void;
}

export interface Overlay {
  host: HTMLElement;
  setSessions(sessions: SessionSummary[], error?: string): void;
  apply(event: AnswerEvent): void;
  reset(): void;
}

/** Companion overlay in a Shadow DOM so meeting pages' CSS can't touch it (atlas/extension.md). */
export function createOverlay(doc: Document, handlers: OverlayHandlers, opacity = DEFAULT_OPACITY): Overlay {
  const host = doc.createElement('interview-copilot-overlay');
  const shadow = host.attachShadow({ mode: 'open' });
  shadow.innerHTML = `
    <style>${STYLES}</style>
    <div class="panel" role="dialog" aria-label="InterviewCopilot">
      <div class="row"><strong style="flex:1">InterviewCopilot</strong><button class="ghost" data-close aria-label="Close">✕</button></div>
      <select data-session aria-label="Live session"></select>
      <form class="row" data-form>
        <input data-question aria-label="Question" placeholder="Type or paste the question" />
        <button type="submit">Answer</button>
      </form>
      <div class="meta" data-meta></div>
      <div class="answer" data-answer aria-live="polite"></div>
      <div class="error" data-error role="alert"></div>
    </div>`;

  const panel = shadow.querySelector<HTMLElement>('.panel')!;
  const select = shadow.querySelector<HTMLSelectElement>('[data-session]')!;
  const input = shadow.querySelector<HTMLInputElement>('[data-question]')!;
  const meta = shadow.querySelector<HTMLElement>('[data-meta]')!;
  const answer = shadow.querySelector<HTMLElement>('[data-answer]')!;
  const error = shadow.querySelector<HTMLElement>('[data-error]')!;
  panel.style.opacity = String(opacity);

  shadow.querySelector('[data-close]')!.addEventListener('click', () => handlers.onClose());
  shadow.querySelector('[data-form]')!.addEventListener('submit', (event) => {
    event.preventDefault();
    const question = input.value.trim();
    if (!question || !select.value) {
      error.textContent = select.value ? '' : 'Start a session in the desktop app first.';
      return;
    }
    overlay.reset();
    handlers.onAsk(select.value, question);
  });

  const overlay: Overlay = {
    host,
    setSessions(sessions, message) {
      select.replaceChildren(
        ...sessions.map((session) => {
          const option = doc.createElement('option');
          option.value = session.id;
          option.textContent = session.title;
          return option;
        }),
      );
      error.textContent = message ?? (sessions.length === 0 ? 'No live session. Start one in the desktop app.' : '');
    },
    apply(event) {
      switch (event.type) {
        case 'meta':
          meta.textContent = `${event.questionType} · v${event.version}`;
          break;
        case 'delta':
          answer.textContent += event.token;
          break;
        case 'error':
          error.textContent = event.message;
          break;
        case 'done':
          break;
      }
    },
    reset() {
      meta.textContent = '';
      answer.textContent = '';
      error.textContent = '';
    },
  };
  return overlay;
}
