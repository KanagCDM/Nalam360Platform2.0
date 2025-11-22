# Hospital Onboarding Database - Quick Reference

## 🚀 Quick Start

### Installation (PostgreSQL)
```bash
# 1. Create database
createdb medway_healthcare

# 2. Run schema
psql -d medway_healthcare -f postgresql/01_hospital_onboarding_schema.sql

# 3. Load seed data
psql -d medway_healthcare -f postgresql/02_seed_data.sql
```

### Installation (SQL Server)
```powershell
# 1. Create database
sqlcmd -S localhost -Q "CREATE DATABASE MedWayHealthCare"

# 2. Run schema
sqlcmd -S localhost -d MedWayHealthCare -i sqlserver/01_hospital_onboarding_schema.sql
```

## 📊 Table Quick Reference

| Table | Purpose | Key Columns | Indexes |
|-------|---------|-------------|---------|
| `hospitals` | Tenant root | `id`, `tenant_id`, `status`, `subscription_status` | 8 |
| `branches` | Hospital locations | `id`, `hospital_id`, `branch_code`, `is_active` | 4 |
| `subscription_plans` | Pricing tiers | `id`, `code`, `base_price`, `max_users` | 4 |
| `payments` | Billing history | `id`, `hospital_id`, `payment_status`, `due_date` | 7 |
| `global_facilities` | Facility catalog | `id`, `code`, `category` | 3 |
| `hospital_facilities` | Hospital→Facility | `hospital_id`, `facility_id` | 3 |
| `branch_facilities` | Branch→Facility | `branch_id`, `hospital_facility_id` | 3 |
| `audit_logs` | Change tracking | `entity_type`, `entity_id`, `performed_at` | 5 |

## 🔑 Common Patterns

### Multi-Tenant Queries (PostgreSQL)
```sql
-- Set tenant context
SET app.current_tenant_id = 'YOUR_TENANT_ID';

-- All queries automatically filtered
SELECT * FROM hospital_onboarding.hospitals;
```

### Multi-Tenant Queries (SQL Server)
```sql
-- Set tenant context
EXEC sp_set_session_context 'TenantId', 'YOUR_TENANT_ID';

-- All queries automatically filtered
SELECT * FROM hospital_onboarding.Hospitals;
```

### CRUD Operations

**Create Hospital**:
```sql
INSERT INTO hospitals (tenant_id, registration_number, name, legal_name, email, phone, 
                       address_line1, city, state, country, postal_code)
VALUES (uuid_generate_v4(), 'REG001', 'Hospital Name', 'Legal Name', 
        'email@hospital.com', '+91-1234567890',
        'Address', 'City', 'State', 'India', '123456');
```

**Update Hospital Status**:
```sql
UPDATE hospitals 
SET status = 'Active',
    approved_at = CURRENT_TIMESTAMP,
    approved_by = 'admin_user_id'
WHERE id = 'hospital_id';
```

**Add Branch**:
```sql
INSERT INTO branches (hospital_id, branch_code, name, email, phone,
                      address_line1, city, state, country, postal_code, is_main_branch)
VALUES ('hospital_id', 'BR001', 'Main Branch', 'branch@hospital.com', '+91-0000000000',
        'Address', 'City', 'State', 'India', '123456', true);
```

**Map Facility to Hospital**:
```sql
-- First, add to hospital
INSERT INTO hospital_facilities (hospital_id, facility_id, is_available, capacity)
VALUES ('hospital_id', 'facility_id', true, 10);

-- Then, map to specific branch
INSERT INTO branch_facilities (branch_id, hospital_facility_id, is_available, floor_number)
VALUES ('branch_id', 'hospital_facility_id', true, '2nd Floor');
```

**Process Payment**:
```sql
INSERT INTO payments (hospital_id, payment_reference, invoice_number,
                      amount, tax_amount, total_amount, payment_method, payment_status,
                      due_date, billing_period_start, billing_period_end)
VALUES ('hospital_id', 'PAY-001', 'INV-001',
        9999.00, 1799.82, 11798.82, 'CreditCard', 'Successful',
        CURRENT_TIMESTAMP + INTERVAL '15 days',
        CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + INTERVAL '1 month');
```

## 🔍 Common Queries

### Get Hospital Dashboard
```sql
SELECT 
    h.name,
    h.status,
    h.subscription_status,
    sp.name AS plan_name,
    COUNT(DISTINCT b.id) AS total_branches,
    COUNT(DISTINCT hf.id) AS total_facilities,
    SUM(CASE WHEN p.payment_status = 'Successful' THEN p.total_amount ELSE 0 END) AS total_paid
FROM hospitals h
LEFT JOIN subscription_plans sp ON h.subscription_plan_id = sp.id
LEFT JOIN branches b ON h.id = b.hospital_id AND b.is_deleted = FALSE
LEFT JOIN hospital_facilities hf ON h.id = hf.hospital_id AND hf.is_deleted = FALSE
LEFT JOIN payments p ON h.id = p.hospital_id AND p.is_deleted = FALSE
WHERE h.id = 'hospital_id'
GROUP BY h.id, sp.id;
```

