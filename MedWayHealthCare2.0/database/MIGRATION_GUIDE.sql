-- ============================================================================
-- MedWay Hospital Onboarding - Database Migration Guide
-- From Development to Production
-- ============================================================================

-- ============================================================================
-- PHASE 1: PRE-MIGRATION CHECKLIST
-- ============================================================================

/*
□ Backup existing database (if any)
□ Review current schema version
□ Test scripts in staging environment
□ Verify user permissions
□ Check disk space (estimate 2x current size during migration)
□ Schedule maintenance window
□ Notify stakeholders
*/

-- ============================================================================
-- PHASE 2: POSTGRESQL MIGRATION
-- ============================================================================

-- 2.1: Create Database
CREATE DATABASE medway_healthcare
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

COMMENT ON DATABASE medway_healthcare IS 'MedWay Healthcare Platform - Multi-tenant SaaS';

-- 2.2: Connect to Database
\c medway_healthcare;

-- 2.3: Create Required Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";    -- UUID generation
CREATE EXTENSION IF NOT EXISTS pg_trgm;        -- Text search
CREATE EXTENSION IF NOT EXISTS pg_stat_statements; -- Query performance monitoring

-- 2.4: Create Application User (PostgreSQL)
CREATE USER medway_app WITH PASSWORD 'ChangeThisPassword!123';
GRANT CONNECT ON DATABASE medway_healthcare TO medway_app;

-- 2.5: Run Schema Script
-- Execute: \i /path/to/01_hospital_onboarding_schema.sql

-- 2.6: Grant Permissions
GRANT USAGE ON SCHEMA hospital_onboarding TO medway_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA hospital_onboarding TO medway_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA hospital_onboarding TO medway_app;

-- Set default privileges for future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA hospital_onboarding
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO medway_app;

ALTER DEFAULT PRIVILEGES IN SCHEMA hospital_onboarding
    GRANT USAGE, SELECT ON SEQUENCES TO medway_app;

-- 2.7: Load Seed Data
-- Execute: \i /path/to/02_seed_data.sql

-- 2.8: Verify Installation
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'hospital_onboarding'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- Check record counts
SELECT 'subscription_plans' AS table_name, COUNT(*) FROM hospital_onboarding.subscription_plans
UNION ALL
SELECT 'global_facilities', COUNT(*) FROM hospital_onboarding.global_facilities
UNION ALL
SELECT 'hospitals', COUNT(*) FROM hospital_onboarding.hospitals
UNION ALL
SELECT 'branches', COUNT(*) FROM hospital_onboarding.branches;

-- ============================================================================
-- PHASE 3: SQL SERVER MIGRATION
-- ============================================================================

-- 3.1: Create Database (Run as SA or sysadmin)
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MedWayHealthCare')
BEGIN
    CREATE DATABASE MedWayHealthCare
    ON PRIMARY (
        NAME = MedWay_Data,
        FILENAME = 'C:\SQLData\MedWayHealthCare.mdf',
        SIZE = 500MB,
        MAXSIZE = UNLIMITED,
        FILEGROWTH = 100MB
    )
    LOG ON (
        NAME = MedWay_Log,
        FILENAME = 'C:\SQLData\MedWayHealthCare_log.ldf',
        SIZE = 100MB,
        MAXSIZE = 2GB,
        FILEGROWTH = 10MB
    );
END
GO

ALTER DATABASE MedWayHealthCare SET RECOVERY FULL;
GO

-- 3.2: Create Application Login
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'medway_app')
BEGIN
    CREATE LOGIN medway_app WITH PASSWORD = 'ChangeThisPassword!123';
END
GO

-- 3.3: Create Database User
USE MedWayHealthCare;
GO

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'medway_app')
BEGIN
    CREATE USER medway_app FOR LOGIN medway_app;
END
GO

