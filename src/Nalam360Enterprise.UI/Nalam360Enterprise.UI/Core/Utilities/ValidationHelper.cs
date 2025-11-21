using System.Text.RegularExpressions;

namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// Common validation rules for healthcare and enterprise applications
/// </summary>
public static class ValidationHelper
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        @"^\+?1?\d{9,15}$",
        RegexOptions.Compiled);

    private static readonly Regex UsPhoneRegex = new(
        @"^(\+?1[-.\s]?)?(\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}$",
        RegexOptions.Compiled);

    private static readonly Regex SsnRegex = new(
        @"^\d{3}-?\d{2}-?\d{4}$",
        RegexOptions.Compiled);

    private static readonly Regex ZipCodeRegex = new(
        @"^\d{5}(-\d{4})?$",
        RegexOptions.Compiled);

    private static readonly Regex UrlRegex = new(
        @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Validates email address format
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email);
    }

    /// <summary>
    /// Validates international phone number format
    /// </summary>
    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var cleaned = Regex.Replace(phone, @"[\s\-\(\)]", "");
        return PhoneRegex.IsMatch(cleaned);
    }

    /// <summary>
    /// Validates US phone number format (various formats accepted)
    /// </summary>
    public static bool IsValidUsPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        return UsPhoneRegex.IsMatch(phone);
    }

    /// <summary>
    /// Validates US Social Security Number format
    /// </summary>
    public static bool IsValidSsn(string? ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
            return false;

        var cleaned = ssn.Replace("-", "");
        if (!SsnRegex.IsMatch(ssn))
            return false;

        // Invalid SSN patterns
        if (cleaned.StartsWith("000") || 
            cleaned.StartsWith("666") || 
            cleaned.StartsWith("9"))
            return false;

        if (cleaned.Substring(3, 2) == "00")
            return false;

        if (cleaned.Substring(5, 4) == "0000")
            return false;

        return true;
    }

    /// <summary>
    /// Validates US ZIP code format (5 or 9 digits)
    /// </summary>
    public static bool IsValidZipCode(string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            return false;

        return ZipCodeRegex.IsMatch(zipCode);
    }

    /// <summary>
    /// Validates URL format
    /// </summary>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return UrlRegex.IsMatch(url);
    }

    /// <summary>
    /// Validates password strength (min 8 chars, uppercase, lowercase, digit, special char)
    /// </summary>
    public static bool IsStrongPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasUpperCase = password.Any(char.IsUpper);
        var hasLowerCase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    /// <summary>
    /// Validates credit card number using Luhn algorithm
    /// </summary>
    public static bool IsValidCreditCard(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        var cleaned = Regex.Replace(cardNumber, @"[\s\-]", "");
        
        if (!Regex.IsMatch(cleaned, @"^\d{13,19}$"))
            return false;

        // Luhn algorithm
        int sum = 0;
        bool alternate = false;

        for (int i = cleaned.Length - 1; i >= 0; i--)
        {
            int digit = cleaned[i] - '0';

            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// Validates date of birth (must be in the past, reasonable age range)
    /// </summary>
    public static bool IsValidDateOfBirth(DateTime? dateOfBirth, int minAge = 0, int maxAge = 150)
    {
        if (!dateOfBirth.HasValue)
            return false;

        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Value.Year;

        if (dateOfBirth.Value.Date > today.AddYears(-age))
            age--;

        return age >= minAge && age <= maxAge && dateOfBirth.Value <= today;
    }

    /// <summary>
    /// Validates medical record number format (alphanumeric, 6-20 chars)
    /// </summary>
    public static bool IsValidMrn(string? mrn)
    {
        if (string.IsNullOrWhiteSpace(mrn))
            return false;

        return Regex.IsMatch(mrn, @"^[A-Z0-9]{6,20}$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Validates National Provider Identifier (NPI) - 10 digits
    /// </summary>
    public static bool IsValidNpi(string? npi)
    {
        if (string.IsNullOrWhiteSpace(npi))
            return false;

        if (!Regex.IsMatch(npi, @"^\d{10}$"))
            return false;

        // Luhn algorithm for NPI validation
        int sum = 0;
        bool alternate = true;

        for (int i = npi.Length - 2; i >= 0; i--)
        {
            int digit = npi[i] - '0';

            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            alternate = !alternate;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (npi[9] - '0');
    }

    /// <summary>
    /// Validates ICD-10 code format
    /// </summary>
    public static bool IsValidIcd10Code(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        // ICD-10: Letter + 2 digits + optional decimal + 1-4 alphanumeric
        return Regex.IsMatch(code, @"^[A-TV-Z]\d{2}(\.\d{1,4})?$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Validates CPT code format (5 digits)
    /// </summary>
    public static bool IsValidCptCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return Regex.IsMatch(code, @"^\d{5}$");
    }

    /// <summary>
    /// Validates insurance policy number format
    /// </summary>
    public static bool IsValidInsurancePolicyNumber(string? policyNumber)
    {
        if (string.IsNullOrWhiteSpace(policyNumber))
            return false;

        // Alphanumeric, 5-20 characters, may include hyphens
        return Regex.IsMatch(policyNumber, @"^[A-Z0-9\-]{5,20}$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Validates required field (not null, empty, or whitespace)
    /// </summary>
    public static bool IsRequired(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Validates string length is within range
    /// </summary>
    public static bool IsValidLength(string? value, int minLength, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value.Length >= minLength && value.Length <= maxLength;
    }

    /// <summary>
    /// Validates numeric value is within range
    /// </summary>
    public static bool IsInRange(decimal? value, decimal min, decimal max)
    {
        if (!value.HasValue)
            return false;

        return value.Value >= min && value.Value <= max;
    }

    /// <summary>
    /// Validates integer value is within range
    /// </summary>
    public static bool IsInRange(int? value, int min, int max)
    {
        if (!value.HasValue)
            return false;

        return value.Value >= min && value.Value <= max;
    }

    /// <summary>
    /// Validates date is within range
    /// </summary>
    public static bool IsDateInRange(DateTime? date, DateTime? minDate, DateTime? maxDate)
    {
        if (!date.HasValue)
            return false;

        if (minDate.HasValue && date.Value < minDate.Value)
            return false;

        if (maxDate.HasValue && date.Value > maxDate.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Validates string matches pattern
    /// </summary>
    public static bool MatchesPattern(string? value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, pattern);
    }

    /// <summary>
    /// Validates alphanumeric characters only
    /// </summary>
    public static bool IsAlphanumeric(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[a-zA-Z0-9]+$");
    }

    /// <summary>
    /// Validates alpha characters only
    /// </summary>
    public static bool IsAlpha(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[a-zA-Z]+$");
    }

    /// <summary>
    /// Validates numeric characters only
    /// </summary>
    public static bool IsNumeric(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^\d+$");
    }

    /// <summary>
    /// Gets validation error message for a failed validation
    /// </summary>
    public static string GetErrorMessage(string fieldName, string validationType)
    {
        return validationType switch
        {
            "Required" => $"{fieldName} is required.",
            "Email" => $"{fieldName} must be a valid email address.",
            "Phone" => $"{fieldName} must be a valid phone number.",
            "SSN" => $"{fieldName} must be a valid Social Security Number.",
            "ZipCode" => $"{fieldName} must be a valid ZIP code.",
            "Url" => $"{fieldName} must be a valid URL.",
            "Password" => $"{fieldName} must be at least 8 characters with uppercase, lowercase, digit, and special character.",
            "CreditCard" => $"{fieldName} must be a valid credit card number.",
            "DateOfBirth" => $"{fieldName} must be a valid date of birth.",
            "MRN" => $"{fieldName} must be a valid medical record number.",
            "NPI" => $"{fieldName} must be a valid National Provider Identifier.",
            "ICD10" => $"{fieldName} must be a valid ICD-10 code.",
            "CPT" => $"{fieldName} must be a valid CPT code.",
            "InsurancePolicy" => $"{fieldName} must be a valid insurance policy number.",
            _ => $"{fieldName} is invalid."
        };
    }
}
