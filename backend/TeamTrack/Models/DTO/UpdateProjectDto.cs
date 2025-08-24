using TeamTrack.Models.Enum;

namespace TeamTrack.Models.DTO
{
    public class UpdateProjectDto
    {
        public string? name {  get; set; }
        public string? description { get; set; }
        public string? ProjectManagerId {  get; set; }
        public int? status { get; set; }
    }
}
