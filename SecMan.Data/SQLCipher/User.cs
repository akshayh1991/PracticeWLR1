using SecMan.Data.Init;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class User
    {
        public User()
        {
        }
        public User(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        [Key]
        public ulong Id { get; set; } = 0;
        public string? Domain { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public DateTime PasswordDate { get; set; } = DateTime.MinValue;
        public bool ChangePassword { get; set; } = false;
       
        public bool PasswordExpiryEnable { get; set; } = false;
        public DateTime PasswordExpiryDate { get; set; } = DateTime.MinValue;
        public DateTime LastLoginDate { get; set; } = DateTime.MinValue;
        public string? RFI { get; set; } = string.Empty;
        public DateTime RFIDate { get; set; } = DateTime.MinValue;
        public string? Biometric { get; set; } = string.Empty;
        public DateTime BiometricDate { get; set; } = DateTime.MinValue;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Language { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public bool Enabled { get; set; } = false;
        public DateTime EnabledDate { get; set; } = DateTime.MinValue;
        public bool Retired { get; set; } = false;
        public DateTime RetiredDate { get; set; } = DateTime.MinValue;
        public bool Locked { get; set; } = false;
        public string LockedReason { get; set; } = string.Empty;
        public DateTime LockedDate { get; set; } = DateTime.MinValue;
        public bool LegacySupport { get; set; } = false;
        public DateTime LastLogoutDate { get; set; } = DateTime.MinValue;

        

        public bool IsLegacy { get; set; } //added newly
        public bool IsActive { get; set; } = true; //added newly
        public DateTime? InActiveDate { get; set; } = DateTime.MinValue; //added newly
        public bool ResetPassword { get; set; } //added newly
        public bool FirstLogin { get; set; } = true; //added newly
        public DateTime CreatedDate { get; set; } = DateTime.MinValue; //added newly


        public string SessionId { get; set; } = string.Empty;
        public DateTime SessionExpiry { get; set; } = DateTime.MinValue;

        public List<Role> Roles { get; set; } = [];
        public List<PasswordHistory> PasswordHistories { get; set; } = [];
        public string? ResetPasswordToken{ get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
    }
}
