# Hospital Onboarding Database Schema

## Overview
Comprehensive multi-tenant database schemas for the MedWay Hospital Onboarding module supporting both **PostgreSQL** and **SQL Server**.

## 🏗️ Architecture Highlights

### Multi-Tenancy Strategy
- **Tenant Isolation**: Each hospital has a unique `TenantId` (GUID/UUID)
- **Row-Level Security (RLS)**: PostgreSQL and SQL Server 2016+ support
- **Data Segregation**: Complete isolation between hospital tenants
- **Shared Schema**: All tenants use the same schema with data-level separation

### Database Support
| Database | Version | File Location | Features |
|----------|---------|---------------|----------|
| **PostgreSQL** | 12+ | `postgresql/01_hospital_onboarding_schema.sql` | Native JSONB, ENUMs, Triggers, Full-text search |
| **SQL Server** | 2016+ | `sqlserver/01_hospital_onboarding_schema.sql` | JSON support, Row-level security, Full-text search |

## 📊 Schema Structure

### Core Tables (7 tables)

#### 1. **Hospitals** (Tenant Root)
Primary tenant entity storing hospital information.

| Key Fields | Type | Description |
|------------|------|-------------|
| `Id` / `id` | GUID/UUID | Primary key |
| `TenantId` / `tenant_id` | GUID/UUID | Unique tenant identifier |
| `RegistrationNumber` | VARCHAR(50) | Unique hospital registration |
| `Name` | VARCHAR(200) | Hospital display name |
| `Email` | VARCHAR(100) | Unique email (login) |
| `Status` | ENUM/VARCHAR | PendingApproval, Active, Rejected, Suspended, Inactive |
| `SubscriptionStatus` | ENUM/VARCHAR | Trial, Active, Expired, Suspended, Cancelled |
| `SubscriptionPlanId` | GUID/UUID | FK to subscription_plans |

**Soft Delete**: Uses `IsDeleted` / `is_deleted` flag  
**Audit**: Full audit trail with created/updated/deleted timestamps and user IDs

#### 2. **Branches**
Multi-branch support for hospital networks.

| Key Fields | Type | Description |
|------------|------|-------------|
| `Id` / `id` | GUID/UUID | Primary key |
| `HospitalId` / `hospital_id` | GUID/UUID | FK to hospitals (CASCADE DELETE) |
| `BranchCode` | VARCHAR(50) | Unique per hospital |
| `Name` | VARCHAR(200) | Branch name |
| `IsMainBranch` | BOOLEAN | Main/headquarters flag |
| `IsActive` | BOOLEAN | Active status |
| `TotalBeds` | INTEGER | Bed capacity |

**Constraint**: `UNIQUE(HospitalId, BranchCode)` ensures unique branch codes per hospital

#### 3. **SubscriptionPlans**
Flexible subscription plans with capacity-based pricing.

| Key Fields | Type | Description |
|------------|------|-------------|
| `Id` / `id` | GUID/UUID | Primary key |
| `Code` | VARCHAR(50) | Unique plan code (TRIAL, BASIC, PRO, ENTERPRISE) |
| `Name` | VARCHAR(200) | Display name |
| `BasePrice` | DECIMAL(10,2) | Base monthly price |
| `BillingCycle` | ENUM/VARCHAR | Monthly, Quarterly, HalfYearly, Yearly |
| `MaxUsers` | INTEGER | Included user count |
| `MaxBranches` | INTEGER | Included branch count |
| `PricePerAdditionalUser` | DECIMAL(10,2) | Overage pricing |
| `IsTrial` | BOOLEAN | Trial plan flag |
| `IsPublic` | BOOLEAN | Show on website |

**Pricing Model**: Base price + overage charges for exceeding limits

#### 4. **Payments**
Complete payment and billing history.

