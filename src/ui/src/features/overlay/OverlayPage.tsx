import { useEffect } from 'react';
import { useParams } from 'react-router';
import { LiveAssistant } from '@/features/copilot/LiveAssistant';

/**
 * Rendered inside the desktop app's transparent, always-on-top overlay window (FR-17).
 * Window opacity is applied by the desktop shell from the opacity setting.
 */
export function OverlayPage() {
  const { sessionId = '' } = useParams();

  useEffect(() => {
    document.documentElement.classList.add('overlay');
    return () => document.documentElement.classList.remove('overlay');
  }, []);

  return (
    <div className="h-screen p-2">
      <div className="h-full overflow-auto rounded-xl border border-border bg-background/95 p-3 shadow-lg [-webkit-app-region:drag]">
        <div className="[-webkit-app-region:no-drag]">
          <LiveAssistant sessionId={sessionId} compact />
        </div>
      </div>
    </div>
  );
}
