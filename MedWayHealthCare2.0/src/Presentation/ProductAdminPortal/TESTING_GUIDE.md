# ProductAdminPortal - Complete Testing Guide

## 🧪 Testing the ProductAdmin API

This guide provides step-by-step instructions for testing the ProductAdminPortal backend API with sample data.

---

## 🚀 Quick Start

### 1. Start the Application

```bash
cd /Users/kanagasubramaniankrishnamurthi/Documents/Nalam360EnterprisePlatform2.0/MedWayHealthCare2.0/src/Presentation/ProductAdminPortal

# Run the application
dotnet run
```

**Expected Output:**
```
✅ Legacy database initialized successfully
✅ ProductAdmin database initialized successfully
🚀 ProductAdminPortal is running!
📖 Blazor UI: https://localhost:5096
📖 API Docs: https://localhost:5096/api-docs
```

### 2. Access Swagger Documentation

Open your browser to: **https://localhost:5096/api-docs**

---

## 📝 Sample Test Data

### Test Tenant & User IDs
```
TENANT_ID="11111111-1111-1111-1111-111111111111"
USER_ID="22222222-2222-2222-2222-222222222222"
```

---

## 🧪 Test Scenarios

### Scenario 1: Create a Product

**Endpoint:** `POST /api/v1/products`

```bash
curl -X POST https://localhost:5096/api/v1/products \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -k \
  -d '{
    "name": "Enterprise CRM System",
    "code": "CRM-ENT",
    "description": "Complete customer relationship management solution",
    "industry": "SaaS",
    "version": "1.0.0"
  }'
```

**Expected Response:**
```json
{
  "id": "33333333-3333-3333-3333-333333333333",
  "name": "Enterprise CRM System",
  "code": "CRM-ENT",
  "description": "Complete customer relationship management solution",
  "industry": "SaaS",
  "version": "1.0.0",
  "isActive": true,
  "createdAt": "2025-11-30T10:30:00Z",
  "modules": []
}
```

Save the returned `id` as `PRODUCT_ID`.

---

### Scenario 2: Add Modules to Product

**Endpoint:** `POST /api/v1/products/{id}/modules`

```bash
# Module 1: Customer Management
curl -X POST https://localhost:5096/api/v1/products/$PRODUCT_ID/modules \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -k \
  -d '{
    "name": "Customer Management",
    "code": "CUST",
    "description": "Manage customer records and interactions",
    "displayOrder": 1,
    "isRequired": true
  }'

# Module 2: Sales Pipeline
curl -X POST https://localhost:5096/api/v1/products/$PRODUCT_ID/modules \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -k \
  -d '{
    "name": "Sales Pipeline",
    "code": "SALES",
    "description": "Track sales opportunities and deals",
    "displayOrder": 2,
    "isRequired": false
  }'
```

---

### Scenario 3: Get All Products

**Endpoint:** `GET /api/v1/products`

```bash
curl -X GET https://localhost:5096/api/v1/products \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -k
```

---

### Scenario 4: Simulate Pricing

**Endpoint:** `POST /api/v1/pricing/simulate`

```bash
curl -X POST https://localhost:5096/api/v1/pricing/simulate \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "subscriptionPlanId": "44444444-4444-4444-4444-444444444444",
    "usageRecords": [
      {
        "entityId": "55555555-5555-5555-5555-555555555555",
        "units": 1000,
        "complexity": "medium"
      },
      {
        "entityId": "66666666-6666-6666-6666-666666666666",
        "units": 50,
        "complexity": "high"
      }
    ]
  }'
```

---

### Scenario 5: Record Usage

**Endpoint:** `POST /api/v1/usage/record`

```bash
curl -X POST https://localhost:5096/api/v1/usage/record \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -k \
  -d '{
    "customerSubscriptionId": "77777777-7777-7777-7777-777777777777",
    "entityId": "55555555-5555-5555-5555-555555555555",
    "units": 100,
    "complexity": "low",
    "metadata": {
      "transaction_type": "api_call",
      "endpoint": "/api/customers"
    }
  }'
```

---

### Scenario 6: Get Audit Logs

**Endpoint:** `GET /api/v1/audit/logs`

```bash
curl "https://localhost:5096/api/v1/audit/logs?entityType=Product&pageSize=10" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -k
```

---

## 🔧 Using Swagger UI

1. Navigate to https://localhost:5096/api-docs
2. Click "Authorize" (if authentication is enabled)
3. Expand any endpoint category (Products, Pricing, Billing, etc.)
4. Click "Try it out"
5. Fill in the required parameters
6. Add headers:
   - `X-Tenant-Id: 11111111-1111-1111-1111-111111111111`
   - `X-User-Id: 22222222-2222-2222-2222-222222222222`
7. Click "Execute"
8. View the response

---

## 📊 Complete Test Workflow

### Step 1: Create Product Structure
```bash
# 1. Create Product
PRODUCT_ID=$(curl -X POST https://localhost:5096/api/v1/products \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -k -s \
  -d '{"name":"Test CRM","code":"TCRM","industry":"SaaS","version":"1.0"}' \
  | jq -r '.id')

echo "Created Product: $PRODUCT_ID"

# 2. Add Module
MODULE_ID=$(curl -X POST https://localhost:5096/api/v1/products/$PRODUCT_ID/modules \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -k -s \
  -d '{"name":"Customers","code":"CUST","isRequired":true}' \
  | jq -r '.id')

echo "Created Module: $MODULE_ID"

# 3. Get Product Details
curl -X GET https://localhost:5096/api/v1/products/$PRODUCT_ID \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -k -s | jq

# 4. View Audit Logs
curl "https://localhost:5096/api/v1/audit/logs?entityType=Product&entityId=$PRODUCT_ID" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -k -s | jq
```

