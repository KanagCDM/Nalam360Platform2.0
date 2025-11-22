using MedWay.Domain.Primitives;

namespace MedWay.BillingManagement.Domain.Entities;

/// <summary>
/// Invoice Line Item entity - represents a single charge on an invoice
/// </summary>
public sealed class InvoiceLineItem : Entity<Guid>
{
    private InvoiceLineItem(
        Guid id,
        string description,
        decimal unitPrice,
        int quantity,
        InvoiceItemType itemType)
        : base(id)
    {
        Description = description;
        UnitPrice = unitPrice;
        Quantity = quantity;
        ItemType = itemType;
    }

    private InvoiceLineItem() : base() { }

    public string Description { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public InvoiceItemType ItemType { get; private set; }
    public string? ServiceCode { get; private set; }
    public decimal TotalAmount => UnitPrice * Quantity;

    public static Result<InvoiceLineItem> Create(
        string description,
        decimal unitPrice,
        int quantity,
        InvoiceItemType itemType,
        string? serviceCode = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<InvoiceLineItem>(
                Error.Validation(nameof(Description), "Description is required"));

        if (unitPrice < 0)
            return Result.Failure<InvoiceLineItem>(
                Error.Validation(nameof(UnitPrice), "Unit price cannot be negative"));

        if (quantity <= 0)
            return Result.Failure<InvoiceLineItem>(
                Error.Validation(nameof(Quantity), "Quantity must be positive"));

        var lineItem = new InvoiceLineItem(
            Guid.NewGuid(),
            description,
            unitPrice,
            quantity,
            itemType)
        {
            ServiceCode = serviceCode
        };

        return lineItem;
    }
}

public enum InvoiceItemType
{
    Consultation = 1,
    Procedure = 2,
    Medication = 3,
    LabTest = 4,
    Imaging = 5,
    RoomCharge = 6,
    Other = 7
}

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Cancelled = 6
}

public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    BankTransfer = 4,
    Insurance = 5,
    MobilePayment = 6
}
