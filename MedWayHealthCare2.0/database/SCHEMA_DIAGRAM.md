# Hospital Onboarding Database - Entity Relationship Diagram

## Visual Schema Overview

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                         HOSPITAL ONBOARDING SCHEMA                                   │
│                         Multi-Tenant SaaS Architecture                               │
└─────────────────────────────────────────────────────────────────────────────────────┘


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                              TENANT ROOT ENTITY                                       │
└──────────────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────────────┐
                              │     HOSPITALS           │
                              │  (Tenant Root Entity)   │
                              ├─────────────────────────┤
                              │ PK: id (UUID)           │
                              │ UK: tenant_id (UUID)    │★ Multi-Tenant Key
                              │ UK: registration_number │
                              │ UK: email               │
                              ├─────────────────────────┤
                              │ name                    │
                              │ legal_name              │
                              │ phone, address          │
                              │ status (ENUM)           │
                              │ subscription_status     │
                              │ FK: subscription_plan_id│──┐
                              │ total_beds              │  │
                              │ monthly_subscription_cost│  │
                              │ settings (JSON)         │  │
                              │ created_at, updated_at  │  │
                              │ is_deleted              │  │
                              └────────┬────────────────┘  │
                                       │                   │
                    ┌──────────────────┼───────────────┐   │
                    │                  │               │   │
                    ▼                  ▼               ▼   │
         ┌──────────────────┐ ┌────────────────┐ ┌────────▼─────────┐
         │    BRANCHES      │ │   PAYMENTS     │ │ HOSPITAL_        │
         │                  │ │                │ │ FACILITIES       │
         ├──────────────────┤ ├────────────────┤ ├──────────────────┤
         │ PK: id           │ │ PK: id         │ │ PK: id           │
         │ FK: hospital_id  │ │ FK: hospital_id│ │ FK: hospital_id  │
         │ UK: branch_code  │ │ UK: payment_ref│ │ FK: facility_id  │
         ├──────────────────┤ │ UK: invoice_no │ │ UK: (hosp,fac)   │
         │ name             │ ├────────────────┤ ├──────────────────┤
         │ email, phone     │ │ amount         │ │ is_available     │
         │ address          │ │ total_amount   │ │ capacity         │
         │ is_main_branch   │ │ payment_method │ │ notes            │
         │ is_active        │ │ payment_status │ │ metadata (JSON)  │
         │ total_beds       │ │ due_date       │ │ created_at       │
         │ branch_manager_id│ │ billing_period │ │ is_deleted       │
         │ operating_hours  │ │ refund_amount  │ └────────┬─────────┘
         │ created_at       │ │ retry_count    │          │
         │ is_deleted       │ │ invoice_url    │          │
         └────────┬─────────┘ │ gateway_response│         │
                  │           │ created_at      │          │
                  │           │ is_deleted      │          │
                  │           └─────────────────┘          │
                  │                                        │
                  │                                        │
                  ▼                                        ▼
         ┌──────────────────┐                   ┌──────────────────┐
         │ BRANCH_          │                   │ GLOBAL_          │
         │ FACILITIES       │                   │ FACILITIES       │
         │                  │                   │ (Master Data)    │
         ├──────────────────┤                   ├──────────────────┤
         │ PK: id           │                   │ PK: id           │
         │ FK: branch_id    │                   │ UK: code         │
         │ FK: hospital_fac │◄──────────────────┤                  │
         │ UK: (br,hosp_fac)│                   │ name             │
         ├──────────────────┤                   │ description      │
         │ is_available     │                   │ category (ENUM)  │
         │ capacity         │                   │ is_standard      │
         │ floor_number     │                   │ requires_cert    │
         │ location_details │                   │ icon_name        │
         │ operating_hours  │                   │ display_order    │
         │ incharge_staff_id│                   │ is_active        │
         │ created_at       │                   │ created_at       │
         │ is_deleted       │                   │ is_deleted       │
         └──────────────────┘                   └──────────────────┘
                                                         ▲
                                                         │
                                                         │
                                     Categories:         │
                                     • Diagnostic        │
                                     • Treatment         │
                                     • Surgical          │
                                     • Emergency         │
                                     • ICU              │
                                     • Pharmacy         │
                                     • Laboratory       │
                                     • Radiology        │


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                         SUBSCRIPTION & PRICING                                        │
└──────────────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────────────┐
                              │ SUBSCRIPTION_PLANS      │
                              │  (Master Data)          │
                              ├─────────────────────────┤
                              │ PK: id (UUID)           │
                              │ UK: code                │
                              ├─────────────────────────┤
                              │ name                    │
                              │ description             │
                              │ base_price              │
                              │ billing_cycle (ENUM)    │
                              │ max_users               │
                              │ max_branches            │
                              │ max_beds                │
                              │ price_per_additional_*  │
                              │ included_features (JSON)│
                              │ is_trial                │
                              │ is_public               │
                              │ is_recommended          │
                              │ display_order           │
                              │ created_at, updated_at  │
                              │ is_deleted              │
                              └─────────────────────────┘
                                         │
                                         │ Referenced by
                                         │ hospitals.subscription_plan_id
                                         │
                                         ▼
                              Plans:
                              • TRIAL (₹0, 30 days)
                              • BASIC (₹2,999/mo)
                              • PROFESSIONAL (₹9,999/mo) ★
                              • ENTERPRISE (₹29,999/mo)
                              • BASIC_YEARLY (₹30,589/yr)


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                           AUDIT & COMPLIANCE                                          │
└──────────────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────────────┐
                              │    AUDIT_LOGS           │
                              │  (Change Tracking)      │
                              ├─────────────────────────┤
                              │ PK: id (UUID)           │
                              ├─────────────────────────┤
                              │ entity_type             │
                              │ entity_id               │
                              │ tenant_id               │★ Multi-Tenant Isolation
                              │ action                  │
                              │ performed_by            │
                              │ performed_at            │
                              │ old_values (JSON)       │
                              │ new_values (JSON)       │
                              │ changed_fields (ARRAY)  │
                              │ ip_address              │
                              │ user_agent              │
                              │ request_id              │
                              │ metadata (JSON)         │
                              └─────────────────────────┘
                                         ▲
                                         │
                                   Auto-populated by
                                   database triggers on
                                   all DML operations


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                      RELATIONSHIP SUMMARY                                             │
└──────────────────────────────────────────────────────────────────────────────────────┘

