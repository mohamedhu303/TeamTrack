import {
  AfterViewInit,
  AfterViewChecked,
  Component,
  OnInit,
  ElementRef,
  ViewChild,
  Renderer2,
  HostListener,
} from '@angular/core';
import { CommonModule, NgIf, NgFor } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { AdminService } from '../../services/admin.service';
import { LoadingService } from '../../services/loading-spinner.service';
import { SpinnerComponent } from '../loading-spinner/loading-spinner.component';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient } from '@angular/common/http';
import { NgSelectModule } from '@ng-select/ng-select';
import { Observable } from 'rxjs';
import { User } from '../../models/models/user.model';
import { AlertService } from '../../services/alert.service';
import { AlertComponent } from '../alert/alert.component';
@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    NgIf,
    NgFor,
    RouterModule,
    FormsModule,
    SpinnerComponent,
    MatMenuModule,
    MatButtonModule,
    MatIconModule,
    NgSelectModule,
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
})
export class AdminDashboardComponent implements OnInit {
  overview: any = null;
  users: any[] = [];
  searchTerm = '';
  selectedRole = '';
  fromDate: string = '';
  toDate: string = '';
  page = 1;
  pageSize = 10;
  totalUsers = 0;
  showModalUserDetails = false;
  selectedUser: any = null;
  NewPhone = '';
  showPhoneModal = false;

  showToast = false;
  toastMessage = '';
  showDeleteModal = false;
  selectedUserForDelete: any = null;
  deleteEmailInput = '';
  deleteErrorMessage = '';
  deleteError = '';
  showToastBox = false;
  toastMessageDelete: string = '';
  showToastBoxDelete: boolean = false;
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  private searchSubject = new Subject<string>();
  private initializedHeaders = new Set<HTMLElement>();
  showUserModal = false;
  showProjectModal = false;
  showTaskModal = false;
  projects: any[] = [];
  searchUsers$ = new Subject<string>();
  selectedRoleChange: { user: any; NewRole: string } | null = null;
  showRoleModal = false;
  createMenuOpen = false;
  showCreateUserModal = false;
  isCreatingUser = false;
  teamMembers: any[] = [];
  filteredTeamMembers: any[] = [];
  teamMemberSearch: string = '';
  allTeamMembers: any[] = [];
  highlightedIndex: number = -1;
  showTeamMemberList: boolean = false;
  selectedTeamMember: any = null;
  assignedTeamMember: any | null = null;
  showSuggestions: boolean = false;
  showUserEditModal = false;
  showConfirmSaveModal = false;
  editUserDto = {
    id: '',
    name: '',
    email: '',
    phone: '',
    role: '',
  };

  constructor(
    private adminService: AdminService,
    public loadingService: LoadingService,
    public router: Router,
    private el: ElementRef,
    public renderer: Renderer2,
    private http: HttpClient,
    private alert: AlertService
  ) {}

  ngOnInit() {
    this.loadOverview();
    this.loadUsers();
    this.loadProjectManagers();
    this.loadProjects();
    this.loadAllTeamMembers();
    this.searchSubject.pipe(debounceTime(500)).subscribe((term) => {
      this.page = 1;
      this.loadUsers(term, this.selectedRole, this.fromDate, this.toDate, this.page, this.pageSize);
    });
    this.adminService.getTeamMembers().subscribe((res: any) => {
      this.allTeamMembers = res;
    });
  }
  @ViewChild(AlertComponent) AlertComponent!: AlertComponent;

  triggerAlert() {
    this.alert.showAlert('Update Successfully', 'success');
  }
  onCreateUser() {
    console.log('Create User clicked');
  }

  onCreateProject() {
    console.log('Create Project clicked');
  }

  onCreateTask() {
    console.log('Create Task clicked');
  }

  onSearchChange(value: string) {
    this.searchSubject.next(value);
  }

  loadOverview() {
    this.adminService.getOverview().subscribe({
      next: (res) => (this.overview = res),
      error: (err) => console.error(err),
    });
  }

