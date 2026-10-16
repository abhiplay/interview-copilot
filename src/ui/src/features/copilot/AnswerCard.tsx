import { RefreshCw, Minimize2, Maximize2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { QUESTION_TYPE_LABELS, type AnswerModifier } from '@/types/api';
import type { AnswerState } from './useAnswerStream';

/** Shows the streaming answer with the Regenerate / Shorter / Longer buttons (FR-09, FR-10). */
export function AnswerCard({ answer, onModify }: { answer: AnswerState; onModify: (modifier: AnswerModifier) => void }) {
  if (answer.status === 'idle') {
    return <p className="text-sm text-muted-foreground">Suggested answers appear here when a question is asked.</p>;
  }

  const busy = answer.status === 'streaming';
  return (
    <article className="grid gap-3" aria-live="polite">
      <header className="grid gap-1">
        <p className="text-sm text-muted-foreground">{answer.question}</p>
        {answer.questionType && (
          <span className="w-fit rounded-full bg-muted px-2 py-0.5 text-xs">
            {QUESTION_TYPE_LABELS[answer.questionType]} · v{answer.version}
          </span>
        )}
      </header>
      <p className="whitespace-pre-wrap text-base leading-relaxed">{answer.text || (busy ? 'Thinking…' : '')}</p>
      {answer.error && (
        <p role="alert" className="text-sm text-destructive">
          {answer.error}
        </p>
      )}
      <div className="flex flex-wrap gap-2">
        <Button size="sm" variant="outline" disabled={busy} onClick={() => onModify('Regenerate')}>
          <RefreshCw className="size-3.5" /> Regenerate
        </Button>
        <Button size="sm" variant="outline" disabled={busy || !answer.text} onClick={() => onModify('Shorter')}>
          <Minimize2 className="size-3.5" /> Shorter
        </Button>
        <Button size="sm" variant="outline" disabled={busy || !answer.text} onClick={() => onModify('Longer')}>
          <Maximize2 className="size-3.5" /> Longer
        </Button>
      </div>
    </article>
  );
}
