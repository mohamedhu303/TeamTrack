import { UserRole } from './../../models/models/user.model';
import { ProjectService } from '../../services/project.service';
import { Component, OnInit, HostListener, ViewChild, ElementRef } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NgSelectModule } from '@ng-select/ng-select';
import { AdminService } from '../../services/admin.service';
import { LoadingService } from '../../services/loading-spinner.service';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import {
  Project,
  ProjectStatus,
  PagedResponse,
  ProjectDto,
} from '../../models/models/project.model';
import { Task } from '../../models/models/task.model';
import { User } from '../../models/models/user.model';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AlertService } from '../../services/alert.service';
import { ActivatedRoute } from '@angular/router';
@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [
    CommonModule,
    NgIf,
    NgFor,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    NgSelectModule,
    RouterModule,
    RouterModule,
  ],
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss'],
})
export class ProjectComponent implements OnInit {
  // Data
  projects: Project[] = [];
  totalProjects = 0;
  Math = Math;

  // Filters & paging
  searchTerm = '';
  selectedStatus = '';
  selectedPmId = '';
  page = 1;
  pageSize = 10;

  // Dropdown sources
  projectManagers: User[] = [];
  teamMembers: User[] = [];

  // UI state
  expandedProjectId: number | null = null;
  sortColumn: 'name' | 'status' | 'tasks' | 'pm' | '' = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  // Create/Edit modals
  showProjectModal = false;
  editProjectId: number | null = null;
  projectDto: ProjectDto = {
    id: 0,
    name: '',
    description: '',
    projectManagerId: '',
    startDate: new Date(),
    finishDate: new Date(),
    status: 'Pending',
  };

  currentPage: number = 1;
  totalPages: number = 0;
  filterStatus: string = '';
  isLoading: boolean = false;
  totalCount: number = 0;
  readonly projectStatusMap: string[] = ['InProgress', 'Suspended', 'Completed'];

  // Task form (inline per project)
  inlineTaskFormForProjectId: number | null = null;
  taskDto: Omit<Task, 'id'> & { id?: number } = {
    title: '',
    description: '',
    percentComplete: 0,
    startDate: new Date(),
    finishDate: new Date(),
    projectId: 0,
    assignedUserId: '',
  };
  editingTaskId: number | null = null;

  tasks: any[] = [];
  selectedTask: any = null;
  currentProjectId!: number;

  // Debounce
  private search$ = new Subject<void>();

  constructor(
    public loadingService: LoadingService,
    private adminService: AdminService,
    private http: HttpClient,
    private ProjectService: ProjectService,
    private alertService: AlertService,
    private route: ActivatedRoute
  ) {}

  private apiUrl = '/api/projects';

  ngOnInit(): void {
    this.bindDebounce();
    this.loadDropdowns();
    this.fetchProjects();

    this.adminService.getProjectManagers().subscribe({
      next: (pmList) => {
        this.projectManagers = pmList || [];
        this.fetchProjects();
      },
      error: () => {
        this.fetchProjects();
      },
    });

    this.route.params.subscribe((params) => {
      this.currentProjectId = +params['id'];
      this.loadTasks(this.currentProjectId);
    });

    this.adminService.getTeamMembers().subscribe({
      next: (res) => (this.teamMembers = res || []),
      error: () => {},
    });
  }

  currentUser = {
    id: '123',
    role: UserRole.Admin,
  };
  canEditProject(project: Project): boolean {
    return (
      this.currentUser.role === UserRole.Admin ||
      (this.currentUser.role === UserRole.ProjectManager &&
        project.projectManagerId === this.currentUser.id)
    );
  }

  canDeleteProject(): boolean {
    return this.currentUser.role === UserRole.Admin;
  }

  canEditTask(task: Task, project: Project): boolean {
    return (
      this.currentUser.role === UserRole.Admin ||
      (this.currentUser.role === UserRole.ProjectManager &&
        project.projectManagerId === this.currentUser.id)
    );
  }

  canDeleteTask(task: Task, project: Project): boolean {
    return this.canEditTask(task, project);
  }

  get pagedProjects(): Project[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filteredProjects.slice(start, start + this.pageSize);
  }

