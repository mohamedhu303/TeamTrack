// import { Task } from './../models/models/task.model';
// import { Injectable } from '@angular/core';
// import { HttpClient, HttpParams } from '@angular/common/http';
// import { Observable } from 'rxjs';
// import { Project } from '../models/models/project.model';

// @Injectable({
//   providedIn: 'root'
// })
// export class ProjectService {
//   private apiUrl = 'http://localhost:5154/api/Project';
//   private projectUrl = '/api/projects';
//   private taskUrl = '/api/tasks';

//   constructor(private http: HttpClient) {}

//     getProjects(params?: any): Observable<any> {
//     let httpParams = new HttpParams();

//     if (params) {
//       Object.keys(params).forEach((key) => {
//         if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
//           httpParams = httpParams.set(key, params[key]);
//         }
//       });
//     }

//     return this.http.get<any>(this.apiUrl, { params: httpParams });
//   }

// //   getProjects(params?: any): Observable<any> {
// //     return this.http.get<any>(this.projectUrl, { params });
// //   }

//   updateProject(id: number, dto: any): Observable<Project> {
//     return this.http.put<Project>(`${this.projectUrl}/${id}`, dto);
//   }

//   deleteProject(id: number): Observable<void> {
//     return this.http.delete<void>(`${this.projectUrl}/${id}`);
//   }

//   // ====== Tasks ======
//   updateTask(id: number, dto: any): Observable<Task> {
//     return this.http.put<Task>(`${this.taskUrl}/${id}`, dto);
//   }

//   getAllProjects(): Observable<any> {
//     return this.http.get(`${this.apiUrl}/Project/GetAll`);
//   }

//   getProjectDetails(id: number): Observable<any> {
//     return this.http.get(`${this.apiUrl}/Project/${id}/details`);
//   }

//   getTasksByProject(projectId: number): Observable<any> {
//     return this.http.get(`${this.apiUrl}/UserTask/ByProject/${projectId}`);
//   }

//   completeTask(taskId: number) {
//   return this.http.post(`${this.apiUrl}/UserTask/complete-task/${taskId}`, {});
// }

// }
