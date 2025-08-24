using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamTrack.Models;
using TeamTrack.Models.DTO;
using TeamTrack.Models.Enum;

namespace TeamTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProjectController : ControllerBase
    {
        private readonly TeamTrackDbContext _context;
        public ProjectController(TeamTrackDbContext context)
        {
            _context = context;
        }

        // ====================
        // = Get All Projects =
        // ====================
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _context.project.OrderByDescending(p => p.createDate).ToListAsync();
            if (!projects.Any())
                return Ok(new { message = "There is not any Projects" });
            return Ok(projects);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("project-managers")]
        public async Task<IActionResult> GetProjectManagers()
        {
            var managers = await _context.Users
                .Where(u => u.userRole == UserRole.ProjectManager)
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

            return Ok(managers);
        }

        // GET: api/Project/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _context.project
                .Select(p => new { p.id, p.name })
                .ToListAsync();

            return Ok(projects);
        }

        // =====================
        // = Get Project By ID =
        // =====================
        [Authorize]
        [HttpGet("{id}/getById")]
        public async Task<IActionResult> Get(int id)
        {
            var project = await _context.project.FindAsync(id);
            if (project == null)
            {
                return NotFound("Project not Found");
            }
            return Ok(project);
        }


        // ======================
        // = Create New Project =
        // ======================
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = new Project
            {
                name = dto.name,
                description = dto.Description,
                projectManagerId = dto.ProjectManagerId 
            };

            _context.project.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project Created Successfully", project });
        }



        // ======================
        // === Delete Project ===
        // ======================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.project.FindAsync(id);
            if (project == null)
                return NotFound("Project Not Found");

            _context.project.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project Deleted" });
        }


        // =======================================
        // ============= UPDATEING ===============
        // ====== Validate Data a Patch All ======
        // =======================================
        [HttpPatch("update/{id}")]
        public async Task<IActionResult> PatchProject(int id, [FromBody] UpdateProjectDto updateDto)
        {
            var project = await _context.project.FindAsync(id);
            if (project == null)
                return NotFound("Project Not Found");

            // Update name
            if (!string.IsNullOrWhiteSpace(updateDto.name))
                project.name = updateDto.name;

            // Update description
            if (!string.IsNullOrWhiteSpace(updateDto.description))
                project.description = updateDto.description;

            // Update status (int -> enum)
            if (updateDto.status.HasValue)
            {
                if (Enum.IsDefined(typeof(ProjectStatus), updateDto.status.Value))
                    project.status = (ProjectStatus)updateDto.status.Value;
                else
                    return BadRequest("Invalid status value");
            }

            // Update project manager
            if (!string.IsNullOrWhiteSpace(updateDto.ProjectManagerId))
                project.projectManagerId = updateDto.ProjectManagerId;

            await _context.SaveChangesAsync();

            var response = new
            {
                message = "Project updated successfully",
                project = new
                {
                    project.id,
                    project.name,
                    project.description,
                    status = project.status.ToString(),
                    project.createDate,
                    project.projectManagerId,
                    project.projectManager
                }
            };

            return Ok(response);
        }



        // =======================================
        // ==== Get Project By ID with Details ===
        // =======================================
        [Authorize(Roles = "Admin, ProjectManager,TeamMember")]
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var project = await _context.project

                .Include(p => p.projectManager)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedUser)
                .Select(p => new
                {
                    p.id,
                    p.name,
                    p.description,
                    p.status,
                    projectManager = new
                    {
                        p.projectManager.Id,
                        p.projectManager.Name,
                        p.projectManager.Email,
                        p.projectManager.PhoneNumber,
                        p.projectManager.createdDate
                    },

                    tasks = p.Tasks.Select(t => new {
                        t.id,
                        t.title,
                        t.description,
                        t.createdDate,
                        t.percentComplete,
                        AssignedUser = new
                        {
                            t.AssignedUser.Id,
                            t.AssignedUser.Name,
                            t.AssignedUser.Email,
                            t.AssignedUser.PhoneNumber,
                            t.AssignedUser.createdDate
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync(p => p.id == id);
            if (project == null)
                return NotFound("Project not found");

            return Ok(project);
        }

        // =======================================
        // ======= Update Patch Project ==========
        // =======================================
        [HttpPatch("dynamic/{id}")]
        public async Task<IActionResult> UpdatePatchProject(int id, [FromBody] JsonElement updates)
        {
            var project = await _context.project.FindAsync(id);
            if (project == null)
                return NotFound("Project Not Found");

            foreach (var prop in updates.EnumerateObject())
            {
                switch (prop.Name.ToLower())
                {
                    case "name":
                        project.name = prop.Value.GetString() ?? project.name;
                        break;

                    case "description":
                        project.description = prop.Value.GetString() ?? project.description;
                        break;

                    case "status":
                        if (prop.Value.TryGetInt32(out int statusValue))
                        {
                            if (Enum.IsDefined(typeof(ProjectStatus), statusValue))
                                project.status = (ProjectStatus)statusValue;
                            else
                                return BadRequest("Invalid status value");
                        }
                        break;

                    case "projectmanagerid":
                        project.projectManagerId = prop.Value.GetString() ?? project.projectManagerId;
                        break;
                }
            }

            _context.project.Update(project);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Project updated successfully",
                project = new
                {
                    project.id,
                    project.name,
                    project.description,
                    status = project.status,
                    project.createDate,
                    project.projectManagerId,
                    project.projectManager
                }
            });
        }


        // =======================================
        // =========== Update Status =============
        // =======================================
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPatch("status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] JsonElement updates)
        {
            var project = await _context.project.FindAsync(id);
            if (project == null)
                return NotFound("Project Not Found");
            
            if (!updates.TryGetProperty("status", out var statusValue))
                return BadRequest("Status id Required");

            if (!Enum.TryParse<ProjectStatus>(statusValue.GetRawText(), out var newStatus))
                return BadRequest("Invalid Staus Value");

            project.status = newStatus;
            _context.project.Update(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully", project });
        }

        // =======================================
        // ====== Get Projects By Manager ========
        // =======================================
        [Authorize(Roles = "Admin, ProjectManager,TeamMember")]
        [HttpGet("ByManager/{managerId}")]
        public async Task<IActionResult> GetProjectsByManager(string managerId)
        {
            var projects = await _context.project
                .Where(p => p.projectManagerId == managerId)
                .Include(p => p.Tasks)
                .Select(p => new
                {
                    p.id,
                    p.name,
                    p.status,
                    taskCount = p.Tasks.Count
                })
                .ToListAsync();

            if (!projects.Any())
                return NotFound(new { message = "No projects found for this manager" });

            var managerExists = await _context.Users.AnyAsync(u => u.Id == managerId);
            if (!managerExists)
                return NotFound(new { message = "Project Manager not found" });

            return Ok(projects);
        }

        [Authorize(Roles = "Admin, ProjectManager, TeamMember")]
        [HttpGet("with-tasks")]
        public async Task<IActionResult> GetProjectsWithTasks()
        {
            var projects = await _context.project
                .Include(p => p.projectManager)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedUser)
                .ToListAsync();

            if (!projects.Any())
                return Ok(new { message = "No projects found" });

            var result = projects.Select(p => new
            {
                p.id,
                p.name,
                p.description,
                p.status,
                ProjectManager = p.projectManager == null ? null : new
                {
                    p.projectManager.Id,
                    p.projectManager.Name, 
                    p.projectManager.Email
                },
                tasks = p.Tasks.Select(t => new
                {
                    t.id,
                    t.title,
                    t.description,
                    t.percentComplete,
                    Status = GetStatus(t.percentComplete), 
                    t.startDate,
                    t.finishDate,
                    AssignedUser = t.AssignedUser == null ? null : new
                    {
                        t.AssignedUser.Id,
                        t.AssignedUser.Name, 
                        t.AssignedUser.Email
                    }
                })
            });

            return Ok(result);
        }

        /// <summary>
        /// Helper method عشان تحسب الـ Status من percentComplete
        /// </summary>
        private static string GetStatus(int percentComplete)
        {
            if (percentComplete == 0) return "Not Started";
            if (percentComplete > 0 && percentComplete < 100) return "In Progress";
            if (percentComplete == 100) return "Completed";
            return "Unknown";
        }

    }
}

