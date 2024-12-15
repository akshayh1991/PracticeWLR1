using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.Model.Common;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace SecMan.BL
{
    public class PasswordBL : IPasswordBl
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionDecryption _encryptionDecryption;
        private readonly IConfiguration _configuration;
        private readonly ISendingEmail _sendingEmail;
        public PasswordBL(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryption, IConfiguration configuration, ISendingEmail sendingEmail)
        {
            _unitOfWork = unitOfWork;
            _encryptionDecryption = encryptionDecryption;
            _configuration = configuration;
            _sendingEmail = sendingEmail;
        }
        public async Task<ServiceResponse<string>> UpdatePasswordAsync(string userName, string oldPassword, string newPassword,ModelStateDictionary modelState)
        {
            string trimmedUsername = userName?.Trim();
            if (oldPassword==newPassword)
            {
                modelState.AddModelError($"newPassword", PasswordExceptionConstants.PasswordOldNewSameError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.ParamNotValid);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            if (newPassword.Contains(userName, StringComparison.OrdinalIgnoreCase))
            {
                modelState.AddModelError($"newPassword", PasswordExceptionConstants.PasswordUserNameNewPasswordError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.ParamNotValid);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            ServiceResponse<string> validationResponse = ValidateInputs(trimmedUsername, oldPassword, newPassword, modelState);
            if (validationResponse.StatusCode != HttpStatusCode.OK)
            {
                return validationResponse;
            }
            GetPasswordComplexityDto passwordComplexityDto = await _unitOfWork.IPasswordRepository.GetPasswordPropsFromSysFeatPropsAsync();
            (bool IsValid, string ErrorMessage) validationResult = ValidatePassword(newPassword, passwordComplexityDto);
            if (!validationResult.IsValid)
            {
                modelState.AddModelError($"newPassword", validationResult.ErrorMessage);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.ParamNotValid);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            UserCredentialsDto? userCredentials = await _unitOfWork.IPasswordRepository.GetUserCredentials(trimmedUsername);
            if (userCredentials == null)
            {
                modelState.AddModelError($"username", PasswordExceptionConstants.PasswordUserNotFoundError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidUserNameError, PasswordExceptionConstants.ParamNotValid);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            ServiceResponse<string> updateResponse = await UpdatePassword(userCredentials, oldPassword, newPassword, modelState);

            return BuildServiceResponse<string>(updateResponse.Message, updateResponse.StatusCode == HttpStatusCode.OK ? HttpStatusCode.OK : HttpStatusCode.BadRequest);

        }
        private ServiceResponse<string> ValidateInputs(string userName, string oldPassword, string newPassword, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                modelState.AddModelError($"username", PasswordExceptionConstants.PasswordUserNameRequiredError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordUserNameRequiredError);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                modelState.AddModelError($"oldPassword", PasswordExceptionConstants.PasswordInvalidCurrentPasswordError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordOldPasswordRequiredError);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                modelState.AddModelError($"newPassword", PasswordExceptionConstants.NewPasswordEmptyError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.NewPasswordEmptyError);

                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            return BuildServiceResponse<string>(PasswordExceptionConstants.PasswordValidationSuccess, HttpStatusCode.OK);
        }
        private (bool IsValid, string ErrorMessage) ValidatePassword(string password, GetPasswordComplexityDto complexityDto)
        {
            int lowerCaseCount=password.Count(char.IsLower);
            int upperCaseCount = password.Count(char.IsUpper);
            int numericCount = password.Count(char.IsDigit);
            int specialCharCount = password.Count(c => !char.IsLetterOrDigit(c));

            if (password.Length < complexityDto.minLength)
            {
                string dynamicPasswordLengthError=PasswordExceptionConstants.DynamicInvalidPasswordLengthError.Replace("{@min}", complexityDto.minLength.ToString()).Replace("{@max}",complexityDto.maxLength.ToString());
                return (false, dynamicPasswordLengthError);
            }
            if (password.Length > complexityDto.maxLength)
            {
                string dynamicPasswordLengthError = PasswordExceptionConstants.DynamicInvalidPasswordLengthError.Replace("{@min}", complexityDto.minLength.ToString()).Replace("{@max}", complexityDto.maxLength.ToString());
                return (false, dynamicPasswordLengthError);
            }

            if (upperCaseCount<complexityDto.upperCase)
            {
                string dynamicUpperCaseErrorMessage = PasswordExceptionConstants.DynamicUpperCaseCharError.Replace("{@count}", complexityDto.upperCase.ToString());
                return (false, dynamicUpperCaseErrorMessage);
            }

            if (lowerCaseCount< complexityDto.lowerCase)
            {
                string dynamicLowerCaseErrorMessage = PasswordExceptionConstants.DynamicLowerCaseCharError.Replace("{@count}", complexityDto.lowerCase.ToString());
                return (false, dynamicLowerCaseErrorMessage);
            }

            if (numericCount< complexityDto.numeric)
            {
                string dynamicnumericErrorMessage = PasswordExceptionConstants.DynamicNumericCharError.Replace("{@count}", complexityDto.numeric.ToString());
                return (false, dynamicnumericErrorMessage);
            }

            if (specialCharCount< complexityDto.nonNumeric)
            {
                string dynamicnonNumericErrorMessage = PasswordExceptionConstants.DynamicSpecialCharError.Replace("{@count}", complexityDto.nonNumeric.ToString());
                return (false, dynamicnonNumericErrorMessage);
            }

            return (true, string.Empty);
        }
        private bool IsValidPasswordFormat(string password)
        {
            string[] parts = password.Split('$');
            return parts.Length >= 2;
        }
        private async Task<ServiceResponse<string>> UpdatePassword(UserCredentialsDto userCredentials, string oldPassword, string newPassword, ModelStateDictionary modelState)
        {
            bool isRetired = await _unitOfWork.IRoleRepository.GetUserRetiredStatusAsync(userCredentials.userId);
            if (!isRetired)
            {
                string[] parts = userCredentials.Password.Split('$');
                switch (parts[1])
                {
                    case "2":
                        return await UpdatePasswordWithPBKDF2(userCredentials, oldPassword, newPassword, modelState);
                    case "1":
                        return await UpdatePasswordWithAES(userCredentials, oldPassword, newPassword, modelState);
                    default:
                        BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordUnsupportedPasswordFormatError);
                        return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
                }
            }
            else
            {
                modelState.AddModelError($"username", PasswordExceptionConstants.PasswordUserNotFoundError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordUserNotFoundError);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
        }
        private async Task<ServiceResponse<string>> UpdatePasswordWithPBKDF2(UserCredentialsDto userCredentials, string oldPassword, string newPassword, ModelStateDictionary modelState)
        {
            await _unitOfWork.BeginTransactionAsync();
            if (userCredentials == null)
            {
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordUserCredentialsNullError);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            bool isValid = _encryptionDecryption.VerifyHashPassword(userCredentials.Password, oldPassword);
            if (!isValid)
            {
                modelState.AddModelError($"oldPassword", PasswordExceptionConstants.PasswordInvalidCurrentPasswordError);
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordOldPasswordIncorrectError);
                return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
            }
            #region PasswordHistoryCheckingForPBKDF    
            List<string> recentPasswords = await _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCredentials.userId);
            string passwordCount=await _unitOfWork.IPasswordRepository.GetHistoryValueAsync();

            foreach (string? pass in recentPasswords)
            {
                if (_encryptionDecryption.VerifyHashPassword(pass, newPassword))
                {
                    string passwordCountError=PasswordExceptionConstants.DynamicPasswordPreviousPasswordsError.Replace("{@Count}", passwordCount);
                    modelState.AddModelError($"newPassword", passwordCountError);
                    BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordLast3PasswordError);
                    return BuildServiceResponse<string>(badRequestResponse.Detail, badRequestResponse.Status);
                }
            }
            #endregion
            string encNewPassword = _encryptionDecryption.EncryptPassword(newPassword, false);
            await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCredentials.userId, encNewPassword.Trim());
            await _unitOfWork.CommitTransactionAsync();
            return BuildServiceResponse<string>(PasswordConstants.PasswordSuccess, HttpStatusCode.OK);
        }
        private async Task<ServiceResponse<string>> UpdatePasswordWithAES(UserCredentialsDto userCredentials, string oldPassword, string newPassword, ModelStateDictionary modelState)
        {
            await _unitOfWork.BeginTransactionAsync();
            if (userCredentials == null)
            {
                BadRequest nullCredentialsResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidCredentialsError, PasswordExceptionConstants.PasswordUserCredentialsNullError);
                return BuildServiceResponse<string>(nullCredentialsResponse.Detail, nullCredentialsResponse.Status);
            }
            #region PasswordHistoryCheckingForAES
            List<string> recentPasswords = await _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCredentials.userId);
            string passwordCount = await _unitOfWork.IPasswordRepository.GetHistoryValueAsync();
            foreach (string? pass in recentPasswords)
            {
                string existingPreviously = _encryptionDecryption.DecryptPasswordAES256(pass);
                if (existingPreviously == newPassword)
                {
                    string passwordCountError = PasswordExceptionConstants.DynamicPasswordPreviousPasswordsError.Replace("{@Count}", passwordCount);
                    modelState.AddModelError($"newPassword", passwordCountError);
                    BadRequest passwordReuseResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordLast3PasswordError);
                    return BuildServiceResponse<string>(passwordReuseResponse.Detail, passwordReuseResponse.Status);
                }
            }
            #endregion
            string decryptedPassword = _encryptionDecryption.DecryptPasswordAES256(userCredentials.Password);
            if (decryptedPassword == oldPassword)
            {
                string encryptedPassword = _encryptionDecryption.EncryptPassword(newPassword, true);
                await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCredentials.userId, encryptedPassword.Trim());
                await _unitOfWork.CommitTransactionAsync();
                return BuildServiceResponse<string>(PasswordConstants.PasswordSuccess, HttpStatusCode.OK);
            }
            modelState.AddModelError($"oldPassword", PasswordExceptionConstants.PasswordInvalidCurrentPasswordError);
            BadRequest incorrectOldPasswordResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidError, PasswordExceptionConstants.PasswordOldPasswordIncorrectError);
            return BuildServiceResponse<string>(incorrectOldPasswordResponse.Detail, incorrectOldPasswordResponse.Status);
        }
        public async Task<ServiceResponse<GetForgetPasswordDto>> ForgetPasswordAsync(string userName)
        {
            string trimmedUsername = userName.Trim();
            await _unitOfWork.BeginTransactionAsync();
            if (string.IsNullOrWhiteSpace(trimmedUsername))
            {
                BadRequest badRequestResponse = new BadRequest(PasswordExceptionConstants.PasswordInvalidUserNameError, PasswordExceptionConstants.PasswordUserNameRequiredError);
                return BuildServiceResponse<GetForgetPasswordDto>(badRequestResponse.Detail, badRequestResponse.Status);

            }
            try
            {
                GetForgetPasswordDto? userCredentials = await _unitOfWork.IPasswordRepository.ForgetPasswordCredentials(trimmedUsername);
                if (userCredentials == null)
                {
                    BadRequest notFoundResponse = new BadRequest(PasswordExceptionConstants.PasswordUserNotFoundError, PasswordExceptionConstants.PasswordUserNotFoundError);
                    return BuildServiceResponse<GetForgetPasswordDto>(notFoundResponse.Detail, notFoundResponse.Status);
                }
                string userCred = $"{userCredentials.domain}:{userCredentials.userName}:{userCredentials.userId}";
                string generatedHashToken = await GenerateHashedToken(userCred);
                string savedHashToken = await _unitOfWork.IPasswordRepository.UpdateHashedUserNamePassword(userCredentials.userId, generatedHashToken);
                await _unitOfWork.CommitTransactionAsync();
                if (savedHashToken == null)
                {
                    BadRequest tokenUpdateErrorResponse = new BadRequest(PasswordExceptionConstants.PasswordHashedTokenError, PasswordExceptionConstants.PasswordHashedTokenError);
                    return BuildServiceResponse<GetForgetPasswordDto>(tokenUpdateErrorResponse.Detail, tokenUpdateErrorResponse.Status);
                }
                string resetPasswordLink = $"{_configuration["ResetPassword:BaseURL"]}?token={Uri.EscapeDataString(generatedHashToken)}&email={Uri.EscapeDataString(userCredentials.emailId)}";
                bool isEmailConfigured = await _unitOfWork.IPasswordRepository.IsEmailConfigurationEnabledAsync();
                if (isEmailConfigured)
                {
                    Dictionary<string, string> emailConfigDetails = await _unitOfWork.IPasswordRepository.GetEmailConfigurationDetailsAsync();
                    EmailConfiguration emailConfig = new EmailConfiguration
                    {
                        MailServer = emailConfigDetails["Addr"],
                        SmtpPort = int.TryParse(emailConfigDetails["Port"], out int port) ? port : 587,
                        EnableSsl = bool.TryParse(emailConfigDetails["TLS"], out bool tls) && tls,
                        From = emailConfigDetails["Sender"],
                        Password = emailConfigDetails["Password"]
                    };
                    EmailMessage emailMessage = new EmailMessage()
                    {
                        Subject = PasswordExceptionConstants.PasswordMailSubject,
                        Body = PasswordExceptionConstants.PasswordMailBody,
                        Title = PasswordExceptionConstants.PasswordTitle

                    };
                    bool mailSent = await _sendingEmail.SendPasswordResetEmailAsync(userCredentials.emailId, resetPasswordLink, emailConfig, emailMessage);
                    if (!mailSent)
                    {
                        BadRequest emailSendErrorResponse = new BadRequest(PasswordExceptionConstants.PasswordEmailFailed, PasswordExceptionConstants.PasswordResetLinkError);
                        return BuildServiceResponse<GetForgetPasswordDto>(emailSendErrorResponse.Detail, emailSendErrorResponse.Status);
                    }
                    GetForgetPasswordDto userCredentialsDto = new GetForgetPasswordDto
                    {
                        userName = userCredentials.userName,
                        emailId = userCredentials.emailId,
                        link = resetPasswordLink
                    };
                    return BuildServiceResponse<GetForgetPasswordDto>(PasswordExceptionConstants.PasswordResetLinkGeneration, HttpStatusCode.OK, userCredentialsDto);
                }
                else
                {
                    BadRequest emailConfigErrorResponse = new BadRequest(PasswordExceptionConstants.PasswordEmailConfigError, PasswordExceptionConstants.PasswordEmailConfigNotEnabled);
                    return BuildServiceResponse<GetForgetPasswordDto>(emailConfigErrorResponse.Detail, emailConfigErrorResponse.Status);
                }
            }
            catch (Exception ex)
            {
                BadRequest errorResponse = new BadRequest(PasswordExceptionConstants.PasswordUnexpectedError, PasswordExceptionConstants.PasswordUnexpectedError);
                return BuildServiceResponse<GetForgetPasswordDto>(errorResponse.Detail, errorResponse.Status);
            }
        }
        public Task<string> GenerateHashedToken(string userNamePassword)
        {
            string hashedToken = _encryptionDecryption.EncryptPassword(userNamePassword, false);
            return Task.FromResult(hashedToken);
        }
        public async Task<ServiceResponse<bool>> GetUserNamePasswordAsync(string email, string token)
        {
            string trimmedEmail = email.Trim();
            GetUserNamePasswordDto getUserNamePasswordResponse = await _unitOfWork.IPasswordRepository.GetUserNamePasswordFromEmailId(trimmedEmail);
            if (getUserNamePasswordResponse == null)
            {
                return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordUserNotFoundError, HttpStatusCode.NotFound, false);
            }
            TimeSpan? timeDifference = DateTime.Now - getUserNamePasswordResponse.hashedUserNamePasswordTime;
            string? passwordExpiryValueString = await _unitOfWork.IPasswordRepository.GetPasswordExpiryWarningValue(SysFeatureConstants.ExpiryWarning);
            if (string.IsNullOrEmpty(passwordExpiryValueString)) { passwordExpiryValueString = "1"; }
            if (!TimeSpan.TryParse(passwordExpiryValueString, CultureInfo.InvariantCulture, out TimeSpan passwordExpiryValue))
            {
                throw new FormatException(PasswordExceptionConstants.PasswordExpiryValueError);
            }
            if (timeDifference < passwordExpiryValue)
            {
                if (token == getUserNamePasswordResponse.hashedUserNamePassword)
                {
                    return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordTokenValidatedSuccess, HttpStatusCode.OK, true);
                }
                else
                {
                    return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordTokenValidatedFailed, HttpStatusCode.Unauthorized, false);
                }
            }
            else
            {
                return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordTokenExpired, HttpStatusCode.Forbidden, false);
            }
        }
        public async Task<ServiceResponse<bool>> CheckForHashedToken(string token, string newPassword)
        {
            await _unitOfWork.BeginTransactionAsync();
            GetPasswordComplexityDto passwordComplexityDto = await _unitOfWork.IPasswordRepository.GetPasswordPropsFromSysFeatPropsAsync();
            (bool IsValid, string ErrorMessage) validationResult = ValidatePassword(newPassword, passwordComplexityDto);
            if (!validationResult.IsValid)
            {
                return BuildServiceResponse<bool>(validationResult.ErrorMessage, HttpStatusCode.BadRequest, false);
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordTokenRequiredError, HttpStatusCode.BadRequest, false);
            }
            UserCredentialsDto userCred = await _unitOfWork.IPasswordRepository.CheckForHashedTokenWithUserDetails(token);
            if (userCred != null)
            {
                if (string.IsNullOrWhiteSpace(userCred.Password))
                {
                    return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordInvalidPasswordFormatError, HttpStatusCode.BadRequest, false);
                }

                string[] parts = userCred.Password.Split('$');
                if (parts.Length < 2)
                {
                    return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordInvalidPasswordFormatError, HttpStatusCode.BadRequest, false);
                }

                if (parts[1] == "2")
                {
                    #region PasswordHistoryCheckingForPBKDF2
                    List<string> recentPasswords = await _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCred.userId);
                    foreach (string? pass in recentPasswords)
                    {
                        bool isPreviouslyUsed = _encryptionDecryption.VerifyHashPassword(pass, newPassword);
                        if (isPreviouslyUsed)
                        {
                            return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordLast3PasswordError, HttpStatusCode.BadRequest, false);
                        }
                    }
                    #endregion

                    string encNewPasswordWithPkdf2 = _encryptionDecryption.EncryptPassword(newPassword, false);
                    await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCred.userId, encNewPasswordWithPkdf2);
                    await _unitOfWork.CommitTransactionAsync();
                    return BuildServiceResponse<bool>(PasswordConstants.PasswordSuccess, HttpStatusCode.OK, true);
                }
                else if (parts[1] == "1")
                {
                    #region PasswordHistoryCheckingForAES
                    List<string> recentPasswords = await _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCred.userId);
                    foreach (string? pass in recentPasswords)
                    {
                        string existingPreviously = _encryptionDecryption.DecryptPasswordAES256(pass);
                        if (existingPreviously == newPassword)
                        {
                            return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordLast3PasswordError, HttpStatusCode.BadRequest, false);
                        }
                    }
                    #endregion

                    string encNewPasswordWithAes = _encryptionDecryption.EncryptPassword(newPassword, true);
                    await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCred.userId, encNewPasswordWithAes);
                    await _unitOfWork.CommitTransactionAsync();
                    return BuildServiceResponse<bool>(PasswordConstants.PasswordSuccess, HttpStatusCode.OK, true);
                }
            }
            return BuildServiceResponse<bool>(PasswordExceptionConstants.PasswordInvalidTokenEmailError, HttpStatusCode.BadRequest, false);
        }


        private ServiceResponse<T> BuildServiceResponse<T>(string message, HttpStatusCode statusCode, T? data = default)
        {
            return new ServiceResponse<T>
            {
                Message = message,
                StatusCode = statusCode,
                Data = data
            };
        }
    }
}
