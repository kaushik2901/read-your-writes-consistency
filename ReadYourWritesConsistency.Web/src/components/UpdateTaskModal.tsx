import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';

type TaskItem = {
  id: number;
  name: string;
  status: string;
  assignedUserId: string;
};

type UpdateTaskModalProps = {
  open: boolean;
  onClose: () => void;
  onSubmit: (task: { id: number; name: string; status: string; assignedUserId: number }) => void;
  task: TaskItem | null;
};

const statusOptions = [
  { value: 'New', label: 'New' },
  { value: 'Active', label: 'Active' },
  { value: 'Blocked', label: 'Blocked' },
];

export function UpdateTaskModal({ open, onClose, onSubmit, task }: UpdateTaskModalProps) {
  const [name, setName] = useState('');
  const [status, setStatus] = useState('New');
  const [assignedUserId, setAssignedUserId] = useState('');

  // Reset form when task changes
  useEffect(() => {
    if (task) {
      setName(task.name);
      setStatus(task.status);
      setAssignedUserId(task.assignedUserId || '');
    } else {
      setName('');
      setStatus('New');
      setAssignedUserId('');
    }
  }, [task]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (task && name && status) {
      onSubmit({
        id: task.id,
        name,
        status,
        assignedUserId: parseInt(assignedUserId, 10) || 0,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Update Task</DialogTitle>
          <DialogDescription>
            Modify the task details. Update the task name, status, or assigned user.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-12 items-center gap-4">
              <Label htmlFor="name" className="text-right col-span-3">
                Name
              </Label>
              <Input
                id="name"
                value={name}
                onChange={e => setName(e.target.value)}
                className="col-span-9"
                required
              />
            </div>

            <div className="grid grid-cols-12 items-center gap-4">
              <Label className="text-right col-span-3">Status</Label>
              <div className="col-span-9">
                <RadioGroup value={status} onValueChange={setStatus} className="flex space-y-1">
                  {statusOptions.map(option => (
                    <div key={option.value} className="flex items-center space-x-2">
                      <RadioGroupItem value={option.value} id={option.value} />
                      <Label htmlFor={option.value}>{option.label}</Label>
                    </div>
                  ))}
                </RadioGroup>
              </div>
            </div>

            <div className="grid grid-cols-12 items-center gap-4">
              <Label htmlFor="assigned-user" className="text-right col-span-3">
                Assigned To
              </Label>
              <Input
                id="assigned-user"
                type="number"
                value={assignedUserId}
                onChange={e => setAssignedUserId(e.target.value)}
                className="col-span-9"
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit">Update Task</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
