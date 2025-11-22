-- ============================================================================
-- MedWay Hospital Onboarding - SQL Server Schema
-- Multi-Tenant SaaS Architecture
-- Version: 1.0
-- Created: November 22, 2025
-- ============================================================================

USE [MedWayHealthCare];
GO

-- ============================================================================
-- SCHEMA CREATION
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'hospital_onboarding')
BEGIN
    EXEC('CREATE SCHEMA hospital_onboarding');
END
GO

-- ============================================================================
-- CORE TABLES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Hospitals Table (Tenant Root)
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.Hospitals', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.Hospitals;
GO

CREATE TABLE hospital_onboarding.Hospitals (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL UNIQUE, -- For multi-tenant isolation
    
    -- Registration Information
    RegistrationNumber NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    LegalName NVARCHAR(200) NOT NULL,
    
    -- Contact Information
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NOT NULL,
    AlternatePhone NVARCHAR(20) NULL,
    Website NVARCHAR(200) NULL,
    
    -- Address
    AddressLine1 NVARCHAR(200) NOT NULL,
    AddressLine2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL DEFAULT 'India',
    PostalCode NVARCHAR(20) NOT NULL,
    
    -- Business Information
    TaxId NVARCHAR(50) NULL,
    LicenseNumber NVARCHAR(100) NULL,
    AccreditationDetails NVARCHAR(MAX) NULL, -- JSON format
    
    -- Status & Workflow
    Status NVARCHAR(50) NOT NULL DEFAULT 'PendingApproval',
        CONSTRAINT CHK_Hospital_Status CHECK (Status IN ('PendingApproval', 'Active', 'Rejected', 'Suspended', 'Inactive')),
    ApprovedAt DATETIMEOFFSET NULL,
    ApprovedBy UNIQUEIDENTIFIER NULL,
    RejectedAt DATETIMEOFFSET NULL,
    RejectedBy UNIQUEIDENTIFIER NULL,
    RejectionReason NVARCHAR(MAX) NULL,
    SuspendedAt DATETIMEOFFSET NULL,
    SuspendedBy UNIQUEIDENTIFIER NULL,
    SuspensionReason NVARCHAR(MAX) NULL,
    
    -- Subscription Information
    SubscriptionStatus NVARCHAR(50) NOT NULL DEFAULT 'Trial',
        CONSTRAINT CHK_Hospital_SubscriptionStatus CHECK (SubscriptionStatus IN ('Trial', 'Active', 'Expired', 'Suspended', 'Cancelled')),
    SubscriptionPlanId UNIQUEIDENTIFIER NULL,
    SubscriptionStartDate DATETIMEOFFSET NULL,
    SubscriptionEndDate DATETIMEOFFSET NULL,
    TrialEndDate DATETIMEOFFSET NULL,
    
    -- Capacity & Billing
    TotalBeds INT DEFAULT 0,
    TotalDoctors INT DEFAULT 0,
    TotalStaff INT DEFAULT 0,
    MonthlySubscriptionCost DECIMAL(10, 2) DEFAULT 0,
    
    -- Metadata
    Settings NVARCHAR(MAX) NULL, -- JSON format
    Metadata NVARCHAR(MAX) NULL, -- JSON format
    
    -- Audit Fields
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    
    -- Constraints
    CONSTRAINT CHK_Hospital_Email CHECK (Email LIKE '%_@__%.__%'),
    CONSTRAINT CHK_Hospital_Beds CHECK (TotalBeds >= 0),
    CONSTRAINT CHK_Hospital_Cost CHECK (MonthlySubscriptionCost >= 0)
);
GO

-- Indexes for Hospitals
CREATE NONCLUSTERED INDEX IX_Hospitals_TenantId 
    ON hospital_onboarding.Hospitals(TenantId) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Hospitals_Status 
    ON hospital_onboarding.Hospitals(Status) 
    INCLUDE (TenantId, Name)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Hospitals_SubscriptionStatus 
    ON hospital_onboarding.Hospitals(SubscriptionStatus) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Hospitals_RegistrationNumber 
    ON hospital_onboarding.Hospitals(RegistrationNumber) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Hospitals_Email 
    ON hospital_onboarding.Hospitals(Email) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Hospitals_SubscriptionEnd 
    ON hospital_onboarding.Hospitals(SubscriptionEndDate) 
    WHERE IsDeleted = 0 AND SubscriptionStatus = 'Active';

