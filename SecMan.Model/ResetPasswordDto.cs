using System.ComponentModel.DataAnnotations;

namespace SecMan.Model
{
    public class ResetPasswordDto
    {
        public string newPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage ="Username cannot be empty")]
        public string? userName { get; set; }
        public string oldPassword { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
    }

    public class UserCredentialsDto
    {
        public ulong userId { get; set; }
        public string? Password { get; set; }
    }

    public class ForgetPasswordDto
    {
        public string userName { get; set; }
    }

    public class GetForgetPasswordDto
    {
        public ulong userId { get; set; }
        public string? domain { get; set; }
        public string? userName { get; set; }
        public string? password { get; set; }
        public string? emailId { get; set; }
        public string link { get; set; }
    }

    public class GetUserNamePasswordDto
    {
        public string? userName { get; set; }
        public string? password { get; set; }
        public string? hashedUserNamePassword { get; set; }
        public DateTime? hashedUserNamePasswordTime { get; set; }
    }

    public class GetPasswordComplexityDto
    {
        public int minLength { get; set; }
        public int maxLength { get; set; }
        public int upperCase { get; set; }
        public int lowerCase { get; set; }
        public int numeric { get; set; }
        public int nonNumeric { get; set; }
    }
}
