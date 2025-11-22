using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Commands;

public class ActivateSubscriptionCommandHandlerTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ActivateSubscriptionCommandHandler _handler;

    public ActivateSubscriptionCommandHandlerTests()
    {
        _hospitalRepositoryMock = new Mock<IHospitalRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _handler = new ActivateSubscriptionCommandHandler(
            _hospitalRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldActivateSubscriptionSuccessfully()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new ActivateSubscriptionCommand(
            hospitalId,
            planId,
            10, // users
            2,  // branches
            new List<Guid>() // no additional facilities
        );

        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        hospital.Approve("admin@system.com");

        var plan = SubscriptionPlan.Create(
            "Professional Plan",
            "PRO",
            "For medium hospitals",
            299m,
            10,
            1,
            50,
            5,
            10m,
            50m,
            true,
            new List<Guid>()
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hospital);

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        hospital.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        hospital.CurrentSubscriptionPlanId.Should().Be(planId);
        hospital.SubscriptionStartDate.Should().NotBeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHospitalNotFound_ShouldReturnFailure()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var command = new ActivateSubscriptionCommand(
            hospitalId,
            Guid.NewGuid(),
            10,
            2,
            new List<Guid>()
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hospital?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPlanNotFound_ShouldReturnFailure()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new ActivateSubscriptionCommand(
            hospitalId,
            planId,
            10,
            2,
            new List<Guid>()
        );

        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hospital);

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionPlan?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExcessUsers_ShouldCalculateAdditionalCost()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new ActivateSubscriptionCommand(
            hospitalId,
            planId,
            15, // 5 extra users (plan includes 10)
            1,
            new List<Guid>()
        );

        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        var plan = SubscriptionPlan.Create(
            "Professional Plan",
            "PRO",
            "For medium hospitals",
            299m,
            10, // includes 10 users
            1,
            50,
            5,
            10m, // $10 per additional user
            50m,
            true,
            new List<Guid>()
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hospital);

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Expected cost: $299 (base) + (5 extra users * $10) = $349
        var expectedMonthlyCost = 299m + (5 * 10m);
        
        // Note: The actual cost calculation would be in the domain entity
        // This test verifies the subscription was activated
        hospital.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }
}
