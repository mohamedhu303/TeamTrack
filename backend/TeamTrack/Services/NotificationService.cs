using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TeamTrack.Models;
using TeamTrack.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class NotificationService : INotificationService
{
    private readonly TeamTrackDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public NotificationService(TeamTrackDbContext context, IEmailSender emailSender, IConfiguration configuration)
    {
        _context = context;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task NotifyPM(UserTask task)
    {
        var project = await _context.project
            .Include(p => p.projectManager)
            .FirstOrDefaultAsync(p => p.id == task.projectId);

        var pmEmail = project?.projectManager?.Email;
        var pmPhone = project?.projectManager?.PhoneNumber;

        if (!string.IsNullOrEmpty(pmEmail))
        {
            await _emailSender.SendEmailAsync(pmEmail,
                "Task Completed ✅",
                $"Task '{task.title}' was marked as completed by {task.AssignedUser?.Name ?? "someone"}.");
        }

        if (!string.IsNullOrEmpty(pmPhone))
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:WhatsAppFrom"];

            TwilioClient.Init(accountSid, authToken);

            await MessageResource.CreateAsync(
                body: $"🎉 Task '{task.title}' has been completed by {task.AssignedUser?.Name ?? "someone"}.",
                from: new Twilio.Types.PhoneNumber(fromNumber),
                to: new Twilio.Types.PhoneNumber($"whatsapp:{pmPhone}")
            );
        }
    }
}
