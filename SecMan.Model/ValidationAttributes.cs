using SecMan.BL.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SecMan.Model
{
    public class ValidPasswordAttribute : ValidationAttribute
    {
        private readonly bool _forUpdate;

        public ValidPasswordAttribute(bool forUpdate = false)
        {
            _forUpdate = forUpdate;
        }
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string? password = value as string;

            if (!_forUpdate && string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult(ValidationConstants.EmptyPasswordError);
            }

            if (password != null)
            {
                if (password.Length < 6 || password.Length > 15)
                {
                    return new ValidationResult(ValidationConstants.InvalidPasswordLengthError);
                }

                if (!Regex.IsMatch(password, @"[A-Z]")) // At least 1 uppercase character
                {
                    return new ValidationResult(ValidationConstants.UpperCaseCharError);
                }

                if (!Regex.IsMatch(password, @"[a-z]")) // At least 1 lowercase character
                {
                    return new ValidationResult(ValidationConstants.LowerCaseCharError);
                }

                if (!Regex.IsMatch(password, @"[0-9]")) // At least 1 numeric character
                {
                    return new ValidationResult(ValidationConstants.NumericCharError);
                }

                if (!Regex.IsMatch(password, @"[^\w]")) // At least 1 non-alphanumeric character
                {
                    return new ValidationResult(ValidationConstants.SpecialCharError);
                }
            }

            return ValidationResult.Success;
        }
    }


    public class ValidUsernameAttribute : ValidationAttribute
    {
        private readonly bool _forUpdate;

        public ValidUsernameAttribute(bool forUpdate = false)
        {
            _forUpdate = forUpdate;
        }
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string? username = value as string;

            if (!_forUpdate && string.IsNullOrWhiteSpace(username))
            {
                return new ValidationResult(ValidationConstants.EmptyUserNameError);
            }

            if (username != null)
            {

                if (username.Length < 3 || username.Length > 32)
                {
                    return new ValidationResult(ValidationConstants.InvalidUserNameLengthError);
                }

                // Check for disallowed characters
                if (Regex.IsMatch(username, @"[\""\[\]:;|=+*?<>/\\,]"))
                {
                    return new ValidationResult(ValidationConstants.InvalidCharError);
                }

                // Check for non-printable characters (ASCII codes less than 32 or equal to 127)
                if (Regex.IsMatch(username, @"[\x00-\x1F\x7F]"))
                {
                    return new ValidationResult(ValidationConstants.NonPrintableCharError);
                }

                // Check that username does not end with a period
                if (username.EndsWith("."))
                {
                    return new ValidationResult(ValidationConstants.EndsWithFullStopError);
                }
            }

            return ValidationResult.Success;
        }
    }


    public class ValidateDomainAttribute : ValidationAttribute
    {
        private readonly bool _forUpdate;

        public ValidateDomainAttribute(bool forUpdate = false)
        {
            _forUpdate = forUpdate;
        }
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string? domain = value as string;

            if (!_forUpdate && string.IsNullOrWhiteSpace(domain))
            {
                return new ValidationResult(ValidationConstants.EmptyDomainError);
            }

            if (domain != null)
            {

                if (domain.Length > 32)
                {
                    return new ValidationResult(ValidationConstants.InvalidDomainLengthError);
                }

                if (!Enum.GetNames(typeof(Domain)).Contains(domain))
                {
                    return new ValidationResult(ValidationConstants.InvalidDomainError);
                }
            }

            return ValidationResult.Success;
        }
    }


    public class ValidateLanguageAttribute : ValidationAttribute
    {
        private readonly bool _forUpdate;

        public ValidateLanguageAttribute(bool forUpdate = false)
        {
            _forUpdate = forUpdate;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string? language = value as string;

            if (!_forUpdate && string.IsNullOrWhiteSpace(language))
            {
                return new ValidationResult(ValidationConstants.InvalidLanguageError);
            }

            if (language != null && !Enum.GetNames(typeof(Languages)).Contains(language))
            {
                return new ValidationResult(ValidationConstants.InvalidLanguageError);
            }

            return ValidationResult.Success;
        }
    }



    public class ValidateEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? email = value as string;

            if (string.IsNullOrEmpty(email))
            {
                return ValidationResult.Success;
            }

            if (email.Length > 80)
            {
                return new ValidationResult(ValidationConstants.InvalidEmailLengthError);
            }

            EmailAddressAttribute emailAttribute = new EmailAddressAttribute();
            if (emailAttribute.IsValid(email))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid email address.");
        }
    }


    public class ValidateMaxLengthAttribute : ValidationAttribute
    {
        private readonly int _maxLength;

        public ValidateMaxLengthAttribute(int maxLength)
        {
            _maxLength = maxLength;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? objectValue = value as string;

            if (string.IsNullOrEmpty(objectValue))
            {
                return ValidationResult.Success;
            }

            if (objectValue.Length > _maxLength)
            {
                return new ValidationResult(ValidationConstants.InvalidObjectLengthError(validationContext.MemberName, _maxLength));
            }

            return ValidationResult.Success;
        }
    }

    public class ValidNotNullAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string? password = value as string;

            if (password == null)
            {
                return new ValidationResult(ValidationConstants.EmptyParamError.Replace("@Param", validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
    }

    public class ValidNotNullOrWhiteSpaceAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string? stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return new ValidationResult(ValidationConstants.EmptyParamError.Replace("@Param", validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
    }
}
