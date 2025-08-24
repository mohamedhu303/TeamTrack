using TeamTrack.Models.Enum;

namespace TeamTrack.Models.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string name { get; set; }
        public UserRole role { get; set; }
    }
}
