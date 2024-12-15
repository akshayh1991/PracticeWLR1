namespace SecMan.Model
{
    public static class StringConstants
    {
        public const string LockedByMultipleFailedLogin = "Account is locked due to multiple failed login attempts";
        public const string InvalidToken = "Invalid access token";
        public const string PassBearerToken = "Please pass bearer token to access this API";
    }




    public static class ResponseUrlType
    {
        public const string Conflict = "conflict";
        public const string Unauthorized = "unauthorized";
        public const string Forbidden = "forbidden";
        public const string ServerError = "internal-server-error";
        public const string ValidationError = "validation-error";
        public const string NotFound = "not-found";

    }
}
