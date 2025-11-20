namespace Nalam360Enterprise.UI.Core.AI.Models;

/// <summary>
/// Context for AI service requests
/// </summary>
public class AIRequestContext
{
    /// <summary>
    /// The user's prompt or message
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// The user ID making the request
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The type of resource being accessed
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// Maximum tokens to generate in response
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for response generation (0.0 to 1.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Additional context or conversation history
    /// </summary>
    public string? ConversationHistory { get; set; }

    /// <summary>
    /// System prompt or instructions
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Additional metadata for the request
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
