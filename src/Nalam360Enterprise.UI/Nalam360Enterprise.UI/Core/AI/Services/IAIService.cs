using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// AI service interface for natural language processing and generation
/// </summary>
public interface IAIService
{
    Task<IntentAnalysisResult> AnalyzeIntentAsync(string message, CancellationToken cancellationToken = default);
    Task<SentimentResult> AnalyzeSentimentAsync(string message, CancellationToken cancellationToken = default);
    Task<string> GenerateResponseAsync(string context, string message, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamResponseAsync(string context, string message, CancellationToken cancellationToken = default);
    Task<List<string>> GenerateSuggestionsAsync(string context, string? intent = null, CancellationToken cancellationToken = default);
}