### List Active Hospitals
```sql
SELECT id, name, email, status, subscription_status, created_at
FROM hospitals
WHERE is_deleted = FALSE
    AND status IN ('Active', 'PendingApproval')
ORDER BY created_at DESC;
```

### Find Expiring Trials
```sql
SELECT name, email, trial_end_date,
       trial_end_date - CURRENT_TIMESTAMP AS time_remaining
FROM hospitals
WHERE subscription_status = 'Trial'
    AND trial_end_date BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '7 days'
    AND is_deleted = FALSE
ORDER BY trial_end_date;
```

### Get Overdue Payments
```sql
SELECT 
    h.name AS hospital_name,
    p.invoice_number,
    p.total_amount,
    p.due_date,
    CURRENT_DATE - p.due_date::date AS days_overdue
FROM payments p
INNER JOIN hospitals h ON p.hospital_id = h.id
WHERE p.payment_status = 'Pending'
    AND p.due_date < CURRENT_TIMESTAMP
    AND p.is_deleted = FALSE
ORDER BY days_overdue DESC;
```

### List Facilities by Branch
```sql
SELECT 
    b.name AS branch_name,
    gf.name AS facility_name,
    gf.category,
    bf.is_available,
    bf.capacity,
    bf.floor_number
FROM branches b
INNER JOIN branch_facilities bf ON b.id = bf.branch_id
INNER JOIN hospital_facilities hf ON bf.hospital_facility_id = hf.id
INNER JOIN global_facilities gf ON hf.facility_id = gf.id
WHERE b.hospital_id = 'hospital_id'
    AND b.is_deleted = FALSE
    AND bf.is_deleted = FALSE
ORDER BY gf.category, gf.name;
```

## 📈 Subscription Pricing

### Calculate Cost
```sql
-- SQL Server Function
SELECT hospital_onboarding.fn_CalculateSubscriptionCost(
    @PlanId = 'plan_id',
    @NumUsers = 75,      -- 25 over Professional limit (50)
    @NumBranches = 8,    -- 3 over limit (5)
    @NumBeds = 600       -- 100 over limit (500)
) AS TotalMonthlyCost;

-- Manual calculation (PostgreSQL):
WITH plan AS (
    SELECT base_price, max_users, max_branches, max_beds,
           price_per_additional_user, price_per_additional_branch, price_per_additional_bed
    FROM subscription_plans WHERE id = 'plan_id'
)
SELECT 
    base_price +
    GREATEST(75 - max_users, 0) * price_per_additional_user +
    GREATEST(8 - max_branches, 0) * price_per_additional_branch +
    GREATEST(600 - COALESCE(max_beds, 999999), 0) * price_per_additional_bed AS total_cost
FROM plan;
```

## 🛡️ Security Examples

### Soft Delete
```sql
-- Never hard delete - use soft delete
UPDATE hospitals 
SET is_deleted = TRUE,
    deleted_at = CURRENT_TIMESTAMP,
    deleted_by = 'user_id'
WHERE id = 'hospital_id';

-- Restore soft-deleted record
UPDATE hospitals 
SET is_deleted = FALSE,
    deleted_at = NULL,
    deleted_by = NULL
WHERE id = 'hospital_id';
```

### Audit Trail Query
```sql
SELECT 
    performed_at,
    action,
    performed_by,
    old_values->>'name' AS old_name,
    new_values->>'name' AS new_name,
    changed_fields
FROM audit_logs
WHERE entity_type = 'hospitals'
    AND entity_id = 'hospital_id'
ORDER BY performed_at DESC
LIMIT 10;
```

## 🔧 Maintenance

### Analyze Tables (PostgreSQL)
```sql
ANALYZE hospital_onboarding.hospitals;
ANALYZE hospital_onboarding.branches;
ANALYZE hospital_onboarding.payments;
```

### Update Statistics (SQL Server)
```sql
UPDATE STATISTICS hospital_onboarding.Hospitals WITH FULLSCAN;
UPDATE STATISTICS hospital_onboarding.Branches WITH FULLSCAN;
UPDATE STATISTICS hospital_onboarding.Payments WITH FULLSCAN;
```

### Check Table Sizes (PostgreSQL)
```sql
SELECT 
    tablename,
    pg_size_pretty(pg_total_relation_size('hospital_onboarding.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'hospital_onboarding'
ORDER BY pg_total_relation_size('hospital_onboarding.'||tablename) DESC;
```