  loadUsers(
    searchTerm?: string,
    role?: string,
    fromDate?: string,
    toDate?: string,
    page: number = 1,
    pageSize: number = 10
  ) {
    this.adminService
      .getUsers(
        searchTerm || this.searchTerm,
        role || this.selectedRole,
        fromDate || this.fromDate,
        toDate || this.toDate,
        page,
        pageSize
      )
      .subscribe({
        next: (res: any) => {
          this.users = res.users;
          this.totalUsers = res.totalUsers;
        },
        error: (err) => console.error(err),
      });
  }

  search() {
    this.page = 1;

    let from: string | undefined = this.fromDate;
    let to: string | undefined = this.toDate;

    if (this.fromDate && this.fromDate.length === 4) from = `${this.fromDate}-01-01`;
    if (this.toDate && this.toDate.length === 4) to = `${this.toDate}-12-31`;

    if (this.fromDate && this.fromDate.length === 7) from = `${this.fromDate}-01`;
    if (this.toDate && this.toDate.length === 7) {
      const [y, m] = this.toDate.split('-');
      const lastDay = new Date(Number(y), Number(m), 0).getDate();
      to = `${y}-${m}-${lastDay.toString().padStart(2, '0')}`;
    }

    this.loadUsers(this.searchTerm, this.selectedRole, from, to, this.page, this.pageSize);
  }

