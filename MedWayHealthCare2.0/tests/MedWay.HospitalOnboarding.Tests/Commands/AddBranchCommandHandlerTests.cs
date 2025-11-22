using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Commands;

public class AddBranchCommandHandlerTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddBranchCommandHandler _handler;

    public AddBranchCommandHandlerTests()
    {
        _hospitalRepositoryMock = new Mock<IHospitalRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _handler = new AddBranchCommandHandler(
            _hospitalRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddBranchSuccessfully()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var command = new AddBranchCommand(
            hospitalId,
            "Downtown Branch",
            "CGH-DT",
            "456 Oak Ave",
            null,
            "New York",
            "NY",
            "10002",
            "USA",
            "+1234567892",
            "downtown@citygeneral.com",
            DateTime.UtcNow.AddDays(30),
            "Mon-Fri: 8AM-6PM"
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

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hospital);

        _branchRepositoryMock
            .Setup(x => x.ExistsByBranchCodeAsync(command.BranchCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _branchRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Branch>(b => 
                b.HospitalId == hospitalId &&
                b.Name == command.Name &&
                b.BranchCode == command.BranchCode &&
                b.IsActive == true
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHospitalNotFound_ShouldReturnFailure()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var command = new AddBranchCommand(
            hospitalId,
            "Downtown Branch",
            "CGH-DT",
            "456 Oak Ave",
            null,
            "New York",
            "NY",
            "10002",
            "USA",
            "+1234567892",
            "downtown@citygeneral.com",
            DateTime.UtcNow.AddDays(30),
            "Mon-Fri: 8AM-6PM"
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hospital?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");

        _branchRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateBranchCode_ShouldReturnFailure()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var command = new AddBranchCommand(
            hospitalId,
            "Downtown Branch",
            "CGH-DT",
            "456 Oak Ave",
            null,
            "New York",
            "NY",
            "10002",
            "USA",
            "+1234567892",
            "downtown@citygeneral.com",
            DateTime.UtcNow.AddDays(30),
            "Mon-Fri: 8AM-6PM"
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

        _branchRepositoryMock
            .Setup(x => x.ExistsByBranchCodeAsync(command.BranchCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
        result.Error.Message.Should().Contain("branch code");

        _branchRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
