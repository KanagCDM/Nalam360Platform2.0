using FluentAssertions;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using MedWay.HospitalOnboarding.Domain.Repositories;
using Moq;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Integration;

/// <summary>
/// Integration test simulating the complete hospital onboarding workflow
/// </summary>
public class HospitalOnboardingWorkflowTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public HospitalOnboardingWorkflowTests()
    {
        _hospitalRepositoryMock = new Mock<IHospitalRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task CompleteOnboardingWorkflow_ShouldSucceed()
    {
        // ============================================================
        // STEP 1: Hospital Registration (Trial)
        // ============================================================
        
        var trialPlan = SubscriptionPlan.Create(
            "Trial Plan",
            "TRIAL",
            "30-day free trial",
            0m,
            5,
            1,
            10,
            3,
            0m,
            0m,
            true,
            new List<Guid>()
        );

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetTrialPlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(trialPlan);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var registerHandler = new RegisterHospitalCommandHandler(
            _hospitalRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var registerCommand = new RegisterHospitalCommand(
            "City General Hospital",
            "CGH001",
            "info@citygeneral.com",
            "+1234567890",
            "123 Main St",
            null,
            "New York",
            "NY",
            "10001",
            "USA",
            "Dr. John Smith",
            "director@citygeneral.com",
            "+1234567891"
        );

        var registerResult = await registerHandler.Handle(registerCommand, CancellationToken.None);

        // Assert registration successful
        registerResult.IsSuccess.Should().BeTrue();
        var hospitalId = registerResult.Value;
        hospitalId.Should().NotBeEmpty();

        // ============================================================
        // STEP 2: Admin Approval
        // ============================================================

        var hospital = Hospital.Create(
            registerCommand.Name,
            registerCommand.RegistrationNumber,
            registerCommand.Email,
            registerCommand.Phone,
            registerCommand.AddressLine1,
            registerCommand.AddressLine2,
            registerCommand.City,
            registerCommand.State,
            registerCommand.PostalCode,
            registerCommand.Country,
            registerCommand.PrimaryContactName,
            registerCommand.PrimaryContactEmail,
            registerCommand.PrimaryContactPhone,
            trialPlan.Id
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hospital);

        var approveHandler = new ApproveHospitalCommandHandler(
            _hospitalRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var approveCommand = new ApproveHospitalCommand(hospitalId, "admin@system.com");
        var approveResult = await approveHandler.Handle(approveCommand, CancellationToken.None);

        // Assert approval successful
        approveResult.IsSuccess.Should().BeTrue();
        hospital.Status.Should().Be(HospitalStatus.Active);
        hospital.ApprovedBy.Should().Be("admin@system.com");
        hospital.ApprovedAt.Should().NotBeNull();

        // ============================================================
        // STEP 3: Add Branch
        // ============================================================

        _branchRepositoryMock
            .Setup(x => x.ExistsByBranchCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var addBranchHandler = new AddBranchCommandHandler(
            _hospitalRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var addBranchCommand = new AddBranchCommand(
            hospitalId,
            "Downtown Branch",
            "CGH-DT",
            "456 Oak Ave",
            null,
            "New York",
            "NY",
            "10002",
            "USA",
            "+1234567892",
            "downtown@citygeneral.com",
            DateTime.UtcNow.AddDays(30),
            "Mon-Fri: 8AM-6PM"
        );

        var addBranchResult = await addBranchHandler.Handle(addBranchCommand, CancellationToken.None);

        // Assert branch added successfully
        addBranchResult.IsSuccess.Should().BeTrue();
        var branchId = addBranchResult.Value;
        branchId.Should().NotBeEmpty();

        // ============================================================
        // STEP 4: Activate Paid Subscription
        // ============================================================

        var professionalPlan = SubscriptionPlan.Create(
            "Professional Plan",
            "PRO",
            "For medium hospitals",
            299m,
            10,
            1,
            50,
            5,
            10m,
            50m,
            true,
            new List<Guid>()
        );

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetByIdAsync(professionalPlan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professionalPlan);

        var activateSubscriptionHandler = new ActivateSubscriptionCommandHandler(
            _hospitalRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var activateSubscriptionCommand = new ActivateSubscriptionCommand(
            hospitalId,
            professionalPlan.Id,
            10, // users
            2,  // branches
            new List<Guid>() // no additional facilities
        );

        var activateSubscriptionResult = await activateSubscriptionHandler.Handle(
            activateSubscriptionCommand, 
            CancellationToken.None);

        // Assert subscription activated successfully
        activateSubscriptionResult.IsSuccess.Should().BeTrue();
        hospital.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        hospital.CurrentSubscriptionPlanId.Should().Be(professionalPlan.Id);

        // ============================================================
        // STEP 5: Process Payment
        // ============================================================

        var processPaymentHandler = new ProcessPaymentCommandHandler(
            _hospitalRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var processPaymentCommand = new ProcessPaymentCommand(
            hospitalId,
            professionalPlan.Id,
            349m, // base $299 + 1 extra branch @ $50
            "USD",
            PaymentMethod.CreditCard,
            "Stripe",
            new Dictionary<string, string>
            {
                { "card_last4", "4242" },
                { "card_brand", "Visa" }
            }
        );

        var processPaymentResult = await processPaymentHandler.Handle(
            processPaymentCommand, 
            CancellationToken.None);

        // Assert payment processed successfully
        processPaymentResult.IsSuccess.Should().BeTrue();
        var paymentId = processPaymentResult.Value;
        paymentId.Should().NotBeEmpty();

        // ============================================================
        // STEP 6: Verify Final State
        // ============================================================

        // Hospital should be:
        hospital.Status.Should().Be(HospitalStatus.Active);
        hospital.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        hospital.ApprovedAt.Should().NotBeNull();
        hospital.SubscriptionStartDate.Should().NotBeNull();

        // Verify all service calls were made
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.AtLeast(5)); // Register, Approve, AddBranch, Subscribe, Payment

        _hospitalRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Hospital>(), It.IsAny<CancellationToken>()), 
            Times.Once);

        _branchRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()), 
            Times.Once);

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task OnboardingWorkflow_WithRejection_ShouldStopProcess()
    {
        // ============================================================
        // STEP 1: Hospital Registration
        // ============================================================
        
        var trialPlan = SubscriptionPlan.Create(
            "Trial Plan",
            "TRIAL",
            "30-day free trial",
            0m,
            5,
            1,
            10,
            3,
            0m,
            0m,
            true,
            new List<Guid>()
        );

        _subscriptionPlanRepositoryMock
            .Setup(x => x.GetTrialPlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(trialPlan);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hospitalRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var registerHandler = new RegisterHospitalCommandHandler(
            _hospitalRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var registerCommand = new RegisterHospitalCommand(
            "Suspicious Hospital",
            "SUS001",
            "info@suspicious.com",
            "+1234567890",
            "123 Main St",
            null,
            "New York",
            "NY",
            "10001",
            "USA",
            "Unknown Person",
            "unknown@suspicious.com",
            "+1234567891"
        );

        var registerResult = await registerHandler.Handle(registerCommand, CancellationToken.None);
        registerResult.IsSuccess.Should().BeTrue();
        var hospitalId = registerResult.Value;

        // ============================================================
        // STEP 2: Admin Rejection
        // ============================================================

        var hospital = Hospital.Create(
            registerCommand.Name,
            registerCommand.RegistrationNumber,
            registerCommand.Email,
            registerCommand.Phone,
            registerCommand.AddressLine1,
            registerCommand.AddressLine2,
            registerCommand.City,
            registerCommand.State,
            registerCommand.PostalCode,
            registerCommand.Country,
            registerCommand.PrimaryContactName,
            registerCommand.PrimaryContactEmail,
            registerCommand.PrimaryContactPhone,
            trialPlan.Id
        );

        _hospitalRepositoryMock
            .Setup(x => x.GetByIdAsync(hospitalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hospital);

        var rejectHandler = new RejectHospitalCommandHandler(
            _hospitalRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var rejectCommand = new RejectHospitalCommand(
            hospitalId, 
            "Invalid documentation provided", 
            "admin@system.com");

        var rejectResult = await rejectHandler.Handle(rejectCommand, CancellationToken.None);

        // Assert rejection successful
        rejectResult.IsSuccess.Should().BeTrue();
        hospital.Status.Should().Be(HospitalStatus.Rejected);
        hospital.RejectionReason.Should().Be("Invalid documentation provided");

        // ============================================================
        // STEP 3: Verify No Further Actions Possible
        // ============================================================

        // Attempting to approve a rejected hospital should fail
        var approveHandler = new ApproveHospitalCommandHandler(
            _hospitalRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var approveCommand = new ApproveHospitalCommand(hospitalId, "admin@system.com");
        var approveResult = await approveHandler.Handle(approveCommand, CancellationToken.None);

        approveResult.IsFailure.Should().BeTrue();
        approveResult.Error.Message.Should().Contain("rejected");
    }
}
