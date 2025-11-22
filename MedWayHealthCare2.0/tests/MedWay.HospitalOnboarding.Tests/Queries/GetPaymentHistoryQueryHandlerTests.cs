using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Queries;

public class GetPaymentHistoryQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly GetPaymentHistoryQueryHandler _handler;

    public GetPaymentHistoryQueryHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _handler = new GetPaymentHistoryQueryHandler(_paymentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidHospitalId_ShouldReturnPaymentHistory()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var query = new GetPaymentHistoryQuery(hospitalId);

        var payments = new List<Payment>
        {
            Payment.Create(
                hospitalId,
                Guid.NewGuid(),
                299m,
                "USD",
                PaymentMethod.CreditCard,
                "Stripe",
                null
            ),
            Payment.Create(
                hospitalId,
                Guid.NewGuid(),
                299m,
                "USD",
                PaymentMethod.BankTransfer,
                "Manual",
                null
            )
        };

        _paymentRepositoryMock
            .Setup(x => x.GetByHospitalIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.All(p => p.HospitalId == hospitalId).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoPaymentsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var query = new GetPaymentHistoryQuery(hospitalId);

        _paymentRepositoryMock
            .Setup(x => x.GetByHospitalIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaymentsOrderedByDate()
    {
        // Arrange
        var hospitalId = Guid.NewGuid();
        var query = new GetPaymentHistoryQuery(hospitalId);

        var oldPayment = Payment.Create(
            hospitalId,
            Guid.NewGuid(),
            299m,
            "USD",
            PaymentMethod.CreditCard,
            "Stripe",
            null
        );

        var newPayment = Payment.Create(
            hospitalId,
            Guid.NewGuid(),
            299m,
            "USD",
            PaymentMethod.CreditCard,
            "Stripe",
            null
        );

        var payments = new List<Payment> { newPayment, oldPayment };

        _paymentRepositoryMock
            .Setup(x => x.GetByHospitalIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments.OrderByDescending(p => p.CreatedAt).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        
        // Verify payments are ordered by date (newest first)
        for (int i = 0; i < result.Value.Count - 1; i++)
        {
            result.Value[i].PaymentDate.Should().BeOnOrAfter(result.Value[i + 1].PaymentDate);
        }
    }
}
