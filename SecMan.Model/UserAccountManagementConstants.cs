using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using System.ComponentModel;

namespace SecMan.Model
{
    public static class PasswordConstants
    {
        public const string PasswordSuccess = "Password updated successfully.";
        public const string PasswordFailed = "Password update failed.";
    }

    public static class PasswordExceptionConstants
    {
        public const string ParamNotValid = "Provided input request parameter is not valid.";
        public const string NewPasswordEmptyError = "New password cannot be empty";
        public const string PasswordTitle = "Reset password";
        public const string PasswordBadRequest = "Invalid request.";
        public const string PasswordComplexityError = "Password complexity feature not found in database.";
        public const string PasswordValidUserIdRequired = "Valid userId is required.";
        public const string PasswordInvalidEmail = "Invalid email.";
        public const string PasswordEmailIdNotExits = "Email Id does not exist";
        public const string PasswordEmailFailed = "Email sending failed";
        public const string PasswordResetLinkGeneration = "Password reset link generated successfully.";
        public const string PasswordChangeFailed = "Password change failed.";
        public const string PasswordInvalidCredentialsError = "Invalid user credentials";

        public const string PasswordFailedResetLink = "Failed to generate password reset link.";
        public const string PasswordInternalServerError = "Internal server error. Please try again later.";
        public const string PasswordTokenRequiredError = "Token is required.";
        public const string PasswordEmailRequiredError = "Email is required.";
        public const string PasswordInvalidTokenEmailError = "Invalid token or email.";
        public const string PasswordNewPasswordRequiredError = "New password is required.";
        public const string PasswordTokenPasswordResetError = "Invalid token or password reset failed.";
        public const string PasswordValidationSuccess = "Validation successful";
        public const string PasswordUnexpectedError = "An unexpected error occurred.";

        public const string PasswordInvalidUserNameError = "Invalid username.";
        public const string PasswordInvalidPasswordFormatError = "Invalid password format.";
        public const string PasswordInvalidError = "Invalid Password";
        public const string PasswordOldNewSameError = "The new password cannot be the same as any of the last three passwords.Please choose a different password";
        public const string PasswordUserNameRequiredError = "Username cannot be empty";
        public const string PasswordUserNameNewPasswordError = "The new password cannot contain username";
        public const string PasswordInvalidCurrentPasswordError = "Invalid current password";
        public const string PasswordPreviousPasswordsError = "The new password cannot be the same as any of the last 10 passwords.Please choose a different password";
        public const string DynamicPasswordPreviousPasswordsError = "The new password cannot be the same as any of the last {@Count} passwords. Please enter a different password";
        public const string PasswordUserNotFoundError = "User not found.";
        public const string PasswordOldPasswordRequiredError = "Old password is required.";
        public const string PasswordOldPasswordIncorrectError = "Old password is incorrect.";
        public const string PasswordNewPassword6CharsError = "New password must be at least 6 characters long.";
        public const string PasswordUnsupportedPasswordFormatError = "Unsupported password format.";
        public const string PasswordUserCredentialsNullError = "User credentials cannot be null.";
        public const string PasswordUserPasswordNullEmptyError = "User password cannot be null or empty.";
        public const string PasswordLast3PasswordError = "The new password cannot be the same as any of the last three passwords. Please choose a different password.";
        public const string PasswordHashedTokenError = "Failed to update hashed token.";
        public const string PasswordEmailIdNotExitsError = "Email Id is not present for this user.";
        public const string PasswordResetLinkError = "Failed to send password reset email.";
        public const string PasswordMailSubject = "Password Reset Link";
        public const string PasswordMailBody = "To reset your password, please click the link below:";
        public const string PasswordExpiryValueError = "Invalid password expiry value.";
        public const string PasswordTokenValidatedSuccess = "Token validated successfully.";
        public const string PasswordTokenValidatedFailed = "Token validation failed.";
        public const string PasswordTokenExpired = "Token expired.";
        public const string PasswordEmailConfigError = "Email password configuration error";
        public const string PasswordEmailConfigNotEnabled = "Email password configuration not enabled";
        public const string HashedTokenRequired = "Hashed token is required.";

        public const string DynamicInvalidPasswordLengthError = "Password must be between {@min} and {@max} characters";
        public const string DynamicUpperCaseCharError = "Password must contain at least {@count} uppercase character";
        public const string DynamicLowerCaseCharError = "Password must contain at least {@count} lowercase character";
        public const string DynamicNumericCharError = "Password must contain at least {@count} numeric character";
        public const string DynamicSpecialCharError = "Password must contain at least {@count} non-alphanumeric character";
    }
}
