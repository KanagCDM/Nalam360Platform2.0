using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Queries;

public class GetSubscriptionPlansQueryHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly GetSubscriptionPlansQueryHandler _handler;

    public GetSubscriptionPlansQueryHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _handler = new GetSubscriptionPlansQueryHandler(_subscriptionPlanRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithPublicOnlyTrue_ShouldReturnOnlyPublicPlans()
    {
        // Arrange
        var query = new GetSubscriptionPlansQuery(publicOnly: true);

        var plans = new List<SubscriptionPlan>
        {
            SubscriptionPlan.Create(
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
                true, // public
                new List<Guid>()
            ),
            SubscriptionPlan.Create(
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
                true, // public
                new List<Guid>()
            )
        };

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.All(p => p.IsPublic).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithPublicOnlyFalse_ShouldReturnAllPlans()
    {
        // Arrange
        var query = new GetSubscriptionPlansQuery(publicOnly: false);

        var plans = new List<SubscriptionPlan>
        {
            SubscriptionPlan.Create(
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
            ),
            SubscriptionPlan.Create(
                "Enterprise Plan",
                "ENT",
                "Custom enterprise solution",
                999m,
                50,
                10,
                null,
                null,
                5m,
                30m,
                false, // private plan
                new List<Guid>()
            )
        };

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(p => !p.IsPublic);
    }

    [Fact]
    public async Task Handle_WhenNoPlansExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetSubscriptionPlansQuery(publicOnly: true);

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionPlan>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
