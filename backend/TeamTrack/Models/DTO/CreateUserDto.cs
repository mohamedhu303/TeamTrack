using TeamTrack.Models.Enum;

namespace TeamTrack.Models.DTO
{
    public class CreateUserDto
    {
        public string name { get; set; }
        public string email { get; set; }
        public UserRole role { get; set; } // Admin, ProjectManager, TeamMember
        public string password { get; set; }
    }
}
