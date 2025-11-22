using FluentValidation;
using MedWay.HospitalOnboarding.Application.Commands;

namespace MedWay.HospitalOnboarding.Application.Validators;

public class AddBranchCommandValidator : AbstractValidator<AddBranchCommand>
{
    public AddBranchCommandValidator()
    {
        RuleFor(x => x.HospitalId)
            .NotEmpty().WithMessage("Hospital ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch name is required")
            .MinimumLength(3).WithMessage("Branch name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Branch name cannot exceed 200 characters");

        RuleFor(x => x.BranchCode)
            .NotEmpty().WithMessage("Branch code is required")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Branch code can only contain uppercase letters, numbers, and hyphens")
            .MaximumLength(20).WithMessage("Branch code cannot exceed 20 characters");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required")
            .MaximumLength(200).WithMessage("Address line 1 cannot exceed 200 characters");

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

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.OpeningDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)).WithMessage("Opening date cannot be more than 1 year in the future");

        RuleFor(x => x.OperatingHours)
            .MaximumLength(500).WithMessage("Operating hours cannot exceed 500 characters");
    }
}
