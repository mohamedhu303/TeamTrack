using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TeamTrack.Models;
using TeamTrack.Models.DTO;
using TeamTrack.Models.Enum;

namespace TeamTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TeamTrackDbContext _context;
        private readonly IEmailSender _emailSender;


        public UserManagementController(UserManager<ApplicationUser> userManager, TeamTrackDbContext context,IEmailSender emailSender )
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
        }

        // GET: api/UserManagement/Search
        
        [Authorize(Roles = "Admin")]
        [HttpGet("Search")]
        public async Task<IActionResult> GetUsersWithSearch(
            string? searchTerm,
            string? role,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1, 
            int pageSize = 10)
        {

            var query = _userManager.Users
                .Include(u => u.projects)
                .Include(u => u.AssignedTasks)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)) ||
                    u.projects.Any(p =>
                        p.name.ToLower().Contains(searchTerm) ||
                        p.description.ToLower().Contains(searchTerm)
                    ) ||
                    u.AssignedTasks.Any(t =>
                        t.title.ToLower().Contains(searchTerm) ||
                        t.description.ToLower().Contains(searchTerm)
                    ) ||
                    u.userRole.ToString().ToLower().Contains(searchTerm)
                );
            }

            if (fromDate.HasValue)
                query = query.Where(u => u.createdDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.createdDate <= toDate.Value);

            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
                query = query.Where(u => u.userRole == parsedRole);

            var totalUsers = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.createdDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = u.userRole.ToString(),
                    Phone = string.IsNullOrEmpty(u.PhoneNumber) ? null : u.PhoneNumber,
                    u.createdDate,
                    Projects = u.projects.Select(p => new { p.id, p.name }),
                    Tasks = u.AssignedTasks.Select(t => new { t.id, t.title })
                })
                .ToListAsync();

            return Ok(new
            {
                totalUsers,
                page,
                users
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("project-managers")]
        public async Task<IActionResult> GetProjectManagers()
        {
            var managers = await _userManager.Users
                .Where(u => u.userRole == UserRole.ProjectManager)
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

            return Ok(managers);
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet("team-members")]
        public async Task<IActionResult> GetTeamMembers()
        {
            var teamMembers = await _userManager.Users
                .Where(u => u.userRole == UserRole.TeamMember)
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

            return Ok(teamMembers);
        }

        // POST: api/UserManagement/create-user-no-otp
        [Authorize(Roles = "Admin")]
        [HttpPost("create-user-no-otp")]
        public async Task<IActionResult> CreateUserNoOtp([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.email);
            if (existingUser != null)
                return BadRequest("Email already exists");

            var user = new ApplicationUser
            {
                UserName = dto.email,
                Name = dto.name,
                Email = dto.email,
                userRole = dto.role,
                createdDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var subject = "Welcome to TeamTrack";
            var body = $@"
                Hello {dto.name},<br><br>
                Your account has been created successfully.<br>
                <b>Email:</b> {dto.email}<br>
                <b>Password:</b> {dto.password}<br><br>
                Please change your password after logging in for the first time.<br><br>
                Regards,<br>
                TeamTrack Team
            ";

            await _emailSender.SendEmailAsync(dto.email, subject, body);

            return Ok(new { message = "User created successfully and email sent", user });
        }



        // GET: api/UserManagement/user
        [Authorize(Roles = "Admin")]
        [HttpGet("user")]
        public async Task<IActionResult> GetUsers(
            string? name,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            const int pageSize = 10;

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(u => u.Name.Contains(name));

            if (fromDate.HasValue)
                query = query.Where(u => u.createdDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.createdDate <= toDate.Value);

            var totalUsers = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.createdDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = u.userRole.ToString(),
                    Phone = string.IsNullOrEmpty(u.PhoneNumber) ? null : u.PhoneNumber,
                    u.createdDate
                })
                .ToListAsync();

            if (!users.Any())
            {
                return Ok(new { message = "There is no data to be displayed." });
            }

            return Ok(new
            {
                totalUsers,
                page,
                users
            });
        }

        // PUT: api/UserManagement/update-role/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("update-role/{id}")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser.userRole != UserRole.Admin)
                return Forbid("Only Admin can update roles");

            if (!Enum.TryParse<UserRole>(dto.NewRole, out var role))
                return BadRequest("Invalid role value");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found");

            user.userRole = role;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Role updated successfully" });
        }

        // GET: api/UserManagement/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.projects)
                .Include(u => u.AssignedTasks)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Phone = u.PhoneNumber,
                    Role = u.userRole,
                    u.createdDate,
                    Projects = u.projects.Select(p => new { p.id, p.name, p.description }),
                    Tasks = u.AssignedTasks.Select(t => new { t.id, t.title, t.description, t.percentComplete })
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        // PUT: api/UserManagement/update-phone/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("update-phone/{id}")]
        public async Task<IActionResult> UpdatePhone(string id, [FromBody] UpdatePhoneDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.NewPhone, @"^\d{11}$"))
                return BadRequest("Phone must be exactly 11 digits");

            user.PhoneNumber = dto.NewPhone;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Phone updated successfully" });
        }


        // DELETE: api/UserManagement/delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser.userRole != UserRole.Admin)
                return Forbid("Only Admin can delete users");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete user" });

            return Ok(new { message = "User deleted successfully" });
        }

        // PUT: api/UserManagement/update/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null)
                    return BadRequest(new { message = "Email already exists" });

                user.Email = dto.Email;
                user.UserName = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Phone, @"^\d{11}$"))
                    return BadRequest(new { message = "Phone must be exactly 11 digits" });

                user.PhoneNumber = dto.Phone;
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
                    return BadRequest(new { message = "Invalid role value" });

                user.userRole = parsedRole;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User updated successfully", user });
        }

    }
}

