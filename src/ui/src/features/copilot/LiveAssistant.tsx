import { useState, type FormEvent } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/form';
import { AnswerCard } from './AnswerCard';
import { useAnswerStream } from './useAnswerStream';

/**
 * The live answer panel. Until audio capture lands (Audio + Transcription modules), questions are
 * typed or pasted here; detected questions will call the same `ask`.
 */
export function LiveAssistant({ sessionId, compact = false }: { sessionId: string; compact?: boolean }) {
  const { answer, ask } = useAnswerStream(sessionId);
  const [question, setQuestion] = useState('');

  function submit(event: FormEvent) {
    event.preventDefault();
    if (question.trim()) {
      void ask(question.trim());
    }
  }

  return (
    <section className={compact ? 'grid gap-3' : 'grid gap-4'}>
      <form onSubmit={submit} className="flex gap-2">
        <Input aria-label="Question" placeholder="Type or paste the question you were asked" value={question} onChange={(e) => setQuestion(e.target.value)} />
        <Button type="submit">Answer</Button>
      </form>
      <AnswerCard answer={answer} onModify={(modifier) => void ask(answer.question, modifier, answer)} />
    </section>
  );
}
