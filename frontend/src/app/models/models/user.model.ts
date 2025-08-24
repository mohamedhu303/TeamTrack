export interface User {
  id: string;
  name: string;
}

export enum UserRole {
  Admin = 0,
  ProjectManager = 1,
  TeamMember = 2
}
