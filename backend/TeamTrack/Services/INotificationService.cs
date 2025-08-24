using TeamTrack.Models;

namespace TeamTrack.Services
{
    public interface INotificationService
    {
        Task NotifyPM(UserTask task);
    }
}