| Key Fields | Type | Description |
|------------|------|-------------|
| `Id` / `id` | GUID/UUID | Primary key |
| `HospitalId` / `hospital_id` | GUID/UUID | FK to hospitals |
| `PaymentReference` | VARCHAR(100) | Unique payment reference |
| `InvoiceNumber` | VARCHAR(50) | Unique invoice number |
| `Amount` | DECIMAL(10,2) | Base amount |
| `TotalAmount` | DECIMAL(10,2) | Final amount (with tax/discount) |
| `PaymentStatus` | ENUM/VARCHAR | Pending, Successful, Failed, Refunded |
| `PaymentMethod` | ENUM/VARCHAR | CreditCard, DebitCard, BankTransfer, UPI, etc. |
| `BillingPeriodStart` | TIMESTAMP | Billing period start |
| `BillingPeriodEnd` | TIMESTAMP | Billing period end |
| `GatewayResponse` | JSON | Payment gateway response |

**Features**: Retry logic, refunds, invoice generation

#### 5. **GlobalFacilities** (Master Data)
Master list of available medical facilities.

| Key Fields | Type | Description |
|------------|------|-------------|
| `Id` / `id` | GUID/UUID | Primary key |
| `Code` | VARCHAR(50) | Unique facility code |
| `Name` | VARCHAR(200) | Facility name |
| `Category` | ENUM/VARCHAR | Diagnostic, Treatment, Surgical, Emergency, etc. |
| `IsStandard` | BOOLEAN | Standard facility |
| `RequiresCertification` | BOOLEAN | Certification required |

**Seed Data**: 35+ pre-configured facilities (ICU, ER, OT, Lab, Radiology, etc.)

#### 6. **HospitalFacilities**
Hospital-level facility mapping.

| Key Fields | Type | Description |
|------------|------|-------------|
| `Id` / `id` | GUID/UUID | Primary key |
| `HospitalId` / `hospital_id` | GUID/UUID | FK to hospitals |
| `FacilityId` / `facility_id` | GUID/UUID | FK to global_facilities |
| `IsAvailable` | BOOLEAN | Availability status |
| `Capacity` | INTEGER | Facility capacity |

**Constraint**: `UNIQUE(HospitalId, FacilityId)`

#### 7. **BranchFacilities**
Branch-level facility mapping.

| Key Fields | Type | Description |
|------------|------|-------------|
| `Id` / `id` | GUID/UUID | Primary key |
| `BranchId` / `branch_id` | GUID/UUID | FK to branches |
| `HospitalFacilityId` | GUID/UUID | FK to hospital_facilities |
| `IsAvailable` | BOOLEAN | Availability at branch |
| `FloorNumber` | VARCHAR(20) | Location details |
| `OperatingHours` | JSON | Operating hours |

**Constraint**: `UNIQUE(BranchId, HospitalFacilityId)`

### Audit Table

#### **AuditLogs**
Complete audit trail for all data changes.

| Key Fields | Type | Description |
|------------|------|-------------|
| `EntityType` | VARCHAR(100) | Table name |
| `EntityId` | GUID/UUID | Record ID |
| `TenantId` | GUID/UUID | Tenant isolation |
| `Action` | VARCHAR(50) | INSERT, UPDATE, DELETE, APPROVE, etc. |
| `PerformedBy` | GUID/UUID | User who performed action |
| `OldValues` | JSON | Before state |
| `NewValues` | JSON | After state |
| `ChangedFields` | ARRAY/JSON | List of changed fields |

**Automatic**: Triggers fire on all DML operations

## 🎯 Multi-Tenancy Implementation

### PostgreSQL Approach

```sql
-- Set tenant context before queries
SET app.current_tenant_id = '20000000-0000-0000-0000-000000000001';
SET app.user_role = 'HospitalAdmin';

-- Row-level security automatically filters data
SELECT * FROM hospitals; -- Only returns hospitals for current tenant
```

**Row-Level Security Policy**:
```sql
CREATE POLICY tenant_isolation_policy ON hospitals
    FOR ALL
    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
```

### SQL Server Approach