  sortData(column: string) {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    this.users.sort((a, b) => {
      const valueA = (a[column] || '').toString().toLowerCase();
      const valueB = (b[column] || '').toString().toLowerCase();

      if (valueA < valueB) return this.sortDirection === 'asc' ? -1 : 1;
      if (valueA > valueB) return this.sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }

  openUserDetails(user: any) {
    this.selectedUser = user;
    this.showModalUserDetails = true;
  }

  closeUserDetails() {
    this.selectedUser = null;
    this.showModalUserDetails = false;
  }

  // Open the phone edit modal
  openPhoneModal(user: any) {
    this.selectedUser = user;
    this.NewPhone = user.phone || '';
    this.showPhoneModal = true;
  }

  // Close the modal
  closePhoneModal() {
    this.showPhoneModal = false;
    this.selectedUser = null;
    this.NewPhone = '';
  }

  // Update phone and show toast
  updatePhone() {
    if (!this.NewPhone.match(/^\d{11}$/)) {
      this.toastMessage = 'Phone must be 11 digits';
      this.showToastMessage();
      return;
    }

    this.adminService.updatePhone(this.selectedUser.id, this.NewPhone).subscribe({
      next: () => {
        this.selectedUser.phone = this.NewPhone;
        this.closePhoneModal();
        this.alert.showAlert('Phone updated successfully', 'success');
      },
      error: (err) => {
        this.alert.showAlert(err.error?.message || 'Phone update failed', 'error');
      },
    });
  }

  // Show toast and auto-hide after 3s
  showToastMessage() {
    this.showToast = true;
    setTimeout(() => {
      this.showToast = false;
    }, 3000);
  }

  showToastDelete(message: string) {
    this.toastMessageDelete = message;
    this.showToastBoxDelete = true;

    setTimeout(() => {
      this.showToastBoxDelete = false;
    }, 3000);
  }

  changeRole(user: any, event: Event) {
    const selectElement = event.target as HTMLSelectElement | null;
    if (!selectElement || !user) return;

    const NewRole = selectElement.value;

    this.adminService.updateRole(user.id, NewRole).subscribe({
      next: () => {
        user.Role = NewRole;
        console.log(`Role updated to ${NewRole}`);
      },
      error: (err) => console.error('Failed to update role', err),
    });
  }

  confirmRoleChange(user: any, event: Event) {
    const selectElement = event.target as HTMLSelectElement;
    const NewRole = selectElement.value;

    // Do nothing if the role is unchanged or user is invalid
    if (!user || user.role === NewRole) return;

    // Store the pending change and show confirmation modal
    this.selectedRoleChange = { user, NewRole };
    this.showRoleModal = true;
  }

  changeRoleConfirmed() {
    if (!this.selectedRoleChange) return;

    const { user, NewRole } = this.selectedRoleChange;

    this.adminService.updateRole(user.id, NewRole).subscribe({
      next: () => {
        user.role = NewRole; // Update the user's role locally
        console.log(`Role successfully updated to ${NewRole}`);
        this.alert.showAlert('Role updated successfully', 'success');
      },
      error: (err) => {
        console.error('Failed to update role', err),
          this.alert.showAlert(err.error?.message || 'Role update failed', 'error');
      },
    });
    this.showRoleModal = false;
    this.selectedRoleChange = null;
  }

  cancelRoleChange() {
    this.showRoleModal = false;
    this.selectedRoleChange = null;
  }

  // Variables
  // Open delete modal
  openDeleteModal(user: any) {
    this.selectedUserForDelete = user;
    this.deleteEmailInput = '';
    this.deleteErrorMessage = '';
    this.showDeleteModal = true;
  }
  // Close delete modal
  closeDeleteModal() {
    this.showDeleteModal = false;
    this.selectedUserForDelete = null;
    this.deleteEmailInput = '';
    this.deleteErrorMessage = '';
  }
  // Confirm deletion
  confirmDelete() {
    if (!this.selectedUserForDelete) return;

    const currentAdminEmail = JSON.parse(sessionStorage.getItem('user') || '{}').email;

    if (this.deleteEmailInput !== currentAdminEmail) {
      this.deleteErrorMessage = 'Email does not match! ⚠️';
      return;
    }

    this.adminService.deleteUser(this.selectedUserForDelete.id).subscribe({
      next: () => {
        this.users = this.users.filter((u) => u.id !== this.selectedUserForDelete!.id);
        this.showDeleteModal = false;
        this.alert.showAlert('User deleted successfully', 'success');
      },
      error: (err) => {
        this.alert.showAlert(err.error?.message || 'User deletion failed', 'error');
      },
    });
  }

  checkEmailAndDelete() {
    const currentEmail = JSON.parse(sessionStorage.getItem('user') || '{}').email;

    if (!this.selectedUserForDelete) return;

    if (this.deleteEmailInput === currentEmail) {
      // Email matches -> execute delete request automatically
      this.adminService.deleteUser(this.selectedUserForDelete.id).subscribe({
        next: () => {
          this.users = this.users.filter((u) => u.id !== this.selectedUserForDelete.id);
          this.showDeleteModal = false;
          this.selectedUserForDelete = null;
          this.deleteEmailInput = '';
          this.deleteError = '';
          this.showToastDelete('User deleted successfully');
        },
        error: (err) => (this.deleteError = err.error?.message || 'Delete failed'),
      });
    } else {
      this.deleteError = 'Email does not match!';
    }
  }

  openUserEditModal(user: any) {
    this.editUserDto = { ...user };
    this.showUserEditModal = true;
  }

  closeUserEditModal() {
    this.showUserEditModal = false;
  }

  openConfirmSaveModal() {
    this.showConfirmSaveModal = true;
  }

  closeConfirmSaveModal() {
    this.showConfirmSaveModal = false;
  }

  saveUserChanges() {
    this.adminService.updateUser(this.editUserDto).subscribe({
      next: (res) => {
        console.log('✅ User updated successfully:', res);

        const index = this.users.findIndex((u) => u.id === this.editUserDto.id);
        if (index !== -1) {
          this.users[index] = { ...this.editUserDto };
        }

        this.showConfirmSaveModal = false;
        this.showUserEditModal = false;
        this.alert.showAlert('User updated successfully', 'success');
      },
      error: (err) => {
        this.alert.showAlert(err.error?.message || 'User update failed', 'error');
      },
    });
  }

  changePage(p: number) {
    this.page = p;
    this.loadUsers(
      this.searchTerm,
      this.selectedRole,
      this.fromDate,
      this.toDate,
      this.page,
      this.pageSize
    );
  }

  onPageSizeChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.pageSize = Number(value);
    console.log('Page Size Changed:', this.pageSize);
    this.page = 1;
    this.loadUsers(
      this.searchTerm,
      this.selectedRole,
      this.fromDate,
      this.toDate,
      this.page,
      this.pageSize
    );
  }

  newUser = {
    name: '',
    email: '',
    password: '',
    role: 'TeamMember' as 'Admin' | 'ProjectManager' | 'TeamMember',
    sendEmail: true,
  };

  createUserErrors: {
    name?: string;
    email?: string;
    password?: string;
    role?: string;
    server?: string;
  } = {};

  userDto = { name: '', email: '', password: '', role: '' };
  projectDto = { name: '', description: '', projectManagerId: '' };
  taskDto = {
    title: '',
    description: '',
    percentComplete: 0,
    projectId: 0,
    assignedUserId: '',
    startDate: '',
    finishDate: '',
  };

  projectManagers: any[] = [];

  toggleCreateMenu() {
    this.createMenuOpen = !this.createMenuOpen;
  }

  closeModals() {
    this.showUserModal = false;
    this.showProjectModal = false;
    this.showTaskModal = false;
  }

  openCreateUserModal() {
    this.closeModals();
    this.showUserModal = true;
  }
  openCreateProjectModal() {
    this.closeModals();
    this.loadProjectManagers();
    this.showProjectModal = true;
  }
  openCreateTaskModal() {
    this.closeModals();
    this.loadProjects();
    this.loadAllTeamMembers();
    this.showTaskModal = true;
  }

  createUser() {
    this.http
      .post('http://localhost:5154/api/UserManagement/create-user-no-otp', this.userDto)
      .subscribe({
        next: () => this.alert.showAlert('User created successfully', 'success'), 
        error: (err) => this.alert.showAlert(err.error?.message || 'User creation failed', 'error'), 
      });
  }

  createProject() {
    if (!this.projectDto.projectManagerId) {
      this.alert.showAlert('Please select a Project Manager', 'error');
      return;
    }
    this.adminService.createProject(this.projectDto).subscribe({
      next: () => {
        this.alert.showAlert('Project created successfully', 'success'); 
        this.closeModals();
      },
      error: (err) =>
        this.alert.showAlert(err.error?.message || 'Project creation failed', 'error'), 
    });
  }

  createTask() {
    if (!this.taskDto.projectId || !this.taskDto.assignedUserId) {
      this.alert.showAlert('Please select Project and Team Member', 'error');
      return;
    }
    this.adminService.createTask(this.taskDto).subscribe({
      next: () => {
        this.alert.showAlert('Task created successfully', 'success'); 
      },
      error: (err) => this.alert.showAlert(err.error?.message || 'Task creation failed', 'error'),
    });
  }

  loadProjectManagers() {
    this.adminService.getProjectManagers().subscribe({
      next: (res) => (this.projectManagers = res),
      error: (err) => console.error(err),
    });
  }

  loadProjects() {
    this.adminService.getProjects().subscribe({
      next: (res) => (this.projects = res),
      error: (err) => console.error(err),
    });
  }

  loadAllTeamMembers() {
    this.adminService.getTeamMembers().subscribe({
      next: (res) => (this.allTeamMembers = res),
      error: (err) => console.error(err),
    });
  }

  onTeamMemberInput() {
    const search = this.teamMemberSearch.toLowerCase();
    this.filteredTeamMembers = search
      ? this.allTeamMembers.filter((m) => m.name.toLowerCase().includes(search))
      : [];
    this.showSuggestions = this.filteredTeamMembers.length > 0;
  }

  selectTeamMember(member: any) {
    this.teamMemberSearch = member.name;
    this.taskDto.assignedUserId = member.id;
    this.showSuggestions = false;
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.team-member-search-overlay')) {
      this.showSuggestions = false;
    }
  }
}
