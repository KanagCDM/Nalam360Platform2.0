namespace Nalam360Enterprise.UI.Core.AI.Models;

/// <summary>
/// Represents the result of sentiment analysis
/// </summary>
public class SentimentResult
{
    /// <summary>
    /// The detected sentiment (Positive, Negative, Neutral, Mixed)
    /// </summary>
    public string Sentiment { get; set; } = "Neutral";

    /// <summary>
    /// Confidence score for the sentiment (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Detailed sentiment scores
    /// </summary>
    public SentimentScores Scores { get; set; } = new();

    /// <summary>
    /// Timestamp when the analysis was performed
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional error message if analysis failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the analysis was successful
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}

/// <summary>
/// Detailed sentiment scores
/// </summary>
public class SentimentScores
{
    /// <summary>
    /// Positive sentiment score (0.0 to 1.0)
    /// </summary>
    public double Positive { get; set; }

    /// <summary>
    /// Negative sentiment score (0.0 to 1.0)
    /// </summary>
    public double Negative { get; set; }

    /// <summary>
    /// Neutral sentiment score (0.0 to 1.0)
    /// </summary>
    public double Neutral { get; set; }

    /// <summary>
    /// Mixed sentiment score (0.0 to 1.0)
    /// </summary>
    public double Mixed { get; set; }
}
