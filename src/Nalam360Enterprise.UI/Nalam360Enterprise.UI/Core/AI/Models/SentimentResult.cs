namespace Nalam360Enterprise.UI.Core.AI.Models;

public class SentimentResult
{
    public string Sentiment { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> SentimentScores { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = true;
}
