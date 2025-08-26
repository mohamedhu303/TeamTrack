# TeamTrack — Secure Project Management & CRM Platform

> A pragmatic, end‑to‑end web application for project & task tracking with enterprise‑grade authentication.

Built with **Angular 17** (frontend) and **ASP.NET Core 9 Web API** (backend), backed by **SQL Server** and **Entity Framework Core**. The system emphasizes **Authentication & Authorization** (Identity + JWT + OTP), **intelligent filtering & live search**, and a clean, scalable architecture.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Tech Stack](#tech-stack)
4. [Core Features](#core-features)
5. [Domain Model](#domain-model)
6. [API Design](#api-design)
7. [Authentication & Security](#authentication--security)
8. [Frontend (Angular) Overview](#frontend-angular-overview)
9. [Installation & Setup](#installation--setup)
10. [Configuration](#configuration)
11. [Database & EF Core](#database--ef-core)
12. [Intelligent Search](#intelligent-search)
13. [Alerts & Loading Spinner](#alerts--loading-spinner)
14. [Validation & Error Handling](#validation--error-handling)
15. [Testing](#testing)
16. [Code Quality](#code-quality)
17. [Roadmap](#roadmap)
18. [Contributing](#contributing)
    
---

## Overview

**TeamTrack** is a **Secure Project Management & CRM Platform**. It combines project lifecycle management (projects, tasks, statuses, assignees) with communication hooks (WhatsApp/Email notifications) and a strong security model (Identity + JWT + OTP + RBAC).

Key objectives:

* Reliable **auth flows** (login, registration, password reset, OTP verification).
* **Task & Project Management** with clear status transitions (Pending → InProgress → Completed → Suspended).
* **Real‑time notifications** to Project Managers upon task completion (WhatsApp via Twilio + Email).
* Clean architecture that’s **scalable** and **maintainable**.

---

## Architecture

### High‑Level

```
[Angular 17 SPA]
   │  (HTTPS, JWT Bearer)
   ▼
[ASP.NET Core 9 Web API]
   │  (Services → Repositories)
   ▼
[EF Core → SQL Server]
```

### Layering (Backend)

* **Presentation**: Controllers (REST endpoints)
* **Application/Domain Services**: Business logic & use cases
* **Infrastructure**: Repositories, EF Core DbContext, external services (Twilio, SMTP)
* **Cross‑cutting**: Auth, validation, logging

### Frontend (Angular)

* **Feature Modules**: `projects`, `tasks`, `auth`, `dashboard`
* **Core Module**: interceptors (JWT, loader), services (API, Auth, Alerts)
* **Shared Module**: UI components, pipes, directives

---

## Tech Stack

* **Frontend**: Angular 17 (TypeScript, RxJS, Reactive Forms, Routing)
* **Backend**: ASP.NET Core 9 (Web API)
* **Database**: SQL Server
* **ORM**: Entity Framework Core
* **Auth**: ASP.NET Core Identity, JWT, OTP (Email & WhatsApp/Twilio), RBAC
* **Build/CI**: Node/NPM + .NET SDK; optional GitHub Actions

---

## Core Features

* Project & Task lifecycle management
* **Secure Authentication**: Identity + JWT + OTP (login & password recovery)
* **RBAC**: Admin, ProjectManager, Member
* **Notifications**: WhatsApp (Twilio) + Email upon significant events (e.g., Task Completed)
* **Intelligent Filtering & Real‑Time Search** (debounced, typo‑tolerant basics)
* **Alert Services** (success/info/warn/error)
* **Loading Spinner** during async operations
* Theme toggle: Dark/Light

---

## Domain Model

### Entities (simplified)

* **User**: Identity user (Id, Name, Email, Phone, Roles)
* **Project**: (Id, Title, Description, Status, StartDate, DueDate, OwnerId)
* **Task**: (Id, ProjectId, Title, Description, AssignedUserId, Status, Priority, DueDate, CompletedAt)
* **Notification**: (Id, UserId, Type \[Email/WhatsApp], TemplateKey, Payload, SentAt, Status)

### Relationships

* User 1‑\* Project (Owner)
* Project 1‑\* Task
* User 1‑\* Task (AssignedUser)

### Status Enums

* **ProjectStatus**: Pending | InProgress | Completed | Suspended
* **TaskStatus**: Pending | InProgress | Completed | Blocked

---

## API Design

Base URL: `/api`

### Auth

* `POST /api/auth/register`
* `POST /api/auth/login` → returns JWT
* `POST /api/auth/send-otp-for-password-change` → Email/WhatsApp OTP
* `POST /api/auth/verify-otp` → verify OTP
* `POST /api/auth/reset-password` → with verified OTP token

**Example**

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "pm@example.com", "password": "P@ssw0rd!" }
```

**Response**

```json
{ "token": "<JWT>", "expiresIn": 3600, "roles": ["ProjectManager"] }
```

### Projects

* `GET /api/projects?status=InProgress&page=1&pageSize=20&search=crm`
* `GET /api/projects/{id}`
* `POST /api/projects` (Admin/PM)
* `PUT /api/projects/{id}` (Admin/PM)
* `DELETE /api/projects/{id}` (Admin)

**Project DTO**

```json
{
  "title": "Website Revamp",
  "description": "UI overhaul",
  "status": "InProgress",
  "startDate": "2025-07-01",
  "dueDate": "2025-09-15",
  "ownerId": "<guid>"
}
```

### Tasks

* `GET /api/projects/{projectId}/tasks?status=Completed&assignedTo=<userId>&q=auth`
* `GET /api/tasks/{id}`
* `POST /api/projects/{projectId}/tasks`
* `PUT /api/tasks/{id}`
* `DELETE /api/tasks/{id}`
* **Event Hook**: When a task becomes `Completed`, send notification to PM (WhatsApp + Email)

### Notifications

* `GET /api/notifications?userId=<id>`
* `POST /api/notifications/test` (dev only)

### Common Query Parameters

* `page`, `pageSize`, `sortBy`, `sortDir`, `status`, `assignedTo`, `q` (search term)

**Pagination Envelope**

```json
{
  "items": [ /* ... */ ],
  "page": 1,
  "pageSize": 20,
  "total": 145
}
```

---

## Authentication & Security

### Identity + JWT

* Passwords hashed by Identity
* Password policy & account lockout recommended
* On login: issue **JWT** with claims (sub, roles, exp)
* Store JWT in **memory** (Angular service) and attach via **Authorization: Bearer** header using an **HTTP interceptor**

**JWT Lifecycle**

* `expiresIn`: e.g., 60 minutes
* Optional refresh token flow (future roadmap)

### OTP (Email & WhatsApp)

* OTP length: 6 digits (configurable)
* Expiry: e.g., 5–10 minutes (configurable)
* Rate limiting: one OTP per channel per X minutes; lock after N failed attempts
* Use Twilio WhatsApp API and SMTP (or service) for Email

**OTP Flow**

1. User requests password change → `send-otp-for-password-change`
2. Backend generates OTP, stores hashed OTP + expiry, sends via Email/WhatsApp
3. User submits `verify-otp` with code
4. On success, backend issues short‑lived token to allow `reset-password`

**Sample C# (pseudo)**

```csharp
var code = OtpGenerator.Generate(6);
_store.SaveHashed(user.Id, Hash(code), expiry: TimeSpan.FromMinutes(10));
await _whatsApp.SendAsync(user.Phone, $"Your TeamTrack code: {code}");
await _email.SendAsync(user.Email, "TeamTrack OTP", $"Code: {code}");
```

### RBAC

* Roles: `Admin`, `ProjectManager`, `Member`
* Endpoints protected with `[Authorize(Roles = "ProjectManager,Admin")]` etc.

### Additional Security Notes

* Enforce HTTPS, HSTS
* CORS allowlist
* Input validation on server (FluentValidation or DataAnnotations)
* Audit logs for sensitive actions (future)

---

## Frontend (Angular) Overview

### Structure (suggested)

```
frontend/
  src/app/
    core/
      interceptors/
        auth.interceptor.ts
        loader.interceptor.ts
      services/
        api.service.ts
        auth.service.ts
        alert.service.ts
        otp.service.ts
      guards/
        auth.guard.ts
    features/
      auth/
      projects/
      tasks/
      dashboard/
    shared/
      components/
      pipes/
      directives/
```

### JWT Interceptor (attach token)

```ts
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService) {}
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = this.auth.token();
    const cloned = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
    return next.handle(cloned);
  }
}
```

### Loader Interceptor (spinner)

```ts
@Injectable()
export class LoaderInterceptor implements HttpInterceptor {
  private requests = 0;
  constructor(private loader: LoaderService) {}
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    this.requests++; this.loader.show();
    return next.handle(req).pipe(finalize(() => { if (--this.requests === 0) this.loader.hide(); }));
  }
}
```

### Alert Service (toast‑style)

```ts
@Injectable({ providedIn: 'root' })
export class AlertService {
  success(msg: string) { /* show success */ }
  error(msg: string)   { /* show error   */ }
  info(msg: string)    { /* show info    */ }
  warn(msg: string)    { /* show warn    */ }
}
```

---

## Installation & Setup

### Prerequisites

* Node.js (v18+)
* .NET 8 SDK
* SQL Server
* Angular CLI

### Clone

```bash
git clone https://github.com/USERNAME/TeamTrack.git
cd TeamTrack
```

### Backend

```bash
cd backend
cp appsettings.Development.example.json appsettings.Development.json
# update connection string, Twilio, SMTP

dotnet restore
# create DB
dotnet ef database update
# run
dotnet run
```

### Frontend

```bash
cd ../frontend
cp src/environments/environment.example.ts src/environments/environment.ts
# set API base URL
npm install
ng serve -o
```

---

## Configuration

### Backend (`appsettings.*.json`)

```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=TeamTrack;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_LONG_RANDOM_SECRET",
    "Issuer": "TeamTrack",
    "Audience": "TeamTrackClient",
    "ExpiresMinutes": 60
  },
  "Otp": {
    "Length": 6,
    "ExpiryMinutes": 10,
    "SendIntervalSeconds": 60,
    "MaxAttempts": 5
  },
  "Twilio": {
    "AccountSid": "...",
    "AuthToken": "...",
    "WhatsAppFrom": "whatsapp:+14155238886"
  },
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "User": "noreply@example.com",
    "Password": "...",
    "From": "noreply@teamtrack.local"
  }
}
```

### Frontend (`environment.ts`)

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:5001/api'
};
```

---

## Database & EF Core

### DbContext (outline)

```csharp
public class AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Project>()
          .HasMany(p => p.Tasks)
          .WithOne(t => t.Project)
          .HasForeignKey(t => t.ProjectId);

        b.Entity<TaskItem>()
          .HasOne(t => t.AssignedUser)
          .WithMany()
          .HasForeignKey(t => t.AssignedUserId)
          .OnDelete(DeleteBehavior.SetNull);
    }
}
```

### Repositories (pattern)

```csharp
public interface IProjectRepository { Task<Project?> GetAsync(Guid id); /* ... */ }
public class ProjectRepository : IProjectRepository { /* EF Core implementation */ }
```

### Migrations

```bash
# add a migration
dotnet ef migrations add Init
# apply to DB
dotnet ef database update
```

### Seed (optional)

* Admin user with `Admin` role
* Sample project & tasks for quick demo

---

## Intelligent Search

### Goal

* Provide live results as the user types
* Avoid unnecessary API calls
* Be resilient to minor typos/spacing

### Angular Implementation

```ts
searchControl.valueChanges.pipe(
  map(v => (v || '').trim().toLowerCase()),
  debounceTime(300),
  distinctUntilChanged(),
  switchMap(term => this.api.searchProjects({ q: term }))
).subscribe(results => this.projects = results.items);
```

**Notes**

* Server supports `q` parameter (applies `LIKE` across title/description)
* Client trims + lowercases to reduce noise
* Optional: highlight matches in UI

---

## Alerts & Loading Spinner

* **AlertService** provides uniform UX for success/error/info
* **LoaderInterceptor** toggles a global spinner on HTTP in‑flight
* Display non‑blocking toasts for quick feedback (e.g., task saved)

---

## Validation & Error Handling

### Backend

* DataAnnotations or FluentValidation on DTOs
* Consistent error envelope:

```json
{ "error": { "code": "VALIDATION_ERROR", "message": "Title required", "details": { "title": ["Required"] } } }
```

* Global exception handler → 500 with correlation id

### Frontend

* Form validation with Reactive Forms
* Map server errors to field messages
* Generic fallback toast for unexpected errors

---

## Testing

### Backend

* Unit tests for services (business rules)
* Integration tests for controllers (WebApplicationFactory)

### Frontend

* Unit tests for services & pure components (Jest/Karma)
* E2E (Cypress) for critical flows:
  - Login & authentication with JWT + OTP
  - Create / update / delete project
  - Create / assign / complete tasks
  - Notifications triggered on task completion
  - Intelligent search & filtering in project list

---

## Code Quality

* Current clean code adherence: ~40% (room for refactoring & conventions)  
* Applied practices:
  - Layered architecture in backend
  - Separation of concerns (Services vs. Controllers vs. Repositories)
  - Angular feature modules & shared components
* Pending improvements:
  - Consistent naming conventions
  - More unit test coverage
  - Linting & static code analysis (SonarLint / ESLint)
  - Better DTO vs. Entity separation

---

## Roadmap

- [ ] Improve clean code quality (target 70%+)
- [ ] Add refresh token flow for JWT
- [ ] Implement role management UI
- [ ] Reporting & analytics dashboards
- [ ] Multi-language support (i18n in Angular)
- [ ] Integration with Slack / MS Teams for notifications
- [ ] Dockerization for local & production deployments
- [ ] CI/CD pipeline with GitHub Actions

---

## Contributing

Contributions are welcome!  
Please fork the repo, create a feature branch, and submit a Pull Request.

Guidelines:
- Write clear commit messages
- Ensure tests pass before PR
- Follow the coding standards outlined above
