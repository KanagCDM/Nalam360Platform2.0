using MedWay.Application.Abstractions;
using MedWay.Domain.Primitives;

namespace MedWay.PatientManagement.Application.Commands;

public sealed record UpdatePatientCommand(
    Guid PatientId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country) : ICommand;
