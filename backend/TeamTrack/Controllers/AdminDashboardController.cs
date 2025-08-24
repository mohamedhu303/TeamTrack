using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamTrack.Models;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class AdminDashboardController : ControllerBase
{
    private readonly TeamTrackDbContext _context;

    public AdminDashboardController(TeamTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var totalUsers = await _context.user.CountAsync();
        var totalProjects = await _context.project.CountAsync();
        var totalTasks = await _context.userTask.CountAsync();

        var teamMembers = await _context.Users.CountAsync(u => u.userRole == TeamTrack.Models.Enum.UserRole.TeamMember);
        var admins = await _context.Users.CountAsync(u => u.userRole == TeamTrack.Models.Enum.UserRole.Admin);

        var completedTasks = await _context.userTask.CountAsync(t => t.percentComplete == 100);
        var inProgressTasks = await _context.userTask.CountAsync(t => t.percentComplete > 0 && t.percentComplete < 100);
        var pendingTasks = await _context.userTask.CountAsync(t => t.percentComplete == 0);

        return Ok(new
        {
            totalUsers,
            teamMembers,
            admins,
            totalProjects,
            totalTasks,
            taskStats = new
            {
                completedTasks,
                inProgressTasks,
                pendingTasks
            }
        });
    }
}