---

## ✅ Validation Checklist

### API Endpoints
- [ ] GET /api/v1/products - Returns 200 with product list
- [ ] POST /api/v1/products - Returns 201 with created product
- [ ] GET /api/v1/products/{id} - Returns 200 with product details
- [ ] PUT /api/v1/products/{id} - Returns 200 with updated product
- [ ] DELETE /api/v1/products/{id} - Returns 204
- [ ] POST /api/v1/products/{id}/modules - Returns 201 with module
- [ ] GET /api/v1/products/{id}/modules - Returns 200 with modules

### Pricing Engine
- [ ] POST /api/v1/pricing/calculate - Returns pricing breakdown
- [ ] POST /api/v1/pricing/simulate - Returns simulated pricing

### Billing
- [ ] POST /api/v1/billing/invoices - Generates invoice
- [ ] GET /api/v1/billing/invoices/{id} - Returns invoice details
- [ ] POST /api/v1/billing/invoices/{id}/mark-paid - Marks as paid

### Usage Tracking
- [ ] POST /api/v1/usage/record - Records usage successfully
- [ ] GET /api/v1/usage/subscriptions/{id}/records - Returns usage list
- [ ] GET /api/v1/usage/subscriptions/{id}/alerts - Returns alerts

### Audit Trail
- [ ] GET /api/v1/audit/logs - Returns filtered logs
- [ ] POST /api/v1/audit/versions - Creates version snapshot
- [ ] POST /api/v1/audit/versions/{id}/rollback - Rolls back config

---

## 🐛 Troubleshooting

### Issue: Connection Refused

**Symptom:** `curl: (7) Failed to connect to localhost port 5096`

**Solution:**
```bash
# Check if application is running
ps aux | grep ProductAdminPortal

# Restart application
dotnet run
```

---

### Issue: Database Not Found

**Symptom:** `Npgsql.PostgresException: database "productadmin" does not exist`

**Solution:**
```bash
# Create database manually
psql -h localhost -U postgres -c "CREATE DATABASE productadmin;"

# Or use Docker
docker run -d \
  --name postgres-productadmin \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=productadmin \
  -p 5432:5432 \
  postgres:15
```

---

### Issue: SSL Certificate Error

**Symptom:** `curl: (60) SSL certificate problem`

**Solution:**
Use the `-k` flag to skip certificate verification (development only):
```bash
curl -k https://localhost:5096/api/v1/products
```

---

## 📈 Performance Testing

### Load Test with Apache Bench

```bash
# Install Apache Bench
brew install httpd  # macOS
# sudo apt-get install apache2-utils  # Linux

# Test product listing
ab -n 1000 -c 10 \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -k \
  https://localhost:5096/api/v1/products
```

---

## 📝 Sample Response Examples

### Product Response
```json
{
  "id": "abc123...",
  "name": "Enterprise CRM",
  "code": "CRM-ENT",
  "description": "Complete CRM solution",
  "industry": "SaaS",
  "version": "1.0.0",
  "isActive": true,
  "createdAt": "2025-11-30T10:00:00Z",
  "modules": [
    {
      "id": "mod123...",
      "name": "Customer Management",
      "code": "CUST",
      "displayOrder": 1,
      "isRequired": true,
      "entities": []
    }
  ]
}
```

### Pricing Calculation Response
```json
{
  "customerSubscriptionId": "sub123...",
  "billingPeriodStart": "2025-11-01T00:00:00Z",
  "billingPeriodEnd": "2025-11-30T23:59:59Z",
  "subtotal": 2659.49,
  "taxAmount": 478.71,
  "discountAmount": 0.00,
  "totalAmount": 3138.20,
  "lineItems": [
    {
      "description": "Enterprise Plan - Base Fee",
      "quantity": 1,
      "unitPrice": 999.00,
      "amount": 999.00
    },
    {
      "description": "Transaction (medium complexity)",
      "quantity": 1250,
      "unitPrice": 1.33,
      "amount": 1660.49
    }
  ]
}
```

---

## 🎯 Success Criteria

Your API is working correctly if:

1. ✅ All product CRUD operations return appropriate status codes
2. ✅ Modules can be added and retrieved
3. ✅ Pricing calculations return numeric results
4. ✅ Usage records are created and queryable
5. ✅ Audit logs capture all mutations
6. ✅ Multi-tenant filtering works (products isolated by tenant)
7. ✅ Swagger UI displays all 23 endpoints
8. ✅ Database is auto-created on first run

---

## 📞 Support

For issues:
1. Check application logs in terminal
2. Verify PostgreSQL is running: `psql -h localhost -U postgres -l`
3. Enable detailed logging in appsettings.json:
   ```json
   "Microsoft.EntityFrameworkCore.Database.Command": "Information"
   ```
4. Review BACKEND_COMPLETE.md for API reference

---

## 🎉 Conclusion

Once all test scenarios pass, your ProductAdminPortal backend is fully functional and ready for:
- Frontend integration
- Authentication layer
- Production deployment
- Load testing
- Security hardening
