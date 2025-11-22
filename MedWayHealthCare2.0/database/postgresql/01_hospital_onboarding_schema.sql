-- ============================================================================
-- MedWay Hospital Onboarding - PostgreSQL Schema
-- Multi-Tenant SaaS Architecture
-- Version: 1.0
-- Created: November 22, 2025
-- ============================================================================

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable pg_trgm for text search
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- ============================================================================
-- SCHEMA CREATION
-- ============================================================================

CREATE SCHEMA IF NOT EXISTS hospital_onboarding;

-- Set search path
SET search_path TO hospital_onboarding, public;

-- ============================================================================
-- ENUMS (PostgreSQL-specific)
-- ============================================================================

CREATE TYPE hospital_status AS ENUM (
    'PendingApproval',
    'Active',
    'Rejected',
    'Suspended',
    'Inactive'
);

CREATE TYPE subscription_status AS ENUM (
    'Trial',
    'Active',
    'Expired',
    'Suspended',
    'Cancelled'
);

CREATE TYPE payment_status AS ENUM (
    'Pending',
    'Successful',
    'Failed',
    'Refunded',
    'PartiallyRefunded'
);

CREATE TYPE payment_method AS ENUM (
    'CreditCard',
    'DebitCard',
    'BankTransfer',
    'UPI',
    'Wallet',
    'Cheque'
);

CREATE TYPE billing_cycle AS ENUM (
    'Monthly',
    'Quarterly',
    'HalfYearly',
    'Yearly'
);

CREATE TYPE facility_category AS ENUM (
    'Diagnostic',
    'Treatment',
    'Surgical',
    'Emergency',
    'ICU',
    'Pharmacy',
    'Laboratory',
    'Radiology',
    'Other'
);

-- ============================================================================
-- CORE TABLES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Hospitals Table (Tenant Root)
-- ----------------------------------------------------------------------------
CREATE TABLE hospitals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL UNIQUE, -- For multi-tenant isolation
    
    -- Registration Information
    registration_number VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    legal_name VARCHAR(200) NOT NULL,
    
    -- Contact Information
    email VARCHAR(100) NOT NULL UNIQUE,
    phone VARCHAR(20) NOT NULL,
    alternate_phone VARCHAR(20),
    website VARCHAR(200),
    
    -- Address
    address_line1 VARCHAR(200) NOT NULL,
    address_line2 VARCHAR(200),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    country VARCHAR(100) NOT NULL DEFAULT 'India',
    postal_code VARCHAR(20) NOT NULL,
    
    -- Business Information
    tax_id VARCHAR(50),
    license_number VARCHAR(100),
    accreditation_details JSONB, -- Store accreditation info as JSON
    
    -- Status & Workflow
    status hospital_status NOT NULL DEFAULT 'PendingApproval',
    approved_at TIMESTAMP WITH TIME ZONE,
    approved_by UUID, -- References system admin user
    rejected_at TIMESTAMP WITH TIME ZONE,
    rejected_by UUID,
    rejection_reason TEXT,
    suspended_at TIMESTAMP WITH TIME ZONE,
    suspended_by UUID,
    suspension_reason TEXT,
    
    -- Subscription Information
    subscription_status subscription_status NOT NULL DEFAULT 'Trial',
    subscription_plan_id UUID,
    subscription_start_date TIMESTAMP WITH TIME ZONE,
    subscription_end_date TIMESTAMP WITH TIME ZONE,
    trial_end_date TIMESTAMP WITH TIME ZONE,
    
    -- Capacity & Billing
    total_beds INTEGER DEFAULT 0,
    total_doctors INTEGER DEFAULT 0,
    total_staff INTEGER DEFAULT 0,
    monthly_subscription_cost DECIMAL(10, 2) DEFAULT 0,
    
    -- Metadata
    settings JSONB, -- Hospital-specific settings
    metadata JSONB, -- Additional custom fields
    
    -- Audit Fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID,
    deleted_at TIMESTAMP WITH TIME ZONE,
    deleted_by UUID,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version INTEGER NOT NULL DEFAULT 1,
    
    -- Constraints
    CONSTRAINT chk_hospital_email CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT chk_hospital_phone CHECK (phone ~* '^[0-9+\-\s()]+$'),
    CONSTRAINT chk_hospital_beds CHECK (total_beds >= 0),
    CONSTRAINT chk_hospital_cost CHECK (monthly_subscription_cost >= 0)
);

