namespace TeamTrack.Models.DTO
{
    public class CreateProjectDto
    {
        public string name { get; set; }
        public string? Description { get; set; }
        public string ProjectManagerId { get; set; }
    }
}