CREATE NONCLUSTERED INDEX IX_Hospitals_TrialEnd 
    ON hospital_onboarding.Hospitals(TrialEndDate) 
    WHERE IsDeleted = 0 AND SubscriptionStatus = 'Trial';

CREATE NONCLUSTERED INDEX IX_Hospitals_CreatedAt 
    ON hospital_onboarding.Hospitals(CreatedAt DESC);

-- Full-text index for searching
CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;
GO

CREATE FULLTEXT INDEX ON hospital_onboarding.Hospitals(Name, LegalName)
    KEY INDEX PK__Hospitals;
GO

-- ----------------------------------------------------------------------------
-- Branches Table
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.Branches', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.Branches;
GO

CREATE TABLE hospital_onboarding.Branches (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    HospitalId UNIQUEIDENTIFIER NOT NULL,
    
    -- Branch Information
    BranchCode NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    
    -- Contact Information
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    AlternatePhone NVARCHAR(20) NULL,
    
    -- Address
    AddressLine1 NVARCHAR(200) NOT NULL,
    AddressLine2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL DEFAULT 'India',
    PostalCode NVARCHAR(20) NOT NULL,
    
    -- Operational Information
    IsMainBranch BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    TotalBeds INT DEFAULT 0,
    OperatingHours NVARCHAR(MAX) NULL, -- JSON format
    
    -- Manager Information
    BranchManagerId UNIQUEIDENTIFIER NULL,
    BranchManagerName NVARCHAR(200) NULL,
    BranchManagerEmail NVARCHAR(100) NULL,
    BranchManagerPhone NVARCHAR(20) NULL,
    
    -- Metadata
    Metadata NVARCHAR(MAX) NULL, -- JSON format
    
    -- Audit Fields
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    
    -- Foreign Keys
    CONSTRAINT FK_Branches_Hospital 
        FOREIGN KEY (HospitalId) REFERENCES hospital_onboarding.Hospitals(Id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT CHK_Branch_Email CHECK (Email LIKE '%_@__%.__%'),
    CONSTRAINT CHK_Branch_Beds CHECK (TotalBeds >= 0),
    CONSTRAINT UQ_Branch_Code_Per_Hospital UNIQUE (HospitalId, BranchCode)
);
GO

-- Indexes for Branches
CREATE NONCLUSTERED INDEX IX_Branches_HospitalId 
    ON hospital_onboarding.Branches(HospitalId) 
    INCLUDE (Name, IsActive)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Branches_IsActive 
    ON hospital_onboarding.Branches(IsActive) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Branches_BranchCode 
    ON hospital_onboarding.Branches(BranchCode) 
    WHERE IsDeleted = 0;

CREATE FULLTEXT INDEX ON hospital_onboarding.Branches(Name)
    KEY INDEX PK__Branches;
GO

-- ----------------------------------------------------------------------------
-- Subscription Plans Table
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.SubscriptionPlans', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.SubscriptionPlans;
GO

CREATE TABLE hospital_onboarding.SubscriptionPlans (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Plan Information
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    
    -- Pricing
    BasePrice DECIMAL(10, 2) NOT NULL,
    BillingCycle NVARCHAR(50) NOT NULL DEFAULT 'Monthly',
        CONSTRAINT CHK_Plan_BillingCycle CHECK (BillingCycle IN ('Monthly', 'Quarterly', 'HalfYearly', 'Yearly')),
    Currency NVARCHAR(3) NOT NULL DEFAULT 'INR',
    
    -- Capacity Limits
    MaxUsers INT NOT NULL DEFAULT 10,
    MaxBranches INT NOT NULL DEFAULT 1,
    MaxBeds INT NULL,
    
    -- Per-Unit Pricing (for exceeding limits)
    PricePerAdditionalUser DECIMAL(10, 2) DEFAULT 0,
    PricePerAdditionalBranch DECIMAL(10, 2) DEFAULT 0,
    PricePerAdditionalBed DECIMAL(10, 2) DEFAULT 0,
    
    -- Features (stored as JSON array of feature codes)
    IncludedFeatures NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    
    -- Plan Settings
    IsTrial BIT NOT NULL DEFAULT 0,
    TrialDurationDays INT DEFAULT 30,
    IsPublic BIT NOT NULL DEFAULT 1, -- Show on website
    IsActive BIT NOT NULL DEFAULT 1,
    IsRecommended BIT NOT NULL DEFAULT 0,
    DisplayOrder INT DEFAULT 0,
    
    -- Metadata
    Metadata NVARCHAR(MAX) NULL, -- JSON format
    
    -- Audit Fields
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    
    -- Constraints
    CONSTRAINT CHK_Plan_BasePrice CHECK (BasePrice >= 0),
    CONSTRAINT CHK_Plan_MaxUsers CHECK (MaxUsers > 0),
    CONSTRAINT CHK_Plan_MaxBranches CHECK (MaxBranches > 0),
    CONSTRAINT CHK_Plan_AdditionalPrices CHECK (
        PricePerAdditionalUser >= 0 AND 
        PricePerAdditionalBranch >= 0 AND 
        PricePerAdditionalBed >= 0
    )
);
GO

-- Indexes for SubscriptionPlans
CREATE NONCLUSTERED INDEX IX_Plans_IsPublic 
    ON hospital_onboarding.SubscriptionPlans(IsPublic, IsActive) 
    INCLUDE (Name, BasePrice)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Plans_IsTrial 
    ON hospital_onboarding.SubscriptionPlans(IsTrial) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Plans_DisplayOrder 
    ON hospital_onboarding.SubscriptionPlans(DisplayOrder, Name) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Plans_Code 
    ON hospital_onboarding.SubscriptionPlans(Code) 
    WHERE IsDeleted = 0;
GO

-- ----------------------------------------------------------------------------
-- Payments Table
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.Payments', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.Payments;
GO

CREATE TABLE hospital_onboarding.Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    HospitalId UNIQUEIDENTIFIER NOT NULL,
    
    -- Payment Information
    PaymentReference NVARCHAR(100) NOT NULL UNIQUE,
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    
    -- Amount Details
    Amount DECIMAL(10, 2) NOT NULL,
    TaxAmount DECIMAL(10, 2) DEFAULT 0,
    DiscountAmount DECIMAL(10, 2) DEFAULT 0,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'INR',
    
    -- Payment Details
    PaymentMethod NVARCHAR(50) NOT NULL,
        CONSTRAINT CHK_Payment_Method CHECK (PaymentMethod IN ('CreditCard', 'DebitCard', 'BankTransfer', 'UPI', 'Wallet', 'Cheque')),
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        CONSTRAINT CHK_Payment_Status CHECK (PaymentStatus IN ('Pending', 'Successful', 'Failed', 'Refunded', 'PartiallyRefunded')),
    TransactionId NVARCHAR(200) NULL, -- From payment gateway
    
    -- Dates
    PaymentDate DATETIMEOFFSET NULL,
    DueDate DATETIMEOFFSET NOT NULL,
    
    -- Billing Period
    BillingPeriodStart DATETIMEOFFSET NOT NULL,
    BillingPeriodEnd DATETIMEOFFSET NOT NULL,
    
    -- Payment Gateway Details
    GatewayName NVARCHAR(100) NULL,
    GatewayResponse NVARCHAR(MAX) NULL, -- JSON format
    
    -- Refund Information
    RefundAmount DECIMAL(10, 2) DEFAULT 0,
    RefundDate DATETIMEOFFSET NULL,
    RefundReason NVARCHAR(MAX) NULL,
    RefundReference NVARCHAR(100) NULL,
    
    -- Retry Information
    RetryCount INT DEFAULT 0,
    LastRetryAt DATETIMEOFFSET NULL,
    NextRetryAt DATETIMEOFFSET NULL,
    
    -- Invoice
    InvoiceUrl NVARCHAR(500) NULL,
    InvoiceGeneratedAt DATETIMEOFFSET NULL,
    
    -- Notes
    Notes NVARCHAR(MAX) NULL,
    Metadata NVARCHAR(MAX) NULL, -- JSON format
    
    -- Audit Fields
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    
    -- Foreign Keys
    CONSTRAINT FK_Payments_Hospital 
        FOREIGN KEY (HospitalId) REFERENCES hospital_onboarding.Hospitals(Id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT CHK_Payment_Amount CHECK (Amount >= 0),
    CONSTRAINT CHK_Payment_Tax CHECK (TaxAmount >= 0),
    CONSTRAINT CHK_Payment_Discount CHECK (DiscountAmount >= 0),
    CONSTRAINT CHK_Payment_Total CHECK (TotalAmount >= 0),
    CONSTRAINT CHK_Payment_Refund CHECK (RefundAmount >= 0 AND RefundAmount <= TotalAmount)
);
GO

-- Indexes for Payments
CREATE NONCLUSTERED INDEX IX_Payments_HospitalId 
    ON hospital_onboarding.Payments(HospitalId) 
    INCLUDE (PaymentStatus, TotalAmount)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Payments_Status 
    ON hospital_onboarding.Payments(PaymentStatus) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Payments_DueDate 
    ON hospital_onboarding.Payments(DueDate) 
    WHERE IsDeleted = 0 AND PaymentStatus = 'Pending';

CREATE NONCLUSTERED INDEX IX_Payments_PaymentDate 
    ON hospital_onboarding.Payments(PaymentDate DESC);

CREATE NONCLUSTERED INDEX IX_Payments_Reference 
    ON hospital_onboarding.Payments(PaymentReference) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Payments_InvoiceNumber 
    ON hospital_onboarding.Payments(InvoiceNumber) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Payments_BillingPeriod 
    ON hospital_onboarding.Payments(BillingPeriodStart, BillingPeriodEnd);
GO

-- ----------------------------------------------------------------------------
-- Global Facilities Table (Master Data)
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.GlobalFacilities', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.GlobalFacilities;
GO

CREATE TABLE hospital_onboarding.GlobalFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Facility Information
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Category NVARCHAR(50) NOT NULL,
        CONSTRAINT CHK_Facility_Category CHECK (Category IN ('Diagnostic', 'Treatment', 'Surgical', 'Emergency', 'ICU', 'Pharmacy', 'Laboratory', 'Radiology', 'Other')),
    
    -- Classification
    IsStandard BIT NOT NULL DEFAULT 1,
    RequiresCertification BIT NOT NULL DEFAULT 0,
    CertificationDetails NVARCHAR(MAX) NULL,
    
    -- Display
    IconName NVARCHAR(100) NULL,
    DisplayOrder INT DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Metadata
    Metadata NVARCHAR(MAX) NULL, -- JSON format
    
    -- Audit Fields
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    
    -- Constraints
    CONSTRAINT UQ_Facility_Code UNIQUE (Code)
);
GO

