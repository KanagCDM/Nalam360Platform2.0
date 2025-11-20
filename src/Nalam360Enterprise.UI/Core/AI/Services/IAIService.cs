using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// Service for AI-powered natural language understanding and generation
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Analyzes the intent of a user message
    /// </summary>
    /// <param name="message">The message to analyze</param>
    /// <param name="endpoint">The AI service endpoint URL</param>
    /// <param name="apiKey">The API key for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Intent analysis result with confidence score and entities</returns>
    Task<IntentAnalysisResult> AnalyzeIntentAsync(
        string message,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes the sentiment of a text
    /// </summary>
    /// <param name="text">The text to analyze</param>
    /// <param name="endpoint">The AI service endpoint URL</param>
    /// <param name="apiKey">The API key for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sentiment result with scores</returns>
    Task<SentimentResult> AnalyzeSentimentAsync(
        string text,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an AI response to a user message
    /// </summary>
    /// <param name="context">The conversation context</param>
    /// <param name="prompt">The user's message/prompt</param>
    /// <param name="endpoint">The AI service endpoint URL</param>
    /// <param name="apiKey">The API key for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI-generated response</returns>
    Task<string> GenerateResponseAsync(
        string context,
        string prompt,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams an AI response token by token for real-time display
    /// </summary>
    /// <param name="context">The conversation context</param>
    /// <param name="prompt">The user's message/prompt</param>
    /// <param name="endpoint">The AI service endpoint URL</param>
    /// <param name="apiKey">The API key for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of response tokens</returns>
    IAsyncEnumerable<string> StreamResponseAsync(
        string context,
        string prompt,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates smart suggestions based on context and intent
    /// </summary>
    /// <param name="context">The conversation context</param>
    /// <param name="intent">The detected intent</param>
    /// <param name="endpoint">The AI service endpoint URL</param>
    /// <param name="apiKey">The API key for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of suggested responses/actions</returns>
    Task<List<string>> GenerateSuggestionsAsync(
        string context,
        string intent,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default);
}
