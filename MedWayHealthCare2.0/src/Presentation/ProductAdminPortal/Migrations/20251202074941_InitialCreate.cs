using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductAdminPortal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    EntityCode = table.Column<string>(type: "TEXT", nullable: false),
                    ModuleName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsCore = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tier = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    YearlyPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxPatients = table.Column<int>(type: "INTEGER", nullable: false),
                    HasAdvancedReports = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasAPIAccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    Has24x7Support = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasCustomBranding = table.Column<bool>(type: "INTEGER", nullable: false),
                    StorageGB = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    subdomain = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModuleEntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanEntity_ModuleEntity_ModuleEntityId",
                        column: x => x.ModuleEntityId,
                        principalTable: "ModuleEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanEntity_SubscriptionPlan_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    company_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    billing_address = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                    table.ForeignKey(
                        name: "FK_customers_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discount_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    discount_type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    valid_from = table.Column<DateTime>(type: "TEXT", nullable: false),
                    valid_until = table.Column<DateTime>(type: "TEXT", nullable: true),
                    usage_limit = table.Column<int>(type: "INTEGER", nullable: true),
                    usage_count = table.Column<int>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount_codes", x => x.id);
                    table.ForeignKey(
                        name: "FK_discount_codes_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    base_pricing = table.Column<string>(type: "jsonb", nullable: true),
                    complexity_levels = table.Column<string>(type: "jsonb", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entities", x => x.id);
                    table.ForeignKey(
                        name: "FK_entities_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    industry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    version = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    permissions = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_roles_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    billing_cycle = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    trial_days = table.Column<int>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscription_plans_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductModule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCore = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModule_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionProduct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionProduct_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionProduct_SubscriptionPlan_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false),
                    is_required = table.Column<bool>(type: "INTEGER", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_modules_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_modules_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    customer_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    subscription_plan_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    auto_renew = table.Column<bool>(type: "INTEGER", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_subscriptions_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customer_subscriptions_subscription_plans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pricing_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    subscription_plan_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    rule_type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    configuration = table.Column<string>(type: "jsonb", nullable: true),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_pricing_rules_subscription_plans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_pricing_rules_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_entities",
                columns: table => new
                {
                    subscription_plan_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    usage_limit = table.Column<int>(type: "INTEGER", nullable: true),
                    soft_limit_threshold = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_entities", x => new { x.subscription_plan_id, x.entity_id });
                    table.ForeignKey(
                        name: "FK_subscription_entities_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscription_entities_subscription_plans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_products",
                columns: table => new
                {
                    subscription_plan_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_products", x => new { x.subscription_plan_id, x.product_id });
                    table.ForeignKey(
                        name: "FK_subscription_products_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscription_products_subscription_plans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    changes = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "configuration_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entity_type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    version_number = table.Column<int>(type: "INTEGER", nullable: false),
                    configuration_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_summary = table.Column<string>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuration_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_configuration_versions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_configuration_versions_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    role_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionModule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductModuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionModule_ProductModule_ProductModuleId",
                        column: x => x.ProductModuleId,
                        principalTable: "ProductModule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionModule_SubscriptionPlan_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module_entities",
                columns: table => new
                {
                    module_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false),
                    is_required = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_entities", x => new { x.module_id, x.entity_id });
                    table.ForeignKey(
                        name: "FK_module_entities_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_module_entities_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_modules",
                columns: table => new
                {
                    subscription_plan_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    module_id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_modules", x => new { x.subscription_plan_id, x.module_id });
                    table.ForeignKey(
                        name: "FK_subscription_modules_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscription_modules_subscription_plans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    customer_subscription_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    customer_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    invoice_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    billing_period_start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    billing_period_end = table.Column<DateTime>(type: "TEXT", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    tax_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    due_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    paid_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoices_customer_subscriptions_customer_subscription_id",
                        column: x => x.customer_subscription_id,
                        principalTable: "customer_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoices_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoices_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usage_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    customer_subscription_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    alert_type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    threshold = table.Column<int>(type: "INTEGER", nullable: false),
                    current_usage = table.Column<int>(type: "INTEGER", nullable: false),
                    is_resolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_alerts", x => x.id);
                    table.ForeignKey(
                        name: "FK_usage_alerts_customer_subscriptions_customer_subscription_id",
                        column: x => x.customer_subscription_id,
                        principalTable: "customer_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usage_alerts_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usage_alerts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usage_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tenant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    customer_subscription_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    units = table.Column<int>(type: "INTEGER", nullable: false),
                    complexity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    billing_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    recorded_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_usage_records_customer_subscriptions_customer_subscription_id",
                        column: x => x.customer_subscription_id,
                        principalTable: "customer_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usage_records_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usage_records_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "complexity_multipliers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    pricing_rule_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    complexity_level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    multiplier = table.Column<decimal>(type: "decimal(10,2)", precision: 15, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_complexity_multipliers", x => x.id);
                    table.ForeignKey(
                        name: "FK_complexity_multipliers_pricing_rules_pricing_rule_id",
                        column: x => x.pricing_rule_id,
                        principalTable: "pricing_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pricing_tiers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    pricing_rule_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    min_units = table.Column<int>(type: "INTEGER", nullable: false),
                    max_units = table.Column<int>(type: "INTEGER", nullable: true),
                    unit_price = table.Column<decimal>(type: "decimal(15,4)", precision: 15, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_tiers", x => x.id);
                    table.ForeignKey(
                        name: "FK_pricing_tiers_pricing_rules_pricing_rule_id",
                        column: x => x.pricing_rule_id,
                        principalTable: "pricing_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_line_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    invoice_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(15,4)", precision: 15, scale: 4, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_line_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_line_items_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_tenant_id_entity_type_entity_id",
                table: "audit_logs",
                columns: new[] { "tenant_id", "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_user_id",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_complexity_multipliers_pricing_rule_id",
                table: "complexity_multipliers",
                column: "pricing_rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_configuration_versions_created_by",
                table: "configuration_versions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_configuration_versions_tenant_id",
                table: "configuration_versions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_subscriptions_customer_id",
                table: "customer_subscriptions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_subscriptions_subscription_plan_id",
                table: "customer_subscriptions",
                column: "subscription_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_tenant_id",
                table: "customers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_discount_codes_code",
                table: "discount_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discount_codes_tenant_id",
                table: "discount_codes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_entities_tenant_id",
                table: "entities",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_line_items_invoice_id",
                table: "invoice_line_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_customer_id",
                table: "invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_customer_subscription_id",
                table: "invoices",
                column: "customer_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_tenant_id",
                table: "invoices",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_module_entities_entity_id",
                table: "module_entities",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_modules_product_id",
                table: "modules",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_modules_tenant_id",
                table: "modules",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_rules_subscription_plan_id",
                table: "pricing_rules",
                column: "subscription_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_rules_tenant_id",
                table: "pricing_rules",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_tiers_pricing_rule_id",
                table: "pricing_tiers",
                column: "pricing_rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_TenantId",
                table: "Product",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModule_ProductId",
                table: "ProductModule",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_products_tenant_id",
                table: "products",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_tenant_id_code",
                table: "products",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_tenant_id",
                table: "roles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_entities_entity_id",
                table: "subscription_entities",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_modules_module_id",
                table: "subscription_modules",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plans_tenant_id",
                table: "subscription_plans",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_products_product_id",
                table: "subscription_products",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModule_ProductModuleId",
                table: "SubscriptionModule",
                column: "ProductModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModule_SubscriptionPlanId",
                table: "SubscriptionModule",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanEntity_ModuleEntityId",
                table: "SubscriptionPlanEntity",
                column: "ModuleEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanEntity_SubscriptionPlanId",
                table: "SubscriptionPlanEntity",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionProduct_ProductId",
                table: "SubscriptionProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionProduct_SubscriptionPlanId",
                table: "SubscriptionProduct",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_subdomain",
                table: "tenants",
                column: "subdomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usage_alerts_customer_subscription_id",
                table: "usage_alerts",
                column: "customer_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_usage_alerts_entity_id",
                table: "usage_alerts",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_usage_alerts_tenant_id",
                table: "usage_alerts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_customer_subscription_id",
                table: "usage_records",
                column: "customer_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_entity_id",
                table: "usage_records",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_tenant_id",
                table: "usage_records",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_tenant_id",
                table: "users",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "complexity_multipliers");

            migrationBuilder.DropTable(
                name: "configuration_versions");

            migrationBuilder.DropTable(
                name: "discount_codes");

            migrationBuilder.DropTable(
                name: "invoice_line_items");

            migrationBuilder.DropTable(
                name: "module_entities");

            migrationBuilder.DropTable(
                name: "pricing_tiers");

            migrationBuilder.DropTable(
                name: "subscription_entities");

            migrationBuilder.DropTable(
                name: "subscription_modules");

            migrationBuilder.DropTable(
                name: "subscription_products");

            migrationBuilder.DropTable(
                name: "SubscriptionModule");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanEntity");

            migrationBuilder.DropTable(
                name: "SubscriptionProduct");

            migrationBuilder.DropTable(
                name: "usage_alerts");

            migrationBuilder.DropTable(
                name: "usage_records");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "pricing_rules");

            migrationBuilder.DropTable(
                name: "modules");

            migrationBuilder.DropTable(
                name: "ProductModule");

            migrationBuilder.DropTable(
                name: "ModuleEntity");

            migrationBuilder.DropTable(
                name: "SubscriptionPlan");

            migrationBuilder.DropTable(
                name: "entities");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "customer_subscriptions");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
