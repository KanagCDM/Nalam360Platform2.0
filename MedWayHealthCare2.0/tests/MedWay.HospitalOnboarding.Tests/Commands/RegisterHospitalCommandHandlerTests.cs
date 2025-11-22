using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Commands;

public class RegisterHospitalCommandHandlerTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterHospitalCommandHandler _handler;

    public RegisterHospitalCommandHandlerTests()
    {
        _hospitalRepositoryMock = new Mock<IHospitalRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _handler = new RegisterHospitalCommandHandler(
            _hospitalRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterHospitalSuccessfully()
    {
        // Arrange
        var command = new RegisterHospitalCommand(
            "City General Hospital",
            "CGH001",
            "info@citygeneral.com",
            "+1234567890",
            "123 Main St",
            null,
            "New York",
            "NY",
            "10001",
            "USA",
            "Dr. John Smith",
            "director@citygeneral.com",
            "+1234567891"
        );

        var trialPlan = SubscriptionPlan.Create(
            "Trial Plan",
            "TRIAL",
            "30-day free trial",
            0m,
            5,
            1,
            10,
            3,
            0m,
            0m,
            true,
            new List<Guid>()
        );

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetTrialPlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(trialPlan);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByRegistrationNumberAsync(command.RegistrationNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _hospitalRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Hospital>(h => 
                h.Name == command.Name &&
                h.RegistrationNumber == command.RegistrationNumber &&
                h.Status == HospitalStatus.PendingApproval &&
                h.SubscriptionStatus == SubscriptionStatus.Trial
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateRegistrationNumber_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterHospitalCommand(
            "City General Hospital",
            "CGH001",
            "info@citygeneral.com",
            "+1234567890",
            "123 Main St",
            null,
            "New York",
            "NY",
            "10001",
            "USA",
            "Dr. John Smith",
            "director@citygeneral.com",
            "+1234567891"
        );

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByRegistrationNumberAsync(command.RegistrationNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
        result.Error.Message.Should().Contain("registration number");

        _hospitalRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Hospital>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterHospitalCommand(
            "City General Hospital",
            "CGH001",
            "info@citygeneral.com",
            "+1234567890",
            "123 Main St",
            null,
            "New York",
            "NY",
            "10001",
            "USA",
            "Dr. John Smith",
            "director@citygeneral.com",
            "+1234567891"
        );

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByRegistrationNumberAsync(command.RegistrationNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
        result.Error.Message.Should().Contain("email");

        _hospitalRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Hospital>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTrialPlanNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterHospitalCommand(
            "City General Hospital",
            "CGH001",
            "info@citygeneral.com",
            "+1234567890",
            "123 Main St",
            null,
            "New York",
            "NY",
            "10001",
            "USA",
            "Dr. John Smith",
            "director@citygeneral.com",
            "+1234567891"
        );

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetTrialPlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionPlan?)null);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByRegistrationNumberAsync(command.RegistrationNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain("trial plan");
    }
}
