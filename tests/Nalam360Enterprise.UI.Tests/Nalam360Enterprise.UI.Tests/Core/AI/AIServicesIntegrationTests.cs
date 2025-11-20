using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Tests.Core.AI;

/// <summary>
/// Integration tests demonstrating end-to-end AI service workflows.
/// These tests show how services work together in realistic healthcare scenarios.
/// </summary>
public class AIServicesIntegrationTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public AIServicesIntegrationTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        var services = new ServiceCollection();
        
        // Register logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Register AI services with mocked HttpClient
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        services.AddSingleton(httpClient);
        services.AddScoped<IAIService>(sp => 
            new AzureOpenAIService(
                httpClient,
                "https://test-openai.openai.azure.com",
                "test-api-key",
                "gpt-4",
                sp.GetRequiredService<ILogger<AzureOpenAIService>>()));
        services.AddScoped<IAIComplianceService, HIPAAComplianceService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task HIPAACompliantChatWorkflow_DetectsAndDeIdentifiesPHI()
    {
        // Arrange
        var aiService = _serviceProvider.GetRequiredService<IAIService>();
        var complianceService = _serviceProvider.GetRequiredService<IAIComplianceService>();
        
        var userMessage = "My name is John Doe, MRN: ABC123456, SSN 123-45-6789. I need to refill my prescription.";
        
        // Mock AI service responses
        SetupMockIntentResponse("PrescriptionInquiry", 0.95);
        SetupMockSentimentResponse("neutral", 0.85);
        SetupMockResponseGeneration("I can help you with your prescription refill. Let me check your records.");

        // Act - Step 1: Detect PHI
        var phiElements = await complianceService.DetectPHIAsync(userMessage, CancellationToken.None);
        
        // Act - Step 2: De-identify message
        var deIdentifiedMessage = await complianceService.DeIdentifyAsync(userMessage, phiElements, CancellationToken.None);
        
        // Act - Step 3: Analyze intent and sentiment with de-identified message
        var intentTask = aiService.AnalyzeIntentAsync(deIdentifiedMessage, CancellationToken.None);
        var sentimentTask = aiService.AnalyzeSentimentAsync(deIdentifiedMessage, CancellationToken.None);
        await Task.WhenAll(intentTask, sentimentTask);
        
        var intentResult = await intentTask;
        var sentimentResult = await sentimentTask;
        
        // Act - Step 4: Generate response
        SetupMockResponseGeneration("I can help you with your prescription refill.");
        var response = await aiService.GenerateResponseAsync(
            $"Intent: {intentResult.Intent}, Sentiment: {sentimentResult.Sentiment}",
            deIdentifiedMessage,
            CancellationToken.None);
        
        // Act - Step 5: Audit the operation
        await complianceService.AuditAIOperationAsync(
            "ChatInteraction",
            "user123",
            deIdentifiedMessage,
            response,
            CancellationToken.None);

        // Assert
        phiElements.Should().NotBeEmpty("PHI should be detected");
        phiElements.Should().Contain(phi => phi.Type == "NAME");
        phiElements.Should().Contain(phi => phi.Type == "MRN");
        phiElements.Should().Contain(phi => phi.Type == "SSN");
        
        deIdentifiedMessage.Should().NotContain("John Doe");
        deIdentifiedMessage.Should().NotContain("ABC123456");
        deIdentifiedMessage.Should().NotContain("123-45-6789");
        deIdentifiedMessage.Should().Contain("[NAME]");
        deIdentifiedMessage.Should().Contain("[MRN]");
        deIdentifiedMessage.Should().Contain("[SSN]");
        
        intentResult.Intent.Should().Be("PrescriptionInquiry");
        intentResult.Confidence.Should().BeGreaterThan(0.9);
        
        sentimentResult.Sentiment.Should().Be("neutral");
        
        response.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EmergencyTriageWorkflow_PrioritizesBasedOnIntentAndSentiment()
    {
        // Arrange
        var aiService = _serviceProvider.GetRequiredService<IAIService>();
        var complianceService = _serviceProvider.GetRequiredService<IAIComplianceService>();
        
        var userMessage = "I'm having severe chest pain and difficulty breathing!";
        
        SetupMockIntentResponse("EmergencyTriage", 0.98);
        SetupMockSentimentResponse("negative", 0.92);

        // Act - Step 1: Check for PHI
        var phiElements = await complianceService.DetectPHIAsync(userMessage, CancellationToken.None);
        
        // Act - Step 2: Analyze intent and sentiment in parallel
        var intentTask = aiService.AnalyzeIntentAsync(userMessage, CancellationToken.None);
        var sentimentTask = aiService.AnalyzeSentimentAsync(userMessage, CancellationToken.None);
        await Task.WhenAll(intentTask, sentimentTask);
        
        var intentResult = await intentTask;
        var sentimentResult = await sentimentTask;
        
        // Act - Step 3: Determine priority
        var isEmergency = intentResult.Intent == "EmergencyTriage" && intentResult.Confidence > 0.95;
        var isDistressed = sentimentResult.Sentiment == "negative" && sentimentResult.Confidence > 0.9;
        
        // Assert
        phiElements.Should().BeEmpty("No PHI in symptom description");
        
        intentResult.Intent.Should().Be("EmergencyTriage");
        intentResult.Confidence.Should().BeGreaterThan(0.95);
        
        sentimentResult.Sentiment.Should().Be("negative");
        sentimentResult.Confidence.Should().BeGreaterThan(0.9);
        
        isEmergency.Should().BeTrue("Should be classified as emergency");
        isDistressed.Should().BeTrue("Should detect patient distress");
    }

    [Fact]
    public async Task AppointmentSchedulingWorkflow_ExtractsEntitiesAndGeneratesSuggestions()
    {
        // Arrange
        var aiService = _serviceProvider.GetRequiredService<IAIService>();
        
        var userMessage = "I need to see Dr. Smith next Tuesday afternoon for a follow-up.";
        
        SetupMockIntentResponse("AppointmentScheduling", 0.93);
        SetupMockSuggestionsResponse(new List<string>
        {
            "Book appointment with Dr. Smith",
            "Check available time slots for next Tuesday",
            "Send appointment confirmation"
        });

        // Act - Step 1: Analyze intent
        var intentResult = await aiService.AnalyzeIntentAsync(userMessage, CancellationToken.None);
        
        // Act - Step 2: Generate contextual suggestions
        var suggestions = await aiService.GenerateSuggestionsAsync(
            "User wants to schedule appointment",
            intentResult.Intent,
            CancellationToken.None);

        // Assert
        intentResult.Intent.Should().Be("AppointmentScheduling");
        intentResult.Entities.Should().ContainKey("doctor");
        intentResult.Entities.Should().ContainKey("timeframe");
        
        suggestions.Should().NotBeEmpty();
        suggestions.Should().HaveCountGreaterOrEqualTo(3);
        suggestions.Should().Contain(s => s.Contains("appointment", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task BatchPHIDetection_HandlesMultipleMessages()
    {
        // Arrange
        var complianceService = _serviceProvider.GetRequiredService<IAIComplianceService>();
        
        var messages = new[]
        {
            "Patient John Doe, MRN: ABC123456",
            "SSN 123-45-6789, DOB: 01/15/1985",
            "Contact: 555-123-4567, Email: john@example.com",
            "This message has no PHI"
        };

        // Act
        var detectionTasks = messages.Select(msg => 
            complianceService.DetectPHIAsync(msg, CancellationToken.None));
        var results = await Task.WhenAll(detectionTasks);

        // Assert
        results[0].Should().NotBeEmpty("First message has PHI");
        results[1].Should().NotBeEmpty("Second message has PHI");
        results[2].Should().NotBeEmpty("Third message has PHI");
        results[3].Should().BeEmpty("Fourth message has no PHI");
        
        results.SelectMany(r => r).Should().HaveCountGreaterOrEqualTo(7); // NAME, MRN, SSN, DATE, PHONE, EMAIL
    }

    [Fact]
    public async Task StreamingResponseWorkflow_ProvidesProgressiveFeedback()
    {
        // Arrange
        var aiService = _serviceProvider.GetRequiredService<IAIService>();
        
        var context = "Patient asking about medication side effects";
        var message = "What are the side effects of this medication?";
        
        var mockStreamContent = "data: {\"choices\":[{\"delta\":{\"content\":\"Common \"}}]}\n\n" +
                                "data: {\"choices\":[{\"delta\":{\"content\":\"side \"}}]}\n\n" +
                                "data: {\"choices\":[{\"delta\":{\"content\":\"effects \"}}]}\n\n" +
                                "data: {\"choices\":[{\"delta\":{\"content\":\"include...\"}}]}\n\n" +
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
                ItExpr.Is<HttpRequestMessage>(req => req.Content != null),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        // Act
        var tokens = new List<string>();
        var fullResponse = new StringBuilder();
        
        await foreach (var token in aiService.StreamResponseAsync(context, message, CancellationToken.None))
        {
            tokens.Add(token);
            fullResponse.Append(token);
        }

        // Assert
        tokens.Should().NotBeEmpty("Should receive streaming tokens");
        fullResponse.Length.Should().BeGreaterThan(0);
        fullResponse.ToString().Should().Contain("side effects");
    }

    [Fact]
    public async Task ComplianceReportGeneration_ProvidesTotalOverview()
    {
        // Arrange
        var complianceService = _serviceProvider.GetRequiredService<IAIComplianceService>();
        
        var text = "Patient John Doe, MRN: ABC123456, SSN 123-45-6789, Phone: 555-123-4567, Email: john@example.com";

        // Act
        var phiElements = await complianceService.DetectPHIAsync(text, CancellationToken.None);
        var report = await complianceService.GenerateComplianceReportAsync(text, phiElements, CancellationToken.None);

        // Assert
        report.Should().NotBeNull();
        report.HasPHI.Should().BeTrue();
        report.PHICount.Should().BeGreaterOrEqualTo(4);
        report.PHITypes.Should().Contain("NAME");
        report.PHITypes.Should().Contain("MRN");
        report.PHITypes.Should().Contain("SSN");
        report.PHITypes.Should().Contain("PHONE");
        report.PHITypes.Should().Contain("EMAIL");
        report.IsCompliant.Should().BeFalse("Contains PHI");
        report.Recommendations.Should().NotBeEmpty();
    }

    // Helper methods to setup mock HTTP responses
    private void SetupMockIntentResponse(string intent, double confidence)
    {
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""choices"": [{{
                    ""message"": {{
                        ""content"": ""{{\""intent\"": \""{intent}\"", \""confidence\"": {confidence.ToString("F2")}, \""entities\"": {{\""doctor\"": \""Smith\"", \""timeframe\"": \""next Tuesday\""}}}}""
                    }}
                }}]
            }}", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("chat/completions") &&
                    req.Content!.ReadAsStringAsync().Result.Contains("Analyze")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);
    }

    private void SetupMockSentimentResponse(string sentiment, double confidence)
    {
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""choices"": [{{
                    ""message"": {{
                        ""content"": ""{{\""sentiment\"": \""{sentiment}\"", \""confidence\"": {confidence.ToString("F2")}, \""scores\"": {{\""positive\"": 0.05, \""negative\"": 0.92, \""neutral\"": 0.02, \""mixed\"": 0.01}}}}""
                    }}
                }}]
            }}", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("chat/completions") &&
                    req.Content!.ReadAsStringAsync().Result.Contains("sentiment")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);
    }

    private void SetupMockResponseGeneration(string response)
    {
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""choices"": [{{
                    ""message"": {{
                        ""content"": ""{response}""
                    }}
                }}]
            }}", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("chat/completions") &&
                    !req.Content!.ReadAsStringAsync().Result.Contains("Analyze") &&
                    !req.Content!.ReadAsStringAsync().Result.Contains("sentiment")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);
    }

    private void SetupMockSuggestionsResponse(List<string> suggestions)
    {
        var suggestionsText = string.Join("\n", suggestions.Select((s, i) => $"{i + 1}. {s}"));
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""choices"": [{{
                    ""message"": {{
                        ""content"": ""{suggestionsText}""
                    }}
                }}]
            }}", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("chat/completions") &&
                    req.Content!.ReadAsStringAsync().Result.Contains("suggestions")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);
    }
}