### Check Index Usage (SQL Server)
```sql
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    s.user_seeks + s.user_scans AS TotalReads
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.tables t ON s.object_id = t.object_id
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE t.schema_id = SCHEMA_ID('hospital_onboarding')
ORDER BY TotalReads DESC;
```

## 📦 Seed Data Reference

### Subscription Plan IDs
```
Trial:        00000000-0000-0000-0000-000000000001
Basic:        00000000-0000-0000-0000-000000000002
Professional: 00000000-0000-0000-0000-000000000003
Enterprise:   00000000-0000-0000-0000-000000000004
Basic Annual: 00000000-0000-0000-0000-000000000005
```

### Common Facility Codes
```
ICU, NICU, CCU, ER, TRAUMA
OT_GENERAL, OT_CARDIAC, OT_NEURO, OT_ORTHO, MINOR_OT
XRAY, CT_SCAN, MRI, ULTRASOUND, MAMMOGRAPHY, ECG, ECHO
LAB_PATH, LAB_MICRO, LAB_BIO, LAB_HEMA, LAB_IMMUNO, BLOOD_BANK
DIALYSIS, PHYSIO, CHEMO, RADIO_THERAPY, DENTAL, OPTHAL
PHARMACY, PHARMACY_24, AMBULANCE, CAFETERIA, PARKING
```

## 🎯 Entity Framework Core

### Configure DbContext
```csharp
services.AddDbContext<HospitalOnboardingDbContext>(options =>
{
    // PostgreSQL
    options.UseNpgsql(configuration.GetConnectionString("HospitalOnboarding"));
    
    // SQL Server
    options.UseSqlServer(configuration.GetConnectionString("HospitalOnboarding"));
});
```

### Query with Multi-Tenancy
```csharp
// Inject tenant ID from current user context
var tenantId = _currentUserService.GetTenantId();

var dbContext = new HospitalOnboardingDbContext(options, tenantId);

// Automatically filtered by tenant
var hospitals = await dbContext.Hospitals
    .Where(h => h.Status == HospitalStatus.Active)
    .ToListAsync();
```

## 🔗 Connection Strings

### PostgreSQL
```json
{
  "ConnectionStrings": {
    "HospitalOnboarding": "Host=localhost;Database=medway_healthcare;Username=medway_app;Password=YourSecurePassword;SearchPath=hospital_onboarding"
  }
}
```

### SQL Server
```json
{
  "ConnectionStrings": {
    "HospitalOnboarding": "Server=localhost;Database=MedWayHealthCare;User Id=medway_app;Password=YourSecurePassword;TrustServerCertificate=True"
  }
}
```

## ⚡ Performance Tips

1. **Always use filtered indexes**: Queries should include `WHERE is_deleted = FALSE`
2. **Use covering indexes**: Include commonly selected columns in index
3. **Avoid SELECT ***: Explicitly list required columns
4. **Use pagination**: Always limit result sets (LIMIT/OFFSET or FETCH NEXT)
5. **Index foreign keys**: All FK columns should be indexed
6. **Analyze query plans**: Use EXPLAIN (PostgreSQL) or execution plans (SQL Server)
7. **Cache master data**: Subscription plans and global facilities rarely change
8. **Batch operations**: Use bulk inserts for multiple records
9. **Connection pooling**: Enable in connection string
10. **Monitor slow queries**: Set up query logging (>1000ms threshold)

## 📞 Troubleshooting

### Issue: RLS not filtering
```sql
-- Verify session context is set
-- PostgreSQL
SHOW app.current_tenant_id;

-- SQL Server
SELECT SESSION_CONTEXT(N'TenantId');
```

### Issue: Trigger not firing
```sql
-- Check trigger status (PostgreSQL)
SELECT tgname, tgenabled FROM pg_trigger WHERE tgname LIKE '%audit%';

-- Check trigger status (SQL Server)
SELECT name, is_disabled FROM sys.triggers WHERE parent_class_desc = 'OBJECT_OR_COLUMN';
```

### Issue: Slow queries
```sql
-- Check missing indexes (PostgreSQL)
SELECT schemaname, tablename, attname, n_distinct, correlation
FROM pg_stats
WHERE schemaname = 'hospital_onboarding'
ORDER BY n_distinct DESC;

-- Check missing indexes (SQL Server)
SELECT 
    DB_NAME(database_id) AS DatabaseName,
    OBJECT_NAME(object_id) AS TableName,
    equality_columns,
    inequality_columns,
    included_columns
FROM sys.dm_db_missing_index_details
WHERE database_id = DB_ID('MedWayHealthCare');
```

---

**Quick Links**:
- Full Documentation: `README.md`
- Migration Guide: `MIGRATION_GUIDE.sql`
- Schema Diagram: `SCHEMA_DIAGRAM.md`
- EF Core Config: `EF_CORE_CONFIGURATION.cs`
- Complete Summary: `DATABASE_COMPLETE_SUMMARY.md`
