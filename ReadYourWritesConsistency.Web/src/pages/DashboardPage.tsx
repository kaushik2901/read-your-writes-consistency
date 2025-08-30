import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useApiBase } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Link } from 'react-router-dom';
import { useAppState } from '@/state/AppState';
import { Calendar, Plus } from 'lucide-react';
import { CreateProjectModal } from '@/components/CreateProjectModal';
import { useState } from 'react';

type DashboardProject = {
  id: number;
  name: string;
  newCount: number;
  activeCount: number;
  blockedCount: number;
  lastUpdatedAtUtc: string;
};

type User = {
  id: number;
  displayName: string;
};

export function DashboardPage() {
  const { api } = useApiBase();
  const { userId, consistencyMode, setLastIntent } = useAppState();
  const queryClient = useQueryClient();
  const [isCreateProjectModalOpen, setIsCreateProjectModalOpen] = useState(false);

  const openCreateProjectModal = () => setIsCreateProjectModalOpen(true);
  const closeCreateProjectModal = () => setIsCreateProjectModalOpen(false);

  const { data, isLoading, error } = useQuery<DashboardProject[]>({
    queryKey: ['dashboard', userId, consistencyMode], // Added userId and consistencyMode to the query key
    queryFn: async () => {
      const response = await api<DashboardProject[]>('/dashboard');
      setLastIntent(response.dbSource);
      return response.value ?? [];
    },
  });

  const { data: users } = useQuery<User[]>({
    queryKey: ['users', consistencyMode],
    queryFn: async () => {
      const response = await api<User[]>('/users');
      setLastIntent(response.dbSource);
      return response.value ?? [];
    },
  });

  const createProject = async (projectData: { name: string; memberUserIds: number[] }) => {
    try {
      const response = await api('/projects', {
        method: 'POST',
        body: {
          name: projectData.name,
          memberUserIds: projectData.memberUserIds,
        },
      });

      if (response.isSuccess) {
        // Refresh the dashboard data
        queryClient.invalidateQueries({
          queryKey: ['dashboard', userId, consistencyMode],
        });
        closeCreateProjectModal();
      } else {
        console.error('Failed to create project:', response.error);
      }
    } catch (error) {
      console.error('Error creating project:', error);
    }
  };

  if (isLoading)
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  if (error)
    return (
      <div className="text-destructive p-4 bg-destructive/10 rounded-lg">
        {String((error as Error).message)}
      </div>
    );
  if (!data || data.length === 0)
    return <div className="text-center p-8 bg-muted rounded-lg">No projects found.</div>;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <Button onClick={openCreateProjectModal}>
          <Plus className="mr-2 h-4 w-4" />
          Create Project
        </Button>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {data.map(p => (
          <Card key={p.id} className="flex flex-col hover:shadow-lg transition-shadow duration-300">
            <CardHeader className="pb-3">
              <CardTitle className="flex items-start justify-between">
                <div className="flex flex-col gap-1">
                  <span className="text-lg">{p.name}</span>
                  <div className="flex items-center text-xs text-muted-foreground">
                    <Calendar className="mr-1.5 h-3 w-3" />
                    <span>
                      Updated {new Date(p.lastUpdatedAtUtc).toLocaleDateString()} at{' '}
                      {new Date(p.lastUpdatedAtUtc).toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </span>
                  </div>
                </div>
                <Button asChild size="sm" variant="outline">
                  <Link to={`/projects/${p.id}`}>View</Link>
                </Button>
              </CardTitle>
            </CardHeader>
            <CardContent className="flex-grow pb-4 space-y-3">
              <div className="grid grid-cols-3 gap-2 pt-2">
                <div className="bg-blue-50 p-2 rounded text-center">
                  <div className="text-sm font-semibold text-blue-700">{p.newCount}</div>
                  <div className="text-xs text-muted-foreground">New</div>
                </div>
                <div className="bg-green-50 p-2 rounded text-center">
                  <div className="text-sm font-semibold text-green-700">{p.activeCount}</div>
                  <div className="text-xs text-muted-foreground">Active</div>
                </div>
                <div className="bg-red-50 p-2 rounded text-center">
                  <div className="text-sm font-semibold text-red-700">{p.blockedCount}</div>
                  <div className="text-xs text-muted-foreground">Blocked</div>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
      <CreateProjectModal
        open={isCreateProjectModalOpen}
        onClose={closeCreateProjectModal}
        onSubmit={createProject}
        users={users || []}
      />
    </div>
  );
}
