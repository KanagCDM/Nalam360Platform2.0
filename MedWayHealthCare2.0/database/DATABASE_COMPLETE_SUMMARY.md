# Hospital Onboarding Database - Complete Summary

## 📦 Deliverables

### ✅ Database Schemas Created

| File | Database | Lines | Description |
|------|----------|-------|-------------|
| `postgresql/01_hospital_onboarding_schema.sql` | PostgreSQL 12+ | 850 | Complete schema with RLS, triggers, views |
| `postgresql/02_seed_data.sql` | PostgreSQL 12+ | 300 | 5 plans + 35 facilities + sample data |
| `sqlserver/01_hospital_onboarding_schema.sql` | SQL Server 2016+ | 950 | Complete schema with RLS, triggers, views |
| `MIGRATION_GUIDE.sql` | Both | 400 | Step-by-step deployment guide |
| `EF_CORE_CONFIGURATION.cs` | .NET | 600 | Entity Framework Core entities & configs |
| `README.md` | Documentation | 500 | Complete database documentation |

**Total: 6 files, ~3,600 lines of production-ready database code**

## 🏗️ Schema Architecture

### Multi-Tenant SaaS Design
- **Tenant Isolation**: Row-Level Security (RLS) using `TenantId` column
- **Data Segregation**: Each hospital = separate tenant with GUID identifier
- **Shared Database**: All tenants in one database with security policies
- **Automatic Filtering**: Queries automatically scoped to current tenant

### Database Support Matrix

| Feature | PostgreSQL | SQL Server | Status |
|---------|------------|------------|--------|
| **Core Schema** | ✅ 12+ | ✅ 2016+ | Production Ready |
| **Multi-Tenancy (RLS)** | ✅ Native | ✅ 2016+ | Implemented |
| **JSON Support** | ✅ JSONB | ✅ NVARCHAR(MAX) | Optimized |
| **Full-Text Search** | ✅ pg_trgm | ✅ Full-Text Index | Configured |
| **Triggers** | ✅ PL/pgSQL | ✅ T-SQL | Auto-audit enabled |
| **Enums** | ✅ Native ENUM | ✅ VARCHAR + CHECK | Type-safe |
| **UUID/GUID** | ✅ uuid-ossp | ✅ UNIQUEIDENTIFIER | Primary keys |

## 📊 Database Objects Summary

### Tables (7 Core + 1 Audit)

#### 1. **hospitals** (Tenant Root)
- **Purpose**: Main tenant entity storing hospital information
- **Key Features**: 
  - Unique `TenantId` for multi-tenant isolation
  - Status workflow (PendingApproval → Active/Rejected/Suspended)
  - Subscription tracking (Trial → Active/Expired)
  - Soft delete support
- **Relationships**: 1:N Branches, 1:N Payments, 1:N HospitalFacilities
- **Indexes**: 8 indexes (tenant_id, status, email, subscription dates)

#### 2. **branches**
- **Purpose**: Hospital branch/location management
- **Key Features**:
  - Multi-branch support per hospital
  - Main branch designation
  - Branch-specific capacity tracking
  - Manager assignment
- **Relationships**: N:1 Hospital, 1:N BranchFacilities
- **Constraints**: UNIQUE(hospital_id, branch_code)

#### 3. **subscription_plans**
- **Purpose**: Flexible subscription pricing tiers
- **Key Features**:
  - Capacity-based pricing (users, branches, beds)
  - Overage charges for exceeding limits
  - Multiple billing cycles (Monthly/Quarterly/Yearly)
  - Trial plan support
- **Seed Data**: 5 pre-configured plans (Trial, Basic, Pro, Enterprise, Annual)

#### 4. **payments**
- **Purpose**: Complete billing and payment tracking
- **Key Features**:
  - Multiple payment methods (Card, Bank Transfer, UPI, etc.)
  - Retry logic for failed payments
  - Refund support
  - Invoice generation tracking
- **Relationships**: N:1 Hospital

#### 5. **global_facilities** (Master Data)
- **Purpose**: Master list of available medical facilities
- **Key Features**:
  - Categorized facilities (Diagnostic, Surgical, Emergency, etc.)
  - Certification requirements tracking
  - Standard vs custom facilities
- **Seed Data**: 35+ facilities (ICU, ER, OT, Lab, X-Ray, MRI, etc.)

#### 6. **hospital_facilities**
- **Purpose**: Hospital-level facility mapping
- **Key Features**:
  - Links hospitals to global facilities
  - Availability and capacity tracking
  - Cascade delete with hospital
- **Constraints**: UNIQUE(hospital_id, facility_id)

