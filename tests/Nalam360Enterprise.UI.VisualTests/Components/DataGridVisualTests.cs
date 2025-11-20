using Microsoft.Playwright;
using NUnit.Framework;

namespace Nalam360Enterprise.UI.VisualTests.Components;

[TestFixture]
public class DataGridVisualTests : PlaywrightTestBase
{
    [Test]
    public async Task DataGrid_DefaultState_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        await HideDynamicElementsAsync();
        
        var grid = Page.Locator("[data-testid='grid-default']");
        await Expect(grid).ToBeVisibleAsync();
        
        // Wait for grid to render
        await Page.WaitForSelectorAsync(".e-grid .e-gridcontent");
        
        await AssertVisualMatchAsync("datagrid-default", grid);
    }

    [Test]
    public async Task DataGrid_WithPaging_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        
        var grid = Page.Locator("[data-testid='grid-paging']");
        await Expect(grid).ToBeVisibleAsync();
        
        // Wait for pager to render
        await Page.WaitForSelectorAsync(".e-gridpager");
        
        await AssertVisualMatchAsync("datagrid-paging", grid);
    }

    [Test]
    public async Task DataGrid_WithFiltering_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        
        var grid = Page.Locator("[data-testid='grid-filtering']");
        
        // Click filter icon
        await grid.Locator(".e-filterbar").First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);
        
        await AssertVisualMatchAsync("datagrid-filtering", grid);
    }

    [Test]
    public async Task DataGrid_WithSorting_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        
        var grid = Page.Locator("[data-testid='grid-sorting']");
        
        // Click column header to sort
        await grid.Locator(".e-headercell").First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);
        
        await AssertVisualMatchAsync("datagrid-sorting", grid);
    }

    [Test]
    public async Task DataGrid_WithGrouping_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        
        var grid = Page.Locator("[data-testid='grid-grouping']");
        await Expect(grid).ToBeVisibleAsync();
        
        // Wait for grouped rows
        await Page.WaitForSelectorAsync(".e-groupcaption");
        
        await AssertVisualMatchAsync("datagrid-grouping", grid);
    }

    [Test]
    public async Task DataGrid_RowSelection_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        
        var grid = Page.Locator("[data-testid='grid-selection']");
        
        // Select first row
        await grid.Locator(".e-row").First.ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        
        await AssertVisualMatchAsync("datagrid-row-selected", grid);
    }

    [Test]
    public async Task DataGrid_EmptyState_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        
        var grid = Page.Locator("[data-testid='grid-empty']");
        await Expect(grid).ToBeVisibleAsync();
        
        await AssertVisualMatchAsync("datagrid-empty", grid);
    }

    [Test]
    public async Task DataGrid_WithToolbar_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/datagrid");
        
        var grid = Page.Locator("[data-testid='grid-toolbar']");
        
        // Wait for toolbar
        await Page.WaitForSelectorAsync(".e-toolbar");
        
        await AssertVisualMatchAsync("datagrid-toolbar", grid);
    }

    [Test]
    public async Task DataGrid_Responsive_MatchesBaseline()
    {
        await TestResponsiveAsync("components/datagrid", "datagrid-responsive");
    }

    [Test]
    public async Task DataGrid_AllThemes_MatchBaseline()
    {
        await TestAllThemesAsync("components/datagrid", "datagrid-themes");
    }
}
