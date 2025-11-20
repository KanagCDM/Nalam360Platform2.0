using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Nalam360Enterprise.UI.Models;
using Nalam360Enterprise.UI.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalam360Enterprise.UI.Components
{
    public partial class N360ProfileEditor<TProfile> where TProfile : UserProfile
    {
        #region Parameters

        [Parameter] public TProfile? Profile { get; set; }
        [Parameter] public EventCallback<TProfile> OnSave { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        [Parameter] public EventCallback<AvatarUploadData> OnAvatarUpload { get; set; }
        [Parameter] public EventCallback<PasswordChangeRequest> OnPasswordChange { get; set; }
        [Parameter] public EventCallback OnDeleteAccount { get; set; }
        [Parameter] public EventCallback<ProfileChangeEvent> OnFieldChange { get; set; }
        
        [Parameter] public bool ShowAvatar { get; set; } = true;
        [Parameter] public bool AllowAvatarUpload { get; set; } = true;
        [Parameter] public bool ShowTabs { get; set; } = true;
        [Parameter] public bool ShowSaveButton { get; set; } = true;
        [Parameter] public bool ShowCancelButton { get; set; } = true;
        [Parameter] public bool ShowDeleteAccount { get; set; }
        [Parameter] public bool ShowPasswordChange { get; set; } = true;
        [Parameter] public bool ShowTwoFactor { get; set; } = true;
        [Parameter] public bool IsReadOnly { get; set; }
        
        [Parameter] public ProfileSection InitialSection { get; set; } = ProfileSection.Personal;
        [Parameter] public List<ProfileSection> HiddenSections { get; set; } = new();
        [Parameter] public ProfileEditorOptions Options { get; set; } = new();
        
        [Parameter] public string? RequiredPermission { get; set; }
        [Parameter] public bool HideIfNoPermission { get; set; }
        [Parameter] public bool EnableAudit { get; set; }
        [Parameter] public string AuditResource { get; set; } = "ProfileEditor";
        [Parameter] public string CssClass { get; set; } = string.Empty;
        [Parameter] public bool IsRtl { get; set; }

        #endregion

        #region Private Fields

        private ProfileSection ActiveSection { get; set; }
        private PasswordChangeRequest _passwordChange = new();
        private int _passwordStrength;
        
        private readonly List<SocialLinkConfig> SocialPlatforms = new()
        {
            new() { Platform = "LinkedIn", Label = "LinkedIn", Icon = "💼", Placeholder = "https://linkedin.com/in/..." },
            new() { Platform = "Twitter", Label = "Twitter", Icon = "🐦", Placeholder = "https://twitter.com/..." },
            new() { Platform = "GitHub", Label = "GitHub", Icon = "🐙", Placeholder = "https://github.com/..." },
            new() { Platform = "Facebook", Label = "Facebook", Icon = "📘", Placeholder = "https://facebook.com/..." },
            new() { Platform = "Instagram", Label = "Instagram", Icon = "📷", Placeholder = "https://instagram.com/..." }
        };

        #endregion

        #region Private Fields

        private string? _errorMessage;
        private bool _showDeleteConfirmation;

        #endregion

        #region Lifecycle

        protected override void OnInitialized()
        {
            ActiveSection = InitialSection;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(RequiredPermission))
            {
                var hasPermission = await PermissionService.HasPermissionAsync(RequiredPermission);
                if (!hasPermission && HideIfNoPermission)
                {
                    return;
                }
            }
        }

        #endregion

        #region Section Management

        private void SetActiveSection(ProfileSection section)
        {
            ActiveSection = section;
        }

        private List<ProfileSection> GetVisibleSections()
        {
            return Enum.GetValues<ProfileSection>()
                .Where(s => !HiddenSections.Contains(s))
                .ToList();
        }

        private string GetSectionIcon(ProfileSection section)
        {
            return section switch
            {
                ProfileSection.Personal => "👤",
                ProfileSection.Contact => "📧",
                ProfileSection.Professional => "💼",
                ProfileSection.Social => "🌐",
                ProfileSection.Security => "🔒",
                ProfileSection.Preferences => "⚙️",
                ProfileSection.Advanced => "🔧",
                _ => "📄"
            };
        }

        #endregion

        #region Avatar Management

        private string GetInitials()
        {
            if (Profile == null) return "?";
            
            var initials = "";
            if (!string.IsNullOrEmpty(Profile.FirstName))
                initials += Profile.FirstName[0];
            if (!string.IsNullOrEmpty(Profile.LastName))
                initials += Profile.LastName[0];
            
            return string.IsNullOrEmpty(initials) 
                ? (Profile.UserName.Length > 0 ? Profile.UserName[..Math.Min(2, Profile.UserName.Length)].ToUpper() : "?")
                : initials.ToUpper();
        }

        private async Task HandleAvatarUpload(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;

            // Check file size
            if (file.Size > Options.MaxAvatarSize)
            {
                _errorMessage = $"File size exceeds maximum allowed size of {Options.MaxAvatarSize / 1024 / 1024}MB";
                StateHasChanged();
                return;
            }

            // Check file type
            if (!Options.AllowedAvatarTypes.Contains(file.ContentType))
            {
                _errorMessage = $"File type '{file.ContentType}' is not allowed. Allowed types: {string.Join(", ", Options.AllowedAvatarTypes)}";
                StateHasChanged();
                return;
            }

            var avatarData = new AvatarUploadData
            {
                FileName = file.Name,
                FileSize = file.Size,
                FileType = file.ContentType
            };

            // Read file data
            var buffer = new byte[file.Size];
            await file.OpenReadStream(Options.MaxAvatarSize).ReadAsync(buffer);
            avatarData.FileData = buffer;
            avatarData.Base64Data = Convert.ToBase64String(buffer);

            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "AvatarUpload",
                    Resource = AuditResource ?? "ProfileEditor",
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalData = new Dictionary<string, object> { ["FileName"] = file.Name }
                });
            }

            await OnAvatarUpload.InvokeAsync(avatarData);
        }

        #endregion

        #region Social Links

        private string? GetSocialLink(string platform)
        {
            return Profile?.SocialLinks.TryGetValue(platform, out var link) == true ? link : null;
        }

        private void SetSocialLink(string platform, string? url)
        {
            if (Profile == null) return;

            if (string.IsNullOrEmpty(url))
            {
                Profile.SocialLinks.Remove(platform);
            }
            else
            {
                Profile.SocialLinks[platform] = url;
            }
        }

        #endregion

        #region Password Management

        private void HandlePasswordInput(ChangeEventArgs e)
        {
            var password = e.Value?.ToString() ?? string.Empty;
            _passwordStrength = CalculatePasswordStrength(password);
        }

        private int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;

            int strength = 0;
            
            // Length
            if (password.Length >= 8) strength += 20;
            if (password.Length >= 12) strength += 20;
            
            // Uppercase
            if (password.Any(char.IsUpper)) strength += 20;
            
            // Lowercase
            if (password.Any(char.IsLower)) strength += 20;
            
            // Numbers
            if (password.Any(char.IsDigit)) strength += 10;
            
            // Special characters
            if (password.Any(c => !char.IsLetterOrDigit(c))) strength += 10;

            return Math.Min(strength, 100);
        }

        private int GetPasswordStrength()
        {
            return _passwordStrength;
        }

        private string GetPasswordStrengthLabel()
        {
            return _passwordStrength switch
            {
                < 30 => "Weak",
                < 60 => "Fair",
                < 80 => "Good",
                _ => "Strong"
            };
        }

        private async Task HandlePasswordChange()
        {
            if (string.IsNullOrEmpty(_passwordChange.CurrentPassword) ||
                string.IsNullOrEmpty(_passwordChange.NewPassword) ||
                string.IsNullOrEmpty(_passwordChange.ConfirmPassword))
            {
                _errorMessage = "All password fields are required";
                StateHasChanged();
                return;
            }

            if (_passwordChange.NewPassword != _passwordChange.ConfirmPassword)
            {
                _errorMessage = "New password and confirmation password do not match";
                StateHasChanged();
                return;
            }
            
            // Validate password strength
            if (_passwordChange.NewPassword.Length < 8)
            {
                _errorMessage = "New password must be at least 8 characters long";
                StateHasChanged();
                return;
            }

            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "PasswordChange",
                    Resource = AuditResource ?? "ProfileEditor",
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalData = new Dictionary<string, object> { ["UserName"] = Profile?.UserName ?? "Unknown" }
                });
            }

            await OnPasswordChange.InvokeAsync(_passwordChange);
            
            // Clear password fields
            _passwordChange = new PasswordChangeRequest();
        }

        #endregion

        #region Form Actions

        private async Task HandleSubmit()
        {
            if (Profile == null) return;

            Profile.LastModified = DateTime.UtcNow;

            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "ProfileUpdate",
                    Resource = AuditResource ?? "ProfileEditor",
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalData = new Dictionary<string, object> { ["UserName"] = Profile.UserName ?? "Unknown" }
                });
            }

            await OnSave.InvokeAsync(Profile);
        }

        private async Task HandleCancel()
        {
            await OnCancel.InvokeAsync();
        }

        private async Task HandleDeleteAccount()
        {
            // Show confirmation dialog
            _showDeleteConfirmation = true;
            StateHasChanged();
        }
        
        private async Task ConfirmDeleteAccount()
        {
            _showDeleteConfirmation = false;
            
            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "AccountDelete",
                    Resource = AuditResource ?? "ProfileEditor",
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalData = new Dictionary<string, object> { ["UserName"] = Profile?.UserName ?? "Unknown" }
                });
            }

            await OnDeleteAccount.InvokeAsync();
        }
        
        private void CancelDeleteAccount()
        {
            _showDeleteConfirmation = false;
            StateHasChanged();
        }

        #endregion
    }
}
