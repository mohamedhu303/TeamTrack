import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, finalize } from 'rxjs';
import { User } from '../models/models/user.model';

export interface CreateUserDto {
  name: string;
  email: string;
  password: string;
  role: 'Admin' | 'ProjectManager' | 'TeamMember';
  sendEmail?: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private baseUrl = 'http://localhost:5154/api/UserManagement';

  constructor(private http: HttpClient) {}

  getOverview(): Observable<any> {
    return this.http.get(`http://localhost:5154/api/admindashboard/overview`);
  }

  getUsers(
    searchTerm: string,
    role: string,
    fromDate: string,
    toDate: string,
    page: number,
    pageSize: number
  ) {
    return this.http.get(`${this.baseUrl}/Search`, {
      params: {
        searchTerm,
        role,
        fromDate,
        toDate,
        page: page.toString(),
        pageSize: pageSize.toString(),
      },
    });
  }

  updatePhone(id: string, newPhone: string) {
    return this.http.put(`${this.baseUrl}/update-phone/${id}`, { NewPhone: newPhone });
  }

  updateRole(id: string, newRole: string) {
    return this.http.put(`${this.baseUrl}/update-role/${id}`, { NewRole: newRole });
  }

  deleteUser(id: string) {
    return this.http.delete(`${this.baseUrl}/delete/${id}`);
  }

  createUserNoOtp(dto: CreateUserDto) {
    return this.http.post(`${this.baseUrl}/create-user-no-otp`, dto);
  }
  getProjectManagers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/project-managers`);
  }

  createProject(projectDto: any): Observable<any> {
    return this.http.post<any>('http://localhost:5154/api/Project/create', projectDto);
  }

  getTeamMembers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/team-members`);
  }

  getProjects(): Observable<any[]> {
    return this.http.get<any[]>(`http://localhost:5154/api/Project/all`);
  }

  createTask(taskDto: any): Observable<any> {
    return this.http.post<any>(`http://localhost:5154/api/UserTask/create`, taskDto);
  }

  updateUser(user: any): Observable<any> {
    const { id, ...dto } = user;
    return this.http.put(`${this.baseUrl}/update/${id}`, dto);
  }
}
