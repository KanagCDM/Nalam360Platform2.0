using MedWay.Application.Abstractions;
using MedWay.Domain.Primitives;
using MedWay.Domain.ValueObjects;

namespace MedWay.PatientManagement.Application.Commands;

public sealed record RegisterPatientCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    int Gender,
    string Email,
    string PhoneNumber,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    Guid BranchId,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Allergies,
    string? MedicalHistory) : ICommand<Result<Guid>>;