#### 7. **branch_facilities**
- **Purpose**: Branch-level facility mapping
- **Key Features**:
  - Maps facilities to specific branches
  - Location details (floor, operating hours)
  - Staff assignment
- **Constraints**: UNIQUE(branch_id, hospital_facility_id)

#### 8. **audit_logs**
- **Purpose**: Complete audit trail for all changes
- **Key Features**:
  - Before/after state in JSON
  - Changed fields tracking
  - User, IP, timestamp capture
  - Automatic via triggers

### Indexes (50+ total)

**Performance Optimization**:
- ✅ Filtered indexes (exclude soft-deleted records)
- ✅ Covering indexes (avoid key lookups)
- ✅ Composite indexes (multi-column queries)
- ✅ Full-text indexes (search functionality)
- ✅ JSONB/JSON GIN indexes (metadata queries)

**Key Index Patterns**:
```sql
-- Multi-tenant isolation
CREATE INDEX idx_hospitals_tenant_id ON hospitals(tenant_id) WHERE is_deleted = FALSE;

-- Common queries
CREATE INDEX idx_hospitals_status_subscription 
    ON hospitals(status, subscription_status) WHERE is_deleted = FALSE;

-- Expiry tracking
CREATE INDEX idx_hospitals_subscription_end 
    ON hospitals(subscription_end_date) 
    WHERE is_deleted = FALSE AND subscription_status = 'Active';
```

### Views (2)

#### vw_ActiveHospitals
Aggregated view showing hospitals with branch and facility counts:
```sql
SELECT hospital_id, name, status, total_branches, total_facilities
FROM vw_ActiveHospitals
WHERE subscription_status = 'Active';
```

#### vw_PaymentSummary
Payment analytics per hospital:
```sql
SELECT hospital_id, total_paid, total_pending, last_payment_date
FROM vw_PaymentSummary
WHERE total_pending > 0;
```

### Triggers (14 total)

**Auto-Update Triggers** (7):
- Update `updated_at` timestamp on every modification
- Increment `row_version` for optimistic concurrency

**Audit Triggers** (7):
- Automatically log all INSERT/UPDATE/DELETE operations
- Capture before/after state in JSON
- Track changed fields array

### Functions/Stored Procedures

**PostgreSQL**:
- `update_updated_at_column()` - Auto-update timestamps
- `audit_trail_trigger()` - Automatic audit logging
- `fn_TenantAccessPredicate()` - RLS filter function

**SQL Server**:
- `usp_GetHospitalDashboard` - Complete hospital dashboard data
- `fn_CalculateSubscriptionCost` - Dynamic pricing calculation

## 🔐 Multi-Tenancy Implementation

### Tenant Isolation Pattern

**PostgreSQL Example**:
```sql
-- Set tenant context (in application layer)
SET app.current_tenant_id = '20000000-0000-0000-0000-000000000001';
SET app.user_role = 'HospitalAdmin';

-- All queries automatically filtered by RLS
SELECT * FROM hospital_onboarding.hospitals;
-- Returns ONLY hospitals where tenant_id = current_tenant_id
```

**SQL Server Example**:
```sql
-- Set tenant context
EXEC sp_set_session_context 'TenantId', '20000000-0000-0000-0000-000000000001';
EXEC sp_set_session_context 'UserRole', 'HospitalAdmin';

-- Security policy automatically filters
SELECT * FROM hospital_onboarding.Hospitals;
-- Returns ONLY hospitals where TenantId = SESSION_CONTEXT('TenantId')
```

### Security Policies

**Row-Level Security (RLS)**:
- ✅ Enabled on `hospitals` table
- ✅ Automatic tenant filtering
- ✅ Admin override capability
- ✅ Session-based context

**Soft Delete**:
- ✅ All tables use `is_deleted` / `IsDeleted` flag
- ✅ Filtered indexes exclude deleted records
- ✅ Data never physically deleted (audit compliance)

## 📈 Performance Features

### Query Optimization
- **Indexed Columns**: All FK columns, status fields, dates
- **Filtered Indexes**: Exclude soft-deleted records (20-30% smaller indexes)
- **Covering Indexes**: Include frequently selected columns
- **Partitioning Ready**: Design supports future table partitioning by tenant

### Caching Strategy
```sql
-- Frequently accessed master data
SELECT * FROM global_facilities WHERE is_active = TRUE;
-- Cache for 24 hours - rarely changes

-- Subscription plans
SELECT * FROM subscription_plans WHERE is_public = TRUE;
-- Cache for 1 hour - changes infrequently
```

