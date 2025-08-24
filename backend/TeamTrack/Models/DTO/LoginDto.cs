using System.ComponentModel.DataAnnotations;

namespace TeamTrack.Models.DTO
{
    public class LoginDto
    {
        [EmailAddress, Required(ErrorMessage = "Email is Required")]
        public string email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is Required")]
        public string password { get; set; } = string.Empty;
    }
}