HOSPITALS
  ├─── 1:N ──► BRANCHES
  │              └─── 1:N ──► BRANCH_FACILITIES
  │                             └─── N:1 ──► HOSPITAL_FACILITIES
  │
  ├─── 1:N ──► PAYMENTS
  │
  ├─── 1:N ──► HOSPITAL_FACILITIES
  │              ├─── N:1 ──► GLOBAL_FACILITIES
  │              └─── 1:N ──► BRANCH_FACILITIES
  │
  └─── N:1 ──► SUBSCRIPTION_PLANS


GLOBAL_FACILITIES (Master Data)
  └─── 1:N ──► HOSPITAL_FACILITIES
                 └─── 1:N ──► BRANCH_FACILITIES


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                      CASCADE DELETE BEHAVIOR                                          │
└──────────────────────────────────────────────────────────────────────────────────────┘

DELETE HOSPITAL
  ├─► CASCADE DELETE all BRANCHES
  │     └─► CASCADE DELETE all BRANCH_FACILITIES
  │
  ├─► CASCADE DELETE all PAYMENTS
  │
  └─► CASCADE DELETE all HOSPITAL_FACILITIES
        └─► RESTRICT (prevents deletion if BRANCH_FACILITIES exist)


DELETE SUBSCRIPTION_PLAN
  └─► SET NULL on HOSPITALS.subscription_plan_id


DELETE GLOBAL_FACILITY
  └─► CASCADE DELETE all HOSPITAL_FACILITIES
        └─► CASCADE DELETE all BRANCH_FACILITIES


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                    MULTI-TENANCY ISOLATION                                            │
└──────────────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│   Tenant A       │       │   Tenant B       │       │   Tenant C       │
│  (Hospital 1)    │       │  (Hospital 2)    │       │  (Hospital 3)    │
├──────────────────┤       ├──────────────────┤       ├──────────────────┤
│ tenant_id:       │       │ tenant_id:       │       │ tenant_id:       │
│ UUID-A           │       │ UUID-B           │       │ UUID-C           │
└────────┬─────────┘       └────────┬─────────┘       └────────┬─────────┘
         │                          │                          │
         │                          │                          │
         └──────────────────┬───────┴──────────────────────────┘
                            │
                            ▼
                ┌───────────────────────┐
                │  ROW-LEVEL SECURITY   │
                │    (RLS Policies)     │
                ├───────────────────────┤
                │ Filter:               │
                │ WHERE tenant_id =     │
                │   SESSION_CONTEXT()   │
                └───────────────────────┘
                            │
                            ▼
         Each tenant sees ONLY their own data
         System admins can override to see all


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                      DATA FLOW EXAMPLE                                                │
└──────────────────────────────────────────────────────────────────────────────────────┘

