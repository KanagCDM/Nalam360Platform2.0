-- ============================================================================
-- MedWay Hospital Onboarding - PostgreSQL Seed Data
-- Multi-Tenant SaaS Architecture
-- Version: 1.0
-- Created: November 22, 2025
-- ============================================================================

SET search_path TO hospital_onboarding, public;

-- ============================================================================
-- SUBSCRIPTION PLANS
-- ============================================================================

-- Trial Plan
INSERT INTO subscription_plans (
    id, code, name, description, base_price, billing_cycle,
    max_users, max_branches, max_beds,
    price_per_additional_user, price_per_additional_branch, price_per_additional_bed,
    included_features, is_trial, trial_duration_days, is_public, is_active, is_recommended, display_order
) VALUES (
    '00000000-0000-0000-0000-000000000001',
    'TRIAL',
    'Trial Plan',
    '30-day free trial with basic features to explore the platform',
    0.00,
    'Monthly',
    5,
    1,
    50,
    0.00,
    0.00,
    0.00,
    '["BASIC_RECORDS", "APPOINTMENT_SCHEDULING", "BASIC_REPORTING"]'::jsonb,
    true,
    30,
    false,
    true,
    false,
    0
) ON CONFLICT (id) DO NOTHING;

-- Basic Plan
INSERT INTO subscription_plans (
    id, code, name, description, base_price, billing_cycle,
    max_users, max_branches, max_beds,
    price_per_additional_user, price_per_additional_branch, price_per_additional_bed,
    included_features, is_trial, is_public, is_active, is_recommended, display_order
) VALUES (
    '00000000-0000-0000-0000-000000000002',
    'BASIC',
    'Basic Plan',
    'Perfect for small clinics and single-branch practices',
    2999.00,
    'Monthly',
    10,
    1,
    100,
    200.00,
    1500.00,
    50.00,
    '["BASIC_RECORDS", "APPOINTMENT_SCHEDULING", "BASIC_REPORTING", "PATIENT_PORTAL", "EMAIL_SUPPORT"]'::jsonb,
    false,
    true,
    true,
    false,
    1
) ON CONFLICT (id) DO NOTHING;

-- Professional Plan (Recommended)
INSERT INTO subscription_plans (
    id, code, name, description, base_price, billing_cycle,
    max_users, max_branches, max_beds,
    price_per_additional_user, price_per_additional_branch, price_per_additional_bed,
    included_features, is_trial, is_public, is_active, is_recommended, display_order
) VALUES (
    '00000000-0000-0000-0000-000000000003',
    'PROFESSIONAL',
    'Professional Plan',
    'Ideal for medium-sized hospitals with multiple departments',
    9999.00,
    'Monthly',
    50,
    5,
    500,
    150.00,
    1200.00,
    30.00,
    '["BASIC_RECORDS", "APPOINTMENT_SCHEDULING", "ADVANCED_REPORTING", "PATIENT_PORTAL", "INVENTORY_MANAGEMENT", "PHARMACY_MODULE", "LABORATORY_MODULE", "PRIORITY_SUPPORT", "MOBILE_APP", "API_ACCESS"]'::jsonb,
    false,
    true,
    true,
    true,
    2
) ON CONFLICT (id) DO NOTHING;

-- Enterprise Plan
INSERT INTO subscription_plans (
    id, code, name, description, base_price, billing_cycle,
    max_users, max_branches, max_beds,
    price_per_additional_user, price_per_additional_branch, price_per_additional_bed,
    included_features, is_trial, is_public, is_active, is_recommended, display_order
) VALUES (
    '00000000-0000-0000-0000-000000000004',
    'ENTERPRISE',
    'Enterprise Plan',
    'For large hospital networks and healthcare organizations',
    29999.00,
    'Monthly',
    200,
    20,
    2000,
    100.00,
    800.00,
    20.00,
    '["BASIC_RECORDS", "APPOINTMENT_SCHEDULING", "ADVANCED_REPORTING", "PATIENT_PORTAL", "INVENTORY_MANAGEMENT", "PHARMACY_MODULE", "LABORATORY_MODULE", "RADIOLOGY_MODULE", "ICU_MANAGEMENT", "ANALYTICS_DASHBOARD", "CUSTOM_INTEGRATIONS", "DEDICATED_SUPPORT", "MOBILE_APP", "API_ACCESS", "WHITE_LABELING", "SLA_GUARANTEED"]'::jsonb,
    false,
    true,
    true,
    false,
    3
) ON CONFLICT (id) DO NOTHING;

-- Yearly Basic Plan (15% discount)
INSERT INTO subscription_plans (
    id, code, name, description, base_price, billing_cycle,
    max_users, max_branches, max_beds,
    price_per_additional_user, price_per_additional_branch, price_per_additional_bed,
    included_features, is_trial, is_public, is_active, is_recommended, display_order
) VALUES (
    '00000000-0000-0000-0000-000000000005',
    'BASIC_YEARLY',
    'Basic Plan (Annual)',
    'Basic plan with annual billing - Save 15%',
    30589.00, -- 2999 * 12 * 0.85
    'Yearly',
    10,
    1,
    100,
    200.00,
    1500.00,
    50.00,
    '["BASIC_RECORDS", "APPOINTMENT_SCHEDULING", "BASIC_REPORTING", "PATIENT_PORTAL", "EMAIL_SUPPORT"]'::jsonb,
    false,
    true,
    true,
    false,
    4
) ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- GLOBAL FACILITIES
-- ============================================================================

