namespace Nalam360Enterprise.UI.Core.AI.Models;

public class PHIElement
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
    public string? SuggestedReplacement { get; set; }
}
