# ProductAdminPortal - JSON Schemas & Data Models

## Complete JSON Schema Definitions

### 1. Product Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid",
      "description": "Unique product identifier"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid",
      "description": "Tenant/organization identifier"
    },
    "name": {
      "type": "string",
      "minLength": 1,
      "maxLength": 255,
      "description": "Product name"
    },
    "key": {
      "type": "string",
      "pattern": "^[A-Z0-9_]+$",
      "minLength": 1,
      "maxLength": 100,
      "description": "Unique product key (uppercase, alphanumeric, underscores)"
    },
    "description": {
      "type": "string",
      "description": "Product description"
    },
    "metadata": {
      "type": "object",
      "description": "Additional custom metadata"
    },
    "modules": {
      "type": "array",
      "items": {
        "type": "string",
        "format": "uuid"
      },
      "description": "Array of module IDs belonging to this product"
    },
    "version": {
      "type": "integer",
      "minimum": 1,
      "description": "Product version number"
    },
    "is_active": {
      "type": "boolean",
      "description": "Whether product is active"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    },
    "updated_at": {
      "type": "string",
      "format": "date-time"
    },
    "created_by": {
      "type": "string",
      "format": "uuid"
    },
    "updated_by": {
      "type": "string",
      "format": "uuid"
    }
  },
  "required": ["tenant_id", "name", "key"],
  "additionalProperties": false
}
```

**Example:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Hospital Management System",
  "key": "HMS_CORE",
  "description": "Complete hospital management platform",
  "metadata": {
    "industry": "healthcare",
    "target_market": "enterprise"
  },
  "modules": [
    "abc123-module-1",
    "def456-module-2"
  ],
  "version": 1,
  "is_active": true,
  "created_at": "2025-11-29T00:00:00Z",
  "updated_at": "2025-11-29T00:00:00Z"
}
```

---

### 2. Module Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid"
    },
    "product_id": {
      "type": "string",
      "format": "uuid",
      "description": "Parent product ID"
    },
    "name": {
      "type": "string",
      "minLength": 1,
      "maxLength": 255
    },
    "description": {
      "type": "string"
    },
    "display_order": {
      "type": "integer",
      "minimum": 0,
      "description": "Display order in UI"
    },
    "entities": {
      "type": "array",
      "items": {
        "type": "string",
        "format": "uuid"
      },
      "description": "Array of entity IDs in this module"
    },
    "metadata": {
      "type": "object"
    },
    "is_active": {
      "type": "boolean"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    },
    "updated_at": {
      "type": "string",
      "format": "date-time"
    }
  },
  "required": ["tenant_id", "product_id", "name"],
  "additionalProperties": false
}
```

**Example:**
```json
{
  "id": "abc123-module-1",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "product_id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Patient Management",
  "description": "Complete patient registration and records management",
  "display_order": 1,
  "entities": [
    "entity-001",
    "entity-002",
    "entity-003"
  ],
  "metadata": {
    "icon": "users",
    "color": "#3498db"
  },
  "is_active": true,
  "created_at": "2025-11-29T00:00:00Z",
  "updated_at": "2025-11-29T00:00:00Z"
}
```

---

### 3. Entity Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid"
    },
    "name": {
      "type": "string",
      "minLength": 1,
      "maxLength": 255
    },
    "entity_type": {
      "type": "string",
      "enum": ["resource", "action", "report", "feature", "integration"],
      "description": "Type of entity"
    },
    "description": {
      "type": "string"
    },
    "pricing_enabled": {
      "type": "boolean",
      "default": true
    },
    "default_price": {
      "type": "number",
      "minimum": 0,
      "multipleOf": 0.0001
    },
    "pricing_unit": {
      "type": "string",
      "enum": ["per_transaction", "per_user", "per_month", "per_year", "per_gb", "per_api_call", "flat"]
    },
    "complexity_levels": {
      "type": "array",
      "items": {
        "type": "string",
        "enum": ["low", "medium", "high", "critical"]
      },
      "default": ["low", "medium", "high"]
    },
    "metadata": {
      "type": "object",
      "properties": {
        "complexity": {
          "type": "array",
          "items": {
            "type": "string",
            "enum": ["low", "medium", "high", "critical"]
          }
        },
        "notes": {
          "type": "string"
        },
        "tags": {
          "type": "array",
          "items": {
            "type": "string"
          }
        }
      }
    },
    "is_active": {
      "type": "boolean"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    },
    "updated_at": {
      "type": "string",
      "format": "date-time"
    }
  },
  "required": ["tenant_id", "name", "entity_type"],
  "additionalProperties": false
}
```

