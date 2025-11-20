using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// Azure OpenAI implementation of the AI service
/// </summary>
public class AzureOpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureOpenAIService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AzureOpenAIService(
        HttpClient httpClient,
        ILogger<AzureOpenAIService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <inheritdoc/>
    public async Task<IntentAnalysisResult> AnalyzeIntentAsync(
        string message,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing intent for message of length {Length}", message.Length);

            var systemPrompt = @"You are a healthcare intent classifier. Analyze the user's message and classify it into one of these categories:
- AppointmentScheduling: Questions about scheduling, rescheduling, or canceling appointments
- PrescriptionInquiry: Questions about medications, prescriptions, refills
- SymptomCheck: Describing symptoms or health concerns
- LabResults: Questions about lab results or test results
- BillingInquiry: Questions about bills, payments, insurance
- EmergencyTriage: Urgent medical concerns requiring immediate attention
- GeneralInquiry: General healthcare questions
- Other: Anything else

Respond with ONLY the category name and a confidence score (0.0-1.0) in JSON format: {""intent"": ""CategoryName"", ""confidence"": 0.95}";

            var request = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = message }
                },
                temperature = 0.3,
                max_tokens = 100
            };

            var response = await SendRequestAsync<OpenAIResponse>(endpoint, apiKey, request, cancellationToken);

            if (response?.Choices != null && response.Choices.Count > 0)
            {
                var content = response.Choices[0].Message?.Content ?? "{}";
                
                // Try to parse the JSON response
                try
                {
                    var intentData = JsonSerializer.Deserialize<IntentResponse>(content, _jsonOptions);
                    
                    return new IntentAnalysisResult
                    {
                        Intent = intentData?.Intent ?? "Other",
                        Confidence = intentData?.Confidence ?? 0.5,
                        AnalyzedAt = DateTime.UtcNow
                    };
                }
                catch (JsonException)
                {
                    // Fallback: treat the entire content as the intent
                    return new IntentAnalysisResult
                    {
                        Intent = content.Trim(),
                        Confidence = 0.7,
                        AnalyzedAt = DateTime.UtcNow
                    };
                }
            }

            return new IntentAnalysisResult
            {
                Intent = "Other",
                Confidence = 0.0,
                ErrorMessage = "No response from AI service"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing intent");
            return new IntentAnalysisResult
            {
                Intent = "Other",
                Confidence = 0.0,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SentimentResult> AnalyzeSentimentAsync(
        string text,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing sentiment for text of length {Length}", text.Length);

            var systemPrompt = @"Analyze the sentiment of the following text. Respond with ONLY a JSON object containing:
- sentiment: Positive, Negative, Neutral, or Mixed
- scores: an object with positive, negative, neutral, and mixed scores (0.0-1.0, totaling 1.0)

Example: {""sentiment"": ""Positive"", ""scores"": {""positive"": 0.8, ""negative"": 0.1, ""neutral"": 0.1, ""mixed"": 0.0}}";

            var request = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = text }
                },
                temperature = 0.3,
                max_tokens = 150
            };

            var response = await SendRequestAsync<OpenAIResponse>(endpoint, apiKey, request, cancellationToken);

            if (response?.Choices != null && response.Choices.Count > 0)
            {
                var content = response.Choices[0].Message?.Content ?? "{}";
                
                try
                {
                    var sentimentData = JsonSerializer.Deserialize<SentimentResponse>(content, _jsonOptions);
                    
                    return new SentimentResult
                    {
                        Sentiment = sentimentData?.Sentiment ?? "Neutral",
                        Confidence = Math.Max(
                            sentimentData?.Scores?.Positive ?? 0,
                            Math.Max(sentimentData?.Scores?.Negative ?? 0, sentimentData?.Scores?.Neutral ?? 0)
                        ),
                        Scores = sentimentData?.Scores ?? new SentimentScores { Neutral = 1.0 },
                        AnalyzedAt = DateTime.UtcNow
                    };
                }
                catch (JsonException)
                {
                    // Fallback to neutral sentiment
                    return new SentimentResult
                    {
                        Sentiment = "Neutral",
                        Confidence = 0.5,
                        Scores = new SentimentScores { Neutral = 1.0 },
                        AnalyzedAt = DateTime.UtcNow
                    };
                }
            }

            return new SentimentResult
            {
                Sentiment = "Neutral",
                Confidence = 0.0,
                ErrorMessage = "No response from AI service"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return new SentimentResult
            {
                Sentiment = "Neutral",
                Confidence = 0.0,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public async Task<string> GenerateResponseAsync(
        string context,
        string prompt,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating AI response for prompt of length {Length}", prompt.Length);

            var systemPrompt = @"You are a helpful healthcare assistant. Provide accurate, empathetic, and professional responses to healthcare-related questions. 
Keep responses concise and actionable. Always remind users to consult with healthcare professionals for medical advice.";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Add context if provided
            if (!string.IsNullOrWhiteSpace(context))
            {
                messages.Add(new { role = "assistant", content = $"Context: {context}" });
            }

            messages.Add(new { role = "user", content = prompt });

            var request = new
            {
                messages = messages.ToArray(),
                temperature = 0.7,
                max_tokens = 500
            };

            var response = await SendRequestAsync<OpenAIResponse>(endpoint, apiKey, request, cancellationToken);

            if (response?.Choices != null && response.Choices.Count > 0)
            {
                return response.Choices[0].Message?.Content ?? "I apologize, but I couldn't generate a response at this time.";
            }

            return "I apologize, but I couldn't generate a response at this time.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response");
            return "I apologize, but an error occurred while generating a response.";
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> StreamResponseAsync(
        string context,
        string prompt,
        string endpoint,
        string apiKey,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Streaming AI response for prompt of length {Length}", prompt.Length);

        var systemPrompt = @"You are a helpful healthcare assistant. Provide accurate, empathetic, and professional responses to healthcare-related questions.";

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (!string.IsNullOrWhiteSpace(context))
        {
            messages.Add(new { role = "assistant", content = $"Context: {context}" });
        }

        messages.Add(new { role = "user", content = prompt });

        var request = new
        {
            messages = messages.ToArray(),
            temperature = 0.7,
            max_tokens = 500,
            stream = true
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Add("api-key", apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var data = line.Substring(6).Trim();
            
            if (data == "[DONE]")
                break;

            try
            {
                var streamResponse = JsonSerializer.Deserialize<OpenAIStreamResponse>(data, _jsonOptions);
                var content = streamResponse?.Choices?[0]?.Delta?.Content;
                
                if (!string.IsNullOrEmpty(content))
                {
                    yield return content;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error parsing stream response: {Data}", data);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GenerateSuggestionsAsync(
        string context,
        string intent,
        string endpoint,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating suggestions for intent: {Intent}", intent);

            var systemPrompt = $@"Based on the conversation context and detected intent '{intent}', generate 3-5 helpful quick response suggestions. 
Respond with ONLY a JSON array of strings. Example: [""Check my appointment"", ""Request prescription refill"", ""View lab results""]";

            var request = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = context }
                },
                temperature = 0.5,
                max_tokens = 200
            };

            var response = await SendRequestAsync<OpenAIResponse>(endpoint, apiKey, request, cancellationToken);

            if (response?.Choices != null && response.Choices.Count > 0)
            {
                var content = response.Choices[0].Message?.Content ?? "[]";
                
                try
                {
                    var suggestions = JsonSerializer.Deserialize<List<string>>(content, _jsonOptions);
                    return suggestions ?? new List<string>();
                }
                catch (JsonException)
                {
                    // Return empty list if parsing fails
                    return new List<string>();
                }
            }

            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating suggestions");
            return new List<string>();
        }
    }

    private async Task<T?> SendRequestAsync<T>(
        string endpoint,
        string apiKey,
        object request,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Add("api-key", apiKey);
        httpRequest.Content = JsonContent.Create(request, options: _jsonOptions);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
    }

    // Response model classes
    private class OpenAIResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

    private class OpenAIStreamResponse
    {
        public List<StreamChoice>? Choices { get; set; }
    }

    private class StreamChoice
    {
        public Delta? Delta { get; set; }
    }

    private class Delta
    {
        public string? Content { get; set; }
    }

    private class IntentResponse
    {
        public string Intent { get; set; } = "Other";
        public double Confidence { get; set; }
    }

    private class SentimentResponse
    {
        public string Sentiment { get; set; } = "Neutral";
        public SentimentScores? Scores { get; set; }
    }
}