  bindDebounce() {
    this.search$.pipe(debounceTime(400)).subscribe(() => {
      this.page = 1;
    });
  }

  onSearch(term: string) {
    this.searchTerm = term;
    this.currentPage = 1;
    this.fetchProjects();
  }
  filteredProjects: Project[] = [];

  loadDropdowns() {
    this.adminService.getProjectManagers().subscribe({
      next: (res) => (this.projectManagers = res || []),
      error: () => {},
    });
    this.adminService.getTeamMembers().subscribe({
      next: (res) => (this.teamMembers = res || []),
      error: () => {},
    });
  }

  getProjects(params?: any): Observable<any> {
    let httpParams = new HttpParams();

    if (params) {
      Object.keys(params).forEach((key) => {
        if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
          httpParams = httpParams.set(key, params[key]);
        }
      });
    }

    return this.http.get(this.apiUrl, { params: httpParams });
  }

  getStatusNumber(status?: ProjectStatus): number {
    return status !== undefined ? status : 0;
  }

  fetchProjects() {
    this.loadingService.show();
    this.ProjectService.getProjectsWithTasks().subscribe({
      next: (res: any[]) => {
        this.projects = res.map((p: any) => ({
          ...p,
          tasks: p.tasks || p.Tasks || [],
        }));
        this.filteredProjects = this.applyFilters(this.projects);
        this.totalCount = this.projects.length;
        this.loadingService.hide();
      },
      error: () => {
        this.alertService.showAlert('Failed to load projects', 'error');
        this.loadingService.hide();
      },
    });
  }

  applyFilters(projects: Project[]): Project[] {
    return projects.filter((p) => {
      // ✅ 1- صلاحيات العرض
      const canView =
        this.currentUser.role === UserRole.Admin ||
        p.projectManagerId === this.currentUser.id ||
        p.tasks.some((t) => t.assignedUserId === this.currentUser.id);

      if (!canView) return false;

      // ✅ 2- فلترة الحالة (Status)
      const statusNum = p.status ?? -1;
      let selectedStatusNum: number | null = null;

      switch (this.selectedStatus) {
        case 'Pending':
          selectedStatusNum = 0;
          break;
        case 'InProgress':
          selectedStatusNum = 1;
          break;
        case 'Suspended':
          selectedStatusNum = 2;
          break;
        case 'Completed':
          selectedStatusNum = 3;
          break;
      }

      const statusMatches = selectedStatusNum === null ? true : statusNum === selectedStatusNum;

      // ✅ 3- فلترة الـ PM
      const pmMatches = !this.selectedPmId || p.projectManagerId === this.selectedPmId;

      // ✅ 4- فلترة البحث
      const searchMatches =
        !this.searchTerm || (p.name?.toLowerCase() ?? '').includes(this.searchTerm.toLowerCase());

      // ✅ لازم تتجمع كلها
      return statusMatches && pmMatches && searchMatches;
    });
  }

  selectedProject: Project | null = null;

  viewProjectDetails(project: Project) {
    this.selectedProject = project;
    this.showProjectModal = true;
  }

  viewTaskDetails(task: Task) {
    this.selectedTask = task;
    this.showTaskModal = true;
  }

  onFilterChange(): void {
    this.filteredProjects = this.applyFilters(this.projects);
  }

  toggleExpand(projectId: number) {
    this.expandedProjectId = this.expandedProjectId === projectId ? null : projectId;
  }

  // Open/close project modal
  openCreateProjectModal() {
    this.editProjectId = null;
    this.projectDto = {
      id: 0,
      name: '',
      description: '',
      projectManagerId: '',
      startDate: new Date(),
      finishDate: new Date(),
      status: 'NotStarted',
    };
    this.showProjectModal = true;
  }

  openEditProjectModal(p: Project) {
    this.editProjectId = p.id;
    this.projectDto = {
      id: p.id,
      name: p.name,
      description: p.description || '',
      projectManagerId: p.projectManagerId,
      startDate: p.startDate ? new Date(p.startDate) : new Date(),
      finishDate: p.finishDate ? new Date(p.finishDate) : new Date(),
      status: ProjectStatus[p.status ?? 0],
    };
    this.showProjectModal = true;
  }

  openProjectModal(project?: ProjectDto) {
    if (project) {
      this.projectDto = { ...project };
    } else {
      this.projectDto = {
        id: 0,
        name: '',
        description: '',
        projectManagerId: '',
        startDate: new Date(),
        finishDate: new Date(),
        status: 'NotStarted',
      };
    }
    this.showProjectModal = true;
  }

  closeProjectModal() {
    this.showProjectModal = false;
    this.projectDto = {
      id: 0,
      name: '',
      description: '',
      projectManagerId: '',
      startDate: new Date(),
      finishDate: new Date(),
      status: 'NotStarted',
    };
  }

  // Create/Update Project
  saveProject() {
    if (!this.projectDto.name || !this.projectDto.startDate || !this.projectDto.finishDate) {
      this.alertService.showAlert('Please fill all required fields', 'error');
      return;
    }

    this.loadingService.show();

    const payload = { ...this.projectDto }; // نخلي status رقم
    const saveObservable = this.projectDto.id
      ? this.ProjectService.updateProject(this.projectDto.id, payload)
      : this.ProjectService.createProject(payload);

    saveObservable.subscribe({
      next: () => {
        this.alertService.showAlert(
          this.projectDto.id ? 'Project updated successfully' : 'Project created successfully',
          'success'
        );
        this.fetchProjects();
        this.closeProjectModal();
        this.loadingService.hide();
      },
      error: (err) => {
        console.error(err);
        this.alertService.showAlert('Failed to save project', 'error');
        this.loadingService.hide();
      },
    });
  }

  // Delete project
  deleteProject(projectId: number) {
    if (!confirm('Are you sure you want to delete this project?')) return;

    this.loadingService.show();
    this.ProjectService.deleteProject(projectId).subscribe({
      next: () => {
        this.alertService.showAlert('Project deleted successfully', 'success');
        this.fetchProjects();
        this.loadingService.hide();
      },
      error: () => {
        this.alertService.showAlert('Failed to delete project', 'error');
        this.loadingService.hide();
      },
    });
  }

  showDeleteModal = false;
