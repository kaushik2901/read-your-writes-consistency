import { createContext, useContext, useMemo, useState } from 'react';

type ConsistencyMode = 'none' | 'ryw';

type AppState = {
  userId: number;
  setUserId: (id: number) => void;
  consistencyMode: ConsistencyMode;
  setConsistencyMode: (m: ConsistencyMode) => void;
  lastIntent: string;
  setLastIntent: (i: string) => void;
};

const Ctx = createContext<AppState | null>(null);

export function AppStateProvider({ children }: { children: React.ReactNode }) {
  const [userId, setUserId] = useState<number>(1);
  const [consistencyMode, setConsistencyMode] = useState<ConsistencyMode>('none');
  const [lastIntent, setLastIntent] = useState<string>('');

  const value = useMemo<AppState>(
    () => ({
      userId,
      setUserId,
      consistencyMode,
      setConsistencyMode,
      lastIntent,
      setLastIntent,
    }),
    [userId, consistencyMode, lastIntent]
  );

  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

export function useAppState() {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error('useAppState must be used within AppStateProvider');
  return ctx;
}