-- 3.4: Run Schema Script
-- Execute: sqlcmd -S localhost -d MedWayHealthCare -i 01_hospital_onboarding_schema.sql

-- 3.5: Grant Permissions
USE MedWayHealthCare;
GO

-- Grant schema permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::hospital_onboarding TO medway_app;
GO

-- Grant execute on stored procedures
GRANT EXECUTE ON SCHEMA::hospital_onboarding TO medway_app;
GO

-- Grant specific permissions on views
GRANT SELECT ON hospital_onboarding.vw_ActiveHospitals TO medway_app;
GRANT SELECT ON hospital_onboarding.vw_PaymentSummary TO medway_app;
GO

-- 3.6: Verify Installation
SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.schema_id = SCHEMA_ID('hospital_onboarding')
    AND p.index_id IN (0, 1)
GROUP BY t.schema_id, t.name, p.rows
ORDER BY TotalSpaceKB DESC;

-- Check record counts
SELECT 'SubscriptionPlans' AS TableName, COUNT(*) AS RecordCount FROM hospital_onboarding.SubscriptionPlans
UNION ALL
SELECT 'GlobalFacilities', COUNT(*) FROM hospital_onboarding.GlobalFacilities
UNION ALL
SELECT 'Hospitals', COUNT(*) FROM hospital_onboarding.Hospitals
UNION ALL
SELECT 'Branches', COUNT(*) FROM hospital_onboarding.Branches;
GO

-- ============================================================================
-- PHASE 4: DATA MIGRATION (If migrating from existing system)
-- ============================================================================

-- 4.1: Export Data from Old System
-- Example for PostgreSQL to PostgreSQL migration:

-- Export hospitals
\COPY (SELECT * FROM old_schema.hospitals) TO '/tmp/hospitals.csv' WITH CSV HEADER;

-- Import to new schema
\COPY hospital_onboarding.hospitals FROM '/tmp/hospitals.csv' WITH CSV HEADER;

-- 4.2: SQL Server Bulk Insert Example
/*
BULK INSERT hospital_onboarding.Hospitals
FROM 'C:\DataMigration\hospitals.csv'
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    TABLOCK
);
*/

-- ============================================================================
-- PHASE 5: POST-MIGRATION VALIDATION
-- ============================================================================

-- 5.1: Verify Row Counts (PostgreSQL)
DO $$
DECLARE
    expected_plans INTEGER := 5;
    expected_facilities INTEGER := 35;
    actual_plans INTEGER;
    actual_facilities INTEGER;
BEGIN
    SELECT COUNT(*) INTO actual_plans FROM hospital_onboarding.subscription_plans WHERE is_deleted = FALSE;
    SELECT COUNT(*) INTO actual_facilities FROM hospital_onboarding.global_facilities WHERE is_deleted = FALSE;
    
    RAISE NOTICE 'Subscription Plans: Expected %, Actual %', expected_plans, actual_plans;
    RAISE NOTICE 'Global Facilities: Expected %, Actual %', expected_facilities, actual_facilities;
    
    IF actual_plans < expected_plans THEN
        RAISE WARNING 'Subscription plans count is below expected!';
    END IF;
    
    IF actual_facilities < expected_facilities THEN
        RAISE WARNING 'Global facilities count is below expected!';
    END IF;
END $$;

-- 5.2: Test Foreign Key Relationships
-- Add test hospital
INSERT INTO hospital_onboarding.hospitals (
    id, tenant_id, registration_number, name, legal_name, email, phone,
    address_line1, city, state, country, postal_code
) VALUES (
    uuid_generate_v4(),
    uuid_generate_v4(),
    'TEST001',
    'Test Hospital',
    'Test Hospital Pvt Ltd',
    'test@hospital.com',
    '+91-1234567890',
    'Test Address',
    'Test City',
    'Test State',
    'India',
    '123456'
);

-- Verify cascade on delete
-- This should also delete any related branches
DELETE FROM hospital_onboarding.hospitals WHERE registration_number = 'TEST001';

