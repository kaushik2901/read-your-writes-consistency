export type TaskItem = {
  id: number;
  name: string;
  status: string;
  assignedUserId: string;
  userName: string | null;
  lastModifiedAtUtc: string;
};