-- Emergency & Critical Care
INSERT INTO global_facilities (id, code, name, description, category, is_standard, requires_certification, display_order, is_active)
VALUES
    (uuid_generate_v4(), 'ICU', 'Intensive Care Unit', 'Advanced critical care monitoring and treatment', 'ICU', true, true, 1, true),
    (uuid_generate_v4(), 'NICU', 'Neonatal ICU', 'Specialized care for newborns', 'ICU', false, true, 2, true),
    (uuid_generate_v4(), 'CCU', 'Cardiac Care Unit', 'Specialized cardiac monitoring and care', 'ICU', false, true, 3, true),
    (uuid_generate_v4(), 'ER', 'Emergency Room', '24/7 emergency medical services', 'Emergency', true, true, 4, true),
    (uuid_generate_v4(), 'TRAUMA', 'Trauma Center', 'Advanced trauma care facility', 'Emergency', false, true, 5, true)
ON CONFLICT (code) DO NOTHING;

-- Surgical Facilities
INSERT INTO global_facilities (id, code, name, description, category, is_standard, requires_certification, display_order, is_active)
VALUES
    (uuid_generate_v4(), 'OT_GENERAL', 'General Operation Theater', 'General surgical procedures', 'Surgical', true, true, 10, true),
    (uuid_generate_v4(), 'OT_CARDIAC', 'Cardiac Surgery OT', 'Specialized cardiac surgical suite', 'Surgical', false, true, 11, true),
    (uuid_generate_v4(), 'OT_NEURO', 'Neurosurgery OT', 'Specialized neurosurgical suite', 'Surgical', false, true, 12, true),
    (uuid_generate_v4(), 'OT_ORTHO', 'Orthopedic Surgery OT', 'Orthopedic surgical procedures', 'Surgical', false, true, 13, true),
    (uuid_generate_v4(), 'MINOR_OT', 'Minor Operation Theater', 'Minor surgical procedures', 'Surgical', true, false, 14, true)
ON CONFLICT (code) DO NOTHING;

-- Diagnostic & Radiology
INSERT INTO global_facilities (id, code, name, description, category, is_standard, requires_certification, display_order, is_active)
VALUES
    (uuid_generate_v4(), 'XRAY', 'X-Ray Imaging', 'Digital X-ray imaging facility', 'Radiology', true, true, 20, true),
    (uuid_generate_v4(), 'CT_SCAN', 'CT Scan', 'Computed Tomography scanning', 'Radiology', false, true, 21, true),
    (uuid_generate_v4(), 'MRI', 'MRI Scan', 'Magnetic Resonance Imaging', 'Radiology', false, true, 22, true),
    (uuid_generate_v4(), 'ULTRASOUND', 'Ultrasound', 'Ultrasound imaging services', 'Diagnostic', true, true, 23, true),
    (uuid_generate_v4(), 'MAMMOGRAPHY', 'Mammography', 'Breast cancer screening', 'Radiology', false, true, 24, true),
    (uuid_generate_v4(), 'ECG', 'ECG/EKG', 'Electrocardiogram testing', 'Diagnostic', true, false, 25, true),
    (uuid_generate_v4(), 'ECHO', 'Echocardiography', 'Cardiac ultrasound', 'Diagnostic', false, true, 26, true)
ON CONFLICT (code) DO NOTHING;

-- Laboratory Services
INSERT INTO global_facilities (id, code, name, description, category, is_standard, requires_certification, display_order, is_active)
VALUES
    (uuid_generate_v4(), 'LAB_PATH', 'Pathology Laboratory', 'Clinical pathology testing', 'Laboratory', true, true, 30, true),
    (uuid_generate_v4(), 'LAB_MICRO', 'Microbiology Lab', 'Microbiological testing and culture', 'Laboratory', false, true, 31, true),
    (uuid_generate_v4(), 'LAB_BIO', 'Biochemistry Lab', 'Clinical biochemistry analysis', 'Laboratory', true, true, 32, true),
    (uuid_generate_v4(), 'LAB_HEMA', 'Hematology Lab', 'Blood testing and analysis', 'Laboratory', true, true, 33, true),
    (uuid_generate_v4(), 'LAB_IMMUNO', 'Immunology Lab', 'Immunological testing', 'Laboratory', false, true, 34, true),
    (uuid_generate_v4(), 'BLOOD_BANK', 'Blood Bank', 'Blood storage and transfusion services', 'Laboratory', false, true, 35, true)
ON CONFLICT (code) DO NOTHING;

