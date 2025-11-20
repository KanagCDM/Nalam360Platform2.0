using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Models;
using Xunit;

namespace Nalam360Enterprise.UI.Tests.Components.Enterprise
{
    public class N360ProfileEditorTests : TestContext
    {
        public N360ProfileEditorTests()
        {
            Services.AddSingleton<IPermissionService, MockPermissionService>();
            Services.AddSingleton<IAuditService, MockAuditService>();
        }

        [Fact]
        public void ProfileEditor_RendersWithoutCrashing()
        {
            // Arrange
            var profile = new UserProfile
            {
                Id = "user1",
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var cut = RenderComponent<N360ProfileEditor<UserProfile>>(parameters => parameters
                .Add(p => p.Profile, profile));

            // Assert
            Assert.NotNull(cut);
            Assert.Contains("n360-profile-editor", cut.Markup);
        }

        [Fact]
        public void ProfileEditor_DisplaysUserInfo()
        {
            // Arrange
            var profile = new UserProfile
            {
                UserName = "johndoe",
                Email = "john@example.com",
                DisplayName = "John Doe",
                FirstName = "John",
                LastName = "Doe",
                EmailVerified = true
            };

            // Act
            var cut = RenderComponent<N360ProfileEditor<UserProfile>>(parameters => parameters
                .Add(p => p.Profile, profile)
                .Add(p => p.ShowAvatar, true));

            // Assert
            Assert.Contains("John Doe", cut.Markup);
            Assert.Contains("john@example.com", cut.Markup);
            Assert.Contains("Verified", cut.Markup);
        }

        [Fact]
        public void ProfileEditor_ShowsAllSections()
        {
            // Arrange
            var profile = new UserProfile { UserName = "test" };

            // Act
            var cut = RenderComponent<N360ProfileEditor<UserProfile>>(parameters => parameters
                .Add(p => p.Profile, profile)
                .Add(p => p.ShowTabs, true));

            // Assert
            var tabs = cut.FindAll(".n360-profile-editor__tab");
            Assert.True(tabs.Count >= 6); // Personal, Contact, Professional, Social, Security, Preferences
        }

        [Fact]
        public void ProfileEditor_HandlesAvatarInitials()
        {
            // Arrange
            var profile = new UserProfile
            {
                UserName = "jdoe",
                FirstName = "Jane",
                LastName = "Doe"
            };

            // Act
            var cut = RenderComponent<N360ProfileEditor<UserProfile>>(parameters => parameters
                .Add(p => p.Profile, profile)
                .Add(p => p.ShowAvatar, true));

            // Assert
            var avatarPlaceholder = cut.Find(".n360-profile-editor__avatar-placeholder");
            Assert.Contains("JD", avatarPlaceholder.TextContent);
        }

        [Fact]
        public void ProfileEditor_ValidatesPasswordStrength()
        {
            // Arrange
            var profile = new UserProfile { UserName = "test" };

            // Act
            var cut = RenderComponent<N360ProfileEditor<UserProfile>>(parameters => parameters
                .Add(p => p.Profile, profile)
                .Add(p => p.ShowPasswordChange, true)
                .Add(p => p.InitialSection, ProfileSection.Security));

            // Change to Security section first
            var securityTab = cut.Find("button:contains('Security')");
            securityTab?.Click();

            var passwordInput = cut.Find("input[type='password']");
            passwordInput.Input("Test123!");

            // Assert
            Assert.Contains("n360-profile-editor__password-strength", cut.Markup);
        }

        [Fact]
        public void ProfileEditor_SaveTriggersCallback()
        {
            // Arrange
            var profile = new UserProfile
            {
                UserName = "test",
                Email = "test@example.com"
            };

            var savedProfile = (UserProfile?)null;
            Action<UserProfile> saveCallback = p => savedProfile = p;

            // Act
            var cut = RenderComponent<N360ProfileEditor<UserProfile>>(parameters => parameters
                .Add(p => p.Profile, profile)
                .Add(p => p.OnSave, saveCallback)
                .Add(p => p.ShowSaveButton, true));

            var saveButton = cut.Find("button[type='submit']");
            saveButton.Click();

            // Assert
            Assert.NotNull(savedProfile);
            Assert.Equal("test", savedProfile.UserName);
        }

        [Fact]
        public void ProfileEditor_ReadOnlyMode()
        {
            // Arrange
            var profile = new UserProfile { UserName = "test" };

            // Act
            var cut = RenderComponent<N360ProfileEditor<UserProfile>>(parameters => parameters
                .Add(p => p.Profile, profile)
                .Add(p => p.IsReadOnly, true));

            // Assert
            var inputs = cut.FindAll("input");
            Assert.All(inputs, input => Assert.True(input.HasAttribute("disabled")));
        }
    }
}
