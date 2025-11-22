using FluentValidation;
using MedWay.HospitalOnboarding.Application.Commands;

namespace MedWay.HospitalOnboarding.Application.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.HospitalId)
            .NotEmpty().WithMessage("Hospital ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Payment amount cannot exceed 1,000,000");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g., USD, INR, EUR)")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be uppercase letters only");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x.PaymentGateway)
            .MaximumLength(50).WithMessage("Payment gateway name cannot exceed 50 characters");
    }
}
