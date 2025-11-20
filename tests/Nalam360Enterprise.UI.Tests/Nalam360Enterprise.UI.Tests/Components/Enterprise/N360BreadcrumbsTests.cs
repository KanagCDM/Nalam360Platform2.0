using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Models;
using Xunit;

namespace Nalam360Enterprise.UI.Tests.Components.Enterprise
{
    public class N360BreadcrumbsTests : TestContext
    {
        public N360BreadcrumbsTests()
        {
            Services.AddSingleton<IPermissionService, MockPermissionService>();
            Services.AddSingleton<IAuditService, MockAuditService>();
        }

        [Fact]
        public void Breadcrumbs_RendersWithoutCrashing()
        {
            // Arrange
            var items = new List<BreadcrumbItem>
            {
                new() { Name = "Home", Url = "/" },
                new() { Name = "Products", Url = "/products" },
                new() { Name = "Details", Url = "/products/1", IsActive = true }
            };

            // Act
            var cut = RenderComponent<N360Breadcrumbs>(parameters => parameters
                .Add(p => p.Items, items));

            // Assert
            Assert.NotNull(cut);
            Assert.Contains("n360-breadcrumbs", cut.Markup);
        }

        [Fact]
        public void Breadcrumbs_DisplaysAllItems()
        {
            // Arrange
            var items = new List<BreadcrumbItem>
            {
                new() { Name = "Home", Url = "/" },
                new() { Name = "Category", Url = "/category" },
                new() { Name = "Subcategory", Url = "/category/sub" }
            };

            // Act
            var cut = RenderComponent<N360Breadcrumbs>(parameters => parameters
                .Add(p => p.Items, items));

            // Assert
            Assert.Contains("Home", cut.Markup);
            Assert.Contains("Category", cut.Markup);
            Assert.Contains("Subcategory", cut.Markup);
        }

        [Fact]
        public void Breadcrumbs_ShowsHomIcon()
        {
            // Arrange
            var items = new List<BreadcrumbItem>
            {
                new() { Name = "Home", Url = "/" },
                new() { Name = "Page", Url = "/page" }
            };

            var options = new BreadcrumbOptions
            {
                ShowHome = true,
                HomeIcon = "🏠",
                HomeUrl = "/"
            };

            // Act
            var cut = RenderComponent<N360Breadcrumbs>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Options, options));

            // Assert
            Assert.Contains("🏠", cut.Markup);
        }

        [Fact]
        public void Breadcrumbs_AutoCollapsesLongPaths()
        {
            // Arrange
            var items = new List<BreadcrumbItem>
            {
                new() { Name = "Home", Url = "/" },
                new() { Name = "Level 1", Url = "/l1" },
                new() { Name = "Level 2", Url = "/l1/l2" },
                new() { Name = "Level 3", Url = "/l1/l2/l3" },
                new() { Name = "Level 4", Url = "/l1/l2/l3/l4" },
                new() { Name = "Current", Url = "/l1/l2/l3/l4/current", IsActive = true }
            };

            var options = new BreadcrumbOptions
            {
                AutoCollapse = true,
                MaxVisibleItems = 3
            };

            // Act
            var cut = RenderComponent<N360Breadcrumbs>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Options, options));

            // Assert
            Assert.Contains("...", cut.Markup); // Ellipsis for collapsed items
        }

        [Fact]
        public void Breadcrumbs_ClickTriggersCallback()
        {
            // Arrange
            var items = new List<BreadcrumbItem>
            {
                new() { Name = "Home", Url = "/" },
                new() { Name = "Page", Url = "/page" }
            };

            BreadcrumbItem? clickedItem = null;
            Action<BreadcrumbItem> clickCallback = item => clickedItem = item;

            // Act
            var cut = RenderComponent<N360Breadcrumbs>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.OnItemClicked, clickCallback));

            var link = cut.Find("a:contains('Home')");
            link.Click();

            // Assert
            Assert.NotNull(clickedItem);
            Assert.Equal("Home", clickedItem.Name);
        }

        [Fact]
        public void Breadcrumbs_HandlesDisabledItems()
        {
            // Arrange
            var items = new List<BreadcrumbItem>
            {
                new() { Name = "Home", Url = "/", IsDisabled = true },
                new() { Name = "Page", Url = "/page" }
            };

            // Act
            var cut = RenderComponent<N360Breadcrumbs>(parameters => parameters
                .Add(p => p.Items, items));

            // Assert
            var disabledLink = cut.Find(".n360-breadcrumbs__item--disabled");
            Assert.NotNull(disabledLink);
        }
    }
}
