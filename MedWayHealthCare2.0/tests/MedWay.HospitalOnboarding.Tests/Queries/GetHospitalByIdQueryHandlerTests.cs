using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Application.DTOs;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Queries;

public class GetHospitalByIdQueryHandlerTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepositoryMock;
    private readonly GetHospitalByIdQueryHandler _handler;

    public GetHospitalByIdQueryHandlerTests()
    {
        _hospitalRepositoryMock = new Mock<IHospitalRepository>();
        _handler = new GetHospitalByIdQueryHandler(_hospitalRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnHospitalDetails()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var query = new GetHospitalByIdQuery(hospitalId);

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

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("City General Hospital");
        result.Value.RegistrationNumber.Should().Be("CGH001");
        result.Value.Email.Should().Be("info@citygeneral.com");
    }

    [Fact]
    public async Task Handle_WhenHospitalNotFound_ShouldReturnFailure()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var query = new GetHospitalByIdQuery(hospitalId);

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hospital?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
    }
}
