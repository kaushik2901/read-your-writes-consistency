import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useApiBase } from '@/lib/api';
import { useAppState } from '@/state/AppState';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Calendar, Pencil, Plus, Trash2 } from 'lucide-react';

type Project = {
  id: number;
  name: string;
  createdByUserId: number;
  lastUpdatedAtUtc: string;
  members: { userId: number; displayName: string }[];
};

type TaskItem = {
  id: number;
  name: string;
  status: string;
};

export function ProjectPage() {
  const { projectId = '' } = useParams();
  const { api } = useApiBase();
  const { userId, consistencyMode, setLastIntent } = useAppState();

  const {
    data: project,
    isLoading: loadingProject,
    error: projectError,
  } = useQuery<Project>({
    queryKey: ['project', projectId, userId, consistencyMode],
    queryFn: async () => {
      const response = await api<Project>(`/projects/${projectId}`);
      setLastIntent(response.dbSource);
      if (!response.value) throw new Error('Project not found');
      return response.value;
    },
    enabled: !!projectId,
  });

  const {
    data: tasks,
    isLoading: loadingTasks,
    error: tasksError,
  } = useQuery<TaskItem[]>({
    queryKey: ['projectTasks', projectId, userId, consistencyMode],
    queryFn: async () => {
      const response = await api<TaskItem[]>(`/projects/${projectId}/tasks`);
      setLastIntent(response.dbSource);
      return response.value ?? [];
    },
    enabled: !!projectId,
  });

  if (loadingProject || loadingTasks)
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  if (projectError)
    return (
      <div className="text-destructive p-4 bg-destructive/10 rounded-lg">
        {String((projectError as Error).message)}
      </div>
    );
  if (!project)
    return <div className="text-center p-8 bg-muted rounded-lg">Project not found.</div>;

  const getStatusVariant = (status: string) => {
    switch (status.toLowerCase()) {
      case 'new':
        return 'default';
      case 'active':
        return 'success';
      case 'blocked':
        return 'destructive';
      default:
        return 'secondary';
    }
  };

  return (
    <div className="space-y-6">
      {/* Project Header - New Layout */}
      <div className="border-b pb-4">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-foreground">{project.name}</h1>
            <div className="flex items-center text-sm text-muted-foreground mt-2">
              <Calendar className="mr-1.5 h-4 w-4" />
              <span>
                Updated {new Date(project.lastUpdatedAtUtc).toLocaleDateString()} at{' '}
                {new Date(project.lastUpdatedAtUtc).toLocaleTimeString([], {
                  hour: '2-digit',
                  minute: '2-digit',
                })}
              </span>
            </div>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" onClick={() => {}}>
              <Pencil className="mr-2 h-4 w-4" />
              Edit Project
            </Button>
          </div>
        </div>
      </div>

      {/* Tasks Section */}
      <div className="space-y-4">
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-semibold">Tasks</h2>
          <Button size="sm" onClick={() => {}}>
            <Plus className="mr-2 h-4 w-4" />
            Add Task
          </Button>
        </div>

        {tasksError && (
          <div className="text-destructive p-4 bg-destructive/10 rounded-lg">
            {String((tasksError as Error).message)}
          </div>
        )}

        {!tasks || tasks.length === 0 ? (
          <div className="text-center p-8 bg-muted rounded-lg">
            <p className="text-muted-foreground">No tasks found for this project.</p>
            <Button className="mt-4" onClick={() => {}}>
              <Plus className="mr-2 h-4 w-4" />
              Create Your First Task
            </Button>
          </div>
        ) : (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[80px]">ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead className="w-[120px]">Status</TableHead>
                  <TableHead className="w-[100px] text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tasks.map(t => (
                  <TableRow key={t.id} className="hover:bg-muted/50">
                    <TableCell className="font-medium">{t.id}</TableCell>
                    <TableCell>{t.name}</TableCell>
                    <TableCell>
                      <Badge variant={getStatusVariant(t.status)} className="text-xs">
                        {t.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button variant="outline" size="sm" onClick={() => {}}>
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button variant="outline" size="sm" onClick={() => {}}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </div>
    </div>
  );
}
