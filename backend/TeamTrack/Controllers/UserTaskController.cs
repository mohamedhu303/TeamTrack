using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamTrack.Models;
using TeamTrack.Models.DTO;
using TeamTrack.Models.Enum;
using TeamTrack.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TeamTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTaskController : ControllerBase
    {
        private readonly TeamTrackDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly NotificationService _notificationService;
        private readonly INotificationService _inotificationService;

        public UserTaskController(TeamTrackDbContext context, INotificationService inotificationService, NotificationService notificationService, IEmailSender emailSender, IConfiguration configuration)
        {
            _context = context;
            _notificationService = notificationService;
            _inotificationService = inotificationService;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        // =======================================
        // ============ Get All Tasks ============
        // ========================================
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _context.userTask
                .Include(t => t.AssignedUser)
                .Include(t => t.project)
                .ToListAsync();

            if (!tasks.Any())
                return Ok(new { message = "No tasks found" });

            var result = tasks.Select(t => new
            {
                t.id,
                t.title,
                t.description,
                t.percentComplete,
                Status = GetStatus(t.percentComplete),
                t.createdDate,
                Project = new { t.project.id, t.project.name },
                AssignedUser = t.AssignedUser == null ? null : new
                {
                    t.AssignedUser.Id,
                    t.AssignedUser.Name,
                    t.AssignedUser.Email
                }
            });

            return Ok(result);
        }

        // ========================================
        // ============== Get Status ==============
        // ========================================
        public static string GetStatus(double percentComplete)
        {
            if (percentComplete == 0) return "Pending";
            if (percentComplete > 0 && percentComplete < 100) return "In Progress";
            return "Completed";
        }


        // ================================================
        // ============== Get Assigned Tasks ==============
        // ================================================
        [Authorize(Roles = "TeamMember")]
        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasksGroupedByProject()
        {
            var userId = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            // Get tasks assigned to this user, including project info
            var tasks = await _context.userTask
                .Where(t => t.AssignedUserId == userId)
                .Include(t => t.project)
                .ToListAsync();

            if (!tasks.Any())
                return Ok(new { message = "No tasks assigned to you yet" });

            // Group tasks by project
            var grouped = tasks
                .GroupBy(t => t.project)
                .Select(g => new
                {
                    ProjectName = g.Key.name,
                    Tasks = g.Select(t => new
                    {
                        t.id,
                        t.title,
                        t.description,
                        t.percentComplete
                    })
                });

            return Ok(grouped);
        }


        // ========================================
        // ============ Get Task By ID ============
        // ========================================
        [Authorize(Roles = "Admin, ProjectManager,TeamMember")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _context.userTask
                .Include(t => t.AssignedUser)
                .Include(t => t.project)
                .Select(t => new
                {
                    t.id,
                    t.title,
                    t.description,
                    t.percentComplete,
                    Status = GetStatus(t.percentComplete),
                    t.createdDate,
                    Project = new { t.project.id, t.project.name },
                    AssignedUser = t.AssignedUser == null ? null : new
                    {
                        t.AssignedUser.Id,
                        t.AssignedUser.Name,
                        t.AssignedUser.Email
                    }
                })
                .FirstOrDefaultAsync(t => t.id == id);

            if (task == null)
                return NotFound("Task not found");

            return Ok(task);
        }

        // =================================================
        // ============ Get Tasks by Project ID ============
        // =================================================
        [Authorize(Roles = "Admin, ProjectManager,TeamMember")]
        [HttpGet("ByProject/{projectId}")]
        public async Task<IActionResult> GetByProject(int projectId)
        {
            var tasks = await _context.userTask
                .Where(t => t.projectId == projectId)
                .Include(t => t.AssignedUser)
                .Select(t => new
                {
                    t.id,
                    t.title,
                    t.percentComplete,
                    Status = GetStatus(t.percentComplete),
                    AssignedUser = t.AssignedUser == null ? null : new
                    {
                        t.AssignedUser.Id,
                        t.AssignedUser.Name,
                        t.AssignedUser.Email
                    }
                }).ToListAsync();

            return Ok(tasks);
        }

        // =====================================
        // ============ Create Task ============
        // =====================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrEmpty(dto.assignedUserId))
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == dto.assignedUserId);
                if (!userExists)
                    return BadRequest($"Assigned user with ID '{dto.assignedUserId}' does not exist.");
            }

            var task = new UserTask
            {
                title = dto.title,
                description = dto.description,
                percentComplete = dto.percentComplete,
                startDate = dto.startDate,
                finishDate = dto.finishDate,
                projectId = dto.projectId, 
                AssignedUserId = string.IsNullOrEmpty(dto.assignedUserId) ? null : dto.assignedUserId
            };

            _context.userTask.Add(task);
            await _context.SaveChangesAsync();

            UserDto? assignedUser = null;
            if (!string.IsNullOrEmpty(task.AssignedUserId))
            {
                assignedUser = await _context.Users
                    .Where(u => u.Id == task.AssignedUserId)
                    .Select(u => new UserDto
                    {
                        name = u.Name,
                        role = u.userRole
                    })
                    .FirstOrDefaultAsync();
            }

            var taskDto = new CreateTaskDto
            {
                id = task.id,
                title = task.title,
                description = task.description,
                percentComplete = task.percentComplete,
                startDate = task.startDate,
                finishDate = task.finishDate,
                projectId = task.projectId,
                assignedUserId = task.AssignedUserId,
                assignedUser = assignedUser
            };

            return Ok(new { message = "Task created", task = taskDto });
        }





        // ====================================
        // ============ Patch Task ============
        // ====================================
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPatch("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonElement updates)
        {
            var task = await _context.userTask.FindAsync(id);
            if (task == null)
                return NotFound("Task not found");

            foreach (var prop in updates.EnumerateObject())
            {
                switch (prop.Name.ToLower())
                {
                    case "title":
                        task.title = prop.Value.GetString() ?? task.title;
                        break;
                    case "description":
                        task.description = prop.Value.GetString() ?? task.description;
                        break;
                    case "percentcomplete":
                        if (int.TryParse(prop.Value.GetRawText(), out var pc))
                            task.percentComplete = pc;
                        break;
                    case "assigneduserid":
                        task.AssignedUserId = prop.Value.GetString() ?? task.AssignedUserId;
                        break;
                }
            }

            _context.userTask.Update(task);
            await _context.SaveChangesAsync();

            var updatedTask = await _context.userTask
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.id == id);

            var response = new
            {
                updatedTask.id,
                updatedTask.title,
                updatedTask.description,
                updatedTask.percentComplete,
                updatedTask.status,
                updatedTask.startDate,
                updatedTask.finishDate,
                updatedTask.projectId,
                AssignedUser = updatedTask.AssignedUser == null ? null : new
                {
                    updatedTask.AssignedUser.Id,
                    updatedTask.AssignedUser.Name,
                    updatedTask.AssignedUser.Email
                }
            };

            return Ok(new { message = "Task updated", task = response });
        }

        // =====================================
        // ============ Delete Task ============
        // =====================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.userTask.FindAsync(id);
            if (task == null)
                return NotFound("Task not found");

            _context.userTask.Remove(task);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Task deleted" });
        }

        // =====================================
        // ========= Task Filtieration =========
        // =====================================
        [Authorize(Roles = "Admin, ProjectManager,TeamMember")]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterTasks(
        [FromQuery] string? keyword,
        [FromQuery] ProjectStatus? status,
        [FromQuery] int? projectId,
        [FromQuery] int? minPercent,
        [FromQuery] int? maxPercent,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = _context.userTask
                .Include(t => t.AssignedUser)
                .Include(t => t.project)
                .AsQueryable();

            // Filtertion with title
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t =>
                    t.title.Contains(keyword) || t.description.Contains(keyword));
            }

            // Filteration with Status
            if (status.HasValue)
            {
                switch (status)
                {
                    case ProjectStatus.Suspended:
                        query = query.Where(t => t.percentComplete == 0);
                        break;
                    case ProjectStatus.InProgress:
                        query = query.Where(t => t.percentComplete > 0 && t.percentComplete < 100);
                        break;
                    case ProjectStatus.Completed:
                        query = query.Where(t => t.percentComplete == 100);
                        break;
                }
            }

            // filteration with Project ID
            if (projectId.HasValue)
            {
                query = query.Where(t => t.projectId == projectId);
            }

            // filteration with Status
            if (minPercent.HasValue)
                query = query.Where(t => t.percentComplete >= minPercent.Value);

            if (maxPercent.HasValue)
                query = query.Where(t => t.percentComplete <= maxPercent.Value);

            // Pagination and Descending
            var totalCount = await query.CountAsync();
            var tasks = await query
                .OrderByDescending(t => t.createdDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.id,
                    t.title,
                    t.description,
                    t.percentComplete,
                    t.createdDate,
                    AssignedUser = t.AssignedUser == null ? null : new
                    {
                        t.AssignedUser.Id,
                        t.AssignedUser.UserName,
                        t.AssignedUser.Email
                    },
                    Project = new
                    {
                        t.project.id,
                        t.project.name
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                currentPage = page,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                tasks
            });
        }

        // =============================================================
        // ========= Notify PM via Email when task is complete =========
        // =============================================================
        [HttpPost("NotifyPM")]
        public async Task<IActionResult> NotifyPM([FromBody] NotifyPMDto request)
        {
            var taskId = request.taskId;
            var fullTask = await _context.userTask
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.id == taskId);

            if (fullTask == null)
                return NotFound("Task not found.");

            var project = await _context.project
                .Include(p => p.projectManager)
                .FirstOrDefaultAsync(p => p.id == fullTask.projectId);

            if (project == null)
                return NotFound("Project not found.");

            var pmEmail = project.projectManager?.Email;
            var pmPhone = project.projectManager?.PhoneNumber;
            var userPhone = fullTask.AssignedUser?.PhoneNumber;
            var assignedUserName = fullTask.AssignedUser?.Name ?? "someone";

            if (!string.IsNullOrEmpty(pmEmail))
            {
                await _emailSender.SendEmailAsync(pmEmail,
                    "Task Completed ✅",
                    $"Task '{fullTask.title}' was marked as completed by {assignedUserName}.");
            }

            if (!string.IsNullOrEmpty(pmPhone))
            {
                await SendWhatsApp(new WhatsAppDto
                {
                    PmPhone = pmPhone,
                    UserPhone = userPhone,
                    Message = $"🎉 Task '{fullTask.title}' has been completed by {assignedUserName}."
                });
            }

            return Ok("PM Notified via email and WhatsApp");
        }


        [HttpPost("SendWhatsApp")]
        public async Task<IActionResult> SendWhatsApp([FromBody] WhatsAppDto request)
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromRaw = _configuration["Twilio:WhatsAppFrom"];

            var fromNumber = fromRaw.Replace("whatsapp:", "").Trim();
            var toNumber = request.PmPhone.Replace("whatsapp:", "").Trim();

            if (fromNumber == toNumber || request.UserPhone == request.PmPhone)
            {
                Console.WriteLine("❌ Can't send WhatsApp message to the same number.");
                return BadRequest("Sender and receiver phone numbers cannot be the same.");
            }

            TwilioClient.Init(accountSid, authToken);

            var msg = await MessageResource.CreateAsync(
                body: request.Message + $"\n📱 Sent by: {request.UserPhone}",
                from: new Twilio.Types.PhoneNumber($"whatsapp:{fromNumber}"),
                to: new Twilio.Types.PhoneNumber($"whatsapp:{toNumber}")
            );

            return Ok("WhatsApp sent ✅");
        }

        // ======================================
        // ========= Denfind Complete Task ======
        // ======================================
        [HttpPost("complete-task/{taskId}")]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            var task = await _context.userTask
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.id == taskId);

            if (task == null)
                return NotFound("Task not found.");

            task.percentComplete = 100;
            task.status = Models.Enum.TaskStatus.Completed;
            task.finishDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.NotifyPM(task); 

            return Ok("Task marked as completed and PM notified.");
        }
    }
}
