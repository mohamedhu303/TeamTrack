import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
// import { environment } from '../../../src/app/'
import { Observable } from 'rxjs';
import { Project } from '../models/models/project.model';
import { Task } from '../models/models/task.model';
import { User } from '../models/models/user.model';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private base = 'http://localhost:5154/api';

  constructor(private http: HttpClient) {}

  // ==== Projects ====
  getProjects(): Observable<any> {
    return this.http.get(`${this.base}/Project/GetAll`, {
      headers: { Authorization: 'Bearer <YOUR_TOKEN>' },
    });
  }

  getProjectDetails(id: number): Observable<any> {
    return this.http.get(`${this.base}/Project/${id}/details`);
  }

  createProject(dto: any): Observable<any> {
    return this.http.post(`${this.base}/Project/create`, dto);
  }

  updateProject(id: number, dto: any): Observable<any> {
    return this.http.patch(`${this.base}/Project/update/${id}`, dto);
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Project/${id}`);
  }

  getProjectManagers(): Observable<any> {
    return this.http.get(`${this.base}/Project/project-managers`);
  }

  // ==== Tasks ====
  getTasksByProject(projectId: number): Observable<any> {
    return this.http.get(`${this.base}/UserTask/ByProject/${projectId}`);
  }

  createTask(dto: any): Observable<any> {
    return this.http.post(`${this.base}/UserTask/create`, dto);
  }

  updateTask(id: number, dto: any): Observable<any> {
    return this.http.patch(`${this.base}/UserTask/Update/${id}`, dto);
  }

  deleteTask(taskId: number) {
    return this.http.delete(`${this.base}/UserTask/delete/${taskId}`);
  }
  getProjectsWithTasks(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/Project/with-tasks`);
  }
}
