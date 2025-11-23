# MedWayHealthCare 2.0 - Kafka Migration Testing Guide

## Overview
This guide validates the cloud-agnostic architecture with **zero code changes** across deployment scenarios.

**Key Principle:** Same application code, different configuration files.

## Prerequisites

### Software Requirements
```bash
# On-Premise Testing
- Docker Desktop 4.x+
- Docker Compose 2.x+
- .NET 10.0 SDK
- PostgreSQL Client (psql) - optional for testing
- Redis CLI (redis-cli) - optional for testing

# Azure Testing (additional)
- Azure CLI (az)
- Azure subscription with Owner/Contributor role
```

## Test Scenario 1: On-Premise Deployment

### Step 1: Start Infrastructure
```bash
# Navigate to project root
cd /Users/kanagasubramaniankrishnamurthi/Documents/Nalam360EnterprisePlatform2.0/MedWayHealthCare2.0

# Start all services (PostgreSQL, Redis, Kafka, MedWay API)
docker-compose -f docker-compose-kafka.yml up -d

# Verify services are running
docker-compose -f docker-compose-kafka.yml ps

# Expected output:
# NAME                  STATUS    PORTS
# medway-api            running   0.0.0.0:5001->8080/tcp
# medway-postgres       running   0.0.0.0:5432->5432/tcp
# medway-redis          running   0.0.0.0:6379->6379/tcp
# medway-kafka          running   0.0.0.0:9092->9092/tcp
# medway-zookeeper      running   2181/tcp
```

### Step 2: Verify Kafka Topics
```bash
# List Kafka topics
docker exec -it medway-kafka kafka-topics.sh \
  --bootstrap-server localhost:9092 \
  --list

# Expected topics:
# medway.patient.events
# medway.appointment.events
# medway.clinical.events
# medway.billing.events
# medway.inventory.events
# medway.notification.events
```

### Step 3: Test Message Publishing
```bash
# Publish test message to patient events topic
docker exec -it medway-kafka kafka-console-producer.sh \
  --broker-list localhost:9092 \
  --topic medway.patient.events

# Type this JSON message and press Enter:
{"eventType":"PatientRegistered","patientId":"P12345","timestamp":"2025-06-15T10:00:00Z"}

# Press Ctrl+C to exit
```

### Step 4: Test Message Consumption
```bash
# In a separate terminal, consume messages
docker exec -it medway-kafka kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic medway.patient.events \
  --from-beginning

# Expected output:
# {"eventType":"PatientRegistered","patientId":"P12345","timestamp":"2025-06-15T10:00:00Z"}

# Press Ctrl+C to exit
```

### Step 5: Verify PostgreSQL
```bash
# Connect to PostgreSQL
docker exec -it medway-postgres psql -U medway_app -d medway

# Test queries
medway=# \dt  -- List tables
medway=# SELECT version();  -- Check PostgreSQL version
medway=# \q  -- Exit
```

### Step 6: Verify Redis
```bash
# Connect to Redis
docker exec -it medway-redis redis-cli

# Test commands
127.0.0.1:6379> PING  -- Expected: PONG
127.0.0.1:6379> SET test "MedWay"
127.0.0.1:6379> GET test  -- Expected: "MedWay"
127.0.0.1:6379> EXIT
```

### Step 7: Run MedWay API (Local Development)
```bash
# Set environment to use on-premise configuration
export ASPNETCORE_ENVIRONMENT=OnPremise

# Build and run the API
cd src/WebAPI
dotnet build
dotnet run

# Expected output:
# info: MedWay.WebAPI[0]
#       Messaging Provider: Kafka
#       Kafka BootstrapServers: localhost:9092
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5001
```

### Step 8: Test API Endpoints
```bash
# Health check
curl http://localhost:5001/health

# Expected response:
# {"status":"Healthy","checks":[...]}

# Test Kafka integration via API
curl -X POST http://localhost:5001/api/patients \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-01",
    "email": "john.doe@example.com"
  }'

# Monitor Kafka topic for event
docker exec -it medway-kafka kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic medway.patient.events \
  --from-beginning
```

---

## Test Scenario 2: Azure Migration (Configuration-Only)