-- Indexes for GlobalFacilities
CREATE NONCLUSTERED INDEX IX_Facilities_Category 
    ON hospital_onboarding.GlobalFacilities(Category, IsActive) 
    INCLUDE (Name)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Facilities_IsActive 
    ON hospital_onboarding.GlobalFacilities(IsActive) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Facilities_DisplayOrder 
    ON hospital_onboarding.GlobalFacilities(DisplayOrder, Name);

CREATE FULLTEXT INDEX ON hospital_onboarding.GlobalFacilities(Name, Description)
    KEY INDEX PK__GlobalFa;
GO

-- ----------------------------------------------------------------------------
-- Hospital Facilities Table (Hospital-level facility mapping)
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.HospitalFacilities', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.HospitalFacilities;
GO

CREATE TABLE hospital_onboarding.HospitalFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    HospitalId UNIQUEIDENTIFIER NOT NULL,
    FacilityId UNIQUEIDENTIFIER NOT NULL,
    
    -- Facility Details at Hospital Level
    IsAvailable BIT NOT NULL DEFAULT 1,
    Capacity INT NULL,
    Notes NVARCHAR(MAX) NULL,
    
    -- Metadata
    Metadata NVARCHAR(MAX) NULL, -- JSON format
    
    -- Audit Fields
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_HospitalFacilities_Hospital 
        FOREIGN KEY (HospitalId) REFERENCES hospital_onboarding.Hospitals(Id) ON DELETE CASCADE,
    CONSTRAINT FK_HospitalFacilities_Facility 
        FOREIGN KEY (FacilityId) REFERENCES hospital_onboarding.GlobalFacilities(Id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT UQ_Hospital_Facility UNIQUE (HospitalId, FacilityId),
    CONSTRAINT CHK_Facility_Capacity CHECK (Capacity IS NULL OR Capacity >= 0)
);
GO

