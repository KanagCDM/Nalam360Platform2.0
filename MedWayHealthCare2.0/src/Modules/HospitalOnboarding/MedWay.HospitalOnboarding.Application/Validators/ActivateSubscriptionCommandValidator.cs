using FluentValidation;
using MedWay.HospitalOnboarding.Application.Commands;

namespace MedWay.HospitalOnboarding.Application.Validators;

public class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
{
    public ActivateSubscriptionCommandValidator()
    {
        RuleFor(x => x.HospitalId)
            .NotEmpty().WithMessage("Hospital ID is required");

        RuleFor(x => x.SubscriptionPlanId)
            .NotEmpty().WithMessage("Subscription plan ID is required");

        RuleFor(x => x.NumberOfUsers)
            .GreaterThanOrEqualTo(1).WithMessage("At least 1 user is required")
            .LessThanOrEqualTo(10000).WithMessage("Number of users cannot exceed 10,000");

        RuleFor(x => x.NumberOfBranches)
            .GreaterThanOrEqualTo(1).WithMessage("At least 1 branch is required")
            .LessThanOrEqualTo(1000).WithMessage("Number of branches cannot exceed 1,000");

        RuleFor(x => x.AdditionalFacilityIds)
            .NotNull().WithMessage("Additional facility IDs list is required (can be empty)");
    }
}