projectToDelete: Project | null = null;

openDeleteModal(project: Project) {
  this.projectToDelete = project;
  this.showDeleteModal = true;
}

cancelDelete() {
  this.projectToDelete = null;
  this.showDeleteModal = false;
}

confirmDelete() {
  if (!this.projectToDelete) return;

  this.loadingService.show();
  this.ProjectService.deleteProject(this.projectToDelete.id).subscribe({
    next: () => {
      this.alertService.showAlert('Project deleted successfully', 'success');
      this.fetchProjects();
      this.loadingService.hide();
      this.cancelDelete(); 
    },
    error: () => {
      this.alertService.showAlert('Failed to delete project', 'error');
      this.loadingService.hide();
      this.cancelDelete();
    },
  });
}


  // Inline Task Form
  openTaskForm(project: Project, task?: Task) {
    this.inlineTaskFormForProjectId = project.id;
    if (task) {
      this.editingTaskId = task.id;
      this.taskDto = {
        id: task.id,
        title: task.title,
        description: task.description || '',
        percentComplete: task.percentComplete || 0,
        startDate: task.startDate,
        finishDate: task.finishDate,
        projectId: project.id,
        assignedUserId: task.assignedUserId || '',
      };
    } else {
      this.editingTaskId = null;
      this.taskDto = {
        title: '',
        description: '',
        percentComplete: 0,
        startDate: new Date(),
        finishDate: new Date(),
        projectId: project.id,
        assignedUserId: '',
      };
    }
  }
  cancelTaskForm() {
    this.inlineTaskFormForProjectId = null;
    this.editingTaskId = null;
  }

  deleteTask(project: Project, task: Task) {
    if (!confirm(`Delete task "${task.title}"?`)) return;
    this.ProjectService.deleteTask(task.id).subscribe({
      next: (_) => {
        project.tasks = project.tasks.filter((x) => x.id !== task.id);
      },
      error: (_) => {},
    });
  }

  // Pagination
  changePage(p: number) {
    this.page = p;
    this.fetchProjects();
  }
  onPageSizeChange(val: string) {
    this.pageSize = Number(val || 10);
    this.page = 1;
    this.fetchProjects();
  }

  // Helpers
  readonly statusMap: Record<string, string> = {
    InProgress: 'In Progress',
    Suspended: 'Suspended',
    Completed: 'Completed',
    Pending: 'Pending',
  };

  loadProjects(): void {
    this.ProjectService.getProjectsWithTasks().subscribe({
      next: (res: Project[]) => {
        this.projects = res;
        this.filteredProjects = this.applyFilters(this.projects);
      },
      error: (err) => {
        this.alertService.showAlert(err.error?.message || 'Failed to load projects', 'error');
      },
    });
  }

  // Helper method
  getProjectStatusLabel(status?: number): string {
    switch (status) {
      case ProjectStatus.Pending:
        return 'Pending';
      case ProjectStatus.InProgress:
        return 'In Progress';
      case ProjectStatus.Suspended:
        return 'Suspended';
      case ProjectStatus.Completed:
        return 'Completed';
      default:
        return 'Unknown';
    }
  }

  badgeClass(status?: number): string {
    switch (status) {
      case 0:
        return 'badge bg-secondary';
      case 1:
        return 'badge bg-primary';
      case 2:
        return 'badge bg-warning text-dark';
      case 3:
        return 'badge bg-success';
      default:
        return 'badge bg-secondary';
    }
  }

  onSort(column: 'name' | 'status' | 'tasks' | 'pm') {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.fetchProjects();
  }
  onPageChange(page: number) {
    if (page < 1 || page > Math.ceil(this.totalCount / this.pageSize)) return;
    this.currentPage = page;
    this.fetchProjects();
  }

  // حالة المودال
  showTaskModal: boolean = false;
  currentProject!: Project;

  // فتح المودال
  openTaskModal(project: Project, task?: Task) {
    this.currentProject = project;
    this.editingTaskId = task ? task.id : null;

    this.taskDto = task
      ? { ...task }
      : {
          title: '',
          description: '',
          percentComplete: 0,
          startDate: new Date(),
          finishDate: new Date(),
          projectId: project.id,
          assignedUserId: '',
        };

    this.showTaskModal = true;
  }

  closeTaskModal() {
    this.showTaskModal = false;
    this.editingTaskId = null;
    this.taskDto = {
      title: '',
      description: '',
      percentComplete: 0,
      startDate: new Date(),
      finishDate: new Date(),
      projectId: 0,
      assignedUserId: '',
    };
  }

  // تعديل saveTask لتستخدم currentProject بدل inline form
  saveTask(project: Project) {
    if (!this.taskDto.title.trim()) return;

    const payload = {
      ...this.taskDto,
      percentComplete: Number(this.taskDto.percentComplete) || 0,
    };

    if (this.editingTaskId == null) {
      this.adminService.createTask(payload).subscribe({
        next: (t) => {
          project.tasks = [...(project.tasks || []), t];
          this.closeTaskModal();
        },
        error: (_) => {},
      });
    } else {
      this.ProjectService.updateTask(this.editingTaskId, payload).subscribe({
        next: (t: Task) => {
          project.tasks = project.tasks.map((x) => (x.id === t.id ? t : x));
          this.closeTaskModal();
        },
        error: (_) => {},
      });
    }
  }

  loadTasks(projectId: number): void {
    this.ProjectService.getTasksByProject(projectId).subscribe({
      next: (res: any[]) => {
        this.tasks = res;
      },
error: (err) => this.alertService.showAlert(err.error?.message || 'Failed to load tasks', 'error')
    });
  }

  updateTask(taskId: number, dto: any): void {
    this.ProjectService.updateTask(taskId, dto).subscribe({
      next: (res) => {
        const index = this.tasks.findIndex((t: any) => t.id === taskId);
        if (index !== -1) {
          this.tasks[index] = res.task;
        }
        this.selectedTask = res.task;
      },
error: (err) => this.alertService.showAlert(err.error?.message || 'Failed to update task', 'error')
    });
  }
}
