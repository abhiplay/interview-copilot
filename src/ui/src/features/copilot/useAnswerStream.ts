import { useCallback, useRef, useState } from 'react';
import { useApp } from '@/lib/app-context';
import type { AnswerModifier, QuestionType } from '@/types/api';

export interface AnswerState {
  question: string;
  text: string;
  questionType: QuestionType | null;
  version: number;
  status: 'idle' | 'streaming' | 'done' | 'error';
  error: string | null;
}

const IDLE: AnswerState = { question: '', text: '', questionType: null, version: 0, status: 'idle', error: null };

/** Streams answers for one session; a new question cancels the previous stream (atlas/copilot-pipeline.md). */
export function useAnswerStream(sessionId: string, llmProvider = 'ollama') {
  const { api, platform } = useApp();
  const [answer, setAnswer] = useState<AnswerState>(IDLE);
  const controller = useRef<AbortController | null>(null);

  const ask = useCallback(
    async (question: string, modifier: AnswerModifier = 'None', previous?: AnswerState) => {
      controller.current?.abort();
      const abort = new AbortController();
      controller.current = abort;
      setAnswer({ ...IDLE, question, status: 'streaming', version: previous?.version ?? 0 });

      const llmApiKey = (await platform.getApiKey(llmProvider)) ?? undefined;
      try {
        await api.ask(
          sessionId,
          {
            question,
            modifier,
            previousAnswer: previous?.text,
            previousVersion: previous?.version ?? 0,
            llmApiKey,
          },
          {
            onHeader: (header) => setAnswer((s) => ({ ...s, questionType: header.questionType, version: header.version })),
            onToken: (token) => setAnswer((s) => ({ ...s, text: s.text + token })),
            onError: (message) => setAnswer((s) => ({ ...s, status: 'error', error: message })),
            onDone: () => setAnswer((s) => ({ ...s, status: 'done' })),
          },
          abort.signal,
        );
      } catch (err) {
        if (!abort.signal.aborted) {
          setAnswer((s) => ({ ...s, status: 'error', error: err instanceof Error ? err.message : 'The answer failed.' }));
        }
      }
    },
    [api, platform, sessionId, llmProvider],
  );

  return { answer, ask };
}