-- Indexes for hospitals
CREATE INDEX idx_hospitals_tenant_id ON hospitals(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_hospitals_status ON hospitals(status) WHERE is_deleted = FALSE;
CREATE INDEX idx_hospitals_subscription_status ON hospitals(subscription_status) WHERE is_deleted = FALSE;
CREATE INDEX idx_hospitals_registration_number ON hospitals(registration_number) WHERE is_deleted = FALSE;
CREATE INDEX idx_hospitals_email ON hospitals(email) WHERE is_deleted = FALSE;
CREATE INDEX idx_hospitals_subscription_end ON hospitals(subscription_end_date) WHERE is_deleted = FALSE AND subscription_status = 'Active';
CREATE INDEX idx_hospitals_trial_end ON hospitals(trial_end_date) WHERE is_deleted = FALSE AND subscription_status = 'Trial';
CREATE INDEX idx_hospitals_created_at ON hospitals(created_at DESC);

-- Full-text search index
CREATE INDEX idx_hospitals_name_trgm ON hospitals USING gin(name gin_trgm_ops);
CREATE INDEX idx_hospitals_legal_name_trgm ON hospitals USING gin(legal_name gin_trgm_ops);

-- ----------------------------------------------------------------------------
-- Branches Table
-- ----------------------------------------------------------------------------
CREATE TABLE branches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    hospital_id UUID NOT NULL,
    
    -- Branch Information
    branch_code VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    
    -- Contact Information
    email VARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    alternate_phone VARCHAR(20),
    
    -- Address
    address_line1 VARCHAR(200) NOT NULL,
    address_line2 VARCHAR(200),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    country VARCHAR(100) NOT NULL DEFAULT 'India',
    postal_code VARCHAR(20) NOT NULL,
    
    -- Operational Information
    is_main_branch BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    total_beds INTEGER DEFAULT 0,
    operating_hours JSONB, -- Store as {"monday": "9:00-17:00", ...}
    
    -- Manager Information
    branch_manager_id UUID, -- References user
    branch_manager_name VARCHAR(200),
    branch_manager_email VARCHAR(100),
    branch_manager_phone VARCHAR(20),
    
    -- Metadata
    metadata JSONB,
    
    -- Audit Fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID,
    deleted_at TIMESTAMP WITH TIME ZONE,
    deleted_by UUID,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version INTEGER NOT NULL DEFAULT 1,
    
    -- Foreign Keys
    CONSTRAINT fk_branches_hospital FOREIGN KEY (hospital_id) 
        REFERENCES hospitals(id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_branch_email CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT chk_branch_phone CHECK (phone ~* '^[0-9+\-\s()]+$'),
    CONSTRAINT chk_branch_beds CHECK (total_beds >= 0),
    CONSTRAINT uq_branch_code_per_hospital UNIQUE (hospital_id, branch_code)
);

-- Indexes for branches
CREATE INDEX idx_branches_hospital_id ON branches(hospital_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_branches_is_active ON branches(is_active) WHERE is_deleted = FALSE;
CREATE INDEX idx_branches_branch_code ON branches(branch_code) WHERE is_deleted = FALSE;
CREATE INDEX idx_branches_name_trgm ON branches USING gin(name gin_trgm_ops);

-- ----------------------------------------------------------------------------
-- Subscription Plans Table
-- ----------------------------------------------------------------------------
CREATE TABLE subscription_plans (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Plan Information
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    
    -- Pricing
    base_price DECIMAL(10, 2) NOT NULL,
    billing_cycle billing_cycle NOT NULL DEFAULT 'Monthly',
    currency VARCHAR(3) NOT NULL DEFAULT 'INR',
    
    -- Capacity Limits
    max_users INTEGER NOT NULL DEFAULT 10,
    max_branches INTEGER NOT NULL DEFAULT 1,
    max_beds INTEGER,
    
    -- Per-Unit Pricing (for exceeding limits)
    price_per_additional_user DECIMAL(10, 2) DEFAULT 0,
    price_per_additional_branch DECIMAL(10, 2) DEFAULT 0,
    price_per_additional_bed DECIMAL(10, 2) DEFAULT 0,
    
    -- Features (stored as JSON array of feature codes)
    included_features JSONB NOT NULL DEFAULT '[]',
    
    -- Plan Settings
    is_trial BOOLEAN NOT NULL DEFAULT FALSE,
    trial_duration_days INTEGER DEFAULT 30,
    is_public BOOLEAN NOT NULL DEFAULT TRUE, -- Show on website
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_recommended BOOLEAN NOT NULL DEFAULT FALSE,
    display_order INTEGER DEFAULT 0,
    
    -- Metadata
    metadata JSONB,
    
    -- Audit Fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID,
    deleted_at TIMESTAMP WITH TIME ZONE,
    deleted_by UUID,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version INTEGER NOT NULL DEFAULT 1,
    
    -- Constraints
    CONSTRAINT chk_plan_base_price CHECK (base_price >= 0),
    CONSTRAINT chk_plan_max_users CHECK (max_users > 0),
    CONSTRAINT chk_plan_max_branches CHECK (max_branches > 0),
    CONSTRAINT chk_plan_additional_prices CHECK (
        price_per_additional_user >= 0 AND 
        price_per_additional_branch >= 0 AND 
        price_per_additional_bed >= 0
    )
);

-- Indexes for subscription_plans
CREATE INDEX idx_plans_is_public ON subscription_plans(is_public, is_active) WHERE is_deleted = FALSE;
CREATE INDEX idx_plans_is_trial ON subscription_plans(is_trial) WHERE is_deleted = FALSE;
CREATE INDEX idx_plans_display_order ON subscription_plans(display_order, name) WHERE is_deleted = FALSE;
CREATE INDEX idx_plans_code ON subscription_plans(code) WHERE is_deleted = FALSE;

-- ----------------------------------------------------------------------------
-- Payments Table
-- ----------------------------------------------------------------------------
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    hospital_id UUID NOT NULL,
    
    -- Payment Information
    payment_reference VARCHAR(100) NOT NULL UNIQUE,
    invoice_number VARCHAR(50) NOT NULL UNIQUE,
    
    -- Amount Details
    amount DECIMAL(10, 2) NOT NULL,
    tax_amount DECIMAL(10, 2) DEFAULT 0,
    discount_amount DECIMAL(10, 2) DEFAULT 0,
    total_amount DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'INR',
    
    -- Payment Details
    payment_method payment_method NOT NULL,
    payment_status payment_status NOT NULL DEFAULT 'Pending',
    transaction_id VARCHAR(200), -- From payment gateway
    
    -- Dates
    payment_date TIMESTAMP WITH TIME ZONE,
    due_date TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Billing Period
    billing_period_start TIMESTAMP WITH TIME ZONE NOT NULL,
    billing_period_end TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Payment Gateway Details
    gateway_name VARCHAR(100),
    gateway_response JSONB, -- Store gateway response
    
    -- Refund Information
    refund_amount DECIMAL(10, 2) DEFAULT 0,
    refund_date TIMESTAMP WITH TIME ZONE,
    refund_reason TEXT,
    refund_reference VARCHAR(100),
    
    -- Retry Information
    retry_count INTEGER DEFAULT 0,
    last_retry_at TIMESTAMP WITH TIME ZONE,
    next_retry_at TIMESTAMP WITH TIME ZONE,
    
    -- Invoice
    invoice_url VARCHAR(500),
    invoice_generated_at TIMESTAMP WITH TIME ZONE,
    
    -- Notes
    notes TEXT,
    metadata JSONB,
    
    -- Audit Fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID,
    deleted_at TIMESTAMP WITH TIME ZONE,
    deleted_by UUID,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version INTEGER NOT NULL DEFAULT 1,
    
    -- Foreign Keys
    CONSTRAINT fk_payments_hospital FOREIGN KEY (hospital_id) 
        REFERENCES hospitals(id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_payment_amount CHECK (amount >= 0),
    CONSTRAINT chk_payment_tax CHECK (tax_amount >= 0),
    CONSTRAINT chk_payment_discount CHECK (discount_amount >= 0),
    CONSTRAINT chk_payment_total CHECK (total_amount >= 0),
    CONSTRAINT chk_payment_refund CHECK (refund_amount >= 0 AND refund_amount <= total_amount)
);

-- Indexes for payments
CREATE INDEX idx_payments_hospital_id ON payments(hospital_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_payments_status ON payments(payment_status) WHERE is_deleted = FALSE;
CREATE INDEX idx_payments_due_date ON payments(due_date) WHERE is_deleted = FALSE AND payment_status = 'Pending';
CREATE INDEX idx_payments_payment_date ON payments(payment_date DESC);
CREATE INDEX idx_payments_reference ON payments(payment_reference) WHERE is_deleted = FALSE;
CREATE INDEX idx_payments_invoice_number ON payments(invoice_number) WHERE is_deleted = FALSE;
CREATE INDEX idx_payments_billing_period ON payments(billing_period_start, billing_period_end);

-- ----------------------------------------------------------------------------
-- Global Facilities Table (Master Data)
-- ----------------------------------------------------------------------------
CREATE TABLE global_facilities (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Facility Information
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    category facility_category NOT NULL,
    
    -- Classification
    is_standard BOOLEAN NOT NULL DEFAULT TRUE,
    requires_certification BOOLEAN NOT NULL DEFAULT FALSE,
    certification_details TEXT,
    
    -- Display
    icon_name VARCHAR(100),
    display_order INTEGER DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- Metadata
    metadata JSONB,
    
    -- Audit Fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID,
    deleted_at TIMESTAMP WITH TIME ZONE,
    deleted_by UUID,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    row_version INTEGER NOT NULL DEFAULT 1,
    
    -- Constraints
    CONSTRAINT uq_facility_code UNIQUE (code)
);

-- Indexes for global_facilities
CREATE INDEX idx_facilities_category ON global_facilities(category, is_active) WHERE is_deleted = FALSE;
CREATE INDEX idx_facilities_is_active ON global_facilities(is_active) WHERE is_deleted = FALSE;
CREATE INDEX idx_facilities_display_order ON global_facilities(display_order, name);
CREATE INDEX idx_facilities_name_trgm ON global_facilities USING gin(name gin_trgm_ops);

-- ----------------------------------------------------------------------------
-- Hospital Facilities Table (Hospital-level facility mapping)
-- ----------------------------------------------------------------------------
CREATE TABLE hospital_facilities (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    hospital_id UUID NOT NULL,
    facility_id UUID NOT NULL,
    
    -- Facility Details at Hospital Level
    is_available BOOLEAN NOT NULL DEFAULT TRUE,
    capacity INTEGER,
    notes TEXT,
    
    -- Metadata
    metadata JSONB,
    
    -- Audit Fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID,
    deleted_at TIMESTAMP WITH TIME ZONE,
    deleted_by UUID,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Foreign Keys
    CONSTRAINT fk_hospital_facilities_hospital FOREIGN KEY (hospital_id) 
        REFERENCES hospitals(id) ON DELETE CASCADE,
    CONSTRAINT fk_hospital_facilities_facility FOREIGN KEY (facility_id) 
        REFERENCES global_facilities(id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT uq_hospital_facility UNIQUE (hospital_id, facility_id),
    CONSTRAINT chk_facility_capacity CHECK (capacity IS NULL OR capacity >= 0)
);

-- Indexes for hospital_facilities
CREATE INDEX idx_hospital_facilities_hospital_id ON hospital_facilities(hospital_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_hospital_facilities_facility_id ON hospital_facilities(facility_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_hospital_facilities_is_available ON hospital_facilities(is_available) WHERE is_deleted = FALSE;

-- ----------------------------------------------------------------------------
-- Branch Facilities Table (Branch-level facility mapping)
-- ----------------------------------------------------------------------------
CREATE TABLE branch_facilities (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    branch_id UUID NOT NULL,
    hospital_facility_id UUID NOT NULL, -- Reference to hospital_facilities
    
    -- Facility Details at Branch Level
    is_available BOOLEAN NOT NULL DEFAULT TRUE,
    capacity INTEGER,
    floor_number VARCHAR(20),
    location_details TEXT,
    operating_hours JSONB,
    
    -- Responsible Staff
    incharge_staff_id UUID,
    incharge_staff_name VARCHAR(200),
    
    -- Metadata
    metadata JSONB,
    
    -- Audit Fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID,
    deleted_at TIMESTAMP WITH TIME ZONE,
    deleted_by UUID,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Foreign Keys
    CONSTRAINT fk_branch_facilities_branch FOREIGN KEY (branch_id) 
        REFERENCES branches(id) ON DELETE CASCADE,
    CONSTRAINT fk_branch_facilities_hospital_facility FOREIGN KEY (hospital_facility_id) 
        REFERENCES hospital_facilities(id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT uq_branch_hospital_facility UNIQUE (branch_id, hospital_facility_id),
    CONSTRAINT chk_branch_facility_capacity CHECK (capacity IS NULL OR capacity >= 0)
);

-- Indexes for branch_facilities
CREATE INDEX idx_branch_facilities_branch_id ON branch_facilities(branch_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_branch_facilities_hospital_facility_id ON branch_facilities(hospital_facility_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_branch_facilities_is_available ON branch_facilities(is_available) WHERE is_deleted = FALSE;

-- ============================================================================
-- AUDIT TABLES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Audit Log Table (Track all changes)
-- ----------------------------------------------------------------------------
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Entity Information
    entity_type VARCHAR(100) NOT NULL, -- hospitals, branches, etc.
    entity_id UUID NOT NULL,
    tenant_id UUID, -- For multi-tenant filtering
    
    -- Action Details
    action VARCHAR(50) NOT NULL, -- INSERT, UPDATE, DELETE, APPROVE, REJECT, etc.
    performed_by UUID NOT NULL,
    performed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Change Details
    old_values JSONB,
    new_values JSONB,
    changed_fields TEXT[], -- Array of field names that changed
    
    -- Request Context
    ip_address INET,
    user_agent TEXT,
    request_id VARCHAR(100),
    
    -- Metadata
    metadata JSONB
);

-- Indexes for audit_logs
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_tenant_id ON audit_logs(tenant_id);
CREATE INDEX idx_audit_logs_performed_at ON audit_logs(performed_at DESC);
CREATE INDEX idx_audit_logs_performed_by ON audit_logs(performed_by);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);

-- ============================================================================
-- VIEWS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Active Hospitals View
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW vw_active_hospitals AS
SELECT 
    h.id,
    h.tenant_id,
    h.registration_number,
    h.name,
    h.legal_name,
    h.email,
    h.phone,
    h.city,
    h.state,
    h.status,
    h.subscription_status,
    h.subscription_end_date,
    h.trial_end_date,
    h.monthly_subscription_cost,
    sp.name AS subscription_plan_name,
    sp.code AS subscription_plan_code,
    COUNT(DISTINCT b.id) AS total_branches,
    COUNT(DISTINCT hf.id) AS total_facilities,
    h.created_at
FROM hospitals h
LEFT JOIN subscription_plans sp ON h.subscription_plan_id = sp.id
LEFT JOIN branches b ON h.id = b.hospital_id AND b.is_deleted = FALSE
LEFT JOIN hospital_facilities hf ON h.id = hf.hospital_id AND hf.is_deleted = FALSE
WHERE h.is_deleted = FALSE
    AND h.status IN ('Active', 'PendingApproval')
GROUP BY h.id, sp.id;

-- ----------------------------------------------------------------------------
-- Payment Summary View
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW vw_payment_summary AS
SELECT 
    h.id AS hospital_id,
    h.tenant_id,
    h.name AS hospital_name,
    COUNT(p.id) AS total_payments,
    SUM(CASE WHEN p.payment_status = 'Successful' THEN p.total_amount ELSE 0 END) AS total_paid,
    SUM(CASE WHEN p.payment_status = 'Pending' THEN p.total_amount ELSE 0 END) AS total_pending,
    SUM(CASE WHEN p.payment_status = 'Failed' THEN p.total_amount ELSE 0 END) AS total_failed,
    SUM(p.refund_amount) AS total_refunded,
    MAX(p.payment_date) AS last_payment_date,
    MIN(CASE WHEN p.payment_status = 'Pending' THEN p.due_date END) AS next_due_date
FROM hospitals h
LEFT JOIN payments p ON h.id = p.hospital_id AND p.is_deleted = FALSE
WHERE h.is_deleted = FALSE
GROUP BY h.id, h.tenant_id, h.name;

-- ============================================================================
-- FUNCTIONS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Function: Update updated_at timestamp
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    NEW.row_version = OLD.row_version + 1;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ----------------------------------------------------------------------------
-- Function: Audit trail trigger function
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION audit_trail_trigger()
RETURNS TRIGGER AS $$
DECLARE
    tenant_id_val UUID;
    changed_fields_array TEXT[];
    old_json JSONB;
    new_json JSONB;
BEGIN
    -- Extract tenant_id if exists
    IF TG_OP = 'DELETE' THEN
        tenant_id_val := OLD.tenant_id;
        old_json := row_to_json(OLD)::JSONB;
        new_json := NULL;
    ELSIF TG_OP = 'INSERT' THEN
        tenant_id_val := NEW.tenant_id;
        old_json := NULL;
        new_json := row_to_json(NEW)::JSONB;
    ELSE -- UPDATE
        tenant_id_val := NEW.tenant_id;
        old_json := row_to_json(OLD)::JSONB;
        new_json := row_to_json(NEW)::JSONB;
        
        -- Find changed fields
        SELECT ARRAY_AGG(key)
        INTO changed_fields_array
        FROM jsonb_each(old_json)
        WHERE old_json->key IS DISTINCT FROM new_json->key;
    END IF;
    
    -- Insert audit record
    INSERT INTO audit_logs (
        entity_type,
        entity_id,
        tenant_id,
        action,
        performed_by,
        old_values,
        new_values,
        changed_fields
    ) VALUES (
        TG_TABLE_NAME,
        COALESCE(NEW.id, OLD.id),
        tenant_id_val,
        TG_OP,
        COALESCE(NEW.updated_by, NEW.created_by, OLD.updated_by),
        old_json,
        new_json,
        changed_fields_array
    );
    
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- TRIGGERS
-- ============================================================================

-- Update timestamp triggers
CREATE TRIGGER trg_hospitals_updated_at
    BEFORE UPDATE ON hospitals
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trg_branches_updated_at
    BEFORE UPDATE ON branches
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trg_subscription_plans_updated_at
    BEFORE UPDATE ON subscription_plans
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trg_payments_updated_at
    BEFORE UPDATE ON payments
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trg_global_facilities_updated_at
    BEFORE UPDATE ON global_facilities
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Audit trail triggers
CREATE TRIGGER trg_hospitals_audit
    AFTER INSERT OR UPDATE OR DELETE ON hospitals
    FOR EACH ROW
    EXECUTE FUNCTION audit_trail_trigger();

CREATE TRIGGER trg_branches_audit
    AFTER INSERT OR UPDATE OR DELETE ON branches
    FOR EACH ROW
    EXECUTE FUNCTION audit_trail_trigger();

CREATE TRIGGER trg_payments_audit
    AFTER INSERT OR UPDATE OR DELETE ON payments
    FOR EACH ROW
    EXECUTE FUNCTION audit_trail_trigger();

-- ============================================================================
-- ROW LEVEL SECURITY (Multi-Tenancy)
-- ============================================================================

-- Enable RLS on hospitals table
ALTER TABLE hospitals ENABLE ROW LEVEL SECURITY;

-- Policy: Hospitals can only see their own data
CREATE POLICY tenant_isolation_policy ON hospitals
    FOR ALL
    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);

-- Policy: System admins can see all
CREATE POLICY admin_all_access_policy ON hospitals
    FOR ALL
    USING (current_setting('app.user_role', true) = 'SystemAdmin');

-- ============================================================================
-- INDEXES FOR PERFORMANCE
-- ============================================================================

-- Composite indexes for common queries
CREATE INDEX idx_hospitals_status_subscription ON hospitals(status, subscription_status) 
    WHERE is_deleted = FALSE;

CREATE INDEX idx_branches_hospital_active ON branches(hospital_id, is_active) 
    WHERE is_deleted = FALSE;

CREATE INDEX idx_payments_hospital_status_date ON payments(hospital_id, payment_status, due_date) 
    WHERE is_deleted = FALSE;

-- JSONB indexes for searching within JSON fields
CREATE INDEX idx_hospitals_settings ON hospitals USING gin(settings);
CREATE INDEX idx_hospitals_metadata ON hospitals USING gin(metadata);
CREATE INDEX idx_payments_gateway_response ON payments USING gin(gateway_response);

-- ============================================================================
-- COMMENTS
-- ============================================================================

COMMENT ON SCHEMA hospital_onboarding IS 'Hospital Onboarding module for MedWay multi-tenant SaaS platform';

COMMENT ON TABLE hospitals IS 'Main table storing hospital tenant information';
COMMENT ON TABLE branches IS 'Hospital branches - each hospital can have multiple branches';
COMMENT ON TABLE subscription_plans IS 'Subscription plans available for hospitals';
COMMENT ON TABLE payments IS 'Payment transactions and billing history';
COMMENT ON TABLE global_facilities IS 'Master list of available medical facilities';
COMMENT ON TABLE hospital_facilities IS 'Facilities available at hospital level';
COMMENT ON TABLE branch_facilities IS 'Facilities mapped to specific branches';
COMMENT ON TABLE audit_logs IS 'Audit trail for all data changes';

COMMENT ON COLUMN hospitals.tenant_id IS 'Unique tenant identifier for multi-tenant isolation';
COMMENT ON COLUMN hospitals.subscription_status IS 'Current subscription state (Trial, Active, etc.)';
COMMENT ON COLUMN branches.is_main_branch IS 'Indicates if this is the primary/headquarters branch';
COMMENT ON COLUMN subscription_plans.is_trial IS 'Whether this plan is a trial plan';
COMMENT ON COLUMN payments.retry_count IS 'Number of payment retry attempts';

-- ============================================================================
-- GRANTS (Example - adjust based on your roles)
-- ============================================================================

-- Grant usage on schema
-- GRANT USAGE ON SCHEMA hospital_onboarding TO hospital_app_role;

-- Grant permissions on tables
-- GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA hospital_onboarding TO hospital_app_role;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA hospital_onboarding TO hospital_app_role;

-- ============================================================================
-- END OF SCHEMA
-- ============================================================================