**Example:**
```json
{
  "id": "entity-001",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Patient Registration",
  "entity_type": "action",
  "description": "Register new patient in the system",
  "pricing_enabled": true,
  "default_price": 0.50,
  "pricing_unit": "per_transaction",
  "complexity_levels": ["low", "medium", "high"],
  "metadata": {
    "complexity": ["low", "medium", "high"],
    "notes": "Complexity depends on data validation requirements",
    "tags": ["patient", "registration", "core"]
  },
  "is_active": true,
  "created_at": "2025-11-29T00:00:00Z",
  "updated_at": "2025-11-29T00:00:00Z"
}
```

---

### 4. SubscriptionPlan Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid"
    },
    "name": {
      "type": "string",
      "minLength": 1,
      "maxLength": 255
    },
    "key": {
      "type": "string",
      "pattern": "^[A-Z0-9_]+$"
    },
    "description": {
      "type": "string"
    },
    "billing_cycle": {
      "type": "string",
      "enum": ["monthly", "yearly", "quarterly", "one_time"]
    },
    "base_fee": {
      "type": "number",
      "minimum": 0,
      "multipleOf": 0.0001
    },
    "currency_code": {
      "type": "string",
      "pattern": "^[A-Z]{3}$",
      "default": "INR"
    },
    "trial_period_days": {
      "type": "integer",
      "minimum": 0
    },
    "included_entities": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "entity_id": {
            "type": "string",
            "format": "uuid"
          },
          "limit": {
            "type": "integer",
            "minimum": 0,
            "description": "Usage limit, null means unlimited"
          },
          "soft_limit_percentage": {
            "type": "number",
            "minimum": 0,
            "maximum": 100
          }
        },
        "required": ["entity_id"]
      }
    },
    "module_access": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "module_id": {
            "type": "string",
            "format": "uuid"
          },
          "enabled": {
            "type": "boolean"
          }
        },
        "required": ["module_id", "enabled"]
      }
    },
    "pricing_rules": {
      "type": "array",
      "items": {
        "type": "string",
        "format": "uuid"
      },
      "description": "Array of pricing rule IDs"
    },
    "version": {
      "type": "integer",
      "minimum": 1
    },
    "effective_from": {
      "type": "string",
      "format": "date"
    },
    "effective_to": {
      "type": "string",
      "format": "date"
    },
    "is_public": {
      "type": "boolean"
    },
    "is_active": {
      "type": "boolean"
    },
    "metadata": {
      "type": "object"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    },
    "updated_at": {
      "type": "string",
      "format": "date-time"
    }
  },
  "required": ["tenant_id", "name", "key", "billing_cycle", "base_fee"],
  "additionalProperties": false
}
```

**Example:**
```json
{
  "id": "plan-gold-001",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Gold Plan",
  "key": "GOLD",
  "description": "Full-featured plan for medium to large organizations",
  "billing_cycle": "monthly",
  "base_fee": 1000.00,
  "currency_code": "INR",
  "trial_period_days": 30,
  "included_entities": [
    {
      "entity_id": "entity-001",
      "limit": 1000,
      "soft_limit_percentage": 80
    },
    {
      "entity_id": "entity-002",
      "limit": null
    }
  ],
  "module_access": [
    {
      "module_id": "abc123-module-1",
      "enabled": true
    },
    {
      "module_id": "def456-module-2",
      "enabled": true
    }
  ],
  "pricing_rules": [
    "rule-001",
    "rule-002"
  ],
  "version": 1,
  "effective_from": "2025-11-01",
  "effective_to": null,
  "is_public": true,
  "is_active": true,
  "metadata": {
    "featured": true,
    "popular": true
  },
  "created_at": "2025-11-29T00:00:00Z",
  "updated_at": "2025-11-29T00:00:00Z"
}
```

---

### 5. PricingRule Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid"
    },
    "name": {
      "type": "string",
      "minLength": 1,
      "maxLength": 255
    },
    "description": {
      "type": "string"
    },
    "scope": {
      "type": "string",
      "enum": ["entity", "module", "subscription", "global"]
    },
    "target_id": {
      "type": "string",
      "format": "uuid",
      "description": "References entity_id, module_id, or subscription_plan_id based on scope"
    },
    "pricing_type": {
      "type": "string",
      "enum": ["flat", "per_unit", "tiered", "multiplier", "percentage", "bundle"]
    },
    "params": {
      "type": "object",
      "description": "Type-specific parameters"
    },
    "effective_from": {
      "type": "string",
      "format": "date"
    },
    "effective_to": {
      "type": "string",
      "format": "date"
    },
    "priority": {
      "type": "integer",
      "minimum": 1,
      "maximum": 100,
      "default": 10
    },
    "is_active": {
      "type": "boolean"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    },
    "updated_at": {
      "type": "string",
      "format": "date-time"
    }
  },
  "required": ["tenant_id", "name", "scope", "pricing_type", "params"],
  "additionalProperties": false
}
```

