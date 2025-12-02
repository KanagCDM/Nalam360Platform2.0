using ProductAdminPortal.Components;
using ProductAdminPortal.Services;
using ProductAdminPortal.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add API controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "ProductAdmin API",
        Version = "v1",
        Description = "Domain-agnostic product configuration and subscription management API"
    });
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Database Contexts
// Legacy database (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=MedWayDb.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Product Admin database (PostgreSQL)
var productAdminConnectionString = builder.Configuration.GetConnectionString("ProductAdminConnection")
    ?? "Host=localhost;Port=5432;Database=productadmin;Username=postgres;Password=postgres";

// Use SQLite for development to avoid PostgreSQL authentication issues
builder.Services.AddDbContext<ProductAdminDbContext>(options =>
    options.UseSqlite("Data Source=ProductAdmin.db"));

// Add Redis cache (optional - configure if Redis is available)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });
}

// Add authentication and authorization
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Register legacy application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ModuleService>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddSingleton<HospitalService>();
builder.Services.AddSingleton<EmailVerificationService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

// Register ProductAdmin services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IUsageService, UsageService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Add CORS for API endpoints
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

// Initialize databases
using (var scope = app.Services.CreateScope())
{
    // Initialize legacy database
    var legacyDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await legacyDbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ Legacy database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Legacy database initialization error: {ex.Message}");
    }

    // Initialize ProductAdmin database
    var productAdminDbContext = scope.ServiceProvider.GetRequiredService<ProductAdminDbContext>();
    try
    {
        // Apply pending migrations
        await productAdminDbContext.Database.MigrateAsync();
        Console.WriteLine("✅ ProductAdmin database migrated successfully");
        
        // Verify tables were created
        var canConnect = await productAdminDbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            Console.WriteLine("✅ ProductAdmin database connection verified");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ProductAdmin database initialization error: {ex.GetType().Name}: {ex.Message}");
        Console.WriteLine("💡 Application will continue, but API endpoints may fail");
        Console.WriteLine($"💡 Connection string: {productAdminConnectionString.Replace(builder.Configuration.GetConnectionString("ProductAdminConnection")?.Split("Password=")[1]?.Split(";")[0] ?? "", "***")}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductAdmin API v1");
        options.RoutePrefix = "api-docs";
    });
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseCors();

app.UseAntiforgery();

// Map API controllers
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine("🚀 ProductAdminPortal is running!");
Console.WriteLine($"📖 Blazor UI: https://localhost:{builder.Configuration["ASPNETCORE_HTTPS_PORTS"] ?? "5096"}");
Console.WriteLine($"📖 API Docs: https://localhost:{builder.Configuration["ASPNETCORE_HTTPS_PORTS"] ?? "5096"}/api-docs");
Console.WriteLine("📋 API Endpoints:");
Console.WriteLine("  - GET/POST    /api/v1/products");
Console.WriteLine("  - POST        /api/v1/pricing/calculate");
Console.WriteLine("  - POST        /api/v1/pricing/simulate");
Console.WriteLine("  - POST        /api/v1/billing/invoices");
Console.WriteLine("  - POST        /api/v1/usage/record");
Console.WriteLine("  - GET         /api/v1/audit/logs");

app.Run();
