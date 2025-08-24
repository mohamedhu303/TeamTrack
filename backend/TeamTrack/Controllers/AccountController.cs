using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamTrack.Models;
using TeamTrack.Models.DTO;

namespace TeamTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [Authorize]
        [HttpPost("send-otp-for-password-change")]
        public async Task<IActionResult> SendOtpForPasswordChange()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            user.otpCode = otpCode;
            user.otpExpiration = DateTime.UtcNow.AddMinutes(10);

            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(user.Email, "OTP for Password Change", $"Your OTP code is: {otpCode}");

            return Ok(new { message = "OTP sent." });


        }

        [AllowAnonymous]
        [HttpPost("change-password-with-otp")]
        public async Task<IActionResult> ChangePasswordWithOtp([FromBody] ChangePasswordWithOtpDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.email);
            if (user == null)
                return NotFound("User not found.");

            if (user.otpCode != model.OtpCode || user.otpExpiration < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            user.otpCode = null;
            user.otpExpiration = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password changed successfully." });
        }

        [Authorize]
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!result)
                return BadRequest("Invalid current password.");

            return Ok(true);
        }

        [Authorize]
        [HttpGet("profile-details")]
        public async Task<IActionResult> GetProfileDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.userRole
            });
        }

    }
}