**Pricing Type Examples:**

#### Flat Pricing
```json
{
  "id": "rule-flat-001",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Flat Setup Fee",
  "scope": "subscription",
  "target_id": "plan-gold-001",
  "pricing_type": "flat",
  "params": {
    "amount": 500.00,
    "one_time": true
  },
  "effective_from": "2025-11-01",
  "effective_to": null,
  "priority": 10,
  "is_active": true
}
```

#### Per-Unit Pricing
```json
{
  "id": "rule-perunit-001",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Transaction Fee",
  "scope": "entity",
  "target_id": "entity-001",
  "pricing_type": "per_unit",
  "params": {
    "unit_price": 0.10,
    "included_units": 1000,
    "overage_price": 0.08
  },
  "effective_from": "2025-11-01",
  "effective_to": null,
  "priority": 10,
  "is_active": true
}
```

#### Tiered Pricing
```json
{
  "id": "rule-tiered-001",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Tiered Transaction Pricing",
  "scope": "entity",
  "target_id": "entity-001",
  "pricing_type": "tiered",
  "params": {
    "tiers": [
      {
        "min_units": 0,
        "max_units": 1000,
        "unit_price": 0.00,
        "flat_fee": 0.00
      },
      {
        "min_units": 1001,
        "max_units": 10000,
        "unit_price": 0.10,
        "flat_fee": 0.00
      },
      {
        "min_units": 10001,
        "max_units": null,
        "unit_price": 0.07,
        "flat_fee": 0.00
      }
    ]
  },
  "effective_from": "2025-11-01",
  "effective_to": null,
  "priority": 10,
  "is_active": true
}
```

#### Complexity Multiplier
```json
{
  "id": "rule-multiplier-001",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Complexity-Based Pricing",
  "scope": "entity",
  "target_id": "entity-001",
  "pricing_type": "multiplier",
  "params": {
    "base_price": 0.10,
    "complexity_multipliers": {
      "low": 1.0,
      "medium": 2.0,
      "high": 4.0,
      "critical": 6.0
    }
  },
  "effective_from": "2025-11-01",
  "effective_to": null,
  "priority": 10,
  "is_active": true
}
```

---

