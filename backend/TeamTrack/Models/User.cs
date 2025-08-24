using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using TeamTrack.Models.Enum;
using Twilio.Types;

namespace TeamTrack.Models
{
    [Index(nameof(email), IsUnique = true)]
    public class User
    {
        public string _email = string.Empty;
        public string _password = string.Empty;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }


        [Required, MinLength(3), MaxLength(1000, ErrorMessage = "Your Name should be between 3-1000 characters")]
        public required string name { get; set; }


        [Required(ErrorMessage = "Email is Required"), EmailAddress]
        public string email
        {
            get => _email;
            set
            {
                if (!DataValidatation.DataValidate.ValidateEmail(value))
                {
                    throw new ArgumentException("Enter a valid email address");
                }
                _email = value;
            }
        }


        [Required(ErrorMessage = "Password is Required")]
        public string password
        {
            private get => _password;
            set
            {
                if (!DataValidatation.DataValidate.ValidatePassword(value))
                {
                    throw new ArgumentException("Password must be 8–32 characters long and include uppercase, lowercase letters, and numbers");
                }
                _password = DataValidatation.DataValidate.HashPassword(value);
            }
        }


        public UserRole userRole { get; set; } = UserRole.TeamMember;


        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime createdDate { get; set; } = DateTime.UtcNow;

        public int OTPCode { get; set; }
        public int OTPExpiry { get; set; }
        public string? otpCode { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? otpExpiration { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public bool isActive { get; set; } = false;

        public PhoneNumber? phoneNumber { get; set; }    



        // Relations
        // 1:M | User => Projects
        public ICollection<Project> managedProjects { get; set; } = new List<Project>();
        // 1:M | User => Tasks
        public ICollection<UserTask> assignedTasks { get; set; } = new List<UserTask>();
    }
}
