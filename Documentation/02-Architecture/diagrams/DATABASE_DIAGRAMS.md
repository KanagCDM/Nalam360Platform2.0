# Nalam360 Enterprise Platform - Database Schema Diagrams

This document contains Entity Relationship Diagrams (ERD) for the platform's core data models.

**Format:** Mermaid  
**Date:** November 18, 2025

---

## Table of Contents

1. [Core Domain Model](#1-core-domain-model)
2. [Multi-Tenancy Schema](#2-multi-tenancy-schema)
3. [RBAC Schema](#3-rbac-schema)
4. [Audit Log Schema](#4-audit-log-schema)
5. [Outbox Pattern Schema](#5-outbox-pattern-schema)
6. [Feature Flags Schema](#6-feature-flags-schema)
7. [Migration History Schema](#7-migration-history-schema)

---

## 1. Core Domain Model

**Description:** Generic domain entities showing common patterns.

```mermaid
erDiagram
    CUSTOMER ||--o{ ORDER : places
    CUSTOMER {
        uuid Id PK
        string Email UK
        string FirstName
        string LastName
        string PhoneNumber
        datetime CreatedAt
        datetime UpdatedAt
        uuid TenantId FK
        bool IsActive
        string ExternalId
    }
    
    ORDER ||--|{ ORDER_ITEM : contains
    ORDER {
        uuid Id PK
        uuid CustomerId FK
        string OrderNumber UK
        decimal TotalAmount
        string Status
        datetime OrderDate
        datetime ShippedDate
        datetime DeliveredDate
        string ShippingAddress
        uuid TenantId FK
        datetime CreatedAt
        datetime UpdatedAt
        string CreatedBy
        string UpdatedBy
    }
    
    ORDER_ITEM }o--|| PRODUCT : references
    ORDER_ITEM {
        uuid Id PK
        uuid OrderId FK
        uuid ProductId FK
        int Quantity
        decimal UnitPrice
        decimal LineTotal
        decimal DiscountAmount
        int LineNumber
    }
    
    PRODUCT {
        uuid Id PK
        string SKU UK
        string Name
        string Description
        decimal Price
        int StockQuantity
        bool IsActive
        uuid CategoryId FK
        uuid TenantId FK
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    PRODUCT }o--|| CATEGORY : belongs_to
    CATEGORY {
        uuid Id PK
        string Name
        string Code UK
        string Description
        uuid ParentCategoryId FK
        int DisplayOrder
        uuid TenantId FK
    }
```

---

## 2. Multi-Tenancy Schema

**Description:** Schema for multi-tenant isolation and configuration.

```mermaid
erDiagram
    TENANT ||--o{ TENANT_USER : has
    TENANT {
        uuid Id PK
        string Name
        string Identifier UK
        string ConnectionString
        string StorageContainer
        bool IsActive
        datetime CreatedAt
        datetime SubscriptionExpiresAt
        string SubscriptionTier
        jsonb Settings
        int MaxUsers
        int MaxStorageMB
    }
    
    TENANT_USER }o--|| USER : references
    TENANT_USER {
        uuid Id PK
        uuid TenantId FK
        uuid UserId FK
        string Role
        bool IsActive
        datetime JoinedAt
        datetime LastAccessAt
        jsonb Permissions
    }
    
    USER {
        uuid Id PK
        string Email UK
        string PasswordHash
        string FirstName
        string LastName
        bool EmailConfirmed
        datetime CreatedAt
        datetime LastLoginAt
        int FailedLoginAttempts
        datetime LockedUntil
    }
    
    TENANT ||--o{ TENANT_SETTING : configures
    TENANT_SETTING {
        uuid Id PK
        uuid TenantId FK
        string Category
        string Key UK
        string Value
        string DataType
        bool IsEncrypted
        datetime UpdatedAt
        string UpdatedBy
    }
    
    TENANT ||--o{ TENANT_FEATURE : enables
    TENANT_FEATURE {
        uuid Id PK
        uuid TenantId FK
        string FeatureId FK
        bool IsEnabled
        datetime EnabledAt
        datetime ExpiresAt
        jsonb Configuration
    }
```

---

## 3. RBAC Schema

**Description:** Role-Based Access Control with hierarchical roles and permissions.

```mermaid
erDiagram
    ROLE ||--o{ USER_ROLE : assigned_to
    ROLE {
        uuid Id PK
        string Name UK
        string Description
        uuid ParentRoleId FK
        bool IsSystemRole
        int Priority
        datetime CreatedAt
        uuid TenantId FK
    }
    
    USER_ROLE }o--|| USER : belongs_to
    USER_ROLE {
        uuid Id PK
        uuid UserId FK
        uuid RoleId FK
        datetime AssignedAt
        datetime ExpiresAt
        string AssignedBy
        uuid TenantId FK
    }
    
    ROLE ||--o{ ROLE_PERMISSION : grants
    ROLE_PERMISSION {
        uuid Id PK
        uuid RoleId FK
        uuid PermissionId FK
        bool IsDenied
        datetime GrantedAt
        string GrantedBy
    }
    
    PERMISSION {
        uuid Id PK
        string Resource UK
        string Action UK
        string Description
        string Category
        bool IsSystemPermission
        datetime CreatedAt
    }
    
    USER ||--o{ USER_PERMISSION : has_direct
    USER_PERMISSION {
        uuid Id PK
        uuid UserId FK
        uuid PermissionId FK
        bool IsDenied
        datetime GrantedAt
        datetime ExpiresAt
        string GrantedBy
        uuid TenantId FK
    }
    
    ROLE ||--o{ ROLE_HIERARCHY : inherits_from
    ROLE_HIERARCHY {
        uuid Id PK
        uuid ParentRoleId FK
        uuid ChildRoleId FK
        int InheritanceLevel
    }
```

---

## 4. Audit Log Schema

**Description:** Comprehensive audit trail for compliance and security.

```mermaid
erDiagram
    AUDIT_LOG {
        uuid Id PK
        uuid TenantId FK
        string UserId
        string UserEmail
        string Action
        string Resource
        string ResourceId
        string IpAddress
        string UserAgent
        datetime Timestamp
        string Status
        int DurationMs
        string CorrelationId
        jsonb OldValues
        jsonb NewValues
        jsonb Metadata
    }
    
    AUDIT_LOG ||--o{ AUDIT_LOG_DETAIL : contains
    AUDIT_LOG_DETAIL {
        uuid Id PK
        uuid AuditLogId FK
        string PropertyName
        string OldValue
        string NewValue
        string DataType
        bool IsSensitive
    }
    
    DATA_ACCESS_LOG {
        uuid Id PK
        uuid TenantId FK
        string UserId
        string TableName
        string Operation
        string RecordIds
        datetime AccessedAt
        string Purpose
        bool WasApproved
        string ApprovedBy
    }
    
    SECURITY_EVENT {
        uuid Id PK
        uuid TenantId FK
        string UserId
        string EventType
        string Severity
        string Description
        string IpAddress
        datetime OccurredAt
        bool IsResolved
        string ResolvedBy
        datetime ResolvedAt
        jsonb Details
    }
```

---

## 5. Outbox Pattern Schema

**Description:** Transactional outbox for reliable event publishing.

```mermaid
erDiagram
    OUTBOX_MESSAGE {
        uuid Id PK
        uuid TenantId FK
        string EventType
        jsonb Payload
        string Status
        datetime CreatedAt
        datetime ProcessedAt
        int RetryCount
        int MaxRetries
        string LastError
        datetime NextRetryAt
        string CorrelationId
        string CausationId
        string PartitionKey
    }
    
    OUTBOX_MESSAGE ||--o{ OUTBOX_DELIVERY_LOG : tracks
    OUTBOX_DELIVERY_LOG {
        uuid Id PK
        uuid OutboxMessageId FK
        string Destination
        datetime AttemptedAt
        bool Succeeded
        string ErrorMessage
        int StatusCode
        int DurationMs
    }
    
    INBOX_MESSAGE {
        uuid Id PK
        uuid MessageId UK
        uuid TenantId FK
        string EventType
        jsonb Payload
        string Status
        datetime ReceivedAt
        datetime ProcessedAt
        int RetryCount
        string LastError
        string CorrelationId
        string SourceSystem
    }
    
    IDEMPOTENCY_KEY {
        string Key PK
        uuid MessageId
        datetime ProcessedAt
        datetime ExpiresAt
        jsonb Result
        string Status
    }
```

---

## 6. Feature Flags Schema

**Description:** Feature flag configuration with targeting and rollout strategies.

```mermaid
erDiagram
    FEATURE_FLAG {
        uuid Id PK
        string Key UK
        string Name
        string Description
        bool DefaultValue
        string Strategy
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
        string CreatedBy
        jsonb Metadata
    }
    
    FEATURE_FLAG ||--o{ FEATURE_FLAG_VARIANT : has
    FEATURE_FLAG_VARIANT {
        uuid Id PK
        uuid FeatureFlagId FK
        string Name
        string Key UK
        jsonb Value
        int Weight
        bool IsControl
        string Description
    }
    
    FEATURE_FLAG ||--o{ FEATURE_FLAG_RULE : governed_by
    FEATURE_FLAG_RULE {
        uuid Id PK
        uuid FeatureFlagId FK
        string Attribute
        string Operator
        jsonb Value
        int Priority
        bool IsActive
        uuid VariantId FK
    }
    
    FEATURE_FLAG ||--o{ FEATURE_FLAG_OVERRIDE : overridden_by
    FEATURE_FLAG_OVERRIDE {
        uuid Id PK
        uuid FeatureFlagId FK
        uuid TenantId FK
        string UserId
        bool Value
        datetime ExpiresAt
        string Reason
        string CreatedBy
        datetime CreatedAt
    }
    
    FEATURE_FLAG_EVALUATION {
        uuid Id PK
        uuid FeatureFlagId FK
        string UserId
        uuid TenantId FK
        bool Result
        uuid VariantId FK
        datetime EvaluatedAt
        int EvaluationTimeMs
        jsonb Context
    }
```

---

## 7. Migration History Schema

**Description:** Database migration tracking and versioning.

```mermaid
erDiagram
    MIGRATION_HISTORY {
        uuid Id PK
        string MigrationId UK
        string Name
        string Description
        datetime AppliedAt
        string AppliedBy
        int ExecutionTimeMs
        string Checksum
        string Status
        string ErrorMessage
        int Version
    }
    
    MIGRATION_HISTORY ||--o{ MIGRATION_SCRIPT : contains
    MIGRATION_SCRIPT {
        uuid Id PK
        uuid MigrationHistoryId FK
        string ScriptType
        string ScriptContent
        int ExecutionOrder
        datetime ExecutedAt
        int DurationMs
        bool Succeeded
        string ErrorMessage
    }
    
    SCHEMA_VERSION {
        int Version PK
        string Description
        datetime AppliedAt
        string AppliedBy
        bool IsBaseline
        bool IsCurrent
        string DatabaseName
    }
    
    MIGRATION_LOCK {
        string LockId PK
        string HeldBy
        datetime AcquiredAt
        datetime ExpiresAt
        string Purpose
    }
```

---

## Schema Design Principles

### 1. Audit Trail
All entities include:
- `CreatedAt`, `UpdatedAt` timestamps
- `CreatedBy`, `UpdatedBy` user tracking
- Soft delete with `IsActive` flag

### 2. Multi-Tenancy
Every business entity includes:
- `TenantId` for data isolation
- Global query filters auto-applied
- Connection string per tenant (optional)

### 3. Concurrency
- `RowVersion` (timestamp) for optimistic locking
- Unique constraints on business keys
- Foreign keys with cascade rules

### 4. Performance
- Indexes on foreign keys
- Composite indexes for common queries
- Partitioning by `TenantId` or date
- Archival strategy for old data

### 5. Security
- Encrypted columns for PII
- Row-level security policies
- Audit all data access
- No cascading deletes for critical data

---

## Migration Strategy

### Schema Versioning
```
V1__Initial_Schema.sql
V2__Add_Tenancy.sql
V3__Add_Audit_Logs.sql
V4__Add_Feature_Flags.sql
```

### Naming Conventions
- Tables: `PascalCase`
- Columns: `PascalCase`
- Primary Keys: `Id`
- Foreign Keys: `{Table}Id`
- Indexes: `IX_{Table}_{Column}`
- Constraints: `CK_{Table}_{Column}`

---

**Document Version:** 1.0  
**Last Updated:** November 18, 2025  
**Maintained By:** Nalam360 Platform Team
