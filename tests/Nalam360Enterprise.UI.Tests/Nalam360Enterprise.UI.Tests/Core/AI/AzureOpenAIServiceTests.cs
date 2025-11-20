using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Tests.Core.AI;

public class AzureOpenAIServiceTests
{
    private readonly Mock<ILogger<AzureOpenAIService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly string _endpoint = "https://test-openai.openai.azure.com";
    private readonly string _apiKey = "test-api-key";
    private readonly string _deploymentName = "gpt-4";

    public AzureOpenAIServiceTests()
    {
        _mockLogger = new Mock<ILogger<AzureOpenAIService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    [Fact]
    public async Task AnalyzeIntentAsync_WithValidMessage_ReturnsIntentAnalysisResult()
    {
        // Arrange
        var message = "I need to schedule an appointment for next week";
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""choices"": [{
                    ""message"": {
                        ""content"": ""{\""intent\"": \""AppointmentScheduling\"", \""confidence\"": 0.95, \""entities\"": {\""timeframe\"": \""next week\""}}\""
                    }
                }]
            }", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var result = await service.AnalyzeIntentAsync(message, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Intent.Should().Be("AppointmentScheduling");
        result.Confidence.Should().Be(0.95);
        result.Entities.Should().ContainKey("timeframe");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AnalyzeIntentAsync_WithEmptyMessage_ReturnsFallbackResult()
    {
        // Arrange
        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var result = await service.AnalyzeIntentAsync("", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Intent.Should().Be("GeneralInquiry");
        result.Confidence.Should().BeLessThan(1.0);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_WithValidMessage_ReturnsSentimentResult()
    {
        // Arrange
        var message = "I'm very happy with the care I received!";
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""choices"": [{
                    ""message"": {
                        ""content"": ""{\""sentiment\"": \""positive\"", \""confidence\"": 0.92, \""scores\"": {\""positive\"": 0.92, \""negative\"": 0.03, \""neutral\"": 0.03, \""mixed\"": 0.02}}\""
                    }
                }]
            }", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var result = await service.AnalyzeSentimentAsync(message, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Sentiment.Should().Be("positive");
        result.Confidence.Should().Be(0.92);
        result.SentimentScores.Should().ContainKey("positive");
        result.SentimentScores["positive"].Should().Be(0.92);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithContextAndMessage_ReturnsResponse()
    {
        // Arrange
        var context = "Patient is asking about lab results";
        var message = "When will my blood test results be ready?";
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""choices"": [{
                    ""message"": {
                        ""content"": ""Your blood test results are typically available within 24-48 hours. I'll check the status for you.""
                    }
                }]
            }", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var result = await service.GenerateResponseAsync(context, message, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("blood test results");
    }

    [Fact]
    public async Task GenerateSuggestionsAsync_WithContext_ReturnsSuggestions()
    {
        // Arrange
        var context = "Patient needs prescription refill";
        var intent = "PrescriptionInquiry";
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""choices"": [{
                    ""message"": {
                        ""content"": ""1. Request prescription refill\n2. Check medication availability\n3. Contact pharmacy""
                    }
                }]
            }", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var result = await service.GenerateSuggestionsAsync(context, intent, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        result.Should().Contain(s => s.Contains("prescription") || s.Contains("medication") || s.Contains("pharmacy"));
    }

    [Fact]
    public async Task StreamResponseAsync_WithValidInput_StreamsTokens()
    {
        // Arrange
        var context = "Patient inquiry";
        var message = "What are visiting hours?";
        var mockStreamContent = "data: {\"choices\":[{\"delta\":{\"content\":\"Visiting \"}}]}\n\n" +
                                "data: {\"choices\":[{\"delta\":{\"content\":\"hours \"}}]}\n\n" +
                                "data: {\"choices\":[{\"delta\":{\"content\":\"are \"}}]}\n\n" +
                                "data: {\"choices\":[{\"delta\":{\"content\":\"8am-8pm.\"}}]}\n\n" +
                                "data: [DONE]\n\n";

        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(mockStreamContent, Encoding.UTF8, "text/event-stream")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var tokens = new List<string>();
        await foreach (var token in service.StreamResponseAsync(context, message, CancellationToken.None))
        {
            tokens.Add(token);
        }

        // Assert
        tokens.Should().NotBeEmpty();
        tokens.Should().Contain("Visiting ");
        tokens.Should().Contain("hours ");
    }

    [Theory]
    [InlineData("I need to schedule an appointment", "AppointmentScheduling")]
    [InlineData("When can I get my prescription refilled?", "PrescriptionInquiry")]
    [InlineData("I have a headache and fever", "SymptomCheck")]
    [InlineData("What are my lab results?", "LabResults")]
    [InlineData("How much do I owe?", "BillingInquiry")]
    public async Task AnalyzeIntentAsync_WithDifferentMessages_ReturnsExpectedIntent(string message, string expectedIntent)
    {
        // Arrange
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""choices"": [{{
                    ""message"": {{
                        ""content"": ""{{\""intent\"": \""{expectedIntent}\"", \""confidence\"": 0.90, \""entities\"": {{}}}}""
                    }}
                }}]
            }}", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var result = await service.AnalyzeIntentAsync(message, CancellationToken.None);

        // Assert
        result.Intent.Should().Be(expectedIntent);
        result.Confidence.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public async Task AnalyzeIntentAsync_WhenHttpRequestFails_ReturnsFallbackResult()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new AzureOpenAIService(_httpClient, _endpoint, _apiKey, _deploymentName, _mockLogger.Object);

        // Act
        var result = await service.AnalyzeIntentAsync("Test message", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Intent.Should().Be("GeneralInquiry");
        result.IsSuccess.Should().BeFalse();
    }
}
