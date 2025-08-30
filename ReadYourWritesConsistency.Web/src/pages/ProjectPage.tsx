import { useParams } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
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
import { TaskModal } from '@/components/TaskModal';
import { UpdateTaskModal } from '@/components/UpdateTaskModal';
import { DeleteTaskModal } from '@/components/DeleteTaskModal';
import { EditProjectModal } from '@/components/EditProjectModal';
import type { TaskItem } from '@/types/TaskItem';
import { useState } from 'react';

type Project = {
  projectMetaData: { id: number; name: string; createdByUserId: number; lastUpdatedAtUtc: string };
  members: { userId: number; displayName: string }[];
};

export function ProjectPage() {
  const { projectId = '' } = useParams();
  const { api } = useApiBase();
  const { userId, consistencyMode, setLastIntent } = useAppState();
  const queryClient = useQueryClient();
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isUpdateModalOpen, setIsUpdateModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isEditProjectModalOpen, setIsEditProjectModalOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<TaskItem | null>(null);

  const openAddModal = () => setIsAddModalOpen(true);
  const closeAddModal = () => setIsAddModalOpen(false);

  const openUpdateModal = (task: TaskItem) => {
    setSelectedTask(task);
    setIsUpdateModalOpen(true);
  };
  const closeUpdateModal = () => {
    setIsUpdateModalOpen(false);
    setSelectedTask(null);
  };

  const openDeleteModal = (task: TaskItem) => {
    setSelectedTask(task);
    setIsDeleteModalOpen(true);
  };
  const closeDeleteModal = () => {
    setIsDeleteModalOpen(false);
    setSelectedTask(null);
  };

  const openEditProjectModal = () => setIsEditProjectModalOpen(true);
  const closeEditProjectModal = () => setIsEditProjectModalOpen(false);

  const { data: project, isLoading: loadingProject } = useQuery<Project>({
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

  const createTask = async (task: { name: string; assignedUserId: number }) => {
    try {
      const response = await api(`/tasks`, {
        method: 'POST',
        body: {
          projectId,
          name: task.name,
          assignedUserId: task.assignedUserId,
          status: 'New',
        },
      });

      if (response.isSuccess) {
        // Refresh the tasks list
        queryClient.invalidateQueries({
          queryKey: ['projectTasks', projectId, userId, consistencyMode],
        });
        closeAddModal();
      } else {
        console.error('Failed to create task:', response.error);
      }
    } catch (error) {
      console.error('Error creating task:', error);
    }
  };

  const updateTask = async (task: {
    id: number;
    name: string;
    status: string;
    assignedUserId: number;
  }) => {
    try {
      const response = await api(`/tasks/${task.id}`, {
        method: 'PUT',
        body: {
          name: task.name,
          status: task.status,
          assignedUserId: task.assignedUserId,
        },
      });

      if (response.isSuccess) {
        // Refresh the tasks list
        queryClient.invalidateQueries({
          queryKey: ['projectTasks', projectId, userId, consistencyMode],
        });
        closeUpdateModal();
      } else {
        console.error('Failed to update task:', response.error);
      }
    } catch (error) {
      console.error('Error updating task:', error);
    }
  };

  const deleteTask = async (task: TaskItem) => {
    try {
      const response = await api(`/tasks/${task.id}`, {
        method: 'DELETE',
      });

      if (response.isSuccess) {
        // Refresh the tasks list
        queryClient.invalidateQueries({
          queryKey: ['projectTasks', projectId, userId, consistencyMode],
        });
        closeDeleteModal();
      } else {
        console.error('Failed to delete task:', response.error);
      }
    } catch (error) {
      console.error('Error deleting task:', error);
    }
  };

  const updateProject = async (projectData: { name: string }) => {
    try {
      const response = await api(`/projects/${projectId}`, {
        method: 'PUT',
        body: {
          name: projectData.name,
        },
      });

      if (response.isSuccess) {
        // Refresh the project data
        queryClient.invalidateQueries({
          queryKey: ['project', projectId, userId, consistencyMode],
        });
        closeEditProjectModal();
      } else {
        console.error('Failed to update project:', response.error);
      }
    } catch (error) {
      console.error('Error updating project:', error);
    }
  };

  if (loadingProject || loadingTasks)
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );

  if (tasksError)
    return (
      <div className="text-destructive p-4 bg-destructive/10 rounded-lg">
        {String((tasksError as Error).message)}
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
            <h1 className="text-3xl font-bold text-foreground">{project.projectMetaData.name}</h1>
            <div className="flex items-center text-sm text-muted-foreground mt-2">
              <Calendar className="mr-1.5 h-4 w-4" />
              <span>
                Updated {new Date(project.projectMetaData.lastUpdatedAtUtc).toLocaleDateString()} at{' '}
                {new Date(project.projectMetaData.lastUpdatedAtUtc).toLocaleTimeString([], {
                  hour: '2-digit',
                  minute: '2-digit',
                })}
              </span>
            </div>
          </div>
          <Button variant="outline" size="sm" onClick={openEditProjectModal}>
            <Pencil className="mr-2 h-4 w-4" />
            Edit Project
          </Button>
        </div>
      </div>

      {/* Tasks Section */}
      <div className="space-y-4">
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-semibold">Tasks</h2>
          <Button size="sm" onClick={openAddModal}>
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
            <Button className="mt-4" onClick={openAddModal}>
              <Plus className="mr-2 h-4 w-4" />
              Create Your First Task
            </Button>
          </div>
        ) : (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[80px] text-center">ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead className="w-[140px]">Status</TableHead>
                  <TableHead className="w-[140px]">Assigned To</TableHead>
                  <TableHead className="w-[180px]">Last Updated</TableHead>
                  <TableHead className="w-[100px] text-right"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tasks.map(t => (
                  <TableRow key={t.id} className="hover:bg-muted/50">
                    <TableCell className="font-medium text-center">{t.id}</TableCell>
                    <TableCell>{t.name}</TableCell>
                    <TableCell>
                      <Badge variant={getStatusVariant(t.status)} className="text-xs">
                        {t.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{t.userName}</TableCell>
                    <TableCell>
                      {new Date(t.lastModifiedAtUtc).toLocaleDateString()} at{' '}
                      {new Date(t.lastModifiedAtUtc).toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button variant="outline" size="sm" onClick={() => openUpdateModal(t)}>
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button variant="outline" size="sm" onClick={() => openDeleteModal(t)}>
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
      <TaskModal open={isAddModalOpen} onClose={closeAddModal} onSubmit={createTask} />
      <UpdateTaskModal
        open={isUpdateModalOpen}
        onClose={closeUpdateModal}
        onSubmit={updateTask}
        task={selectedTask}
      />
      <DeleteTaskModal
        open={isDeleteModalOpen}
        onClose={closeDeleteModal}
        onSubmit={deleteTask}
        task={selectedTask}
      />
      <EditProjectModal
        open={isEditProjectModalOpen}
        onClose={closeEditProjectModal}
        onSubmit={updateProject}
        project={project ? { name: project.projectMetaData.name } : null}
      />
    </div>
  );
}
