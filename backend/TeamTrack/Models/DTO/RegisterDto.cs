using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TeamTrack.Models.Enum;

namespace TeamTrack.Models.DTO
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is Required")]
        public string name { get; set; } = string.Empty;
        [EmailAddress,Required(ErrorMessage ="Email is Required")]
        public string email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is Required")]
        public string password { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole userRole { get; set; } = UserRole.TeamMember;
        public DateTime CreatedDate { get; set; }

        // Check the OTP 
        public int otp { get; set; }
        public bool isActive { get; set; } = false;
        public DateTime? otpExpiration { get; set; }
    }
}