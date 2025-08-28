import { useMemo } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { useAppState } from '@/state/AppState';
import { Database, User, Shield } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { useApiBase } from '@/lib/api';

type User = {
  id: number;
  userName: string;
  displayName: string;
};

export function Header() {
  const { pathname } = useLocation();
  const { api } = useApiBase();
  const { userId, setUserId, consistencyMode, setConsistencyMode, lastIntent } = useAppState();

  // Fetch users from API
  const { data: users = [] } = useQuery<User[]>({
    queryKey: ['users'],
    queryFn: async () => {
      const usersData = await api<User[]>('/users');
      return usersData;
    },
  });

  // Determine DB source based on last intent
  const dbSource = useMemo(() => {
    return lastIntent === 'write' ? 'Master' : 'Replica';
  }, [lastIntent]);

  return (
    <header className="w-full border-b bg-background/95 backdrop-blur">
      <div className="container mx-auto max-w-6xl px-4 sm:px-6 md:px-8 py-3 flex flex-col sm:flex-row sm:items-center gap-4">
        <div className="flex items-center gap-2">
          <Shield className="h-6 w-6 text-primary" />
          <Link to="/" className="text-xl font-bold tracking-tight">
            Read Your Writes
          </Link>
        </div>

        <Separator orientation="vertical" className="hidden sm:block h-6" />

        <nav className="text-sm text-muted-foreground flex-1">
          <span className="hidden sm:inline truncate">{pathname}</span>
        </nav>

        <div className="flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2">
            <User className="h-4 w-4 text-muted-foreground" />
            <Select value={String(userId)} onValueChange={v => setUserId(Number(v))}>
              <SelectTrigger className="w-40">
                <SelectValue>
                  {users.find(u => u.id === userId)?.displayName ||
                    users.find(u => u.id === userId)?.userName ||
                    'Select User'}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {users.map(user => (
                  <SelectItem key={user.id} value={String(user.id)}>
                    {user.displayName || user.userName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center gap-2">
            <Shield className="h-4 w-4 text-muted-foreground" />
            <Select value={consistencyMode} onValueChange={setConsistencyMode}>
              <SelectTrigger className="w-44">
                <SelectValue>
                  {consistencyMode === 'ryw' ? 'Read your writes (v2)' : 'None (v1)'}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">None (v1)</SelectItem>
                <SelectItem value="ryw">Read your writes (v2)</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <Separator orientation="vertical" className="hidden sm:block h-6" />
          <div className="flex items-center gap-1.5">
            <Database className="h-4 w-4 text-muted-foreground" />
            <div className="text-sm">
              <span className="text-muted-foreground hidden sm:inline">DB:</span>
              <span
                className={
                  dbSource === 'Master' ? 'text-green-600 font-medium' : 'text-blue-600 font-medium'
                }
              >
                {dbSource}
              </span>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}
