using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.Model.Common;

namespace SecMan.Data.Repository
{
    public class PasswordRepository : IPasswordRepository
    {
        private readonly Db _context;
        public PasswordRepository(Db context)
        {
            _context = context;
        }
        public async Task<UserCredentialsDto> GetUserCredentials(string userName)
        {
            UserCredentialsDto existingUser = await _context.Users
                .Where(x => x.UserName == userName)
                .Select(x => new UserCredentialsDto
                {
                    userId = x.Id,
                    Password = x.Password
                })
                .FirstOrDefaultAsync();
            return existingUser;
        }
        public async Task<string> UpdatePasswordAsync(ulong userId, string newPassword)
        {
            SQLCipher.User user = await _context.Users.FindAsync(userId);
            user.Password = newPassword;
            await InsertPasswordHistoryAsync(userId, newPassword, DateTime.Now);
            return user.Password;
        }
        public async Task<List<string>> GetRecentPasswordsAsync(ulong userId)
        {
            SQLCipher.SysFeatProp result = await _context.SysFeatProps.FirstOrDefaultAsync(x => x.Name == SysFeatureConstants.History);
            int val = Convert.ToInt32(result.Val);
            return await _context.PasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedDate)
                .Take(val)
                .Select(ph => ph.Password)
                .ToListAsync();
        }

        public async Task<Tuple<List<string>, string>> GetRecentPasswordsWithHistoryCountAsync(ulong userId)
        {
            SQLCipher.SysFeatProp result = await _context.SysFeatProps.FirstOrDefaultAsync(x => x.Name == SysFeatureConstants.History);
            return new Tuple<List<string>, string>(await GetRecentPasswordsAsync(userId), result.Val);
        }

        public async Task<string> GetHistoryValueAsync()
        {
            var result = await _context.SysFeatProps
                .FirstOrDefaultAsync(x => x.Name == SysFeatureConstants.History);
            return result.Val;
        }

        private async Task InsertPasswordHistoryAsync(ulong userId, string password, DateTime changeDate)
        {
            PasswordHistory passwordHistory = new PasswordHistory
            {
                UserId = userId,
                Password = password,
                CreatedDate = changeDate
            };
            _context.PasswordHistories.Add(passwordHistory);
        }
        public async Task<GetForgetPasswordDto> ForgetPasswordCredentials(string userName)
        {
            GetForgetPasswordDto existingUser = await _context.Users
                .Where(x => x.UserName == userName)
                .Select(x => new GetForgetPasswordDto
                {
                    userId = x.Id,
                    domain = x.Domain,
                    userName = x.UserName,
                    password = x.Password,
                    emailId = x.Email
                })
                .FirstOrDefaultAsync();
            return existingUser;
        }
        public async Task<string> UpdateHashedUserNamePassword(ulong userId, string hashedToken)
        {
            SQLCipher.User user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.ResetPasswordToken = hashedToken;
            user.ResetPasswordTokenExpiry = DateTime.Now;
            return user.ResetPasswordToken;
        }
        public async Task<GetUserNamePasswordDto> GetUserNamePasswordFromEmailId(string email)
        {
            GetUserNamePasswordDto userCreds = await _context.Users
                .Where(x => x.Email == email)
                .Select(x => new GetUserNamePasswordDto
                {
                    userName = x.UserName,
                    password = x.Password,
                    hashedUserNamePassword = x.ResetPasswordToken,
                    hashedUserNamePasswordTime = x.ResetPasswordTokenExpiry
                })
                .FirstOrDefaultAsync();
            return userCreds;
        }
        public async Task<string> GetPasswordExpiryWarningValue(string name)
        {
            SQLCipher.SysFeatProp result = await _context.SysFeatProps.FirstOrDefaultAsync(x => x.Name == name);
            if (result == null)
            {
                return string.Empty;
            }
            return result.Val;
        }
        public async Task<ulong> CheckForHashedToken(string hashedToken)
        {
            if (string.IsNullOrWhiteSpace(hashedToken))
            {
                throw new ArgumentException(PasswordExceptionConstants.HashedTokenRequired);
            }
            SQLCipher.User user = await _context.Users.FirstOrDefaultAsync(x => x.ResetPasswordToken == hashedToken);
            if (user == null)
            {
                return 0;
            }
            return user.Id;
        }
        public async Task<UserCredentialsDto> CheckForHashedTokenWithUserDetails(string hashedToken)
        {
            SQLCipher.User user = await _context.Users.FirstOrDefaultAsync(x => x.ResetPasswordToken == hashedToken);
            if (user != null)
            {
                return new UserCredentialsDto
                {
                    userId = user.Id,
                    Password = user.Password
                };
            }
            return null;
        }
        public async Task<bool> IsEmailConfigurationEnabledAsync()
        {
            bool emailConfig = await _context.SysFeats
                .Where(sf => sf.Name == SysFeatureConstants.Email)
                .Select(sf => sf.Common)
                .FirstOrDefaultAsync();
            return emailConfig;
        }
        public async Task<Dictionary<string, string>> GetEmailConfigurationDetailsAsync()
        {
            SQLCipher.SysFeat emailConfig = await _context.SysFeats
                .Where(sf => sf.Name == SysFeatureConstants.Email)
                .Include(sf => sf.SysFeatProps)
                .FirstOrDefaultAsync();

            if (emailConfig == null || emailConfig.SysFeatProps == null)
            {
                return new Dictionary<string, string>();
            }
            return emailConfig.SysFeatProps
                .Where(prop => !string.IsNullOrEmpty(prop.Val))
                .ToDictionary(prop => prop.Name, prop => prop.Val);
        }
        public async Task<GetPasswordComplexityDto> GetPasswordPropsFromSysFeatPropsAsync()
        {
            ulong sysFeatId = await _context.SysFeats
                .Where(f => f.Name == SysFeatureConstants.PasswordComplexity)
                .Select(f => f.Id)
                .FirstOrDefaultAsync();
            List<SQLCipher.SysFeatProp> passwordProps = await _context.SysFeatProps
                .Include(p => p.SysFeat)
                .Where(p => p.SysFeat != null && p.SysFeat.Id == sysFeatId)
                .ToListAsync();
            GetPasswordComplexityDto complexityDto = new GetPasswordComplexityDto();
            foreach (SQLCipher.SysFeatProp prop in passwordProps)
            {
                switch (prop.Name)
                {
                    case SysFeatureConstants.MinimumLength:
                        complexityDto.minLength = Convert.ToInt32(prop.Val);
                        break;
                    case SysFeatureConstants.MaximumLength:
                        complexityDto.maxLength = Convert.ToInt32(prop.Val);
                        break;
                    case SysFeatureConstants.UppercaseCharacters:
                        complexityDto.upperCase = Convert.ToInt32(prop.Val);
                        break;
                    case SysFeatureConstants.LowercaseCharacters:
                        complexityDto.lowerCase = Convert.ToInt32(prop.Val);
                        break;
                    case SysFeatureConstants.NumericCharacters:
                        complexityDto.numeric = Convert.ToInt32(prop.Val);
                        break;
                    case SysFeatureConstants.NonNumericCharacters:
                        complexityDto.nonNumeric = Convert.ToInt32(prop.Val);
                        break;
                }
            }
            return complexityDto;
        }
    }
}
