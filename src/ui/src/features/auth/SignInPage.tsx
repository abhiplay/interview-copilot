import { useState, type FormEvent } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardDescription, CardTitle } from '@/components/ui/card';
import { Input, Label } from '@/components/ui/form';
import { useApp } from '@/lib/app-context';

/** FR-12: local accounts. Mode C replaces this with Google/GitHub sign-in. */
export function SignInPage() {
  const { api, completeSignIn } = useApp();
  const [mode, setMode] = useState<'sign-in' | 'register'>('sign-in');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      if (mode === 'register') {
        await api.register(username, password);
      }
      await completeSignIn(await api.signIn(username, password));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-sm items-center px-4">
      <Card className="w-full">
        <CardTitle>{mode === 'sign-in' ? 'Sign in' : 'Create a local account'}</CardTitle>
        <CardDescription className="mb-4">Your account and data stay on this computer.</CardDescription>
        <form onSubmit={submit} className="flex flex-col gap-3">
          <Label htmlFor="username">Username</Label>
          <Input id="username" autoComplete="username" value={username} onChange={(e) => setUsername(e.target.value)} required />
          <Label htmlFor="password">Password</Label>
          <Input
            id="password"
            type="password"
            autoComplete={mode === 'sign-in' ? 'current-password' : 'new-password'}
            minLength={8}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          {error && (
            <p role="alert" className="text-sm text-destructive">
              {error}
            </p>
          )}
          <Button type="submit" disabled={busy}>
            {mode === 'sign-in' ? 'Sign in' : 'Create account'}
          </Button>
          <Button type="button" variant="ghost" onClick={() => setMode(mode === 'sign-in' ? 'register' : 'sign-in')}>
            {mode === 'sign-in' ? 'New here? Create an account' : 'Have an account? Sign in'}
          </Button>
        </form>
      </Card>
    </main>
  );
}
