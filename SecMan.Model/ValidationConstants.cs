namespace SecMan.Model
{
    public static class ValidationConstants
    {
        public const string EmptyPasswordError = "Password cannot be empty";
        public const string InvalidPasswordLengthError = "Password must be between 8 and 32 characters";
        public const string UpperCaseCharError = "Password must contain at least 1 uppercase character";
        public const string LowerCaseCharError = "Password must contain at least 1 lowercase character";
        public const string NumericCharError = "Password must contain at least 1 numeric character";
        public const string SpecialCharError = "Password must contain at least 1 non-alphanumeric character";

        public const string InvalidPasswordLengthStartError = "Password must be between";
        public const string InvalidPasswordLengthMiddleError = "and";
        public const string InvalidPasswordLengthEndError = "characters";
        public const string UpperCaseCharStartError = "Password must contain at least";
        public const string UpperCaseCharEndError = "uppercase character";
        public const string LowerCaseCharStartError = "Password must contain at least";
        public const string LowerCaseCharEndError = "lowercase character";
        public const string NumericCharStartError = "Password must contain at least";
        public const string NumericCharEndError = "numeric character";
        public const string SpecialCharStartError = "Password must contain at least";
        public const string SpecialCharEndError = "non-alphanumeric character";

        public const string EmptyUserNameError = "Username cannot be empty";
        public const string InvalidUserNameLengthError = "Username must be between 3 and 32 characters";
        public const string InvalidDomainLengthError = "Domain must be less than 32 characters";
        public const string InvalidEmailLengthError = "Email must be less than 80 characters";
        public const string InvalidEmail = "Invalid email";
        public const string InvalidBoolean = "Invalid boolean";
        public const string InvalidNumber = "Invalid number";
        public const string ValueIsLessThanMin = "The value is less than minimum value";
        public const string ValueIsLessThanMax = "The value is greater than maximum value";
        public const string InvalidIP = "Invalid ip address";
        public const string InvalidCharError = "Username contains invalid characters";
        public const string NonPrintableCharError = "Username contains non-printable characters";
        public const string EndsWithFullStopError = "Username cannot end with a period";


        public const string EmptyDomainError = "Domain cannot be empty";
        public const string EmptyLanguageError = "Language cannot be empty";


        public static string InvalidObjectLengthError(string? propertyName, int maxLength)
        {
            return $"{propertyName} must be less than {maxLength} characters";
        }


        public const string InvalidDomainError = "Please enter valid domain";
        public const string InvalidLanguageError = "Unsupported language code";
        public const string EmptyParamError = "@Param cannot be empty";

    }
}
