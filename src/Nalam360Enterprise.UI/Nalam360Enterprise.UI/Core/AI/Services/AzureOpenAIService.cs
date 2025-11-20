using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// Azure OpenAI service implementation for natural language understanding and generation
/// </summary>
public class AzureOpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAIService>? _logger;

    public AzureOpenAIService(
        HttpClient httpClient,
        string endpoint,
        string apiKey,
        string deploymentName = "gpt-4",
        ILogger<AzureOpenAIService>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _deploymentName = deploymentName;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
    }

    public async Task<IntentAnalysisResult> AnalyzeIntentAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $@"Analyze the following healthcare message and classify its intent. Respond with ONLY a JSON object.

Message: ""{message}""

Possible intents:
- AppointmentScheduling: Scheduling, rescheduling, or canceling appointments
- PrescriptionInquiry: Questions about medications, refills, dosages
- SymptomCheck: Describing symptoms or health concerns
- LabResults: Inquiries about test results
- BillingInquiry: Questions about bills, insurance, payments
- EmergencyTriage: Urgent or emergency situations
- GeneralInquiry: General questions or other topics

Response format:
{{
  ""intent"": ""<intent name>"",
  ""confidence"": <0.0-1.0>,
  ""entities"": {{
    ""key"": ""value""
  }}
}}";

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a healthcare intent classification assistant. Respond only with valid JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 150
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-15-preview",
                requestBody,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken);
            var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "{}";

            // Parse JSON response
            var intentResult = JsonSerializer.Deserialize<IntentJsonResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new IntentAnalysisResult
            {
                Intent = intentResult?.Intent ?? "GeneralInquiry",
                Confidence = intentResult?.Confidence ?? 0.5,
                Entities = intentResult?.Entities,
                Timestamp = DateTime.UtcNow,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error analyzing intent for message: {Message}", message);
            return new IntentAnalysisResult
            {
                Intent = "GeneralInquiry",
                Confidence = 0.0,
                Timestamp = DateTime.UtcNow,
                IsSuccess = false
            };
        }
    }

    public async Task<SentimentResult> AnalyzeSentimentAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $@"Analyze the sentiment of this healthcare message. Respond with ONLY a JSON object.

Message: ""{message}""

Response format:
{{
  ""sentiment"": ""Positive|Negative|Neutral|Mixed"",
  ""confidence"": <0.0-1.0>,
  ""scores"": {{
    ""positive"": <0.0-1.0>,
    ""negative"": <0.0-1.0>,
    ""neutral"": <0.0-1.0>,
    ""mixed"": <0.0-1.0>
  }}
}}";

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a sentiment analysis assistant. Respond only with valid JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 150
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-15-preview",
                requestBody,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken);
            var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "{}";

            var sentimentResult = JsonSerializer.Deserialize<SentimentJsonResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new SentimentResult
            {
                Sentiment = sentimentResult?.Sentiment ?? "Neutral",
                Confidence = sentimentResult?.Confidence ?? 0.5,
                SentimentScores = sentimentResult?.Scores ?? new Dictionary<string, double>
                {
                    { "positive", 0.33 },
                    { "negative", 0.33 },
                    { "neutral", 0.34 },
                    { "mixed", 0.0 }
                },
                Timestamp = DateTime.UtcNow,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error analyzing sentiment for message: {Message}", message);
            return new SentimentResult
            {
                Sentiment = "Neutral",
                Confidence = 0.0,
                Timestamp = DateTime.UtcNow,
                IsSuccess = false
            };
        }
    }

    public async Task<string> GenerateResponseAsync(string context, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $@"You are a helpful healthcare assistant. Provide a professional, empathetic response.

Previous conversation:
{context}

Current message: ""{message}""

Respond professionally and address the user's concern.";

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a professional healthcare assistant. Be helpful, empathetic, and concise." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 300
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-15-preview",
                requestBody,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken);
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "I'm here to help. How can I assist you?";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating response for message: {Message}", message);
            return "I apologize, but I'm having trouble processing your request. Please try again.";
        }
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        string context,
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var prompt = $@"You are a helpful healthcare assistant. Provide a professional, empathetic response.

Previous conversation:
{context}

Current message: ""{message}""

Respond professionally and address the user's concern.";

        var requestBody = new
        {
            messages = new[]
            {
                new { role = "system", content = "You are a professional healthcare assistant. Be helpful, empathetic, and concise." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 300,
            stream = true
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-15-preview")
        {
            Content = content
        };

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;
        
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
                if (line.Contains("[DONE]")) break;

                var data = line.Substring(6); // Remove "data: " prefix
                
                OpenAIStreamResponse? streamResponse = null;
                try
                {
                    streamResponse = JsonSerializer.Deserialize<OpenAIStreamResponse>(data);
                }
                catch
                {
                    // Skip malformed JSON chunks
                    continue;
                }
                
                var token = streamResponse?.Choices?.FirstOrDefault()?.Delta?.Content;
                if (!string.IsNullOrEmpty(token))
                {
                    yield return token;
                }
            }
        }
        finally
        {
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();
        }
    }

    public async Task<List<string>> GenerateSuggestionsAsync(
        string context,
        string? intent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $@"Generate 3 helpful suggestions for the user based on the conversation and intent.

Intent: {intent ?? "General"}
Context: {context}

Respond with ONLY a JSON array of 3 suggestion strings.
Example: [""Suggestion 1"", ""Suggestion 2"", ""Suggestion 3""]";

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant. Respond only with a JSON array of strings." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 150
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-15-preview",
                requestBody,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken);
            var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "[]";

            var suggestions = JsonSerializer.Deserialize<List<string>>(content);
            return suggestions ?? new List<string> { "How can I help you?", "Tell me more", "Any other questions?" };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating suggestions");
            return new List<string> { "How can I help you?", "Tell me more", "Any other questions?" };
        }
    }

    #region Internal Models

    private class OpenAIResponse
    {
        public List<OpenAIChoice>? Choices { get; set; }
    }

    private class OpenAIChoice
    {
        public OpenAIMessage? Message { get; set; }
    }

    private class OpenAIMessage
    {
        public string? Content { get; set; }
    }

    private class OpenAIStreamResponse
    {
        public List<OpenAIStreamChoice>? Choices { get; set; }
    }

    private class OpenAIStreamChoice
    {
        public OpenAIStreamDelta? Delta { get; set; }
    }

    private class OpenAIStreamDelta
    {
        public string? Content { get; set; }
    }

    private class IntentJsonResponse
    {
        public string? Intent { get; set; }
        public double Confidence { get; set; }
        public Dictionary<string, object>? Entities { get; set; }
    }

    private class SentimentJsonResponse
    {
        public string? Sentiment { get; set; }
        public double Confidence { get; set; }
        public Dictionary<string, double>? Scores { get; set; }
    }

    #endregion
}
