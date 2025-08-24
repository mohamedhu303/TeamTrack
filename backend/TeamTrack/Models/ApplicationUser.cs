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

namespace TeamTrack.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; }
        public UserRole userRole { get; set; } = UserRole.TeamMember;
        public DateTime createdDate { get; set; } = DateTime.UtcNow;
        public string? otpCode { get; set; }
        public DateTime? otpExpiration { get; set; }
        public bool isActive { get; set; } = false;
        public virtual ICollection<Project> projects { get; set; } = new List<Project>();
        public ICollection<UserTask> AssignedTasks { get; set; } = new List<UserTask>();



    }
}