-- Indexes for HospitalFacilities
CREATE NONCLUSTERED INDEX IX_HospitalFacilities_HospitalId 
    ON hospital_onboarding.HospitalFacilities(HospitalId) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_HospitalFacilities_FacilityId 
    ON hospital_onboarding.HospitalFacilities(FacilityId) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_HospitalFacilities_IsAvailable 
    ON hospital_onboarding.HospitalFacilities(IsAvailable) 
    WHERE IsDeleted = 0;
GO

-- ----------------------------------------------------------------------------
-- Branch Facilities Table (Branch-level facility mapping)
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.BranchFacilities', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.BranchFacilities;
GO

CREATE TABLE hospital_onboarding.BranchFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BranchId UNIQUEIDENTIFIER NOT NULL,
    HospitalFacilityId UNIQUEIDENTIFIER NOT NULL, -- Reference to HospitalFacilities
    
    -- Facility Details at Branch Level
    IsAvailable BIT NOT NULL DEFAULT 1,
    Capacity INT NULL,
    FloorNumber NVARCHAR(20) NULL,
    LocationDetails NVARCHAR(MAX) NULL,
    OperatingHours NVARCHAR(MAX) NULL, -- JSON format
    
    -- Responsible Staff
    InchargeStaffId UNIQUEIDENTIFIER NULL,
    InchargeStaffName NVARCHAR(200) NULL,
    
    -- Metadata
    Metadata NVARCHAR(MAX) NULL, -- JSON format
    
    -- Audit Fields
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_BranchFacilities_Branch 
        FOREIGN KEY (BranchId) REFERENCES hospital_onboarding.Branches(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BranchFacilities_HospitalFacility 
        FOREIGN KEY (HospitalFacilityId) REFERENCES hospital_onboarding.HospitalFacilities(Id),
    
    -- Constraints
    CONSTRAINT UQ_Branch_Hospital_Facility UNIQUE (BranchId, HospitalFacilityId),
    CONSTRAINT CHK_Branch_Facility_Capacity CHECK (Capacity IS NULL OR Capacity >= 0)
);
GO

