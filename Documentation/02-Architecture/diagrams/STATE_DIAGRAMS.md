# Nalam360 Enterprise Platform - State Machine Diagrams

This document contains state transition diagrams for key business processes and workflows.

**Format:** Mermaid  
**Date:** November 18, 2025

---

## Table of Contents

1. [Order Lifecycle](#1-order-lifecycle)
2. [Payment Processing](#2-payment-processing)
3. [User Registration](#3-user-registration)
4. [Feature Flag Lifecycle](#4-feature-flag-lifecycle)
5. [Outbox Message Processing](#5-outbox-message-processing)
6. [Circuit Breaker States](#6-circuit-breaker-states)
7. [Tenant Activation](#7-tenant-activation)

---

## 1. Order Lifecycle

**Description:** State transitions for order processing from creation to completion.

```mermaid
stateDiagram-v2
    [*] --> Draft: Create Order
    
    Draft --> PendingValidation: Submit
    Draft --> Cancelled: Cancel
    
    PendingValidation --> PendingPayment: Validation Passed
    PendingValidation --> ValidationFailed: Validation Failed
    
    ValidationFailed --> Draft: Fix Issues
    ValidationFailed --> Cancelled: Cancel
    
    PendingPayment --> PaymentProcessing: Initiate Payment
    PendingPayment --> Cancelled: Timeout/Cancel
    
    PaymentProcessing --> PaymentFailed: Payment Declined
    PaymentProcessing --> Confirmed: Payment Successful
    
    PaymentFailed --> PendingPayment: Retry Payment
    PaymentFailed --> Cancelled: Max Retries Exceeded
    
    Confirmed --> Processing: Start Fulfillment
    
    Processing --> Shipped: Ship Items
    Processing --> PartiallyShipped: Ship Some Items
    Processing --> Cancelled: Cancel Before Ship
    
    PartiallyShipped --> Shipped: Ship Remaining
    PartiallyShipped --> Cancelled: Cancel Remainder
    
    Shipped --> InTransit: Out for Delivery
    
    InTransit --> Delivered: Successful Delivery
    InTransit --> DeliveryFailed: Delivery Failed
    
    DeliveryFailed --> InTransit: Retry Delivery
    DeliveryFailed --> Returned: Max Attempts Exceeded
    
    Delivered --> Completed: Customer Confirms
    Delivered --> Returned: Return Requested
    
    Returned --> Refunded: Process Refund
    Refunded --> [*]
    
    Completed --> [*]
    Cancelled --> [*]
    
    note right of PendingValidation
        Validate inventory,
        customer credit,
        shipping address
    end note
    
    note right of PaymentProcessing
        Authorize payment,
        capture funds,
        send confirmation
    end note
    
    note right of Shipped
        Generate tracking,
        notify customer,
        update inventory
    end note
```

---

## 2. Payment Processing

**Description:** Payment transaction state machine with retry logic.

```mermaid
stateDiagram-v2
    [*] --> Initiated: Create Transaction
    
    Initiated --> Authorizing: Request Authorization
    
    Authorizing --> Authorized: Auth Approved
    Authorizing --> AuthFailed: Auth Declined
    Authorizing --> Timeout: Gateway Timeout
    
    Timeout --> Authorizing: Retry (max 3)
    Timeout --> Failed: Max Retries
    
    AuthFailed --> Failed: Permanent Failure
    
    Authorized --> Capturing: Capture Funds
    
    Capturing --> Captured: Capture Success
    Capturing --> CaptureFailed: Capture Failed
    
    CaptureFailed --> Voiding: Void Authorization
    
    Captured --> Settled: Funds Settled
    Captured --> Refunding: Refund Requested
    
    Refunding --> Refunded: Refund Success
    Refunding --> RefundFailed: Refund Failed
    
    RefundFailed --> Refunding: Retry Refund
    RefundFailed --> ManualReview: Escalate
    
    Voiding --> Voided: Void Success
    Voiding --> VoidFailed: Void Failed
    
    VoidFailed --> ManualReview: Escalate
    
    Settled --> [*]
    Refunded --> [*]
    Voided --> [*]
    Failed --> [*]
    
    ManualReview --> Refunded: Manual Refund
    ManualReview --> Settled: Resolve Issue
    ManualReview --> [*]: Close Case
    
    note right of Authorized
        Hold period:
        7-30 days
        depending on card
    end note
    
    note right of Captured
        Final charge
        to customer
        cannot be reversed
    end note
```

---

## 3. User Registration

**Description:** User account creation and activation flow.

```mermaid
stateDiagram-v2
    [*] --> Started: Begin Registration
    
    Started --> ValidatingInput: Submit Form
    
    ValidatingInput --> ValidationFailed: Invalid Data
    ValidatingInput --> CheckingEmail: Valid Data
    
    ValidationFailed --> Started: Fix Errors
    
    CheckingEmail --> EmailExists: Duplicate Email
    CheckingEmail --> CreatingAccount: Email Available
    
    EmailExists --> Started: Use Different Email
    EmailExists --> InitiateReset: Forgot Password
    
    CreatingAccount --> PendingEmailVerification: Account Created
    
    PendingEmailVerification --> VerifyingEmail: Click Email Link
    PendingEmailVerification --> ResendingEmail: Resend Link
    PendingEmailVerification --> Expired: Link Expired (24h)
    
    ResendingEmail --> PendingEmailVerification: New Link Sent
    
    VerifyingEmail --> EmailVerified: Valid Token
    VerifyingEmail --> InvalidToken: Invalid/Expired
    
    InvalidToken --> PendingEmailVerification: Request New Link
    
    EmailVerified --> SettingUpProfile: Set Display Name
    
    SettingUpProfile --> Active: Profile Complete
    SettingUpProfile --> EmailVerified: Save for Later
    
    Active --> Suspended: Violation
    Active --> Inactive: User Deactivates
    
    Suspended --> Active: Appeal Approved
    Suspended --> Banned: Permanent Ban
    
    Inactive --> Active: Reactivate
    
    Expired --> Started: Start Over
    Banned --> [*]
    
    InitiateReset --> ResetPassword: Send Reset Email
    ResetPassword --> Active: Password Reset
    
    note right of PendingEmailVerification
        Email verification
        required for security
        and spam prevention
    end note
    
    note right of Active
        Full access to
        platform features
    end note
```

---

## 4. Feature Flag Lifecycle

**Description:** Feature flag rollout and retirement process.

```mermaid
stateDiagram-v2
    [*] --> Draft: Create Flag
    
    Draft --> Testing: Deploy to Test
    Draft --> Deleted: Discard
    
    Testing --> Draft: Fix Issues
    Testing --> Canary: Test Passed
    
    Canary --> Testing: Rollback (Issues)
    Canary --> Rollout: Canary Success
    
    Rollout --> PartialRollout: 10% Users
    
    PartialRollout --> Rollout: Increase to 25%
    PartialRollout --> Canary: Rollback
    
    Rollout --> PartialRollout: 25% Users
    PartialRollout --> Rollout: Increase to 50%
    
    Rollout --> PartialRollout: 50% Users
    PartialRollout --> Rollout: Increase to 75%
    
    Rollout --> FullRollout: 100% Users
    
    FullRollout --> Permanent: Remove Flag (Code Cleanup)
    FullRollout --> Rollout: Issues Found
    
    Permanent --> [*]
    Deleted --> [*]
    
    state PartialRollout {
        [*] --> Monitoring
        Monitoring --> EvaluatingMetrics
        EvaluatingMetrics --> [*]
    }
    
    note right of Canary
        5% of users
        Heavy monitoring
        Quick rollback
    end note
    
    note right of FullRollout
        Flag always true
        Ready for removal
        Technical debt cleanup
    end note
```

---

## 5. Outbox Message Processing

**Description:** Reliable message delivery with retry logic.

```mermaid
stateDiagram-v2
    [*] --> Pending: Save to Outbox
    
    Pending --> Publishing: Processor Picks Up
    
    Publishing --> Published: Publish Success
    Publishing --> Failed: Publish Failed
    Publishing --> Timeout: Gateway Timeout
    
    Timeout --> Retrying: Retry (Exponential Backoff)
    Failed --> Retrying: Retry (Exponential Backoff)
    
    Retrying --> Publishing: Attempt Retry
    Retrying --> DeadLetter: Max Retries (5)
    
    Published --> Completed: Ack Received
    
    DeadLetter --> ManualReview: Alert Sent
    
    ManualReview --> Retrying: Fix & Retry
    ManualReview --> Cancelled: Discard
    
    Completed --> Archived: After 30 Days
    Archived --> [*]
    Cancelled --> [*]
    
    note right of Retrying
        Retry schedule:
        1s, 2s, 4s, 8s, 16s
        Then dead letter
    end note
    
    note right of DeadLetter
        Human intervention
        required for
        message recovery
    end note
```

---

## 6. Circuit Breaker States

**Description:** Circuit breaker pattern state transitions for resilience.

```mermaid
stateDiagram-v2
    [*] --> Closed: Initialize
    
    Closed --> Open: Threshold Reached
    
    state Closed {
        [*] --> Healthy
        Healthy --> MonitoringFailures: Request Failed
        MonitoringFailures --> Healthy: Request Succeeded
        MonitoringFailures --> [*]: Failure Count < Threshold
    }
    
    Open --> HalfOpen: Wait Period Elapsed
    
    state Open {
        [*] --> Blocking
        Blocking --> WaitingForCooldown: Time Delay
        WaitingForCooldown --> [*]
        
        note right of Blocking
            All requests fail fast
            Return cached data
            or error response
        end note
    }
    
    HalfOpen --> Closed: Test Success
    HalfOpen --> Open: Test Failed
    
    state HalfOpen {
        [*] --> TestingConnection
        TestingConnection --> EvaluatingResult
        EvaluatingResult --> [*]
        
        note right of TestingConnection
            Allow single request
            to test if service
            is healthy again
        end note
    }
    
    note left of Open
        Break duration: 30s
        Prevents cascade
        failures
    end note
    
    note left of Closed
        Failure threshold: 5
        Time window: 60s
        Normal operation
    end note
```

---

## 7. Tenant Activation

**Description:** Tenant onboarding and subscription lifecycle.

```mermaid
stateDiagram-v2
    [*] --> PendingRegistration: Start Signup
    
    PendingRegistration --> PendingPayment: Complete Form
    
    PendingPayment --> PaymentProcessing: Submit Payment
    PendingPayment --> Cancelled: Abandon Signup
    
    PaymentProcessing --> Provisioning: Payment Success
    PaymentProcessing --> PaymentFailed: Payment Failed
    
    PaymentFailed --> PendingPayment: Retry
    PaymentFailed --> Cancelled: Max Retries
    
    Provisioning --> Active: Provisioning Complete
    
    state Provisioning {
        [*] --> CreateDatabase
        CreateDatabase --> CreateStorage
        CreateStorage --> CreateAdmin
        CreateAdmin --> SendWelcome
        SendWelcome --> [*]
    }
    
    Active --> GracePeriod: Subscription Expired
    Active --> Suspended: Payment Failed
    Active --> Cancelled: User Cancels
    
    GracePeriod --> Active: Payment Received
    GracePeriod --> Suspended: Grace Period Expired
    
    Suspended --> Active: Payment Received
    Suspended --> Terminated: Suspended > 90 Days
    
    Terminated --> [*]: Data Deleted
    Cancelled --> [*]: Data Exported
    
    note right of Active
        Full access
        All features enabled
        Regular billing
    end note
    
    note right of GracePeriod
        7 days grace
        Limited features
        Warning emails
    end note
    
    note right of Terminated
        Data retention:
        90 days in cold storage
        Then permanently deleted
    end note
```

---

## State Transition Rules

### General Principles

1. **Explicit Transitions**: Every state change must be explicit and intentional
2. **Event-Driven**: State changes triggered by events (commands, time, external signals)
3. **Idempotent**: Multiple identical events should not cause issues
4. **Audited**: All state transitions logged with timestamp, user, and reason
5. **Reversible**: Critical states allow rollback (within constraints)

### Implementation Pattern

```csharp
public class OrderStateMachine
{
    public Result<Order> TransitionTo(OrderStatus targetStatus, Order order)
    {
        if (!CanTransitionTo(order.Status, targetStatus))
            return Result.Failure<Order>(
                Error.Validation("Invalid state transition"));
        
        var previousStatus = order.Status;
        order.Status = targetStatus;
        
        order.AddDomainEvent(new OrderStatusChangedEvent(
            order.Id, previousStatus, targetStatus));
        
        return Result.Success(order);
    }
    
    private bool CanTransitionTo(OrderStatus from, OrderStatus to)
    {
        return _validTransitions[from].Contains(to);
    }
}
```

---

**Document Version:** 1.0  
**Last Updated:** November 18, 2025  
**Maintained By:** Nalam360 Platform Team