Hospital Onboarding Workflow:

1. REGISTER HOSPITAL
   ┌──────────────────┐
   │ INSERT INTO      │
   │ hospitals        │──► Status: PendingApproval
   │                  │──► SubscriptionStatus: Trial
   │ tenant_id: NEW   │──► subscription_plan_id: TRIAL plan
   │ status: Pending  │──► trial_end_date: +30 days
   └──────────────────┘

2. ADMIN APPROVAL
   ┌──────────────────┐
   │ UPDATE hospitals │
   │ SET status =     │──► Status: Active
   │   'Active'       │──► approved_at: NOW
   │ WHERE id = ?     │──► approved_by: admin_user_id
   └──────────────────┘

3. ADD BRANCHES
   ┌──────────────────┐
   │ INSERT INTO      │
   │ branches         │──► hospital_id: FK
   │                  │──► branch_code: UNIQUE per hospital
   │ is_main_branch   │──► One main branch
   └──────────────────┘

4. MAP FACILITIES
   ┌─────────────────────────────────────┐
   │ INSERT INTO hospital_facilities     │
   │ (hospital_id, facility_id)          │──► Hospital level
   └─────────────────────────────────────┘
              │
              ▼
   ┌─────────────────────────────────────┐
   │ INSERT INTO branch_facilities       │
   │ (branch_id, hospital_facility_id)   │──► Branch level
   └─────────────────────────────────────┘

5. ACTIVATE SUBSCRIPTION
   ┌──────────────────┐
   │ UPDATE hospitals │
   │ SET subscription │──► subscription_status: Active
   │   _plan_id = ?   │──► subscription_start_date: NOW
   │   _status = ?    │──► subscription_end_date: +1 month
   └──────────────────┘

6. PROCESS PAYMENT
   ┌──────────────────┐
   │ INSERT INTO      │
   │ payments         │──► hospital_id: FK
   │                  │──► amount: calculated cost
   │ payment_status   │──► Status: Successful/Failed
   │ invoice_number   │──► Auto-generated
   └──────────────────┘


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                      INDEX STRATEGY                                                   │
└──────────────────────────────────────────────────────────────────────────────────────┘

PRIMARY KEYS (Clustered)
  • All tables: id (UUID/GUID)
  • Fast lookups by primary key

UNIQUE INDEXES
  • hospitals: tenant_id, registration_number, email
  • branches: (hospital_id, branch_code)
  • subscription_plans: code
  • payments: payment_reference, invoice_number
  • global_facilities: code
  • hospital_facilities: (hospital_id, facility_id)
  • branch_facilities: (branch_id, hospital_facility_id)

FILTERED INDEXES (Performance)
  • WHERE is_deleted = FALSE (all tables)
  • WHERE status = 'Active' (hospitals)
  • WHERE payment_status = 'Pending' (payments)

COVERING INDEXES
  • Include frequently queried columns to avoid key lookups

FULL-TEXT INDEXES
  • hospitals: name, legal_name
  • branches: name
  • global_facilities: name, description

JSONB/JSON GIN INDEXES
  • hospitals: settings, metadata
  • payments: gateway_response


┌──────────────────────────────────────────────────────────────────────────────────────┐
│                      SECURITY LAYERS                                                  │
└──────────────────────────────────────────────────────────────────────────────────────┘

Layer 1: Database Authentication
  └─► Application user with limited permissions (SELECT, INSERT, UPDATE, DELETE)

Layer 2: Row-Level Security (RLS)
  └─► Session-based tenant filtering (tenant_id = SESSION_CONTEXT)

Layer 3: Soft Delete
  └─► Logical deletion (is_deleted flag) with filtered indexes

Layer 4: Audit Trail
  └─► All changes logged automatically via triggers

Layer 5: Application Layer
  └─► RBAC authorization (SuperAdmin, SystemAdmin, HospitalAdmin, BranchAdmin)


Legend:
──────
PK  = Primary Key
FK  = Foreign Key
UK  = Unique Key
★   = Critical for multi-tenancy
1:N = One-to-Many relationship
N:1 = Many-to-One relationship
◄── = Foreign key reference direction