-- Indexes for BranchFacilities
CREATE NONCLUSTERED INDEX IX_BranchFacilities_BranchId 
    ON hospital_onboarding.BranchFacilities(BranchId) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_BranchFacilities_HospitalFacilityId 
    ON hospital_onboarding.BranchFacilities(HospitalFacilityId) 
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_BranchFacilities_IsAvailable 
    ON hospital_onboarding.BranchFacilities(IsAvailable) 
    WHERE IsDeleted = 0;
GO

-- ============================================================================
-- AUDIT TABLES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Audit Log Table (Track all changes)
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.AuditLogs', 'U') IS NOT NULL
    DROP TABLE hospital_onboarding.AuditLogs;
GO

CREATE TABLE hospital_onboarding.AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Entity Information
    EntityType NVARCHAR(100) NOT NULL, -- Hospitals, Branches, etc.
    EntityId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NULL, -- For multi-tenant filtering
    
    -- Action Details
    Action NVARCHAR(50) NOT NULL, -- INSERT, UPDATE, DELETE, APPROVE, REJECT, etc.
    PerformedBy UNIQUEIDENTIFIER NOT NULL,
    PerformedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    
    -- Change Details
    OldValues NVARCHAR(MAX) NULL, -- JSON format
    NewValues NVARCHAR(MAX) NULL, -- JSON format
    ChangedFields NVARCHAR(MAX) NULL, -- JSON array of field names
    
    -- Request Context
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    RequestId NVARCHAR(100) NULL,
    
    -- Metadata
    Metadata NVARCHAR(MAX) NULL -- JSON format
);
GO

-- Indexes for AuditLogs
CREATE NONCLUSTERED INDEX IX_AuditLogs_Entity 
    ON hospital_onboarding.AuditLogs(EntityType, EntityId);

CREATE NONCLUSTERED INDEX IX_AuditLogs_TenantId 
    ON hospital_onboarding.AuditLogs(TenantId);

CREATE NONCLUSTERED INDEX IX_AuditLogs_PerformedAt 
    ON hospital_onboarding.AuditLogs(PerformedAt DESC);

CREATE NONCLUSTERED INDEX IX_AuditLogs_PerformedBy 
    ON hospital_onboarding.AuditLogs(PerformedBy);

CREATE NONCLUSTERED INDEX IX_AuditLogs_Action 
    ON hospital_onboarding.AuditLogs(Action);
GO

