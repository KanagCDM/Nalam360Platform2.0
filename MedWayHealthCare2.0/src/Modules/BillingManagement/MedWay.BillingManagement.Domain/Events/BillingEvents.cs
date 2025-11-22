using MedWay.Domain.Primitives;
using MedWay.BillingManagement.Domain.Entities;

namespace MedWay.BillingManagement.Domain.Events;

public sealed record InvoiceCreatedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid InvoiceId,
    Guid PatientId,
    string InvoiceNumber) : IDomainEvent;

public sealed record InvoiceIssuedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid InvoiceId,
    Guid PatientId,
    decimal TotalAmount) : IDomainEvent;

public sealed record PaymentReceivedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod PaymentMethod) : IDomainEvent;

public sealed record InvoicePaidEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid InvoiceId,
    Guid PatientId,
    decimal TotalAmount) : IDomainEvent;

public sealed record InvoiceCancelledEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid InvoiceId,
    string Reason) : IDomainEvent;
