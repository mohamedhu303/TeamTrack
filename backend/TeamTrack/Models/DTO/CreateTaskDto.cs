using System.ComponentModel.DataAnnotations;

namespace TeamTrack.Models.DTO
{
    public class CreateTaskDto
    {
        public int id { get; set; }
        public string title { get; set; }
        public string? description { get; set; }
        public int percentComplete { get; set; }
        public DateTime startDate { get; set; }
        public DateTime finishDate { get; set; }
        public int projectId { get; set; }

        public string? assignedUserId { get; set; }

        public UserDto? assignedUser { get; set; }
    }
}
