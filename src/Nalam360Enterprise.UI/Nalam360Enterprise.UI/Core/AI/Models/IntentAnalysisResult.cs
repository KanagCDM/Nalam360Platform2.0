namespace Nalam360Enterprise.UI.Core.AI.Models;

public class IntentAnalysisResult
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object>? Entities { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = true;
}
