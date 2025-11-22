using MediatR;
using MedWay.Domain.Primitives;
using MedWay.Domain.ValueObjects;
using MedWay.PatientManagement.Application.Commands;
using MedWay.PatientManagement.Domain.Entities;
using MedWay.PatientManagement.Domain.ValueObjects;

namespace MedWay.PatientManagement.Application.Handlers;

public sealed class RegisterPatientHandler : IRequestHandler<RegisterPatientCommand, Result<Guid>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterPatientHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        RegisterPatientCommand command,
        CancellationToken cancellationToken)
    {
        // Create value objects
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        var phoneResult = PhoneNumber.Create(command.PhoneNumber);
        if (phoneResult.IsFailure)
            return Result.Failure<Guid>(phoneResult.Error);

        var addressResult = Address.Create(
            command.Street,
            command.City,
            command.State,
            command.PostalCode,
            command.Country);
        if (addressResult.IsFailure)
            return Result.Failure<Guid>(addressResult.Error);

        // Check if email already exists
        var existingPatient = await _patientRepository.GetByEmailAsync(
            emailResult.Value,
            cancellationToken);

        if (existingPatient is not null)
            return Result.Failure<Guid>(
                Error.Conflict("A patient with this email already exists"));

        // Generate MRN
        var branchCode = await GetBranchCodeAsync(command.BranchId, cancellationToken);
        var sequence = await _patientRepository.GetNextSequenceAsync(cancellationToken);
        var mrnResult = MedicalRecordNumber.Generate(branchCode, sequence);
        if (mrnResult.IsFailure)
            return Result.Failure<Guid>(mrnResult.Error);

        // Create patient
        var patientResult = Patient.Create(
            mrnResult.Value,
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            (Gender)command.Gender,
            emailResult.Value,
            phoneResult.Value,
            addressResult.Value,
            command.BranchId);

        if (patientResult.IsFailure)
            return Result.Failure<Guid>(patientResult.Error);

        var patient = patientResult.Value;

        // Update medical info if provided
        if (!string.IsNullOrWhiteSpace(command.EmergencyContactPhone))
        {
            var emergencyPhoneResult = PhoneNumber.Create(command.EmergencyContactPhone);
            if (emergencyPhoneResult.IsSuccess)
            {
                patient.UpdateMedicalInfo(
                    command.BloodType,
                    command.EmergencyContactName,
                    emergencyPhoneResult.Value,
                    command.Allergies,
                    command.MedicalHistory);
            }
        }
        else
        {
            patient.UpdateMedicalInfo(
                command.BloodType,
                command.EmergencyContactName,
                null,
                command.Allergies,
                command.MedicalHistory);
        }

        // Save patient
        await _patientRepository.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(patient.Id);
    }

    private async Task<string> GetBranchCodeAsync(Guid branchId, CancellationToken cancellationToken)
    {
        // TODO: Implement branch repository to get actual branch code
        // For now, return a default
        await Task.CompletedTask;
        return "HQ1";
    }
}

/// <summary>
/// Patient repository interface
/// </summary>
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Patient?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<Patient?> GetByMRNAsync(MedicalRecordNumber mrn, CancellationToken cancellationToken = default);
    Task<int> GetNextSequenceAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    void Update(Patient patient);
}

/// <summary>
/// Unit of Work interface
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
