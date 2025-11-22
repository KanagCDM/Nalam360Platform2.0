using MedWay.Domain.Primitives;

namespace MedWay.HospitalOnboarding.Domain.Events;

// Hospital Events
public record HospitalRegisteredEvent(
    Guid HospitalId,
    string TenantId,
    string HospitalName,
    string PrimaryEmail,
    DateTime TrialEndDate) : IDomainEvent;

public record HospitalUpdatedEvent(
    Guid HospitalId,
    string TenantId,
    string HospitalName) : IDomainEvent;

public record HospitalSubmittedEvent(
    Guid HospitalId,
    string HospitalName) : IDomainEvent;

public record HospitalApprovedEvent(
    Guid HospitalId,
    string HospitalName,
    Guid ApprovedBy) : IDomainEvent;

public record HospitalRejectedEvent(
    Guid HospitalId,
    string Reason,
    Guid RejectedBy) : IDomainEvent;

public record HospitalSuspendedEvent(
    Guid HospitalId,
    string Reason) : IDomainEvent;

public record HospitalReactivatedEvent(
    Guid HospitalId) : IDomainEvent;

// Facility Events
public record HospitalFacilityAddedEvent(
    Guid HospitalId,
    Guid GlobalFacilityId) : IDomainEvent;

public record HospitalFacilityRemovedEvent(
    Guid HospitalId,
    Guid GlobalFacilityId) : IDomainEvent;

// Branch Events
public record BranchAddedEvent(
    Guid HospitalId,
    Guid BranchId,
    string BranchName) : IDomainEvent;

// Subscription Events
public record SubscriptionActivatedEvent(
    Guid HospitalId,
    Guid SubscriptionPlanId,
    DateTime StartDate,
    DateTime EndDate) : IDomainEvent;

// Payment Events
public record PaymentCreatedEvent(
    Guid PaymentId,
    Guid HospitalId,
    decimal Amount,
    DateTime DueDate) : IDomainEvent;

public record PaymentSuccessfulEvent(
    Guid PaymentId,
    Guid HospitalId,
    Guid SubscriptionPlanId,
    decimal Amount,
    string TransactionId) : IDomainEvent;

public record PaymentFailedEvent(
    Guid PaymentId,
    Guid HospitalId,
    decimal Amount,
    string FailureReason) : IDomainEvent;

public record PaymentRefundedEvent(
    Guid PaymentId,
    Guid HospitalId,
    decimal Amount,
    string Reason) : IDomainEvent;

public record PaymentRetryInitiatedEvent(
    Guid PaymentId,
    Guid HospitalId,
    decimal Amount) : IDomainEvent;