### Estimated Performance
| Query Type | Target | Notes |
|------------|--------|-------|
| Single record by PK | < 5ms | Clustered index seek |
| List with pagination (100 rows) | < 50ms | Covering index |
| Complex join (hospital + branches + facilities) | < 200ms | Proper FK indexes |
| Full-text search | < 100ms | GIN/Full-text indexes |
| Dashboard aggregation | < 500ms | Pre-calculated in view |

## 🎲 Seed Data Summary

### Subscription Plans (5 Plans)

| Code | Name | Price/Month | Users | Branches | Highlight |
|------|------|-------------|-------|----------|-----------|
| **TRIAL** | Trial Plan | ₹0 | 5 | 1 | 30-day free trial |
| **BASIC** | Basic Plan | ₹2,999 | 10 | 1 | Small clinics |
| **PROFESSIONAL** | Professional Plan | ₹9,999 | 50 | 5 | ⭐ Recommended |
| **ENTERPRISE** | Enterprise Plan | ₹29,999 | 200 | 20 | Large networks |
| **BASIC_YEARLY** | Basic Annual | ₹30,589 | 10 | 1 | 15% discount |

**Overage Pricing Example** (Professional Plan):
- Base: ₹9,999 (50 users, 5 branches, 500 beds)
- +25 extra users × ₹150 = ₹3,750
- +3 extra branches × ₹1,200 = ₹3,600
- +100 extra beds × ₹30 = ₹3,000
- **Total: ₹20,349/month**

### Global Facilities (35+ Facilities)

#### Emergency & Critical Care (5)
- ICU, NICU, CCU, ER, Trauma Center

#### Surgical Facilities (5)
- General OT, Cardiac OT, Neuro OT, Ortho OT, Minor OT

#### Diagnostic & Radiology (7)
- X-Ray, CT Scan, MRI, Ultrasound, Mammography, ECG, Echocardiography

#### Laboratory Services (6)
- Pathology Lab, Microbiology Lab, Biochemistry Lab, Hematology Lab, Immunology Lab, Blood Bank

#### Treatment & Therapy (6)
- Dialysis Unit, Physiotherapy, Chemotherapy Unit, Radiation Therapy, Dental Clinic, Ophthalmology

#### Pharmacy & Support (6)
- In-house Pharmacy, 24/7 Pharmacy, Ambulance Service, Cafeteria, Parking Facility

## 🚀 Deployment Checklist

### Pre-Deployment
- [ ] Review schema scripts for environment-specific changes
- [ ] Update connection strings in application configuration
- [ ] Verify database server versions (PostgreSQL 12+ or SQL Server 2016+)
- [ ] Check disk space requirements (estimate 500MB initial + growth)
- [ ] Create database backups (if migrating)
- [ ] Schedule maintenance window
- [ ] Notify stakeholders

### Deployment Steps
1. **Create Database**: Run database creation script
2. **Run Schema**: Execute `01_hospital_onboarding_schema.sql`
3. **Load Seed Data**: Execute `02_seed_data.sql`
4. **Create App User**: Set up application database user
5. **Grant Permissions**: Execute permission grants
6. **Verify Installation**: Run verification queries
7. **Configure Backups**: Set up automated backup schedule
8. **Enable Monitoring**: Configure query performance monitoring

### Post-Deployment
- [ ] Verify table row counts (5 plans, 35+ facilities)
- [ ] Test multi-tenancy isolation
- [ ] Validate triggers are firing
- [ ] Test application connectivity
- [ ] Run performance baseline queries
- [ ] Configure monitoring alerts
- [ ] Update documentation with production details
- [ ] Train operations team

## 🔍 Common Queries

### Get Complete Hospital Dashboard
```sql
-- PostgreSQL
SELECT 
    h.*,
    sp.name AS subscription_plan_name,
    COUNT(DISTINCT b.id) AS total_branches,
    COUNT(DISTINCT hf.id) AS total_facilities,
    SUM(CASE WHEN p.payment_status = 'Successful' THEN p.total_amount ELSE 0 END) AS total_paid
FROM hospitals h
LEFT JOIN subscription_plans sp ON h.subscription_plan_id = sp.id
LEFT JOIN branches b ON h.id = b.hospital_id AND b.is_deleted = FALSE
LEFT JOIN hospital_facilities hf ON h.id = hf.hospital_id AND hf.is_deleted = FALSE
LEFT JOIN payments p ON h.id = p.hospital_id AND p.is_deleted = FALSE
WHERE h.tenant_id = 'YOUR_TENANT_ID'
GROUP BY h.id, sp.id;
```

