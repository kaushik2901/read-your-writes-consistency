import { useState } from 'react';
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

type TaskModalProps = {
  open: boolean;
  onClose: () => void;
  onSubmit: (task: { name: string; assignedUserId: number }) => void;
};

export function TaskModal({ open, onClose, onSubmit }: TaskModalProps) {
  const [name, setName] = useState('');
  const [assignedUserId, setAssignedUserId] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (name && assignedUserId) {
      onSubmit({
        name,
        assignedUserId: parseInt(assignedUserId, 10),
      });
      // Reset form
      setName('');
      setAssignedUserId('');
    }
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Add New Task</DialogTitle>
          <DialogDescription>
            Create a new task for your project. Status will be set to "New" automatically.
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
              <Label htmlFor="assigned-user" className="text-right col-span-3">
                Assigned To
              </Label>
              <Input
                id="assigned-user"
                type="number"
                value={assignedUserId}
                onChange={e => setAssignedUserId(e.target.value)}
                className="col-span-9"
                required
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit">Add Task</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
