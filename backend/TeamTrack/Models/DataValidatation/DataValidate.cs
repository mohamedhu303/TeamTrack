using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using TeamTrack.Models.DTO;

namespace TeamTrack.Models.DataValidatation
{
    public class DataValidate
    {
        public static bool ValidateEmail(string email)
        {
            try
            {
                var emailAdress = new MailAddress(email);
                return emailAdress.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public static bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (password.Length < 8 || password.Length > 32) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
            return true;
        }
        internal static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        public static ValidationResult ValidateFinishDate(DateTime finishDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as CreateTaskDto;
            if (instance == null)
                return ValidationResult.Success;

            if (finishDate < instance.startDate)
                return new ValidationResult("Finish date must be after start date.");

            return ValidationResult.Success;
        }
    }
}
