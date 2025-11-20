namespace Nalam360Enterprise.UI.VisualTests.Components;

[TestFixture]
public class FormVisualTests : VisualTestBase
{
    [Test]
    public async Task TextBox_DefaultState_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/textbox");
        await WaitForComponentAsync(".n360-textbox");
        await AssertVisualAsync("textbox_default", ".n360-textbox");
    }

    [Test]
    public async Task TextBox_WithValue_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/textbox");
        var input = await Page.QuerySelectorAsync(".n360-textbox input");
        await input!.FillAsync("Test Value");
        await Task.Delay(300);
        await AssertVisualAsync("textbox_with_value", ".n360-textbox");
    }

    [Test]
    public async Task TextBox_FocusState_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/textbox");
        var input = await Page.QuerySelectorAsync(".n360-textbox input");
        await input!.FocusAsync();
        await Task.Delay(300);
        await AssertVisualAsync("textbox_focused", ".n360-textbox");
    }

    [Test]
    public async Task TextBox_ErrorState_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/textbox?error=true");
        await WaitForComponentAsync(".n360-textbox");
        await AssertVisualAsync("textbox_error", ".n360-textbox");
    }

    [Test]
    public async Task TextBox_DisabledState_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/textbox?disabled=true");
        await WaitForComponentAsync(".n360-textbox");
        await AssertVisualAsync("textbox_disabled", ".n360-textbox");
    }

    [Test]
    public async Task DatePicker_DefaultState_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/datepicker");
        await WaitForComponentAsync(".e-datepicker");
        await AssertVisualAsync("datepicker_default", ".e-datepicker");
    }

    [Test]
    public async Task DatePicker_OpenCalendar_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/datepicker");
        var datePickerButton = await Page.QuerySelectorAsync(".e-input-group-icon");
        await datePickerButton!.ClickAsync();
        await WaitForComponentAsync(".e-calendar");
        await Task.Delay(500);
        await AssertVisualAsync("datepicker_calendar", ".e-calendar");
    }

    [Test]
    public async Task DropDown_DefaultState_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/dropdown");
        await WaitForComponentAsync(".e-dropdownlist");
        await AssertVisualAsync("dropdown_default", ".e-dropdownlist");
    }

    [Test]
    public async Task DropDown_OpenPopup_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/dropdown");
        var dropdown = await Page.QuerySelectorAsync(".e-dropdownlist");
        await dropdown!.ClickAsync();
        await WaitForComponentAsync(".e-popup");
        await Task.Delay(500);
        await AssertVisualAsync("dropdown_open", ".e-popup");
    }

    [Test]
    public async Task Form_ValidationErrors_MatchesBaseline()
    {
        await Page.GotoAsync($"{BaseUrl}/components/form");
        
        // Submit form without filling required fields
        var submitButton = await Page.QuerySelectorAsync("button[type='submit']");
        await submitButton!.ClickAsync();
        await Task.Delay(500);
        
        await AssertVisualAsync("form_validation_errors", ".n360-form");
    }
}