-- 5.3: Test Triggers (PostgreSQL)
-- Insert test record
INSERT INTO hospital_onboarding.hospitals (
    tenant_id, registration_number, name, legal_name, email, phone,
    address_line1, city, state, country, postal_code
) VALUES (
    uuid_generate_v4(), 'TRIGGER_TEST', 'Trigger Test', 'Trigger Test Ltd',
    'trigger@test.com', '+91-0000000000',
    'Test', 'Test', 'Test', 'India', '000000'
);

-- Check if audit log was created
SELECT * FROM hospital_onboarding.audit_logs 
WHERE entity_type = 'hospitals' 
    AND new_values->>'registration_number' = 'TRIGGER_TEST'
ORDER BY performed_at DESC LIMIT 1;

-- Update and check audit
UPDATE hospital_onboarding.hospitals 
SET name = 'Trigger Test Updated' 
WHERE registration_number = 'TRIGGER_TEST';

SELECT * FROM hospital_onboarding.audit_logs 
WHERE entity_type = 'hospitals' 
    AND entity_id = (SELECT id FROM hospital_onboarding.hospitals WHERE registration_number = 'TRIGGER_TEST')
ORDER BY performed_at DESC;

-- Cleanup
DELETE FROM hospital_onboarding.hospitals WHERE registration_number = 'TRIGGER_TEST';

-- 5.4: Test Row-Level Security (PostgreSQL)
-- Set tenant context
SET app.current_tenant_id = '20000000-0000-0000-0000-000000000001';
SET app.user_role = 'HospitalAdmin';

-- Should return only hospitals for this tenant
SELECT COUNT(*) FROM hospital_onboarding.hospitals;

-- Reset context
RESET app.current_tenant_id;
RESET app.user_role;

-- 5.5: Performance Test Queries
EXPLAIN ANALYZE
SELECT h.*, sp.name AS plan_name
FROM hospital_onboarding.hospitals h
LEFT JOIN hospital_onboarding.subscription_plans sp ON h.subscription_plan_id = sp.id
WHERE h.is_deleted = FALSE
    AND h.status = 'Active';

-- Check index usage
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    idx_tup_read,
    idx_tup_fetch
FROM pg_stat_user_indexes
WHERE schemaname = 'hospital_onboarding'
ORDER BY idx_scan DESC;

-- ============================================================================
-- PHASE 6: OPTIMIZATION
-- ============================================================================

-- 6.1: Update Statistics (PostgreSQL)
ANALYZE hospital_onboarding.hospitals;
ANALYZE hospital_onboarding.branches;
ANALYZE hospital_onboarding.payments;
ANALYZE hospital_onboarding.subscription_plans;

-- 6.2: Update Statistics (SQL Server)
/*
UPDATE STATISTICS hospital_onboarding.Hospitals WITH FULLSCAN;
UPDATE STATISTICS hospital_onboarding.Branches WITH FULLSCAN;
UPDATE STATISTICS hospital_onboarding.Payments WITH FULLSCAN;
UPDATE STATISTICS hospital_onboarding.SubscriptionPlans WITH FULLSCAN;
*/

-- 6.3: Rebuild Indexes (PostgreSQL)
REINDEX SCHEMA hospital_onboarding;

-- 6.4: Rebuild Indexes (SQL Server)
/*
ALTER INDEX ALL ON hospital_onboarding.Hospitals REBUILD WITH (ONLINE = ON);
ALTER INDEX ALL ON hospital_onboarding.Branches REBUILD WITH (ONLINE = ON);
*/

-- ============================================================================
-- PHASE 7: MONITORING SETUP
-- ============================================================================

-- 7.1: Enable Query Logging (PostgreSQL)
-- Add to postgresql.conf:
-- log_statement = 'mod'
-- log_duration = on
-- log_min_duration_statement = 1000

