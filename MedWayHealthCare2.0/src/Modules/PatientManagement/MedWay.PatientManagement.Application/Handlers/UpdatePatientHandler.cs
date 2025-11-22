using MediatR;
using MedWay.Domain.Primitives;
using MedWay.Domain.ValueObjects;
using MedWay.PatientManagement.Application.Commands;

namespace MedWay.PatientManagement.Application.Handlers;

public sealed class UpdatePatientHandler : IRequestHandler<UpdatePatientCommand, Result>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePatientHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdatePatientCommand command,
        CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(
            command.PatientId,
            cancellationToken);

        if (patient is null)
            return Result.Failure(Error.NotFound("Patient", command.PatientId));

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        var phoneResult = PhoneNumber.Create(command.PhoneNumber);
        if (phoneResult.IsFailure)
            return Result.Failure(phoneResult.Error);

        var addressResult = Address.Create(
            command.Street,
            command.City,
            command.State,
            command.PostalCode,
            command.Country);
        if (addressResult.IsFailure)
            return Result.Failure(addressResult.Error);

        var updateResult = patient.Update(
            command.FirstName,
            command.LastName,
            emailResult.Value,
            phoneResult.Value,
            addressResult.Value);

        if (updateResult.IsFailure)
            return updateResult;

        _patientRepository.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