-- ============================================================================
-- VIEWS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Active Hospitals View
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.vw_ActiveHospitals', 'V') IS NOT NULL
    DROP VIEW hospital_onboarding.vw_ActiveHospitals;
GO

CREATE VIEW hospital_onboarding.vw_ActiveHospitals
AS
SELECT 
    h.Id,
    h.TenantId,
    h.RegistrationNumber,
    h.Name,
    h.LegalName,
    h.Email,
    h.Phone,
    h.City,
    h.State,
    h.Status,
    h.SubscriptionStatus,
    h.SubscriptionEndDate,
    h.TrialEndDate,
    h.MonthlySubscriptionCost,
    sp.Name AS SubscriptionPlanName,
    sp.Code AS SubscriptionPlanCode,
    COUNT(DISTINCT b.Id) AS TotalBranches,
    COUNT(DISTINCT hf.Id) AS TotalFacilities,
    h.CreatedAt
FROM hospital_onboarding.Hospitals h
LEFT JOIN hospital_onboarding.SubscriptionPlans sp ON h.SubscriptionPlanId = sp.Id
LEFT JOIN hospital_onboarding.Branches b ON h.Id = b.HospitalId AND b.IsDeleted = 0
LEFT JOIN hospital_onboarding.HospitalFacilities hf ON h.Id = hf.HospitalId AND hf.IsDeleted = 0
WHERE h.IsDeleted = 0
    AND h.Status IN ('Active', 'PendingApproval')
GROUP BY 
    h.Id, h.TenantId, h.RegistrationNumber, h.Name, h.LegalName,
    h.Email, h.Phone, h.City, h.State, h.Status, h.SubscriptionStatus,
    h.SubscriptionEndDate, h.TrialEndDate, h.MonthlySubscriptionCost,
    sp.Name, sp.Code, h.CreatedAt;
GO

-- ----------------------------------------------------------------------------
-- Payment Summary View
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.vw_PaymentSummary', 'V') IS NOT NULL
    DROP VIEW hospital_onboarding.vw_PaymentSummary;
GO

CREATE VIEW hospital_onboarding.vw_PaymentSummary
AS
SELECT 
    h.Id AS HospitalId,
    h.TenantId,
    h.Name AS HospitalName,
    COUNT(p.Id) AS TotalPayments,
    SUM(CASE WHEN p.PaymentStatus = 'Successful' THEN p.TotalAmount ELSE 0 END) AS TotalPaid,
    SUM(CASE WHEN p.PaymentStatus = 'Pending' THEN p.TotalAmount ELSE 0 END) AS TotalPending,
    SUM(CASE WHEN p.PaymentStatus = 'Failed' THEN p.TotalAmount ELSE 0 END) AS TotalFailed,
    SUM(p.RefundAmount) AS TotalRefunded,
    MAX(p.PaymentDate) AS LastPaymentDate,
    MIN(CASE WHEN p.PaymentStatus = 'Pending' THEN p.DueDate END) AS NextDueDate
FROM hospital_onboarding.Hospitals h
LEFT JOIN hospital_onboarding.Payments p ON h.Id = p.HospitalId AND p.IsDeleted = 0
WHERE h.IsDeleted = 0
GROUP BY h.Id, h.TenantId, h.Name;
GO

-- ============================================================================
-- STORED PROCEDURES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Procedure: Get Hospital Dashboard
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.usp_GetHospitalDashboard', 'P') IS NOT NULL
    DROP PROCEDURE hospital_onboarding.usp_GetHospitalDashboard;
GO

CREATE PROCEDURE hospital_onboarding.usp_GetHospitalDashboard
    @HospitalId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Hospital Details
    SELECT 
        h.*,
        sp.Name AS SubscriptionPlanName,
        sp.Code AS SubscriptionPlanCode,
        sp.BasePrice,
        sp.BillingCycle
    FROM hospital_onboarding.Hospitals h
    LEFT JOIN hospital_onboarding.SubscriptionPlans sp ON h.SubscriptionPlanId = sp.Id
    WHERE h.Id = @HospitalId AND h.IsDeleted = 0;
    
    -- Branch Summary
    SELECT 
        COUNT(*) AS TotalBranches,
        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveBranches,
        SUM(CASE WHEN IsMainBranch = 1 THEN 1 ELSE 0 END) AS MainBranches,
        SUM(TotalBeds) AS TotalBedsAcrossBranches
    FROM hospital_onboarding.Branches
    WHERE HospitalId = @HospitalId AND IsDeleted = 0;
    
    -- Payment Summary
    SELECT * 
    FROM hospital_onboarding.vw_PaymentSummary
    WHERE HospitalId = @HospitalId;
    
    -- Facility Count
    SELECT 
        COUNT(*) AS TotalFacilities,
        SUM(CASE WHEN IsAvailable = 1 THEN 1 ELSE 0 END) AS AvailableFacilities
    FROM hospital_onboarding.HospitalFacilities
    WHERE HospitalId = @HospitalId AND IsDeleted = 0;
