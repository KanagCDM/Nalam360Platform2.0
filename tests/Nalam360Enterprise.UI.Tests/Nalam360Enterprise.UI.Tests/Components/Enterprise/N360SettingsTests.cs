using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Models;
using Xunit;

namespace Nalam360Enterprise.UI.Tests.Components.Enterprise
{
    public class N360SettingsTests : TestContext
    {
        public N360SettingsTests()
        {
            Services.AddSingleton<IPermissionService, MockPermissionService>();
            Services.AddSingleton<IAuditService, MockAuditService>();
        }

        [Fact]
        public void Settings_RendersWithoutCrashing()
        {
            // Arrange
            var config = new SettingsConfiguration
            {
                Sections = new List<SettingsSection>
                {
                    new() 
                    { 
                        Id = "general",
                        Name = "General",
                        Items = new List<SettingItem>
                        {
                            new() { Id = "siteName", Name = "Site Name", Type = SettingType.Text, Value = "Test" }
                        }
                    }
                }
            };

            // Act
            var cut = RenderComponent<N360Settings>(parameters => parameters
                .Add(p => p.Configuration, config));

            // Assert
            Assert.NotNull(cut);
            Assert.Contains("n360-settings", cut.Markup);
        }

        [Fact]
        public void Settings_DisplaysAllSections()
        {
            // Arrange
            var config = new SettingsConfiguration
            {
                Sections = new List<SettingsSection>
                {
                    new() { Id = "section1", Name = "Section 1", Items = new() },
                    new() { Id = "section2", Name = "Section 2", Items = new() },
                    new() { Id = "section3", Name = "Section 3", Items = new() }
                }
            };

            // Act
            var cut = RenderComponent<N360Settings>(parameters => parameters
                .Add(p => p.Configuration, config)
                .Add(p => p.ShowTabs, true));

            // Assert
            var tabs = cut.FindAll(".n360-settings__tab");
            Assert.Equal(3, tabs.Count);
        }

        [Fact]
        public void Settings_HandlesTextInput()
        {
            // Arrange
            var config = new SettingsConfiguration
            {
                Sections = new List<SettingsSection>
                {
                    new()
                    {
                        Id = "general",
                        Name = "General",
                        Items = new List<SettingItem>
                        {
                            new() { Id = "name", Name = "Name", Type = SettingType.Text, Value = "Initial" }
                        }
                    }
                }
            };

            // Act
            var cut = RenderComponent<N360Settings>(parameters => parameters
                .Add(p => p.Configuration, config));

            var input = cut.Find("input[type='text']");
            input.Change("Updated Value");

            // Assert
            Assert.Equal("Updated Value", input.GetAttribute("value"));
        }

        [Fact]
        public void Settings_ValidatesRequiredFields()
        {
            // Arrange
            var config = new SettingsConfiguration
            {
                Sections = new List<SettingsSection>
                {
                    new()
                    {
                        Id = "general",
                        Name = "General",
                        Items = new List<SettingItem>
                        {
                            new() 
                            { 
                                Id = "email",
                                Name = "Email",
                                Type = SettingType.Email,
                                Value = "",
                                ValidationRules = new SettingsValidationRules { Required = true }
                            }
                        }
                    }
                }
            };

            // Act
            var cut = RenderComponent<N360Settings>(parameters => parameters
                .Add(p => p.Configuration, config)
                .Add(p => p.ValidateOnChange, true));

            var saveButton = cut.Find("button[type='submit']");
            saveButton.Click();

            // Assert
            Assert.Contains("n360-settings__validation-error", cut.Markup);
        }

        [Fact]
        public void Settings_SearchFiltersItems()
        {
            // Arrange
            var config = new SettingsConfiguration
            {
                Sections = new List<SettingsSection>
                {
                    new()
                    {
                        Id = "general",
                        Name = "General",
                        Items = new List<SettingItem>
                        {
                            new() { Id = "name", Name = "Site Name", Type = SettingType.Text },
                            new() { Id = "desc", Name = "Description", Type = SettingType.Text },
                            new() { Id = "email", Name = "Contact Email", Type = SettingType.Email }
                        }
                    }
                }
            };

            // Act
            var cut = RenderComponent<N360Settings>(parameters => parameters
                .Add(p => p.Configuration, config)
                .Add(p => p.ShowSearch, true));

            var searchInput = cut.Find(".n360-settings__search input");
            searchInput.Input("email");

            // Assert - Only email setting should be visible
            var visibleItems = cut.FindAll(".n360-settings__item:not([style*='display: none'])");
            Assert.Single(visibleItems);
        }
    }
}
