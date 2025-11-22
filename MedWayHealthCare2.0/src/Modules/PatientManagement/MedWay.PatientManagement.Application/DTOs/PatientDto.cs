namespace MedWay.PatientManagement.Application.DTOs;

public sealed record PatientDto(
    Guid Id,
    string MRN,
    string FirstName,
    string LastName,
    string FullName,
    DateTime DateOfBirth,
    int Age,
    string Gender,
    string Email,
    string PhoneNumber,
    AddressDto Address,
    Guid BranchId,
    DateTime RegistrationDate,
    bool IsActive,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Allergies,
    string? MedicalHistory);

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