-- Treatment & Therapy
INSERT INTO global_facilities (id, code, name, description, category, is_standard, requires_certification, display_order, is_active)
VALUES
    (uuid_generate_v4(), 'DIALYSIS', 'Dialysis Unit', 'Renal dialysis services', 'Treatment', false, true, 40, true),
    (uuid_generate_v4(), 'PHYSIO', 'Physiotherapy', 'Physical therapy and rehabilitation', 'Treatment', true, false, 41, true),
    (uuid_generate_v4(), 'CHEMO', 'Chemotherapy Unit', 'Cancer treatment facility', 'Treatment', false, true, 42, true),
    (uuid_generate_v4(), 'RADIO_THERAPY', 'Radiation Therapy', 'Radiotherapy for cancer treatment', 'Treatment', false, true, 43, true),
    (uuid_generate_v4(), 'DENTAL', 'Dental Clinic', 'Dental care services', 'Treatment', true, false, 44, true),
    (uuid_generate_v4(), 'OPTHAL', 'Ophthalmology', 'Eye care and surgery', 'Treatment', false, false, 45, true)
ON CONFLICT (code) DO NOTHING;

-- Pharmacy & Support
INSERT INTO global_facilities (id, code, name, description, category, is_standard, requires_certification, display_order, is_active)
VALUES
    (uuid_generate_v4(), 'PHARMACY', 'In-house Pharmacy', 'Hospital pharmacy services', 'Pharmacy', true, true, 50, true),
    (uuid_generate_v4(), 'PHARMACY_24', '24/7 Pharmacy', 'Round-the-clock pharmacy services', 'Pharmacy', false, true, 51, true),
    (uuid_generate_v4(), 'AMBULANCE', 'Ambulance Service', 'Emergency ambulance facility', 'Emergency', true, false, 52, true),
    (uuid_generate_v4(), 'CAFETERIA', 'Cafeteria', 'Food and beverage services', 'Other', true, false, 53, true),
    (uuid_generate_v4(), 'PARKING', 'Parking Facility', 'Vehicle parking area', 'Other', true, false, 54, true)
ON CONFLICT (code) DO NOTHING;

-- ============================================================================
-- SAMPLE HOSPITAL DATA (For Testing)
-- ============================================================================

-- Sample Hospital 1: City General Hospital
INSERT INTO hospitals (
    id, tenant_id, registration_number, name, legal_name,
    email, phone, website,
    address_line1, address_line2, city, state, country, postal_code,
    tax_id, license_number,
    status, subscription_status,
    subscription_plan_id, subscription_start_date, subscription_end_date,
    total_beds, total_doctors, total_staff,
    created_at
) VALUES (
    '10000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000001',
    'CGH2025001',
    'City General Hospital',
    'City General Hospital Private Limited',
    'admin@citygeneralhospital.com',
    '+91-44-12345678',
    'https://citygeneralhospital.com',
    '123 MG Road',
    'T Nagar',
    'Chennai',
    'Tamil Nadu',
    'India',
    '600017',
    'GSTIN29AABCC1234F1Z5',
    'LIC/TN/2025/001',
    'Active',
    'Active',
    '00000000-0000-0000-0000-000000000003', -- Professional Plan
    CURRENT_TIMESTAMP - INTERVAL '60 days',
    CURRENT_TIMESTAMP + INTERVAL '305 days',
    250,
    45,
    180,
    CURRENT_TIMESTAMP - INTERVAL '90 days'
) ON CONFLICT (id) DO NOTHING;

-- Sample Hospital 2: Metro Health Clinic (Trial)
INSERT INTO hospitals (
    id, tenant_id, registration_number, name, legal_name,
    email, phone,
    address_line1, city, state, country, postal_code,
    status, subscription_status,
    subscription_plan_id, trial_end_date,
    total_beds, total_doctors, total_staff,
    created_at
) VALUES (
    '10000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000002',
    'MHC2025002',
    'Metro Health Clinic',
    'Metro Health Clinic LLP',
    'info@metrohealthclinic.com',
    '+91-80-98765432',
    '456 Brigade Road',
    'Bangalore',
    'Karnataka',
    'India',
    '560001',
    'PendingApproval',
    'Trial',
    '00000000-0000-0000-0000-000000000001', -- Trial Plan
    CURRENT_TIMESTAMP + INTERVAL '15 days',
    50,
    8,
    25,
    CURRENT_TIMESTAMP - INTERVAL '15 days'
) ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- COMMENTS
-- ============================================================================

COMMENT ON TABLE subscription_plans IS 'Contains 5 subscription plans: Trial, Basic, Professional, Enterprise, and Basic Annual';
COMMENT ON TABLE global_facilities IS 'Contains 35+ medical facilities across 8 categories';

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Verify subscription plans
DO $$
BEGIN
    RAISE NOTICE 'Subscription Plans Count: %', (SELECT COUNT(*) FROM subscription_plans WHERE is_deleted = FALSE);
    RAISE NOTICE 'Global Facilities Count: %', (SELECT COUNT(*) FROM global_facilities WHERE is_deleted = FALSE);
    RAISE NOTICE 'Sample Hospitals Count: %', (SELECT COUNT(*) FROM hospitals WHERE is_deleted = FALSE);
END $$;

-- ============================================================================
-- END OF SEED DATA
-- ============================================================================
