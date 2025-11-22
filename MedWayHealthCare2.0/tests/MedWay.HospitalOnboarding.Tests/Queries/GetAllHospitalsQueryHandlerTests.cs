using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Application.DTOs;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Queries;

public class GetAllHospitalsQueryHandlerTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepositoryMock;
    private readonly GetAllHospitalsQueryHandler _handler;

    public GetAllHospitalsQueryHandlerTests()
    {
        _hospitalRepositoryMock = new Mock<IHospitalRepository>();
        _handler = new GetAllHospitalsQueryHandler(_hospitalRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedHospitals()
    {
        // Arrange
        var query = new GetAllHospitalsQuery(1, 10, null, null);

        var hospitals = new List<Hospital>
        {
            Hospital.Create(
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
            ),
            Hospital.Create(
                "Metro Medical Center",
                "MMC002",
                "info@metromedical.com",
                "+1234567893",
                "456 Park Ave",
                null,
                "Los Angeles",
                "CA",
                "90001",
                "USA",
                "Dr. Jane Doe",
                "director@metromedical.com",
                "+1234567894",
                Guid.NewGuid()
            )
        };

        _hospitalRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<HospitalStatus?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((hospitals, 2));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnFilteredHospitals()
    {
        // Arrange
        var query = new GetAllHospitalsQuery(1, 10, HospitalStatus.Active, null);

        var hospitals = new List<Hospital>
        {
            Hospital.Create(
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
            )
        };

        hospitals[0].Approve("admin@system.com");

        _hospitalRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                HospitalStatus.Active,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((hospitals, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Status.Should().Be(HospitalStatus.Active);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldReturnMatchingHospitals()
    {
        // Arrange
        var query = new GetAllHospitalsQuery(1, 10, null, "Metro");

        var hospitals = new List<Hospital>
        {
            Hospital.Create(
                "Metro Medical Center",
                "MMC002",
                "info@metromedical.com",
                "+1234567893",
                "456 Park Ave",
                null,
                "Los Angeles",
                "CA",
                "90001",
                "USA",
                "Dr. Jane Doe",
                "director@metromedical.com",
                "+1234567894",
                Guid.NewGuid()
            )
        };

        _hospitalRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<HospitalStatus?>(),
                "Metro",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((hospitals, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Contain("Metro");
    }
}
