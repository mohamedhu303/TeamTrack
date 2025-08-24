using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using TeamTrack.Models.Enum;

namespace TeamTrack.Models
{
    public class Project
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        
        
        [Required, MinLength(3), MaxLength(1000, ErrorMessage = "Your Name should be between 3-1000 characters")]
        public required string name { get; set; }


        [MinLength(5), MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? description { get; set; }
        
        
        public ProjectStatus status { get; set; } = ProjectStatus.Completed;
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime createDate { get; set; } = DateTime.UtcNow;



        // Relations
        // 1:M | User => Projects
        [Required]
        public required string projectManagerId { get; set; }
        [ForeignKey("projectManagerId")]
        public ApplicationUser? projectManager { get; set; }

        // 1:M | Projects => Tasks
        public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();

    }
}