END
GO

-- ============================================================================
-- TRIGGERS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Trigger: Update UpdatedAt on Hospitals
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.trg_Hospitals_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER hospital_onboarding.trg_Hospitals_UpdatedAt;
GO

CREATE TRIGGER hospital_onboarding.trg_Hospitals_UpdatedAt
ON hospital_onboarding.Hospitals
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE hospital_onboarding.Hospitals
    SET UpdatedAt = SYSDATETIMEOFFSET()
    FROM hospital_onboarding.Hospitals h
    INNER JOIN inserted i ON h.Id = i.Id;
END
GO

-- ----------------------------------------------------------------------------
-- Trigger: Audit Trail for Hospitals
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.trg_Hospitals_Audit', 'TR') IS NOT NULL
    DROP TRIGGER hospital_onboarding.trg_Hospitals_Audit;
GO

CREATE TRIGGER hospital_onboarding.trg_Hospitals_Audit
ON hospital_onboarding.Hospitals
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Action NVARCHAR(50);
    
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        SET @Action = 'UPDATE';
    ELSE IF EXISTS (SELECT * FROM inserted)
        SET @Action = 'INSERT';
    ELSE
        SET @Action = 'DELETE';
    
    -- Insert audit records
    INSERT INTO hospital_onboarding.AuditLogs (
        EntityType,
        EntityId,
        TenantId,
        Action,
        PerformedBy,
        OldValues,
        NewValues
    )
    SELECT 
        'Hospitals',
        COALESCE(i.Id, d.Id),
        COALESCE(i.TenantId, d.TenantId),
        @Action,
        COALESCE(i.UpdatedBy, i.CreatedBy, d.UpdatedBy),
        (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i
    FULL OUTER JOIN deleted d ON i.Id = d.Id;
END
GO

-- ============================================================================
-- FUNCTIONS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Function: Calculate Subscription Cost
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.fn_CalculateSubscriptionCost', 'FN') IS NOT NULL
    DROP FUNCTION hospital_onboarding.fn_CalculateSubscriptionCost;
GO

CREATE FUNCTION hospital_onboarding.fn_CalculateSubscriptionCost (
    @PlanId UNIQUEIDENTIFIER,
    @NumUsers INT,
    @NumBranches INT,
    @NumBeds INT
)
RETURNS DECIMAL(10, 2)
AS
BEGIN
    DECLARE @TotalCost DECIMAL(10, 2) = 0;
    DECLARE @BasePrice DECIMAL(10, 2);
    DECLARE @MaxUsers INT;
    DECLARE @MaxBranches INT;
    DECLARE @MaxBeds INT;
    DECLARE @PricePerUser DECIMAL(10, 2);
    DECLARE @PricePerBranch DECIMAL(10, 2);
    DECLARE @PricePerBed DECIMAL(10, 2);
    
    -- Get plan details
    SELECT 
        @BasePrice = BasePrice,
        @MaxUsers = MaxUsers,
        @MaxBranches = MaxBranches,
        @MaxBeds = ISNULL(MaxBeds, 999999),
        @PricePerUser = PricePerAdditionalUser,
        @PricePerBranch = PricePerAdditionalBranch,
        @PricePerBed = PricePerAdditionalBed
    FROM hospital_onboarding.SubscriptionPlans
    WHERE Id = @PlanId AND IsDeleted = 0;
    
    -- Calculate base cost
    SET @TotalCost = @BasePrice;
    
    -- Add cost for additional users
    IF @NumUsers > @MaxUsers
        SET @TotalCost = @TotalCost + ((@NumUsers - @MaxUsers) * @PricePerUser);
    
    -- Add cost for additional branches
    IF @NumBranches > @MaxBranches
        SET @TotalCost = @TotalCost + ((@NumBranches - @MaxBranches) * @PricePerBranch);
    
    -- Add cost for additional beds
    IF @NumBeds > @MaxBeds
        SET @TotalCost = @TotalCost + ((@NumBeds - @MaxBeds) * @PricePerBed);
    
    RETURN @TotalCost;
END
GO

-- ============================================================================
-- ROW LEVEL SECURITY (Multi-Tenancy) - SQL Server 2016+
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Security Predicate Function
-- ----------------------------------------------------------------------------
IF OBJECT_ID('hospital_onboarding.fn_TenantAccessPredicate', 'IF') IS NOT NULL
    DROP FUNCTION hospital_onboarding.fn_TenantAccessPredicate;
GO

CREATE FUNCTION hospital_onboarding.fn_TenantAccessPredicate(@TenantId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
    SELECT 1 AS AccessResult
    WHERE 
        @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS UNIQUEIDENTIFIER)
        OR SESSION_CONTEXT(N'UserRole') = 'SystemAdmin';
GO

-- ----------------------------------------------------------------------------
-- Security Policy
-- ----------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.security_policies WHERE name = 'TenantIsolationPolicy')
    DROP SECURITY POLICY hospital_onboarding.TenantIsolationPolicy;