### Step 1: Create Azure Resources
```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Create resource group
az group create \
  --name medway-healthcare-rg \
  --location eastus

# Create PostgreSQL Flexible Server
az postgres flexible-server create \
  --name medway-db \
  --resource-group medway-healthcare-rg \
  --location eastus \
  --admin-user medway_app \
  --admin-password 'MedWay@2025!Secure' \
  --sku-name Standard_B2s \
  --tier Burstable \
  --storage-size 32 \
  --version 16

# Create database
az postgres flexible-server db create \
  --resource-group medway-healthcare-rg \
  --server-name medway-db \
  --database-name medway

# Create Azure Cache for Redis
az redis create \
  --name medway-cache \
  --resource-group medway-healthcare-rg \
  --location eastus \
  --sku Basic \
  --vm-size c0

# Get Redis access key
az redis list-keys \
  --name medway-cache \
  --resource-group medway-healthcare-rg

# Create Event Hubs Namespace (Kafka compatible)
az eventhubs namespace create \
  --name medway-eventhub \
  --resource-group medway-healthcare-rg \
  --location eastus \
  --sku Standard

# Create Event Hubs (Kafka topics)
az eventhubs eventhub create \
  --name medway.patient.events \
  --namespace-name medway-eventhub \
  --resource-group medway-healthcare-rg

az eventhubs eventhub create \
  --name medway.appointment.events \
  --namespace-name medway-eventhub \
  --resource-group medway-healthcare-rg

# Get Event Hubs connection string
az eventhubs namespace authorization-rule keys list \
  --resource-group medway-healthcare-rg \
  --namespace-name medway-eventhub \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

### Step 2: Update Configuration (NO CODE CHANGES)
```bash
# Stop on-premise containers
docker-compose -f docker-compose-kafka.yml down

# Set environment variables for Azure
export ASPNETCORE_ENVIRONMENT=Azure
export AZURE_POSTGRES_PASSWORD='MedWay@2025!Secure'
export AZURE_REDIS_KEY='<REDIS_ACCESS_KEY_FROM_STEP_1>'
export AZURE_EVENTHUB_CONNECTION_STRING='<EVENTHUB_CONNECTION_STRING_FROM_STEP_1>'
export AZURE_JWT_SECRET='MedWay_JWT_Secret_Key_CHANGE_IN_PRODUCTION_Minimum32Characters_2025'

# Run the SAME application code
cd src/WebAPI
dotnet run

# Expected output:
# info: MedWay.WebAPI[0]
#       Messaging Provider: Kafka
#       Kafka BootstrapServers: medway-eventhub.servicebus.windows.net:9093
#       SecurityProtocol: SaslSsl
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5001
```

### Step 3: Test Azure Event Hubs Kafka Protocol
```bash
# Test API with Azure backend
curl -X POST http://localhost:5001/api/patients \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith",
    "dateOfBirth": "1985-05-15",
    "email": "jane.smith@example.com"
  }'

# Verify message in Azure Event Hubs
az eventhubs eventhub show \
  --resource-group medway-healthcare-rg \
  --namespace-name medway-eventhub \
  --name medway.patient.events

# Check message count
az monitor metrics list \
  --resource /subscriptions/{subscription-id}/resourceGroups/medway-healthcare-rg/providers/Microsoft.EventHub/namespaces/medway-eventhub/eventhubs/medway.patient.events \
  --metric IncomingMessages
```

### Step 4: Compare Configurations
```bash
# On-Premise Kafka
grep -A 10 "Messaging" appsettings.OnPremise.json

# Azure Kafka (Event Hubs)
grep -A 15 "Messaging" appsettings.Azure.json

# KEY DIFFERENCES (only connection details):
# BootstrapServers: localhost:9092 → medway-eventhub.servicebus.windows.net:9093
# SecurityProtocol: (none) → SaslSsl
# SaslMechanism: (none) → Plain
# SaslPassword: (none) → Connection String
```

---

## Test Scenario 3: Hybrid Deployment

### Configuration
```json
{
  "//": "Database in Azure, Messaging on-premise for data sovereignty",
  "ConnectionStrings": {
    "DefaultConnection": "Host=medway-db.postgres.database.azure.com;..."
  },
  "Messaging": {
    "Provider": "Kafka",
    "Kafka": {
      "BootstrapServers": "localhost:9092"
    }
  }
}
```

### Use Case
- **Sensitive data** (patient records) in Azure with encryption/compliance
- **Event processing** on-premise for real-time control
- **Gradual migration** path without full cloud commitment

---

## Validation Checklist

### ✅ On-Premise Deployment
- [ ] Docker Compose starts all services (PostgreSQL, Redis, Kafka, API)
- [ ] Kafka topics created automatically
- [ ] Messages published and consumed successfully
- [ ] PostgreSQL accepts connections
- [ ] Redis caching works
- [ ] API health checks pass
- [ ] Patient registration event published to Kafka

### ✅ Azure Deployment
- [ ] Azure resources created (PostgreSQL, Redis, Event Hubs)
- [ ] Environment variables configured
- [ ] Same API code runs without changes
- [ ] Kafka protocol works with Azure Event Hubs (SASL/SSL)
- [ ] Messages published to Event Hubs
- [ ] Patient registration event published to Azure Event Hubs
- [ ] No code differences between on-premise and Azure

### ✅ Configuration Verification
- [ ] Only `appsettings.*.json` files differ
- [ ] No code changes in Domain/Application/Infrastructure
- [ ] Same NuGet packages used (Confluent.Kafka)
- [ ] Environment variable substitution works
- [ ] Connection strings switchable via environment

---

## Performance Comparison

### On-Premise (Docker Compose)
```bash
# Measure latency
time curl -X POST http://localhost:5001/api/patients -H "Content-Type: application/json" -d '{...}'

