using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Commands;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _hospitalRepositoryMock = new Mock<IHospitalRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _handler = new ProcessPaymentCommandHandler(
            _hospitalRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldProcessPaymentSuccessfully()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(
            hospitalId,
            planId,
            299m,
            "USD",
            PaymentMethod.CreditCard,
            "Stripe",
            new Dictionary<string, string>
            {
                { "card_last4", "4242" },
                { "card_brand", "Visa" }
            }
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Payment>(p => 
                p.HospitalId == hospitalId &&
                p.Amount == command.Amount &&
                p.Currency == command.Currency &&
                p.PaymentMethod == command.PaymentMethod &&
                p.Status == PaymentStatus.Successful
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHospitalNotFound_ShouldReturnFailure()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(
            hospitalId,
            Guid.NewGuid(),
            299m,
            "USD",
            PaymentMethod.CreditCard,
            "Stripe",
            null
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hospital?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(
            hospitalId,
            Guid.NewGuid(),
            0m, // Invalid amount
            "USD",
            PaymentMethod.CreditCard,
            "Stripe",
            null
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("amount");

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
