import { User } from './user.model';

export interface Task {
  id: number;
  title: string;
  description?: string;
  percentComplete: number;
  startDate: string | Date;
  finishDate: string | Date;
  projectId: number;
  assignedUserId?: string;
  assignedUser?: User | null;
}
