using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models
{
    /// <summary>
    /// Represents a user profile with all editable information
    /// </summary>
    public class UserProfile
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string? TimeZone { get; set; }
        public string? Language { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Website { get; set; }
        public Dictionary<string, string> SocialLinks { get; set; } = new();
        public Dictionary<string, object?> CustomFields { get; set; } = new();
        public ProfilePreferences Preferences { get; set; } = new();
        public ProfileSecurity Security { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
    }

    /// <summary>
    /// Profile preferences and settings
    /// </summary>
    public class ProfilePreferences
    {
        public bool ReceiveEmailNotifications { get; set; } = true;
        public bool ReceiveSmsNotifications { get; set; }
        public bool ShowOnlineStatus { get; set; } = true;
        public bool AllowMessaging { get; set; } = true;
        public string Theme { get; set; } = "Light";
        public string DateFormat { get; set; } = "MM/dd/yyyy";
        public string TimeFormat { get; set; } = "12h";
        public bool ShowAvatar { get; set; } = true;
        public string PrivacyLevel { get; set; } = "Public";
    }

    /// <summary>
    /// Profile security settings
    /// </summary>
    public class ProfileSecurity
    {
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorMethod { get; set; }
        public List<string> ActiveSessions { get; set; } = new();
        public DateTime? LastPasswordChange { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<string> LoginHistory { get; set; } = new();
    }

    /// <summary>
    /// Sections of the profile editor
    /// </summary>
    public enum ProfileSection
    {
        Personal,
        Contact,
        Professional,
        Social,
        Security,
        Preferences,
        Advanced
    }

    /// <summary>
    /// Profile validation result
    /// </summary>
    public class ProfileValidationResult
    {
        public bool IsValid { get; set; } = true;
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> GeneralErrors { get; set; } = new();
    }

    /// <summary>
    /// Avatar upload data
    /// </summary>
    public class AvatarUploadData
    {
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? FileType { get; set; }
        public byte[]? FileData { get; set; }
        public string? Base64Data { get; set; }
        public int? CropX { get; set; }
        public int? CropY { get; set; }
        public int? CropWidth { get; set; }
        public int? CropHeight { get; set; }
    }

    /// <summary>
    /// Password change request
    /// </summary>
    public class PasswordChangeRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Profile change event data
    /// </summary>
    public class ProfileChangeEvent
    {
        public string Field { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Profile editor display options
    /// </summary>
    public class ProfileEditorOptions
    {
        public bool ShowAvatar { get; set; } = true;
        public bool AllowAvatarUpload { get; set; } = true;
        public bool ShowSections { get; set; } = true;
        public bool ShowSaveButton { get; set; } = true;
        public bool ShowCancelButton { get; set; } = true;
        public bool ShowDeleteAccount { get; set; }
        public bool ShowPasswordChange { get; set; } = true;
        public bool ShowTwoFactor { get; set; } = true;
        public bool ShowChangeHistory { get; set; }
        public bool ValidateOnChange { get; set; }
        public bool RequireEmailVerification { get; set; }
        public bool RequirePhoneVerification { get; set; }
        public int MaxAvatarSize { get; set; } = 5242880; // 5MB
        public List<string> AllowedAvatarTypes { get; set; } = new() { "image/jpeg", "image/png", "image/gif" };
    }

    /// <summary>
    /// Social link configuration
    /// </summary>
    public class SocialLinkConfig
    {
        public string Platform { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public string? ValidationPattern { get; set; }
    }
}