GO

CREATE SECURITY POLICY hospital_onboarding.TenantIsolationPolicy
ADD FILTER PREDICATE hospital_onboarding.fn_TenantAccessPredicate(TenantId)
ON hospital_onboarding.Hospitals
WITH (STATE = ON);
GO

-- ============================================================================
-- SAMPLE DATA (Optional - for testing)
-- ============================================================================

-- Insert sample subscription plans
INSERT INTO hospital_onboarding.SubscriptionPlans (Code, Name, Description, BasePrice, BillingCycle, MaxUsers, MaxBranches, IsTrial, IsPublic, IsActive, DisplayOrder)
VALUES 
    ('TRIAL', 'Trial Plan', '30-day trial with basic features', 0, 'Monthly', 5, 1, 1, 0, 1, 0),
    ('BASIC', 'Basic Plan', 'For small clinics', 2999, 'Monthly', 10, 1, 0, 1, 1, 1),
    ('PROFESSIONAL', 'Professional Plan', 'For medium hospitals', 9999, 'Monthly', 50, 5, 0, 1, 1, 2),
    ('ENTERPRISE', 'Enterprise Plan', 'For large hospital networks', 29999, 'Monthly', 200, 20, 0, 1, 1, 3);

-- Insert sample global facilities
INSERT INTO hospital_onboarding.GlobalFacilities (Code, Name, Category, IsStandard, IsActive, DisplayOrder)
VALUES 
    ('ICU', 'Intensive Care Unit', 'ICU', 1, 1, 1),
    ('ER', 'Emergency Room', 'Emergency', 1, 1, 2),
    ('OT', 'Operation Theater', 'Surgical', 1, 1, 3),
    ('LAB', 'Pathology Laboratory', 'Laboratory', 1, 1, 4),
    ('XRAY', 'X-Ray Imaging', 'Radiology', 1, 1, 5),
    ('MRI', 'MRI Scan', 'Radiology', 0, 1, 6),
    ('CT', 'CT Scan', 'Radiology', 0, 1, 7),
    ('PHARMACY', 'In-house Pharmacy', 'Pharmacy', 1, 1, 8);

GO

-- ============================================================================
-- EXTENDED PROPERTIES (Documentation)
-- ============================================================================

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Hospital Onboarding module for MedWay multi-tenant SaaS platform',
    @level0type = N'SCHEMA', 
    @level0name = N'hospital_onboarding';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Main table storing hospital tenant information',
    @level0type = N'SCHEMA', @level0name = N'hospital_onboarding',
    @level1type = N'TABLE', @level1name = N'Hospitals';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Unique tenant identifier for multi-tenant isolation',
    @level0type = N'SCHEMA', @level0name = N'hospital_onboarding',
    @level1type = N'TABLE', @level1name = N'Hospitals',
    @level2type = N'COLUMN', @level2name = N'TenantId';
GO

-- ============================================================================
-- END OF SCHEMA
-- ============================================================================

PRINT 'Hospital Onboarding schema created successfully for SQL Server';
PRINT 'Multi-tenant isolation enabled with Row Level Security';
PRINT 'Sample subscription plans and facilities inserted';
GO
