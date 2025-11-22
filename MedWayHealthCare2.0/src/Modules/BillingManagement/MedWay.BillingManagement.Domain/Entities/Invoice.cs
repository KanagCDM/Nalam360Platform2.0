using MedWay.Domain.Primitives;
using MedWay.BillingManagement.Domain.Events;
using MedWay.BillingManagement.Domain.ValueObjects;

namespace MedWay.BillingManagement.Domain.Entities;

/// <summary>
/// Invoice aggregate root - represents a patient bill
/// </summary>
public sealed class Invoice : AggregateRoot<Guid>
{
    private readonly List<InvoiceLineItem> _lineItems = new();

    private Invoice(
        Guid id,
        Guid patientId,
        Guid branchId,
        string invoiceNumber)
        : base(id)
    {
        PatientId = patientId;
        BranchId = branchId;
        InvoiceNumber = invoiceNumber;
        Status = InvoiceStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    private Invoice() : base() { }

    public Guid PatientId { get; private set; }
    public Guid BranchId { get; private set; }
    public string InvoiceNumber { get; private set; } = null!;
    public InvoiceStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public Guid? EncounterId { get; private set; }

    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    public decimal SubTotal => _lineItems.Sum(x => x.TotalAmount);
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount => SubTotal + TaxAmount - DiscountAmount;
    public decimal AmountPaid { get; private set; }
    public decimal Balance => TotalAmount - AmountPaid;

    public static Result<Invoice> Create(
        Guid patientId,
        Guid branchId,
        string invoiceNumber,
        Guid? encounterId = null)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<Invoice>(
                Error.Validation(nameof(PatientId), "Patient ID is required"));

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return Result.Failure<Invoice>(
                Error.Validation(nameof(InvoiceNumber), "Invoice number is required"));

        var invoice = new Invoice(
            Guid.NewGuid(),
            patientId,
            branchId,
            invoiceNumber)
        {
            EncounterId = encounterId
        };

        invoice.AddDomainEvent(new InvoiceCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            invoice.Id,
            invoice.PatientId,
            invoice.InvoiceNumber));

        return invoice;
    }

    public Result AddLineItem(InvoiceLineItem lineItem)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(Error.Validation(nameof(Status), "Can only add items to draft invoices"));

        _lineItems.Add(lineItem);
        return Result.Success();
    }

    public Result RemoveLineItem(Guid lineItemId)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(Error.Validation(nameof(Status), "Can only modify draft invoices"));

        var item = _lineItems.FirstOrDefault(x => x.Id == lineItemId);
        if (item == null)
            return Result.Failure(Error.NotFound("LineItem", lineItemId));

        _lineItems.Remove(item);
        return Result.Success();
    }

    public Result ApplyTax(decimal taxRate)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(Error.Validation(nameof(Status), "Can only modify draft invoices"));

        if (taxRate < 0 || taxRate > 1)
            return Result.Failure(Error.Validation(nameof(taxRate), "Tax rate must be between 0 and 1"));

        TaxAmount = SubTotal * taxRate;
        return Result.Success();
    }

    public Result ApplyDiscount(decimal discountAmount)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(Error.Validation(nameof(Status), "Can only modify draft invoices"));

        if (discountAmount < 0)
            return Result.Failure(Error.Validation(nameof(discountAmount), "Discount cannot be negative"));

        if (discountAmount > SubTotal)
            return Result.Failure(Error.Validation(nameof(discountAmount), "Discount cannot exceed subtotal"));

        DiscountAmount = discountAmount;
        return Result.Success();
    }

    public Result Issue(int dueDays = 30)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(Error.Validation(nameof(Status), "Only draft invoices can be issued"));

        if (!_lineItems.Any())
            return Result.Failure(Error.Validation(nameof(LineItems), "Invoice must have at least one line item"));

        Status = InvoiceStatus.Issued;
        IssuedAt = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(dueDays);

        AddDomainEvent(new InvoiceIssuedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            PatientId,
            TotalAmount));

        return Result.Success();
    }

    public Result RecordPayment(decimal amount, PaymentMethod paymentMethod)
    {
        if (Status == InvoiceStatus.Draft)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot pay draft invoice"));

        if (Status == InvoiceStatus.Paid)
            return Result.Failure(Error.Validation(nameof(Status), "Invoice is already paid"));

        if (Status == InvoiceStatus.Cancelled)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot pay cancelled invoice"));

        if (amount <= 0)
            return Result.Failure(Error.Validation(nameof(amount), "Payment amount must be positive"));

        if (amount > Balance)
            return Result.Failure(Error.Validation(nameof(amount), "Payment exceeds outstanding balance"));

        AmountPaid += amount;

        if (Balance == 0)
        {
            Status = InvoiceStatus.Paid;
            PaidAt = DateTime.UtcNow;

            AddDomainEvent(new InvoicePaidEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Id,
                PatientId,
                TotalAmount));
        }
        else
        {
            Status = InvoiceStatus.PartiallyPaid;

            AddDomainEvent(new PaymentReceivedEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Id,
                amount,
                paymentMethod));
        }

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == InvoiceStatus.Paid)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot cancel paid invoice"));

        if (Status == InvoiceStatus.Cancelled)
            return Result.Failure(Error.Validation(nameof(Status), "Invoice is already cancelled"));

        if (AmountPaid > 0)
            return Result.Failure(Error.Validation(nameof(AmountPaid), "Cannot cancel invoice with payments. Issue refund first."));

        Status = InvoiceStatus.Cancelled;

        AddDomainEvent(new InvoiceCancelledEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            reason));

        return Result.Success();
    }
}