```sql
-- Set tenant context
EXEC sp_set_session_context 'TenantId', '20000000-0000-0000-0000-000000000001';
EXEC sp_set_session_context 'UserRole', 'HospitalAdmin';

-- Security policy automatically filters
SELECT * FROM hospital_onboarding.Hospitals;
```

**Security Policy**:
```sql
CREATE SECURITY POLICY hospital_onboarding.TenantIsolationPolicy
ADD FILTER PREDICATE hospital_onboarding.fn_TenantAccessPredicate(TenantId)
ON hospital_onboarding.Hospitals
WITH (STATE = ON);
```

## 📈 Indexes & Performance

### Key Indexes

**PostgreSQL**:
```sql
-- Tenant isolation
CREATE INDEX idx_hospitals_tenant_id ON hospitals(tenant_id) WHERE is_deleted = FALSE;

-- Status queries
CREATE INDEX idx_hospitals_status ON hospitals(status) WHERE is_deleted = FALSE;

-- Subscription expiry
CREATE INDEX idx_hospitals_subscription_end ON hospitals(subscription_end_date) 
    WHERE is_deleted = FALSE AND subscription_status = 'Active';

-- Full-text search
CREATE INDEX idx_hospitals_name_trgm ON hospitals USING gin(name gin_trgm_ops);
```

**SQL Server**:
```sql
-- Covering indexes
CREATE NONCLUSTERED INDEX IX_Hospitals_Status 
    ON hospital_onboarding.Hospitals(Status) 
    INCLUDE (TenantId, Name)
    WHERE IsDeleted = 0;

-- Full-text search
CREATE FULLTEXT INDEX ON hospital_onboarding.Hospitals(Name, LegalName);
```

### Performance Features
- ✅ **Filtered indexes** for active records only
- ✅ **Covering indexes** to avoid key lookups
- ✅ **Full-text indexes** for search functionality
- ✅ **JSONB/JSON indexes** for metadata queries
- ✅ **Composite indexes** for common query patterns

## 🔄 Triggers & Automation

### PostgreSQL Triggers

```sql
-- Auto-update timestamps
CREATE TRIGGER trg_hospitals_updated_at
    BEFORE UPDATE ON hospitals
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Automatic audit logging
CREATE TRIGGER trg_hospitals_audit
    AFTER INSERT OR UPDATE OR DELETE ON hospitals
    FOR EACH ROW
    EXECUTE FUNCTION audit_trail_trigger();
```

### SQL Server Triggers

```sql
-- Auto-update timestamps
CREATE TRIGGER trg_Hospitals_UpdatedAt
ON hospital_onboarding.Hospitals
AFTER UPDATE
AS BEGIN
    UPDATE hospital_onboarding.Hospitals
    SET UpdatedAt = SYSDATETIMEOFFSET()
    FROM inserted i WHERE Hospitals.Id = i.Id;
END

-- Audit logging
CREATE TRIGGER trg_Hospitals_Audit
ON hospital_onboarding.Hospitals
AFTER INSERT, UPDATE, DELETE
AS BEGIN
    INSERT INTO AuditLogs (EntityType, EntityId, Action, OldValues, NewValues)
    SELECT 'Hospitals', COALESCE(i.Id, d.Id), @Action,
           (SELECT d.* FOR JSON PATH),
           (SELECT i.* FOR JSON PATH)
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END
```

## 📊 Views

### vw_ActiveHospitals
Aggregated view of active hospitals with statistics.

```sql
SELECT 
    h.id,
    h.tenant_id,
    h.name,
    h.status,
    h.subscription_status,
    sp.name AS subscription_plan_name,
    COUNT(DISTINCT b.id) AS total_branches,
    COUNT(DISTINCT hf.id) AS total_facilities
FROM hospitals h
LEFT JOIN subscription_plans sp ON h.subscription_plan_id = sp.id
LEFT JOIN branches b ON h.id = b.hospital_id
LEFT JOIN hospital_facilities hf ON h.id = hf.hospital_id
WHERE h.is_deleted = FALSE
GROUP BY h.id, sp.id;
```

