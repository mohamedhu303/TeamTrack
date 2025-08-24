import { User } from './user.model';
import { Task } from './task.model';

export enum ProjectStatus {
  Pending = 0,
  InProgress = 1,
  Suspended = 2,
  Completed = 3
}

export enum UserRole {
  Admin = 0,
  ProjectManager = 1,
  TeamMember = 2
}


export interface Project {
  id: number;
  name: string;
  description?: string;

  status?: ProjectStatus;

  projectManagerId: string;
  projectManager?: User | null;

  tasks: Task[];
  teamMembers?: User[];

  startDate?: string;  
  finishDate?: string;

  createdAt?: string;
  updatedAt?: string;
}


export interface PagedResponse<T> {
  items: T[];
  totalPages: number;
  totalCount: number;
}

export interface ProjectDto {
  id: number;
  name: string;
  description: string;
  projectManagerId: string;
  startDate: Date;
  finishDate: Date;
  status: string;
}
