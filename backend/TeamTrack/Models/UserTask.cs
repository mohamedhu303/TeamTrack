using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamTrack.Models
{
    public class UserTask
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        
        
        [Required, MaxLength(1000, ErrorMessage = "Your Title should be between 3-1000 characters")]
        public required string title { get; set; }
        
        
        [MinLength(5), MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? description { get; set; }


        [Range(0, 100, ErrorMessage = "Task %Complete must be between 0 and 100")]
        public int percentComplete { get; set; } = 0;
        public Enum.TaskStatus status { get; set; } = Enum.TaskStatus.Suspended;


        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime startDate { get; set; }


        [Required(ErrorMessage = "Finish Date is required.")]
        //[CustomValidation(typeof(UserTask), nameof(DataValidatation.DataValidate))]
        public DateTime finishDate { get; set; }
        public DateTime createdDate { get; set; } = DateTime.UtcNow;



        //Relations
        // 1:M | Proejct => Tasks
        [Required]
        public int projectId { get; set; }
        [ForeignKey("projectId")]
        public Project? project { get; set; }

        // 1:M | User => Tasks
        public string? AssignedUserId { get; set; }
        [ForeignKey("AssignedUserId")]
        public ApplicationUser? AssignedUser { get; set; }
    }
}
