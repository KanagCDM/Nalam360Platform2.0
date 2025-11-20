namespace Nalam360Enterprise.UI.Components.Input;

public class MentionEventArgs
{
    public object Suggestion { get; }
    public string Value { get; }
    public string Display { get; }

    public MentionEventArgs(object suggestion, string value, string display)
    {
        Suggestion = suggestion;
        Value = value;
        Display = display;
    }
}
