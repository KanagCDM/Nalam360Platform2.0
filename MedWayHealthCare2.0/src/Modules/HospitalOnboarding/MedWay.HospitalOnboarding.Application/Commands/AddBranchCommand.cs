using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.Domain.ValueObjects;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Nalam360.Platform.Core.Abstractions;

namespace MedWay.HospitalOnboarding.Application.Commands;

/// <summary>
/// Command to add a new branch to a hospital.
/// Hospital admin can add branches to their hospital.
/// </summary>
public record AddBranchCommand(
    Guid HospitalId,
    string Name,
    string BranchCode,
    string AddressLine1,
    string AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Phone,
    string Email,
    DateTime OpeningDate,
    string? OperatingHours) : ICommand<Result<Guid>>;

public class AddBranchHandler : IRequestHandler<AddBranchCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<AddBranchHandler> _logger;

    public AddBranchHandler(
        IUnitOfWork unitOfWork,
        IGuidProvider guidProvider,
        ILogger<AddBranchHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddBranchCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding branch {BranchCode} to hospital {HospitalId}", 
            command.BranchCode, command.HospitalId);

        // Retrieve hospital with branches
        var hospital = await _unitOfWork.Hospitals.GetWithBranchesAsync(command.HospitalId, cancellationToken);
        if (hospital == null)
        {
            _logger.LogWarning("Hospital {HospitalId} not found", command.HospitalId);
            return Result<Guid>.Failure(Error.NotFound("Hospital", command.HospitalId));
        }

        // Check if branch code is unique within hospital
        var existingBranch = await _unitOfWork.Branches.GetByBranchCodeAsync(
            command.HospitalId, command.BranchCode, cancellationToken);
        
        if (existingBranch != null)
        {
            _logger.LogWarning("Branch code {BranchCode} already exists for hospital {HospitalId}", 
                command.BranchCode, command.HospitalId);
            return Result<Guid>.Failure(Error.Conflict("BranchCode", 
                $"Branch code '{command.BranchCode}' already exists for this hospital"));
        }

        // Create value objects
        var addressResult = Address.Create(
            command.AddressLine1,
            command.AddressLine2,
            command.City,
            command.State,
            command.PostalCode,
            command.Country);

        if (addressResult.IsFailure)
            return Result<Guid>.Failure(addressResult.Error);

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result<Guid>.Failure(phoneResult.Error);

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result<Guid>.Failure(emailResult.Error);

        // Create branch entity
        var branchId = _guidProvider.NewGuid();
        var branchResult = Branch.Create(
            branchId,
            command.HospitalId,
            command.Name,
            command.BranchCode,
            addressResult.Value,
            phoneResult.Value,
            emailResult.Value,
            command.OpeningDate,
            command.OperatingHours);

        if (branchResult.IsFailure)
            return Result<Guid>.Failure(branchResult.Error);

        var branch = branchResult.Value;

        // Add branch to hospital
        var addResult = hospital.AddBranch(branch);
        if (addResult.IsFailure)
            return Result<Guid>.Failure(addResult.Error);

        // Persist changes
        await _unitOfWork.Branches.AddAsync(branch, cancellationToken);
        _unitOfWork.Hospitals.Update(hospital);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Branch {BranchId} added successfully to hospital {HospitalId}", 
            branch.Id, command.HospitalId);

        // TODO: Update subscription cost if additional branch exceeds plan limit
        // TODO: Publish BranchAddedEvent to integration event bus

        return Result<Guid>.Success(branch.Id);
    }
}
