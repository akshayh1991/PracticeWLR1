namespace SecMan.Model
{
    public static class ResponseConstants
    {
        public const string Success = "Success";
        public const string UserAlreadyExists = "User already exists";
        public const string UserNameAndPasswordAreSame = "Password cannot be same as username, please choose different password";
        public const string UserDoesNotExists = "User not found";
        public const string RoleDoesNotExists = "Role not found";
        public const string SettingDoesNotExists = "Setting not found";
        public const string SecurityPolicyDoesNotExists = "Security policy not found";
        public const string CantEdit = "User is retired, cannot edit";
        public const string SomeOfTheRoleNotPresent = "Some of the roles not present";
        public const string InvalidPassword = "Invalid password";
        public const string InvalidPermissions = "Insufficient permissions";
        public const string UserAlreadyRetired = "User is already retired";


        public const string InvalidRequest = "Invalid request";
        public const string NotFound = "Not Found";
        public const string Forbidden = "Forbidden";
        public const string Conflict = "Conflict";


        public const string InvalidSessionId = "Invalid session cookie";
        public const string AccountLocked = "Account is locked";
        public const string AccountRetired = "User is retired, login not permitted";
        public const string AccountInActive = "User is inactive, login not permitted";
        public const string PasswordExpired = "Password is expired, please reset to login again";




        public const string MissingSessionCookie = "Missing session cookie";
        public const string ExpiredSessionCookie = "Expired session cookie";
        public const string InvalidSessionCookie = "Invalid session cookie";
        public const string PermissionDenied = "Permission denied";



        public const string MultipleChangeAreNotAllowed = "Changes are detected in multiple objects, please send only one entity at a time";





        public static string GetTypeUrl(Microsoft.AspNetCore.Http.HttpContext context, string type)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}/epm-suite/api/problem/{type.ToLower()}";
        }

        public const string InvalidId = "Invalid id";
        public const string NotFullyProcessed = "Some of the operations failed";

        public const string UserDetailNotFound = "User details not found";
        public const string InvalidUsername = "Invalid username";
        public const string NoteRequired = "Note is required when IsNote is true";
        public const string IsAuthorizeUsernamePasswordRequired = "Username and Password are required when IsAuthorize is true";
        public const string SignatureVerified = "Signature Verified";
        public const string Status500InternalServerError = "Something went wrong, please try again later";
        public const string RequestEmpty = "Request is empty";
        public const string FailedSignatureVerified = "Signature verification failed";
        public const string InvalidPasswordFormat = "Invalid password format";
        public const string UnsupportedPasswordFormat = "Unsupported password format";


        #region DeviceResponseConstants
        public const string DeviceAlreadyExists = "Device already exists";
        public const string DeviceInvalidTypeId = "Invalid Type Id";
        public const string DeviceInvalidZoneId = "Invalid Zone Id";
        #endregion





    }


    public static class RoleResponseConstants
    {
        public const string BadRequest = "BadRequest";
        public const string RoleAlreadyExists = "Role already exists";
        public const string InvalidUserIds = "Some provided user IDs do not exist. Please provide valid user IDs.";
        public const string InvalidRoleId = "Role not found";
        public const string RoleNameIsNullOrEmpty = "Role name is required";
    }
    public static class DeviceResponseConstants
    {
        public const string BadRequest = "BadRequest";
        public const string DeviceAlreadyExists = "Device already exists";
        public const string InvalidUserIds = "Some provided user IDs do not exist. Please provide valid user IDs.";
        public const string InvalidDeviceId = "Device not found";
        public const string RoleNameIsNullOrEmpty = "Role name is required";
    }

    public static class ResponseHeaders
    {
        public const string TotalCount = "x-total-count";
        public const string FieldRevert = "x-field-revert";
        public const string ObjectRevert = "x-object-revert";
        public const string SSOSessionId = "SSO_SESSION_ID";
        public const string JWT = "JWT";
    }


    public static class EncryptionClassConstants
    {
        public const string NullEncryptedString = "The encryption key is not properly configured. It cannot be empty.";
        public const string InvalidEncryptedStringFormat = "Invalid encrypted string format.";
        public const string InvalidHexStringFormat = "Hex string must have an even number of digits.";
        public const string InvalidHashStringFormat = "The hashed password format is invalid.";
    }
}