-- 7.2: Create Monitoring Views (PostgreSQL)
CREATE OR REPLACE VIEW hospital_onboarding.vw_table_sizes AS
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS total_size,
    pg_size_pretty(pg_relation_size(schemaname||'.'||tablename)) AS table_size,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename) - 
                   pg_relation_size(schemaname||'.'||tablename)) AS indexes_size
FROM pg_tables
WHERE schemaname = 'hospital_onboarding'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- 7.3: Create Performance Monitoring Query (SQL Server)
/*
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.tables t ON s.object_id = t.object_id
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE t.schema_id = SCHEMA_ID('hospital_onboarding')
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;
*/

-- ============================================================================
-- PHASE 8: BACKUP CONFIGURATION
-- ============================================================================

-- 8.1: PostgreSQL Backup Script
-- Create backup directory: mkdir -p /var/backups/medway

-- Full backup:
-- pg_dump -U postgres -F c -b -v -f /var/backups/medway/medway_full_$(date +%Y%m%d).backup medway_healthcare

-- Schema only:
-- pg_dump -U postgres -s -F p -f /var/backups/medway/medway_schema_$(date +%Y%m%d).sql medway_healthcare

-- Data only:
-- pg_dump -U postgres -a -F p -f /var/backups/medway/medway_data_$(date +%Y%m%d).sql medway_healthcare

-- 8.2: SQL Server Backup T-SQL
/*
-- Full backup
BACKUP DATABASE MedWayHealthCare
TO DISK = 'C:\SQLBackups\MedWayHealthCare_Full.bak'
WITH FORMAT, COMPRESSION, STATS = 10;

-- Differential backup
BACKUP DATABASE MedWayHealthCare
TO DISK = 'C:\SQLBackups\MedWayHealthCare_Diff.bak'
WITH DIFFERENTIAL, COMPRESSION, STATS = 10;

-- Transaction log backup
BACKUP LOG MedWayHealthCare
TO DISK = 'C:\SQLBackups\MedWayHealthCare_Log.trn'
WITH COMPRESSION, STATS = 10;
*/

-- ============================================================================
-- PHASE 9: ROLLBACK PLAN (If migration fails)
-- ============================================================================

-- 9.1: PostgreSQL Rollback
/*
-- 1. Stop application
-- 2. Drop new database
DROP DATABASE IF EXISTS medway_healthcare;

-- 3. Restore from backup
pg_restore -U postgres -d medway_healthcare /var/backups/medway/medway_full_20251122.backup

-- 4. Restart application
*/

-- 9.2: SQL Server Rollback
/*
-- 1. Stop application
-- 2. Restore database
USE master;
ALTER DATABASE MedWayHealthCare SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE MedWayHealthCare
FROM DISK = 'C:\SQLBackups\MedWayHealthCare_Full.bak'
WITH REPLACE, RECOVERY;

ALTER DATABASE MedWayHealthCare SET MULTI_USER;

-- 3. Restart application
*/

-- ============================================================================
-- MIGRATION COMPLETE CHECKLIST
-- ============================================================================

/*
POST-MIGRATION VERIFICATION:

□ All tables created (7 core + 1 audit)
□ All indexes created and valid
□ All triggers functioning
□ Row-level security policies active
□ Seed data loaded (5 plans, 35+ facilities)
□ Application user created with correct permissions
□ Audit logging working
□ Foreign key relationships intact
□ Views accessible
□ Stored procedures/functions operational
□ Backup strategy configured
□ Monitoring enabled
□ Performance baseline established
□ Documentation updated
□ Stakeholders notified
□ Application tested with new database

PERFORMANCE BASELINES:
□ Query response time < 100ms for single record lookups
□ List queries < 500ms for 100 records
□ Complex aggregations < 2s
□ Index usage > 90% for filtered queries
*/

-- ============================================================================
-- END OF MIGRATION GUIDE
-- ============================================================================
