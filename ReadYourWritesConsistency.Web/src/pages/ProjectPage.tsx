import { useParams } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useApiBase } from '@/lib/api';
import { useAppState } from '@/state/AppState';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Calendar, Pencil, Plus, Trash2 } from 'lucide-react';
import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';

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

type User = {
  id: number;
  userName: string;
  displayName: string;
};

const formSchema = z.object({
  name: z.string().min(1, 'Project name is required'),
});

type FormValues = z.infer<typeof formSchema>;

const taskFormSchema = z.object({
  name: z.string().min(1, 'Task name is required'),
  status: z.string().min(1, 'Status is required'),
  assignedUserId: z.number().optional(),
});

type TaskFormValues = z.infer<typeof taskFormSchema>;

export function ProjectPage() {
  const { projectId = '' } = useParams();
  const { api } = useApiBase();
  const { userId, consistencyMode, setLastIntent } = useAppState();
  const queryClient = useQueryClient();
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isCreateTaskDialogOpen, setIsCreateTaskDialogOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<TaskItem | null>(null);

  const {
    data: project,
    isLoading: loadingProject,
    error: projectError,
  } = useQuery<Project>({
    queryKey: ['project', projectId, userId, consistencyMode],
    queryFn: async () => {
      const projectData = await api<Project>(`/projects/${projectId}`);
      setLastIntent('read');
      return projectData;
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
      const tasksData = await api<TaskItem[]>(`/projects/${projectId}/tasks`);
      setLastIntent('read');
      return tasksData;
    },
    enabled: !!projectId,
  });

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: project?.name || '',
    },
  });

  const taskForm = useForm<TaskFormValues>({
    resolver: zodResolver(taskFormSchema),
    defaultValues: {
      name: '',
      status: 'new',
      assignedUserId: undefined,
    },
  });

  // Update form default values when project data loads
  React.useEffect(() => {
    if (project) {
      form.reset({ name: project.name });
    }
  }, [project, form]);

  const handleEditProject = async (values: FormValues) => {
    try {
      await api(`/projects/${projectId}`, {
        method: 'PUT',
        body: {
          name: values.name,
          memberUserIds: [], // Send empty array instead of null
        },
      });

      // Close dialog and refresh project data
      setIsEditDialogOpen(false);
      queryClient.invalidateQueries({ queryKey: ['project', projectId, userId, consistencyMode] });
    } catch (error) {
      console.error('Failed to update project:', error);
      // TODO: Show error to user
    }
  };

  const handleCreateTask = async (values: TaskFormValues) => {
    try {
      await api('/tasks', {
        method: 'POST',
        body: {
          projectId: parseInt(projectId),
          name: values.name,
          status: values.status,
          assignedUserId: values.assignedUserId || null,
        },
      });

      // Close dialog and refresh tasks data
      setIsCreateTaskDialogOpen(false);
      taskForm.reset({ name: '', status: 'new', assignedUserId: undefined });
      queryClient.invalidateQueries({
        queryKey: ['projectTasks', projectId, userId, consistencyMode],
      });
    } catch (error) {
      console.error('Failed to create task:', error);
      // TODO: Show error to user
    }
  };

  const handleEditTask = (task: TaskItem) => {
    setEditingTask(task);
    taskForm.reset({
      name: task.name,
      status: task.status,
      assignedUserId: undefined, // We don't have this info in the current task model
    });
  };

  // Update task form when editing task changes
  React.useEffect(() => {
    if (editingTask) {
      taskForm.reset({
        name: editingTask.name,
        status: editingTask.status,
        assignedUserId: undefined, // We don't have this info in the current task model
      });
    }
  }, [editingTask, taskForm]);

  const handleUpdateTask = async (values: TaskFormValues) => {
    if (!editingTask) return;

    try {
      await api(`/tasks/${editingTask.id}`, {
        method: 'PUT',
        body: {
          taskId: editingTask.id,
          name: values.name || undefined,
          status: values.status || undefined,
          assignedUserId: values.assignedUserId || null,
        },
      });

      // Close dialog and refresh tasks data
      setEditingTask(null);
      taskForm.reset({ name: '', status: 'new', assignedUserId: undefined });
      queryClient.invalidateQueries({
        queryKey: ['projectTasks', projectId, userId, consistencyMode],
      });
    } catch (error) {
      console.error('Failed to update task:', error);
      // TODO: Show error to user
    }
  };

  const handleDeleteTask = async (taskId: number) => {
    try {
      await api(`/tasks/${taskId}`, {
        method: 'DELETE',
      });

      // Refresh tasks data
      queryClient.invalidateQueries({
        queryKey: ['projectTasks', projectId, userId, consistencyMode],
      });
    } catch (error) {
      console.error('Failed to delete task:', error);
      // TODO: Show error to user
    }
  };

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
            <Button variant="outline" size="sm" onClick={() => setIsEditDialogOpen(true)}>
              <Pencil className="mr-2 h-4 w-4" />
              Edit Project
            </Button>
          </div>
        </div>
      </div>

      {/* Edit Project Dialog */}
      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Edit Project</DialogTitle>
            <DialogDescription>
              Make changes to your project here. Click save when you're done.
            </DialogDescription>
          </DialogHeader>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(handleEditProject)} className="space-y-4">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Project Name</FormLabel>
                    <FormControl>
                      <Input placeholder="Project name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <DialogFooter>
                <Button type="button" variant="outline" onClick={() => setIsEditDialogOpen(false)}>
                  Cancel
                </Button>
                <Button type="submit">Save Changes</Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      {/* Create Task Dialog */}
      <Dialog open={isCreateTaskDialogOpen} onOpenChange={setIsCreateTaskDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Create New Task</DialogTitle>
            <DialogDescription>
              Add a new task to your project. Click create when you're done.
            </DialogDescription>
          </DialogHeader>
          <Form {...taskForm}>
            <form onSubmit={taskForm.handleSubmit(handleCreateTask)} className="space-y-4">
              <FormField
                control={taskForm.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Task Name</FormLabel>
                    <FormControl>
                      <Input placeholder="Task name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={taskForm.control}
                name="status"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Status</FormLabel>
                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select status" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="new">New</SelectItem>
                        <SelectItem value="active">Active</SelectItem>
                        <SelectItem value="blocked">Blocked</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={taskForm.control}
                name="assignedUserId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Assign To</FormLabel>
                    <Select
                      onValueChange={value =>
                        field.onChange(value === '0' ? null : parseInt(value))
                      }
                      value={field.value?.toString() || '0'}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select user" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="0">Unassigned</SelectItem>
                        {project.members.map(member => (
                          <SelectItem key={member.userId} value={member.userId.toString()}>
                            {member.displayName}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setIsCreateTaskDialogOpen(false)}
                >
                  Cancel
                </Button>
                <Button type="submit">Create Task</Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      {/* Edit Task Dialog */}
      <Dialog open={!!editingTask} onOpenChange={open => !open && setEditingTask(null)}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Edit Task</DialogTitle>
            <DialogDescription>
              Make changes to your task. Click save when you're done.
            </DialogDescription>
          </DialogHeader>
          <Form {...taskForm}>
            <form onSubmit={taskForm.handleSubmit(handleUpdateTask)} className="space-y-4">
              <FormField
                control={taskForm.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Task Name</FormLabel>
                    <FormControl>
                      <Input placeholder="Task name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={taskForm.control}
                name="status"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Status</FormLabel>
                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select status" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="new">New</SelectItem>
                        <SelectItem value="active">Active</SelectItem>
                        <SelectItem value="blocked">Blocked</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={taskForm.control}
                name="assignedUserId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Assign To</FormLabel>
                    <Select
                      onValueChange={value =>
                        field.onChange(value === '0' ? null : parseInt(value))
                      }
                      value={field.value?.toString() || '0'}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select user" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="0">Unassigned</SelectItem>
                        {project.members.map(member => (
                          <SelectItem key={member.userId} value={member.userId.toString()}>
                            {member.displayName}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <DialogFooter>
                <Button type="button" variant="outline" onClick={() => setEditingTask(null)}>
                  Cancel
                </Button>
                <Button type="submit">Save Changes</Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      {/* Tasks Section */}
      <div className="space-y-4">
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-semibold">Tasks</h2>
          <Button size="sm" onClick={() => setIsCreateTaskDialogOpen(true)}>
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
            <Button className="mt-4" onClick={() => setIsCreateTaskDialogOpen(true)}>
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
                        <Button variant="outline" size="sm" onClick={() => handleEditTask(t)}>
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button variant="outline" size="sm" onClick={() => handleDeleteTask(t.id)}>
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