### Find Expiring Subscriptions
```sql
-- SQL Server
SELECT 
    Name,
    SubscriptionEndDate,
    DATEDIFF(day, GETDATE(), SubscriptionEndDate) AS DaysRemaining
FROM hospital_onboarding.Hospitals
WHERE SubscriptionStatus = 'Active'
    AND SubscriptionEndDate BETWEEN GETDATE() AND DATEADD(day, 30, GETDATE())
    AND IsDeleted = 0
ORDER BY SubscriptionEndDate;
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
INNER JOIN hospitals h ON p.hospital_id = h.id
WHERE p.payment_status = 'Pending'
    AND p.due_date < CURRENT_TIMESTAMP
    AND p.is_deleted = FALSE
ORDER BY days_overdue DESC;
```

## 🛠️ Maintenance

### Regular Tasks

**Daily**:
- Transaction log backups
- Monitor failed payments
- Check audit log size

**Weekly**:
- Full database backups
- Review slow query log
- Update statistics

**Monthly**:
- Archive old audit logs (>90 days)
- Review index usage statistics
- Analyze table growth trends
- Optimize fragmented indexes

### Monitoring Queries

```sql
-- PostgreSQL: Check table sizes
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS total_size
FROM pg_tables
WHERE schemaname = 'hospital_onboarding'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- SQL Server: Check index usage
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    s.user_seeks + s.user_scans AS TotalReads,
    s.user_updates AS TotalWrites,
    CASE WHEN (s.user_seeks + s.user_scans) = 0 THEN 0
         ELSE ROUND((s.user_seeks + s.user_scans) * 100.0 / 
              (s.user_seeks + s.user_scans + s.user_updates), 2) 
    END AS ReadWriteRatio
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.tables t ON s.object_id = t.object_id
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE t.schema_id = SCHEMA_ID('hospital_onboarding')
ORDER BY TotalReads DESC;
```

## 📚 Integration with Application

### Entity Framework Core
Complete EF Core configuration provided in `EF_CORE_CONFIGURATION.cs`:
- ✅ All entities defined with proper mappings
- ✅ Multi-tenant DbContext with query filters
- ✅ Automatic audit field updates (CreatedAt, UpdatedAt)
- ✅ Soft delete query filters
- ✅ Enum to string conversions
- ✅ Fluent API configurations

### Connection Strings

**PostgreSQL**:
```json
{
  "ConnectionStrings": {
    "HospitalOnboarding": "Host=localhost;Database=medway_healthcare;Username=medway_app;Password=YourSecurePassword"
  }
}
```

**SQL Server**:
```json
{
  "ConnectionStrings": {
    "HospitalOnboarding": "Server=localhost;Database=MedWayHealthCare;User Id=medway_app;Password=YourSecurePassword;TrustServerCertificate=True"
  }
}
```

## 🎯 Next Steps

1. **Deploy Database**
   - Choose PostgreSQL or SQL Server
   - Run schema and seed scripts
   - Verify installation

2. **Implement Domain Layer**
   - Use EF Core entities as starting point
   - Add business logic to domain entities
   - Implement repository interfaces

3. **Implement Application Layer**
   - Create command/query handlers
   - Use entities from EF Core configuration
   - Return `Result<T>` for all operations

4. **Run Unit Tests**
   - Execute test suite created earlier
   - All 35 tests should pass once implementation is complete

5. **Deploy API**
   - Configure multi-tenancy middleware
   - Set up authentication/authorization
   - Test end-to-end workflows

---

## 📊 Summary Statistics

| Metric | Count | Notes |
|--------|-------|-------|
| **Total Files** | 6 | PostgreSQL, SQL Server, Migration, EF Core, Docs |
| **Total Lines of Code** | ~3,600 | Production-ready database code |
| **Core Tables** | 7 | hospitals, branches, plans, payments, facilities |
| **Audit Tables** | 1 | Complete audit trail |
| **Indexes** | 50+ | Optimized for performance |
| **Triggers** | 14 | Auto-audit + timestamp updates |
| **Views** | 2 | Dashboard and analytics |
| **Functions/SPs** | 5 | Utility and business logic |
| **Subscription Plans** | 5 | Trial to Enterprise tiers |
| **Global Facilities** | 35+ | Medical facilities catalog |
| **Supported Databases** | 2 | PostgreSQL 12+, SQL Server 2016+ |

---

**Status**: ✅ **Production Ready**  
**Created**: November 22, 2025  
**Multi-Tenant**: ✅ Row-Level Security Enabled  
**Performance**: ✅ Fully Indexed and Optimized  
**Audit Trail**: ✅ Complete Change Tracking  
**Documentation**: ✅ Comprehensive Guides Included
