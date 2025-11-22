using MediatR;
using MedWay.Domain.Primitives;
using MedWay.PatientManagement.Application.DTOs;
using MedWay.PatientManagement.Application.Queries;
using MedWay.PatientManagement.Domain.Entities;
using MedWay.PatientManagement.Domain.ValueObjects;

namespace MedWay.PatientManagement.Application.Handlers;

public sealed class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientByIdHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Result<PatientDto>> Handle(
        GetPatientByIdQuery query,
        CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(
            query.PatientId,
            cancellationToken);

        if (patient is null)
            return Result.Failure<PatientDto>(Error.NotFound("Patient", query.PatientId));

        return Result.Success(MapToDto(patient));
    }

    private static PatientDto MapToDto(Patient patient) => new(
        patient.Id,
        patient.MRN.Value,
        patient.FirstName,
        patient.LastName,
        patient.FullName,
        patient.DateOfBirth,
        patient.Age,
        patient.Gender.ToString(),
        patient.Email.Value,
        patient.PhoneNumber.Value,
        new AddressDto(
            patient.Address.Street,
            patient.Address.City,
            patient.Address.State,
            patient.Address.PostalCode,
            patient.Address.Country),
        patient.BranchId,
        patient.RegistrationDate,
        patient.IsActive,
        patient.BloodType,
        patient.EmergencyContactName,
        patient.EmergencyContactPhone?.Value,
        patient.Allergies,
        patient.MedicalHistory);
}

public sealed class GetPatientByMRNHandler : IRequestHandler<GetPatientByMRNQuery, Result<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientByMRNHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Result<PatientDto>> Handle(
        GetPatientByMRNQuery query,
        CancellationToken cancellationToken)
    {
        var mrnResult = MedicalRecordNumber.Create(query.MRN);
        if (mrnResult.IsFailure)
            return Result.Failure<PatientDto>(mrnResult.Error);

        var patient = await _patientRepository.GetByMRNAsync(
            mrnResult.Value,
            cancellationToken);

        if (patient is null)
            return Result.Failure<PatientDto>(Error.NotFound("Patient", query.MRN));

        return Result.Success(MapToDto(patient));
    }

    private static PatientDto MapToDto(Patient patient) => new(
        patient.Id,
        patient.MRN.Value,
        patient.FirstName,
        patient.LastName,
        patient.FullName,
        patient.DateOfBirth,
        patient.Age,
        patient.Gender.ToString(),
        patient.Email.Value,
        patient.PhoneNumber.Value,
        new AddressDto(
            patient.Address.Street,
            patient.Address.City,
            patient.Address.State,
            patient.Address.PostalCode,
            patient.Address.Country),
        patient.BranchId,
        patient.RegistrationDate,
        patient.IsActive,
        patient.BloodType,
        patient.EmergencyContactName,
        patient.EmergencyContactPhone?.Value,
        patient.Allergies,
        patient.MedicalHistory);
}
