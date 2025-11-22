using MedWay.Domain.Primitives;

namespace MedWay.PharmacyManagement.Domain.Entities;

/// <summary>
/// Medication Inventory entity - tracks medication stock levels
/// </summary>
public sealed class MedicationInventory : Entity<Guid>
{
    private MedicationInventory(
        Guid id,
        string medicationName,
        string batchNumber,
        int quantity,
        DateTime expiryDate,
        Guid pharmacyId)
        : base(id)
    {
        MedicationName = medicationName;
        BatchNumber = batchNumber;
        Quantity = quantity;
        ExpiryDate = expiryDate;
        PharmacyId = pharmacyId;
        CreatedAt = DateTime.UtcNow;
    }

    private MedicationInventory() : base() { }

    public string MedicationName { get; private set; } = null!;
    public string BatchNumber { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public Guid PharmacyId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string? Manufacturer { get; private set; }

    public bool IsExpired => ExpiryDate < DateTime.UtcNow;
    public bool IsLowStock(int threshold = 10) => Quantity <= threshold;

    public static Result<MedicationInventory> Create(
        string medicationName,
        string batchNumber,
        int quantity,
        DateTime expiryDate,
        Guid pharmacyId,
        decimal? unitPrice = null,
        string? manufacturer = null)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
            return Result.Failure<MedicationInventory>(
                Error.Validation(nameof(MedicationName), "Medication name is required"));

        if (string.IsNullOrWhiteSpace(batchNumber))
            return Result.Failure<MedicationInventory>(
                Error.Validation(nameof(BatchNumber), "Batch number is required"));

        if (quantity < 0)
            return Result.Failure<MedicationInventory>(
                Error.Validation(nameof(Quantity), "Quantity cannot be negative"));

        if (expiryDate <= DateTime.UtcNow)
            return Result.Failure<MedicationInventory>(
                Error.Validation(nameof(ExpiryDate), "Expiry date must be in the future"));

        var inventory = new MedicationInventory(
            Guid.NewGuid(),
            medicationName,
            batchNumber,
            quantity,
            expiryDate,
            pharmacyId)
        {
            UnitPrice = unitPrice,
            Manufacturer = manufacturer
        };

        return inventory;
    }

    public Result AddStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Validation(nameof(quantity), "Quantity must be positive"));

        if (IsExpired)
            return Result.Failure(Error.Validation(nameof(ExpiryDate), "Cannot add stock to expired batch"));

        Quantity += quantity;
        return Result.Success();
    }

    public Result ReduceStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Validation(nameof(quantity), "Quantity must be positive"));

        if (Quantity < quantity)
            return Result.Failure(Error.Validation(nameof(Quantity), "Insufficient stock"));

        Quantity -= quantity;
        return Result.Success();
    }
}