### 6. UsageRecord Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid"
    },
    "customer_subscription_id": {
      "type": "string",
      "format": "uuid"
    },
    "entity_id": {
      "type": "string",
      "format": "uuid"
    },
    "user_id": {
      "type": "string",
      "format": "uuid"
    },
    "timestamp": {
      "type": "string",
      "format": "date-time"
    },
    "units": {
      "type": "integer",
      "minimum": 1,
      "default": 1
    },
    "complexity": {
      "type": "string",
      "enum": ["low", "medium", "high", "critical"]
    },
    "metadata": {
      "type": "object"
    },
    "billed": {
      "type": "boolean",
      "default": false
    },
    "invoice_id": {
      "type": "string",
      "format": "uuid"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    }
  },
  "required": ["tenant_id", "customer_subscription_id", "entity_id", "timestamp"],
  "additionalProperties": false
}
```

**Example:**
```json
{
  "id": "usage-001",
  "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "customer_subscription_id": "cust-sub-001",
  "entity_id": "entity-001",
  "user_id": "user-123",
  "timestamp": "2025-11-29T10:30:00Z",
  "units": 1,
  "complexity": "medium",
  "metadata": {
    "patient_id": "PAT-12345",
    "registration_type": "new",
    "department": "emergency"
  },
  "billed": false,
  "invoice_id": null,
  "created_at": "2025-11-29T10:30:00Z"
}
```

---

### 7. Invoice Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid"
    },
    "customer_subscription_id": {
      "type": "string",
      "format": "uuid"
    },
    "invoice_number": {
      "type": "string",
      "pattern": "^INV-[0-9]{8}$"
    },
    "billing_period_start": {
      "type": "string",
      "format": "date"
    },
    "billing_period_end": {
      "type": "string",
      "format": "date"
    },
    "line_items": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "description": {
            "type": "string"
          },
          "item_type": {
            "type": "string",
            "enum": ["base_fee", "usage", "one_time", "discount", "tax", "adjustment"]
          },
          "entity_id": {
            "type": "string",
            "format": "uuid"
          },
          "quantity": {
            "type": "integer",
            "minimum": 0
          },
          "unit_price": {
            "type": "number"
          },
          "total_price": {
            "type": "number"
          },
          "metadata": {
            "type": "object"
          }
        }
      }
    },
    "subtotal": {
      "type": "number",
      "minimum": 0
    },
    "tax_amount": {
      "type": "number",
      "minimum": 0
    },
    "discount_amount": {
      "type": "number",
      "minimum": 0
    },
    "total_amount": {
      "type": "number",
      "minimum": 0
    },
    "currency_code": {
      "type": "string",
      "pattern": "^[A-Z]{3}$"
    },
    "status": {
      "type": "string",
      "enum": ["draft", "pending", "paid", "overdue", "cancelled"]
    },
    "due_date": {
      "type": "string",
      "format": "date"
    },
    "paid_at": {
      "type": "string",
      "format": "date-time"
    },
    "payment_method": {
      "type": "string"
    },
    "notes": {
      "type": "string"
    },
    "metadata": {
      "type": "object"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    },
    "updated_at": {
      "type": "string",
      "format": "date-time"
    }
  },
  "required": ["tenant_id", "customer_subscription_id", "invoice_number", "billing_period_start", "billing_period_end", "total_amount", "currency_code", "status"],
  "additionalProperties": false
}
```

---

### 8. Customer Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "tenant_id": {
      "type": "string",
      "format": "uuid"
    },
    "name": {
      "type": "string",
      "minLength": 1,
      "maxLength": 255
    },
    "email": {
      "type": "string",
      "format": "email"
    },
    "company_name": {
      "type": "string"
    },
    "billing_address": {
      "type": "object",
      "properties": {
        "street": {"type": "string"},
        "city": {"type": "string"},
        "state": {"type": "string"},
        "postal_code": {"type": "string"},
        "country": {"type": "string"}
      }
    },
    "contact_info": {
      "type": "object",
      "properties": {
        "phone": {"type": "string"},
        "mobile": {"type": "string"},
        "fax": {"type": "string"}
      }
    },
    "metadata": {
      "type": "object"
    },
    "is_active": {
      "type": "boolean"
    },
    "created_at": {
      "type": "string",
      "format": "date-time"
    },
    "updated_at": {
      "type": "string",
      "format": "date-time"
    }
  },
  "required": ["tenant_id", "name", "email"],
  "additionalProperties": false
}
```

---

## NoSQL Considerations

For high-volume usage records and audit logs, consider NoSQL (MongoDB/DynamoDB):

### Usage Records (NoSQL)
```json
{
  "_id": "ObjectId",
  "tenant_id": "uuid",
  "customer_subscription_id": "uuid",
  "entity_id": "uuid",
  "timestamp": "ISODate",
  "units": 1,
  "complexity": "medium",
  "metadata": {},
  "billed": false,
  "ttl_expire_at": "ISODate" // Auto-expire after retention period
}
```

**Indexes:**
- Compound: `{tenant_id: 1, customer_subscription_id: 1, timestamp: -1}`
- Compound: `{entity_id: 1, timestamp: -1}`
- Single: `{billed: 1}` (partial index where billed = false)
- TTL: `{ttl_expire_at: 1}`

### Audit Logs (NoSQL)
```json
{
  "_id": "ObjectId",
  "tenant_id": "uuid",
  "entity_type": "product",
  "entity_id": "uuid",
  "action": "update",
  "old_values": {},
  "new_values": {},
  "changed_fields": [],
  "user_id": "uuid",
  "ip_address": "127.0.0.1",
  "user_agent": "string",
  "timestamp": "ISODate",
  "ttl_expire_at": "ISODate"
}
```

**Indexes:**
- Compound: `{tenant_id: 1, timestamp: -1}`
- Compound: `{entity_type: 1, entity_id: 1, timestamp: -1}`
- Single: `{user_id: 1}`
- TTL: `{ttl_expire_at: 1}`
