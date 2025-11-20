using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Tests.Core.AI;

public class HIPAAComplianceServiceTests
{
    private readonly Mock<ILogger<HIPAAComplianceService>> _mockLogger;
    private readonly HIPAAComplianceService _service;

    public HIPAAComplianceServiceTests()
    {
        _mockLogger = new Mock<ILogger<HIPAAComplianceService>>();
        _service = new HIPAAComplianceService(_mockLogger.Object);
    }

    [Theory]
    [InlineData("Patient MRN: ABC123456", "MRN", "ABC123456")]
    [InlineData("SSN 123-45-6789", "SSN", "123-45-6789")]
    [InlineData("Call me at 555-123-4567", "PHONE", "555-123-4567")]
    [InlineData("Date of birth: 01/15/1985", "DATE", "01/15/1985")]
    [InlineData("Email: patient@example.com", "EMAIL", "patient@example.com")]
    [InlineData("Address: 123 Main St, City, ST 12345", "ADDRESS", "123 Main St, City, ST 12345")]
    public async Task DetectPHIAsync_WithSinglePHIType_DetectsCorrectly(string text, string expectedType, string expectedValue)
    {
        // Act
        var result = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(phi => phi.Type == expectedType);
        var detectedPhi = result.First(phi => phi.Type == expectedType);
        detectedPhi.Value.Should().Contain(expectedValue);
        detectedPhi.Confidence.Should().BeGreaterThan(0.5);
        detectedPhi.SuggestedReplacement.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DetectPHIAsync_WithMultiplePHIElements_DetectsAll()
    {
        // Arrange
        var text = "Patient John Doe, MRN: XYZ789012, SSN 987-65-4321, lives at 456 Oak Ave, Springfield, IL 62701. Contact: 217-555-0100 or john.doe@email.com. DOB: 03/22/1970";

        // Act
        var result = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterOrEqualTo(5); // MRN, SSN, ADDRESS, PHONE, EMAIL, DATE, NAME
        result.Should().Contain(phi => phi.Type == "MRN");
        result.Should().Contain(phi => phi.Type == "SSN");
        result.Should().Contain(phi => phi.Type == "PHONE");
        result.Should().Contain(phi => phi.Type == "EMAIL");
        result.Should().Contain(phi => phi.Type == "ADDRESS");
    }

    [Fact]
    public async Task DetectPHIAsync_WithNoPHI_ReturnsEmptyList()
    {
        // Arrange
        var text = "This is a general message with no protected health information.";

        // Act
        var result = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeIdentifyAsync_WithDetectedPHI_ReplacesAllPHI()
    {
        // Arrange
        var text = "Patient MRN: ABC123456, SSN 123-45-6789, Phone: 555-123-4567";
        var phiElements = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Act
        var deIdentified = await _service.DeIdentifyAsync(text, phiElements, CancellationToken.None);

        // Assert
        deIdentified.Should().NotContain("ABC123456");
        deIdentified.Should().NotContain("123-45-6789");
        deIdentified.Should().NotContain("555-123-4567");
        deIdentified.Should().Contain("[MRN]");
        deIdentified.Should().Contain("[SSN]");
        deIdentified.Should().Contain("[PHONE]");
    }

    [Fact]
    public async Task DeIdentifyAsync_WithEmptyPHIList_ReturnsOriginalText()
    {
        // Arrange
        var text = "This is a general message.";
        var phiElements = new List<PHIElement>();

        // Act
        var result = await _service.DeIdentifyAsync(text, phiElements, CancellationToken.None);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public async Task DeIdentifyAsync_WithOverlappingPHI_HandlesCorrectly()
    {
        // Arrange
        var text = "John Doe at john.doe@example.com";
        var phiElements = new List<PHIElement>
        {
            new PHIElement
            {
                Type = "NAME",
                Value = "John Doe",
                StartPosition = 0,
                EndPosition = 8,
                Confidence = 0.60,
                SuggestedReplacement = "[NAME]"
            },
            new PHIElement
            {
                Type = "EMAIL",
                Value = "john.doe@example.com",
                StartPosition = 12,
                EndPosition = 32,
                Confidence = 0.90,
                SuggestedReplacement = "[EMAIL]"
            }
        };

        // Act
        var result = await _service.DeIdentifyAsync(text, phiElements, CancellationToken.None);

        // Assert
        result.Should().Contain("[NAME]");
        result.Should().Contain("[EMAIL]");
        result.Should().NotContain("John Doe");
        result.Should().NotContain("john.doe@example.com");
    }

    [Fact]
    public async Task AuditAIOperationAsync_LogsOperation()
    {
        // Arrange
        var operation = "IntentAnalysis";
        var userId = "user123";
        var input = "Test input";
        var output = "Test output";

        // Act
        await _service.AuditAIOperationAsync(operation, userId, input, output, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(operation)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("https://test-openai.openai.azure.com", "eastus", true)]
    [InlineData("https://test-openai.openai.azure.com", "westus", true)]
    [InlineData("https://test-openai.openai.azure.com", "centralus", true)]
    [InlineData("https://test-openai.openai.azure.com", "unknown", false)]
    public async Task ValidateDataResidencyAsync_ChecksUSRegions(string endpoint, string region, bool expectedValid)
    {
        // Act
        var result = await _service.ValidateDataResidencyAsync(endpoint, region, CancellationToken.None);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("https://secure-endpoint.com", true)]
    [InlineData("http://insecure-endpoint.com", false)]
    public async Task ValidateEncryptionAsync_ChecksHTTPS(string endpoint, bool expectedValid)
    {
        // Act
        var result = await _service.ValidateEncryptionAsync(endpoint, CancellationToken.None);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Fact]
    public async Task GenerateComplianceReportAsync_WithPHI_ReturnsDetailedReport()
    {
        // Arrange
        var text = "Patient MRN: ABC123456, SSN 123-45-6789, Phone: 555-123-4567";
        var phiElements = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Act
        var report = await _service.GenerateComplianceReportAsync(text, phiElements, CancellationToken.None);

        // Assert
        report.Should().NotBeNull();
        report.HasPHI.Should().BeTrue();
        report.PHICount.Should().BeGreaterThan(0);
        report.PHITypes.Should().NotBeEmpty();
        report.Recommendations.Should().NotBeEmpty();
        report.IsCompliant.Should().BeFalse(); // Contains PHI, so not compliant for transmission
    }

    [Fact]
    public async Task GenerateComplianceReportAsync_WithoutPHI_ReturnsCompliantReport()
    {
        // Arrange
        var text = "This is a general healthcare message without any PHI.";
        var phiElements = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Act
        var report = await _service.GenerateComplianceReportAsync(text, phiElements, CancellationToken.None);

        // Assert
        report.Should().NotBeNull();
        report.HasPHI.Should().BeFalse();
        report.PHICount.Should().Be(0);
        report.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public async Task DetectPHIAsync_WithMRNVariations_DetectsAll()
    {
        // Arrange
        var text = "MRN ABC123456, Medical Record: XYZ789012, Record # DEF456789";

        // Act
        var result = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Assert
        var mrnElements = result.Where(phi => phi.Type == "MRN").ToList();
        mrnElements.Should().HaveCountGreaterOrEqualTo(2); // At least 2 variations detected
        mrnElements.All(phi => phi.Confidence > 0.9).Should().BeTrue();
    }

    [Fact]
    public async Task DetectPHIAsync_WithSSNVariations_DetectsAll()
    {
        // Arrange
        var text = "SSN 123-45-6789 or 987654321";

        // Act
        var result = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Assert
        var ssnElements = result.Where(phi => phi.Type == "SSN").ToList();
        ssnElements.Should().HaveCountGreaterOrEqualTo(2); // Both formatted and unformatted
        ssnElements.All(phi => phi.Confidence > 0.95).Should().BeTrue();
    }

    [Fact]
    public async Task DetectPHIAsync_WithPhoneNumberVariations_DetectsAll()
    {
        // Arrange
        var text = "Call 555-123-4567 or (555) 987-6543 or 5551234567";

        // Act
        var result = await _service.DetectPHIAsync(text, CancellationToken.None);

        // Assert
        var phoneElements = result.Where(phi => phi.Type == "PHONE").ToList();
        phoneElements.Should().HaveCountGreaterOrEqualTo(2);
        phoneElements.All(phi => phi.Confidence > 0.8).Should().BeTrue();
    }

    [Fact]
    public async Task DetectPHIAsync_PerformanceTest_HandlesLargeText()
    {
        // Arrange
        var largeText = string.Concat(Enumerable.Repeat(
            "Patient MRN: ABC123456, SSN 123-45-6789, Contact: 555-123-4567. ", 100));

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.DetectPHIAsync(largeText, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in less than 1 second
    }
}
