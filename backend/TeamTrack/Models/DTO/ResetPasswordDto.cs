using System.ComponentModel.DataAnnotations;

public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Otp { get; set; }

    [Required]
    public string NewPassword { get; set; }
}