### vw_PaymentSummary
Payment analytics per hospital.

```sql
SELECT 
    h.id AS hospital_id,
    h.name AS hospital_name,
    SUM(CASE WHEN p.payment_status = 'Successful' THEN p.total_amount ELSE 0 END) AS total_paid,
    SUM(CASE WHEN p.payment_status = 'Pending' THEN p.total_amount ELSE 0 END) AS total_pending,
    MAX(p.payment_date) AS last_payment_date
FROM hospitals h
LEFT JOIN payments p ON h.id = p.hospital_id
GROUP BY h.id, h.name;
```

## 🎲 Seed Data

### Subscription Plans (5 plans)
1. **Trial Plan** - Free 30-day trial (5 users, 1 branch)
2. **Basic Plan** - ₹2,999/month (10 users, 1 branch)
3. **Professional Plan** - ₹9,999/month (50 users, 5 branches) ⭐ Recommended
4. **Enterprise Plan** - ₹29,999/month (200 users, 20 branches)
5. **Basic Annual** - ₹30,589/year (15% discount)

### Global Facilities (35+ facilities)
- **Emergency**: ICU, NICU, CCU, ER, Trauma Center
- **Surgical**: General OT, Cardiac OT, Neuro OT, Ortho OT, Minor OT
- **Radiology**: X-Ray, CT Scan, MRI, Ultrasound, Mammography
- **Laboratory**: Pathology, Microbiology, Biochemistry, Hematology, Blood Bank
- **Treatment**: Dialysis, Physiotherapy, Chemotherapy, Radiation Therapy, Dental
- **Pharmacy**: In-house Pharmacy, 24/7 Pharmacy
- **Support**: Ambulance, Cafeteria, Parking

## 🚀 Installation

### PostgreSQL

```bash
# 1. Create database
createdb medway_healthcare

# 2. Connect
psql -d medway_healthcare

# 3. Run schema
\i database/postgresql/01_hospital_onboarding_schema.sql

# 4. Load seed data
\i database/postgresql/02_seed_data.sql

# 5. Verify
SELECT COUNT(*) FROM hospital_onboarding.subscription_plans;
SELECT COUNT(*) FROM hospital_onboarding.global_facilities;
```

### SQL Server

```powershell
# 1. Create database
sqlcmd -S localhost -Q "CREATE DATABASE MedWayHealthCare"

# 2. Run schema
sqlcmd -S localhost -d MedWayHealthCare -i database/sqlserver/01_hospital_onboarding_schema.sql

# 3. Verify
sqlcmd -S localhost -d MedWayHealthCare -Q "SELECT COUNT(*) FROM hospital_onboarding.SubscriptionPlans"
```

## 🔍 Common Queries

### Get Hospital with Branches and Facilities
```sql
-- PostgreSQL
SELECT 
    h.name AS hospital_name,
    h.status,
    h.subscription_status,
    json_agg(DISTINCT b.*) AS branches,
    json_agg(DISTINCT hf.*) AS facilities
FROM hospitals h
LEFT JOIN branches b ON h.id = b.hospital_id AND b.is_deleted = FALSE
LEFT JOIN hospital_facilities hf ON h.id = hf.hospital_id AND hf.is_deleted = FALSE
WHERE h.tenant_id = '20000000-0000-0000-0000-000000000001'
GROUP BY h.id;
```

### Calculate Subscription Cost
```sql
-- SQL Server Function
SELECT hospital_onboarding.fn_CalculateSubscriptionCost(
    @PlanId = '00000000-0000-0000-0000-000000000003',
    @NumUsers = 75,      -- 25 over limit (50)
    @NumBranches = 8,    -- 3 over limit (5)
    @NumBeds = 600       -- 100 over limit (500)
) AS TotalMonthlyCost;
-- Returns: 9999 + (25*150) + (3*1200) + (100*30) = 19,599
```

