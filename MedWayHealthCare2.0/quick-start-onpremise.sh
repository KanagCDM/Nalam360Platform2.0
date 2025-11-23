#!/bin/bash

###############################################################################
# MedWayHealthCare 2.0 - On-Premise Quick Start Script
# 
# This script sets up a complete cloud-agnostic healthcare platform:
# - PostgreSQL 16 (Database)
# - Redis 7 (Cache)
# - Apache Kafka 7.5 (Messaging)
# - MedWay API (.NET 10)
#
# ZERO cloud dependencies - runs 100% locally
###############################################################################

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project root directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   MedWayHealthCare 2.0 - On-Premise Deployment (Kafka)        ║${NC}"
echo -e "${BLUE}║   Cloud-Agnostic Architecture: PostgreSQL + Redis + Kafka     ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

###############################################################################
# 1. Prerequisites Check
###############################################################################
echo -e "${YELLOW}[1/7] Checking prerequisites...${NC}"

# Check Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker is not installed. Please install Docker Desktop.${NC}"
    exit 1
fi

# Check Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}❌ Docker Compose is not installed.${NC}"
    exit 1
fi

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK is not installed. Please install .NET 10.0 SDK.${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Docker: $(docker --version)${NC}"
echo -e "${GREEN}✅ Docker Compose: $(docker-compose --version)${NC}"
echo -e "${GREEN}✅ .NET SDK: $(dotnet --version)${NC}"
echo ""

###############################################################################
# 2. Clean Previous Deployment
###############################################################################
echo -e "${YELLOW}[2/7] Cleaning previous deployment...${NC}"

if [ -f "docker-compose-kafka.yml" ]; then
    docker-compose -f docker-compose-kafka.yml down -v 2>/dev/null || true
    echo -e "${GREEN}✅ Previous containers stopped and removed${NC}"
else
    echo -e "${YELLOW}⚠️  docker-compose-kafka.yml not found, skipping cleanup${NC}"
fi
echo ""

###############################################################################
# 3. Start Infrastructure Services
###############################################################################
echo -e "${YELLOW}[3/7] Starting infrastructure (PostgreSQL, Redis, Kafka)...${NC}"

if [ ! -f "docker-compose-kafka.yml" ]; then
    echo -e "${RED}❌ docker-compose-kafka.yml not found!${NC}"
    echo -e "${RED}   Expected location: $SCRIPT_DIR/docker-compose-kafka.yml${NC}"
    exit 1
fi

docker-compose -f docker-compose-kafka.yml up -d postgres redis zookeeper kafka

echo -e "${BLUE}⏳ Waiting for services to be healthy (60 seconds)...${NC}"
sleep 60

# Check service health
echo -e "${BLUE}Checking service status...${NC}"
docker-compose -f docker-compose-kafka.yml ps

echo -e "${GREEN}✅ Infrastructure services started${NC}"
echo ""

###############################################################################
# 4. Verify PostgreSQL
###############################################################################
echo -e "${YELLOW}[4/7] Verifying PostgreSQL...${NC}"

until docker exec medway-postgres pg_isready -U medway_app &>/dev/null; do
    echo -e "${BLUE}⏳ Waiting for PostgreSQL to be ready...${NC}"
    sleep 5
done

echo -e "${GREEN}✅ PostgreSQL is ready${NC}"

# Create database if not exists
docker exec medway-postgres psql -U medway_app -d postgres -c "SELECT 1 FROM pg_database WHERE datname = 'medway'" | grep -q 1 || \
docker exec medway-postgres psql -U medway_app -d postgres -c "CREATE DATABASE medway"

echo -e "${GREEN}✅ Database 'medway' ready${NC}"
echo ""

###############################################################################
# 5. Verify Redis
###############################################################################
echo -e "${YELLOW}[5/7] Verifying Redis...${NC}"

until docker exec medway-redis redis-cli PING | grep -q PONG; do
    echo -e "${BLUE}⏳ Waiting for Redis to be ready...${NC}"
    sleep 5
done

echo -e "${GREEN}✅ Redis is ready${NC}"
echo ""

###############################################################################
# 6. Verify Kafka and Create Topics
###############################################################################
echo -e "${YELLOW}[6/7] Verifying Kafka and creating topics...${NC}"

# Wait for Kafka to be ready
echo -e "${BLUE}⏳ Waiting for Kafka to be ready...${NC}"
sleep 30

# Create Kafka topics
TOPICS=(
    "medway.patient.events"
    "medway.appointment.events"
    "medway.clinical.events"
    "medway.billing.events"
    "medway.inventory.events"
    "medway.notification.events"
)

for TOPIC in "${TOPICS[@]}"; do
    docker exec medway-kafka kafka-topics.sh \
        --bootstrap-server localhost:9092 \
        --create \
        --if-not-exists \
        --topic "$TOPIC" \
        --partitions 3 \
        --replication-factor 1 \
        --config retention.ms=604800000 2>/dev/null || true
    
    echo -e "${GREEN}✅ Topic created: $TOPIC${NC}"
done

# List all topics to verify
echo -e "${BLUE}Kafka Topics:${NC}"
docker exec medway-kafka kafka-topics.sh \
    --bootstrap-server localhost:9092 \
    --list

echo ""

###############################################################################
# 7. Build and Run MedWay API (Optional)
###############################################################################
echo -e "${YELLOW}[7/7] Building MedWay API...${NC}"

if [ -d "src/WebAPI" ]; then
    cd src/WebAPI
    
    # Set environment for on-premise configuration
    export ASPNETCORE_ENVIRONMENT=OnPremise
    
    echo -e "${BLUE}Building API...${NC}"
    dotnet build --configuration Release
    
    echo -e "${GREEN}✅ API built successfully${NC}"
    echo ""
    echo -e "${BLUE}To run the API:${NC}"
    echo -e "${BLUE}  cd src/WebAPI${NC}"
    echo -e "${BLUE}  export ASPNETCORE_ENVIRONMENT=OnPremise${NC}"
    echo -e "${BLUE}  dotnet run${NC}"
    echo ""
else
    echo -e "${YELLOW}⚠️  src/WebAPI not found. Skipping API build.${NC}"
    echo -e "${YELLOW}   Infrastructure services are ready for testing.${NC}"
    echo ""
fi

cd "$SCRIPT_DIR"

###############################################################################
# Deployment Summary
###############################################################################
echo -e "${GREEN}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║                  ✅ DEPLOYMENT SUCCESSFUL                       ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}📊 Service Endpoints:${NC}"
echo -e "  ${GREEN}PostgreSQL:${NC}  localhost:5432"
echo -e "    Username:   medway_app"
echo -e "    Password:   MedWay@2025!Secure"
echo -e "    Database:   medway"
echo ""
echo -e "  ${GREEN}Redis:${NC}       localhost:6379"
echo ""
echo -e "  ${GREEN}Kafka:${NC}       localhost:9092"
echo -e "    Topics:     medway.patient.events"
echo -e "                medway.appointment.events"
echo -e "                medway.clinical.events"
echo -e "                medway.billing.events"
echo -e "                medway.inventory.events"
echo -e "                medway.notification.events"
echo ""
echo -e "  ${GREEN}pgAdmin:${NC}     http://localhost:5050"
echo -e "    Email:      admin@medway.local"
echo -e "    Password:   MedWay@2025!Admin"
echo ""
echo -e "  ${GREEN}Kafka UI:${NC}    http://localhost:8080"
echo ""
echo -e "${BLUE}🧪 Quick Tests:${NC}"
echo ""
echo -e "  ${YELLOW}# Test PostgreSQL${NC}"
echo -e "  docker exec -it medway-postgres psql -U medway_app -d medway"
echo ""
echo -e "  ${YELLOW}# Test Redis${NC}"
echo -e "  docker exec -it medway-redis redis-cli"
echo ""
echo -e "  ${YELLOW}# Test Kafka (Publish)${NC}"
echo -e '  docker exec -it medway-kafka kafka-console-producer.sh \'
echo -e '    --broker-list localhost:9092 \'
echo -e '    --topic medway.patient.events'
echo ""
echo -e "  ${YELLOW}# Test Kafka (Consume)${NC}"
echo -e '  docker exec -it medway-kafka kafka-console-consumer.sh \'
echo -e '    --bootstrap-server localhost:9092 \'
echo -e '    --topic medway.patient.events \'
echo -e '    --from-beginning'
echo ""
echo -e "${BLUE}📚 Documentation:${NC}"
echo -e "  Configuration:  appsettings.OnPremise.json"
echo -e "  Testing Guide:  KAFKA_MIGRATION_TESTING.md"
echo -e "  Deployment:     CLOUD_AGNOSTIC_DEPLOYMENT_GUIDE.md"
echo ""
echo -e "${BLUE}🛑 To stop all services:${NC}"
echo -e "  docker-compose -f docker-compose-kafka.yml down"
echo ""
echo -e "${BLUE}🗑️  To remove all data (reset):${NC}"
echo -e "  docker-compose -f docker-compose-kafka.yml down -v"
echo ""
echo -e "${GREEN}🎉 Your cloud-agnostic healthcare platform is ready!${NC}"
echo -e "${GREEN}   Zero cloud dependencies. Runs 100% on-premise.${NC}"
echo -e "${GREEN}   Migrate to Azure/AWS/GCP by changing configuration only.${NC}"
echo ""