# Expected: ~50-100ms (local network)
```

### Azure (Managed Services)
```bash
# Measure latency (including Azure network)
time curl -X POST https://medway-api.azurewebsites.net/api/patients -H "Content-Type: application/json" -d '{...}'

# Expected: ~100-200ms (internet + Azure processing)
```

### Kafka Throughput Test
```bash
# On-Premise
kafka-producer-perf-test.sh \
  --topic medway.patient.events \
  --num-records 10000 \
  --record-size 1024 \
  --throughput -1 \
  --producer-props bootstrap.servers=localhost:9092

# Azure Event Hubs (same test, different server)
kafka-producer-perf-test.sh \
  --topic medway.patient.events \
  --num-records 10000 \
  --record-size 1024 \
  --throughput -1 \
  --producer-props \
    bootstrap.servers=medway-eventhub.servicebus.windows.net:9093 \
    security.protocol=SASL_SSL \
    sasl.mechanism=PLAIN
```

---

## Troubleshooting

### Issue: Kafka Connection Refused
```bash
# Check Kafka is running
docker-compose -f docker-compose-kafka.yml ps medway-kafka

# Check logs
docker-compose -f docker-compose-kafka.yml logs medway-kafka

# Restart Kafka
docker-compose -f docker-compose-kafka.yml restart medway-kafka
```

### Issue: Azure Event Hubs Authentication Failed
```bash
# Verify connection string format
echo $AZURE_EVENTHUB_CONNECTION_STRING

# Expected format:
# Endpoint=sb://medway-eventhub.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...

# Test connection
az eventhubs namespace show \
  --name medway-eventhub \
  --resource-group medway-healthcare-rg
```

### Issue: PostgreSQL Connection Timeout
```bash
# On-Premise: Check port mapping
docker port medway-postgres

# Azure: Check firewall rules
az postgres flexible-server firewall-rule create \
  --resource-group medway-healthcare-rg \
  --name medway-db \
  --rule-name AllowMyIP \
  --start-ip-address YOUR_PUBLIC_IP \
  --end-ip-address YOUR_PUBLIC_IP
```

---

## Migration Success Criteria

✅ **Zero Code Changes:** Same C# code runs on-premise and Azure  
✅ **Configuration-Only Switch:** Environment variables + config files  
✅ **Kafka Protocol Universal:** Works with Apache Kafka, Azure Event Hubs, AWS MSK  
✅ **Performance Parity:** Similar latency for comparable workloads  
✅ **Data Portability:** PostgreSQL backups restore across environments  
✅ **Developer Experience:** Same debugging, same tools, same workflows  

---

## Next Steps

1. **Load Testing:**
   ```bash
   # Install k6 (load testing tool)
   brew install k6  # macOS
   
   # Run load test
   k6 run --vus 100 --duration 30s load-test-kafka.js
   ```

2. **Monitoring Setup:**
   - On-Premise: Prometheus + Grafana dashboards
   - Azure: Application Insights + Azure Monitor

3. **Disaster Recovery:**
   - Test PostgreSQL backup/restore
   - Test Kafka topic replication
   - Document RTO/RPO requirements

4. **Security Hardening:**
   - Change default passwords
   - Enable TLS for Kafka
   - Configure Azure Key Vault for secrets
   - Enable network security groups

---

## Conclusion

This guide demonstrates **plug-and-play cloud migration** with:
- **Same application code** (0 changes)
- **Kafka protocol** (universal messaging)
- **Configuration-based switching** (environment variables)
- **Full stack portability** (PostgreSQL + Redis + Kafka)

**Result:** MedWayHealthCare 2.0 runs identically on-premise, Azure, AWS, or GCP.
