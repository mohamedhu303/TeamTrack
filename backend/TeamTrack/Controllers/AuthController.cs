using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using TeamTrack.Models;
using TeamTrack.Models.DTO;
using TeamTrack.Models.Enum;

namespace TeamTrack.Controllers
{
    /// <summary>
    /// Controller responsible for authentication and account management.
    /// Includes registration, login, logout, OTP verification, password reset, and profile access.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly TeamTrackDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;


        public AuthController(IJwtTokenService jwtTokenService, TeamTrackDbContext context, IMemoryCache memoryCache, IEmailSender emailSender, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _memoryCache = memoryCache;
            _emailSender = emailSender;
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Handles user registeration
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var normalizedEmail = model.email.Trim().ToLower();
            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);

            if (existingUser != null)
                return BadRequest("Email is already registered.");

            var otp = new Random().Next(100000, 999999).ToString();

            var user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                Name = model.name,
                userRole = model.userRole,
                createdDate = DateTime.UtcNow,
                otpCode = otp,
                otpExpiration = DateTime.UtcNow.AddMinutes(20),
                isActive = false
            };

            var result = await _userManager.CreateAsync(user, model.password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _emailSender.SendEmailAsync(model.email, "Your OTP Code", $"Your OTP is: {otp} . It will expire in 20 minutes.");

            return Ok(new { message = "OTP sent to email. Please confirm to complete registration." });

        }



        /// <summary>
        /// Send an Confirmation Email and token after registeration
        /// </summary>
        [AllowAnonymous]
        [HttpPost("confirm-otp")]
        public async Task<IActionResult> ConfirmOtp(ConfirmOtpDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLower();

            var user = await _userManager.FindByEmailAsync(normalizedEmail);
            if (user == null)
                return BadRequest("User not found.");

            if (user.isActive)
                return BadRequest("User already confirmed.");

            if (user.otpCode != dto.Otp || user.otpExpiration < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            user.isActive = true;
            user.otpCode = null;
            user.otpExpiration = null;

            await _userManager.UpdateAsync(user);

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new { message = "Account confirmed successfully.", token });
        }




        /// <summary>
        /// Authenticates user and without sends OTP via email.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto model)
        {
            Console.WriteLine($"📩 Email from request: {model.email}");
            Console.WriteLine($"🔑 Password from request: {model.password}");

            var user = await _userManager.FindByEmailAsync(model.email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.password))
                return Unauthorized(new { message = "Invalid credentials." });

            if (!user.isActive)
                return Unauthorized(new { message = "Account is not confirmed. Please verify OTP." });

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    role = user.userRole
                }
            });
        }




        /// <summary>
        /// Gets profile for authenticated user.
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            string roleName = Enum.GetName(typeof(UserRole), user.userRole);

            return Ok(new
            {
                name = user.Name ?? user.Name,
                role = roleName
            });
        }





        /// <summary>
        /// Logs out the user and revokes their token.
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is missing or invalid");

            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var expiration = jwtToken.ValidTo;

            var revokedToken = new RevokedToken
            {
                Token = token,
                RevokeAt = DateTime.UtcNow,
                ExpirationDate = expiration.ToUniversalTime()
            };

            _context.RevokedTokens.Add(revokedToken);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logout successfully and token revoked" });
        }

        /// <summary>
        /// if forget password send code to Email and Reset it.
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request");

            var user = await _userManager.FindByEmailAsync(model.email);
            if (user == null)
                return Ok(new { message = "The email not found" });


            var otp = new Random().Next(100000, 999999).ToString();
            user.otpCode = otp;
            user.otpExpiration = DateTime.UtcNow.AddMinutes(15);
            user.isActive = false;

            await _userManager.UpdateAsync(user);
            await _emailSender.SendEmailAsync(user.Email, "Reset Password OTP", $"Your OTP is: {otp}. It will expire in 15 minutes.");

            return Ok(new { message = "OTP Sent to You, Check Your Email" });

        }


        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request");

            if (user.otpCode != model.Otp || user.otpExpiration < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { message = "Reset failed", errors = result.Errors.Select(e => e.Description) });

            user.otpCode = null;
            user.otpExpiration = null;
            user.isActive = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password has been reset successfully." });
        }


    }
}