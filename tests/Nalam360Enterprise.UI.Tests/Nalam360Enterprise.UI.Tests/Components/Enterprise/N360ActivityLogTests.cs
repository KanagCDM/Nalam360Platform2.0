using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Models;
using Xunit;

namespace Nalam360Enterprise.UI.Tests.Components.Enterprise
{
    public class N360ActivityLogTests : TestContext
    {
        public N360ActivityLogTests()
        {
            // Register required services
            Services.AddSingleton<IPermissionService, MockPermissionService>();
            Services.AddSingleton<IAuditService, MockAuditService>();
        }

        [Fact]
        public void ActivityLog_RendersWithoutCrashing()
        {
            // Arrange
            var entries = new List<ActivityLogEntry>
            {
                new() { Type = ActivityLogType.Create, Severity = ActivityLogSeverity.Info, Message = "Test entry" }
            };

            // Act
            var cut = RenderComponent<N360ActivityLog<ActivityLogEntry>>(parameters => parameters
                .Add(p => p.Entries, entries));

            // Assert
            Assert.NotNull(cut);
            cut.MarkupMatches("<div class=\"n360-activity-log\" dir=\"ltr\">*</div>");
        }

        [Fact]
        public void ActivityLog_DisplaysCorrectEntryCount()
        {
            // Arrange
            var entries = new List<ActivityLogEntry>
            {
                new() { Type = ActivityLogType.Create, Severity = ActivityLogSeverity.Info, Message = "Entry 1" },
                new() { Type = ActivityLogType.Update, Severity = ActivityLogSeverity.Warning, Message = "Entry 2" },
                new() { Type = ActivityLogType.Delete, Severity = ActivityLogSeverity.Error, Message = "Entry 3" }
            };

            // Act
            var cut = RenderComponent<N360ActivityLog<ActivityLogEntry>>(parameters => parameters
                .Add(p => p.Entries, entries)
                .Add(p => p.ShowStatistics, true));

            // Assert
            var statsElement = cut.Find(".n360-activity-log__statistics");
            Assert.Contains("3", statsElement.TextContent);
        }

        [Fact]
        public void ActivityLog_FiltersEntriesByType()
        {
            // Arrange
            var entries = new List<ActivityLogEntry>
            {
                new() { Type = ActivityLogType.Create, Message = "Create entry" },
                new() { Type = ActivityLogType.Update, Message = "Update entry" },
                new() { Type = ActivityLogType.Delete, Message = "Delete entry" }
            };

            var filter = new ActivityLogFilter
            {
                Types = new List<ActivityLogType> { ActivityLogType.Create }
            };

            // Act
            var cut = RenderComponent<N360ActivityLog<ActivityLogEntry>>(parameters => parameters
                .Add(p => p.Entries, entries)
                .Add(p => p.Filter, filter));

            // Assert - Should only show 1 filtered entry
            var entryElements = cut.FindAll(".n360-activity-log__entry");
            Assert.Single(entryElements);
            Assert.Contains("Create entry", entryElements[0].TextContent);
        }

        [Fact]
        public void ActivityLog_SwitchesViewModes()
        {
            // Arrange
            var entries = new List<ActivityLogEntry>
            {
                new() { Type = ActivityLogType.Create, Message = "Test" }
            };

            // Act
            var cut = RenderComponent<N360ActivityLog<ActivityLogEntry>>(parameters => parameters
                .Add(p => p.Entries, entries)
                .Add(p => p.ShowViewToggle, true));

            // Timeline button
            var timelineButton = cut.Find("button[title='Timeline View']");
            timelineButton.Click();

            // Assert
            Assert.Contains("n360-activity-log__timeline", cut.Markup);
        }

        [Fact]
        public void ActivityLog_ExportsData()
        {
            // Arrange
            var entries = new List<ActivityLogEntry>
            {
                new() { Type = ActivityLogType.Create, Message = "Export test" }
            };

            var exportCalled = false;
            Action<ActivityLogExportOptions> exportCallback = _ => exportCalled = true;

            // Act
            var cut = RenderComponent<N360ActivityLog<ActivityLogEntry>>(parameters => parameters
                .Add(p => p.Entries, entries)
                .Add(p => p.ShowExport, true)
                .Add(p => p.OnExport, exportCallback));

            var exportButton = cut.Find("button[title='Export']");
            exportButton.Click();

            // Assert
            Assert.True(exportCalled);
        }
    }

    // Mock services for testing
    public class MockPermissionService : IPermissionService
    {
        public Task<bool> HasPermissionAsync(string permission) => Task.FromResult(true);
        public Task<bool> HasAnyPermissionAsync(params string[] permissions) => Task.FromResult(true);
        public Task<bool> HasAllPermissionsAsync(params string[] permissions) => Task.FromResult(true);
        public Task<bool> IsInRoleAsync(string role) => Task.FromResult(true);
        public Task<IEnumerable<string>> GetUserPermissionsAsync() => Task.FromResult<IEnumerable<string>>(new List<string>());
    }

    public class MockAuditService : IAuditService
    {
        public Task LogAsync(AuditMetadata metadata) => Task.CompletedTask;
        public event EventHandler<AuditEventArgs>? AuditEventOccurred;
    }
}
