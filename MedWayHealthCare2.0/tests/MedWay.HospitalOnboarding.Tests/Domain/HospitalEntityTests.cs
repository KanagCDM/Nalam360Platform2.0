using FluentAssertions;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Enums;
using Xunit;

namespace MedWay.HospitalOnboarding.Tests.Domain;

public class HospitalEntityTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateHospital()
    {
        // Act
        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        // Assert
        hospital.Should().NotBeNull();
        hospital.Name.Should().Be("City General Hospital");
        hospital.RegistrationNumber.Should().Be("CGH001");
        hospital.Status.Should().Be(HospitalStatus.PendingApproval);
        hospital.SubscriptionStatus.Should().Be(SubscriptionStatus.Trial);
        hospital.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Approve_WhenPendingApproval_ShouldSetStatusToActive()
    {
        // Arrange
        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        // Act
        hospital.Approve("admin@system.com");

        // Assert
        hospital.Status.Should().Be(HospitalStatus.Active);
        hospital.ApprovedBy.Should().Be("admin@system.com");
        hospital.ApprovedAt.Should().NotBeNull();
        hospital.ApprovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Reject_WhenPendingApproval_ShouldSetStatusToRejected()
    {
        // Arrange
        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        // Act
        hospital.Reject("Invalid documents", "admin@system.com");

        // Assert
        hospital.Status.Should().Be(HospitalStatus.Rejected);
        hospital.RejectionReason.Should().Be("Invalid documents");
        hospital.RejectedBy.Should().Be("admin@system.com");
        hospital.RejectedAt.Should().NotBeNull();
    }

    [Fact]
    public void Suspend_WhenActive_ShouldSetStatusToSuspended()
    {
        // Arrange
        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        hospital.Approve("admin@system.com");

        // Act
        hospital.Suspend("Payment overdue", "admin@system.com");

        // Assert
        hospital.Status.Should().Be(HospitalStatus.Suspended);
        hospital.SuspensionReason.Should().Be("Payment overdue");
        hospital.SuspendedBy.Should().Be("admin@system.com");
        hospital.SuspendedAt.Should().NotBeNull();
    }

    [Fact]
    public void ActivateSubscription_ShouldSetSubscriptionDetails()
    {
        // Arrange
        var hospital = Hospital.Create(
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
            "+1234567891",
            Guid.NewGuid()
        );

        var planId = Guid.NewGuid();

        // Act
        hospital.ActivateSubscription(planId);

        // Assert
        hospital.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        hospital.CurrentSubscriptionPlanId.Should().Be(planId);
        hospital.SubscriptionStartDate.Should().NotBeNull();
        hospital.SubscriptionStartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