### Get Overdue Payments
```sql
-- PostgreSQL
SELECT 
    h.name AS hospital_name,
    p.invoice_number,
    p.total_amount,
    p.due_date,
    CURRENT_DATE - p.due_date::date AS days_overdue
FROM payments p
JOIN hospitals h ON p.hospital_id = h.id
WHERE p.payment_status = 'Pending'
    AND p.due_date < CURRENT_TIMESTAMP
    AND p.is_deleted = FALSE
ORDER BY p.due_date;
```

## 🔐 Security Features

### 1. Row-Level Security
- ✅ Automatic tenant isolation
- ✅ Admin override for system operations
- ✅ Session-based context management

### 2. Soft Deletes
- ✅ All tables use `IsDeleted` / `is_deleted` flag
- ✅ Filtered indexes exclude deleted records
- ✅ Audit trail preserves deletion history

### 3. Audit Logging
- ✅ Automatic triggers on all DML operations
- ✅ Before/after state captured in JSON
- ✅ User and timestamp tracking
- ✅ IP address and request context

### 4. Data Validation
- ✅ Check constraints for email, phone, amounts
- ✅ Enum/constraint validation for status fields
- ✅ Foreign key cascades for referential integrity
- ✅ Unique constraints for business rules

## 📝 Entity Relationship Diagram

```
┌─────────────┐
│  Hospitals  │◄───────┐
│ (Tenant)    │        │
└──────┬──────┘        │
       │               │
       │ 1:N           │ 1:1
       ▼               │
┌─────────────┐   ┌────┴────────────────┐
│  Branches   │   │ SubscriptionPlans   │
└──────┬──────┘   └─────────────────────┘
       │
       │ 1:N          ┌─────────────────┐
       ▼              │ GlobalFacilities│
┌──────────────────┐ │  (Master Data)  │
│BranchFacilities  │ └────────┬────────┘
└──────┬───────────┘          │
       │                      │ 1:N
       │ N:1                  ▼
       │            ┌───────────────────┐
       └───────────►│HospitalFacilities │
                    └───────────────────┘

┌─────────────┐
│  Payments   │──────► Hospitals (FK)
└─────────────┘

┌─────────────┐
│ AuditLogs   │──────► All Tables (Trigger)
└─────────────┘
```

## 🛠️ Maintenance

### Backup Recommendations
- **Daily**: Transaction log backups
- **Weekly**: Full database backups
- **Monthly**: Archive old audit logs (>90 days)

### Monitoring Queries
```sql
-- Check for expiring subscriptions (next 7 days)
SELECT name, subscription_end_date
FROM hospitals
WHERE subscription_status = 'Active'
    AND subscription_end_date BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '7 days';

-- Check for overdue payments
SELECT COUNT(*) AS overdue_count
FROM payments
WHERE payment_status = 'Pending'
    AND due_date < CURRENT_TIMESTAMP;
```

## 📚 Documentation Files

| File | Description |
|------|-------------|
| `postgresql/01_hospital_onboarding_schema.sql` | PostgreSQL complete schema (850 lines) |
| `postgresql/02_seed_data.sql` | PostgreSQL seed data (300 lines) |
| `sqlserver/01_hospital_onboarding_schema.sql` | SQL Server complete schema (950 lines) |
| `README.md` | This documentation |

## 🎯 Next Steps

1. **Deploy Schema**: Run scripts in your environment
2. **Configure Multi-Tenancy**: Set up tenant isolation in application layer
3. **Test Queries**: Verify performance with sample data
4. **Integrate Application**: Connect with Domain/Application layers
5. **Monitor Performance**: Add query monitoring and optimization

---

**Version**: 1.0  
**Created**: November 22, 2025  
**License**: Proprietary - MedWay Healthcare Platform  
**Support**: Database schema supports multi-tenant SaaS architecture with complete audit trail
