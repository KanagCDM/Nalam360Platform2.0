using FluentValidation;
using MedWay.HospitalOnboarding.Application.Commands;

namespace MedWay.HospitalOnboarding.Application.Validators;

public class RegisterHospitalCommandValidator : AbstractValidator<RegisterHospitalCommand>
{
    public RegisterHospitalCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hospital name is required")
            .MinimumLength(3).WithMessage("Hospital name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Hospital name cannot exceed 200 characters");

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("Registration number is required")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Registration number can only contain uppercase letters, numbers, and hyphens")
            .MaximumLength(50).WithMessage("Registration number cannot exceed 50 characters");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Tax number cannot exceed 50 characters");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required")
            .MaximumLength(200).WithMessage("Address line 1 cannot exceed 200 characters");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Address line 2 cannot exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State/Province is required")
            .MaximumLength(100).WithMessage("State/Province cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format (E.164 format expected)");

        RuleFor(x => x.EstablishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Established date cannot be in the future")
            .GreaterThan(new DateTime(1800, 1, 1)).WithMessage("Established date is invalid");

        RuleFor(x => x.TrialDays)
            .GreaterThanOrEqualTo(0).WithMessage("Trial days cannot be negative")
            .LessThanOrEqualTo(90).WithMessage("Trial days cannot exceed 90 days");
    }
}
