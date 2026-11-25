import { createContext, useContext, useMemo, useState, type PropsWithChildren } from 'react';
import type { AuthSession } from '@/api/identity';

interface AuthContextValue {
  session: AuthSession | null;
  setSession: (session: AuthSession | null) => void;
  signOut: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children, initialSession = null }: PropsWithChildren<{ initialSession?: AuthSession | null }>) {
  const [session, setSession] = useState<AuthSession | null>(initialSession);
  const value = useMemo(() => ({ session, setSession, signOut: () => setSession(null) }), [session]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used inside AuthProvider.');
  return context;
}
