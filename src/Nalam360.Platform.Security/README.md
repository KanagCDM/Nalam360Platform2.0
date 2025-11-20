# Nalam360 Platform - Security & Authorization

Enterprise-grade security module with JWT authentication, encryption, key vault integration, and complete RBAC system.

## Features

### 🔐 Authentication & Tokens
- JWT token generation and validation
- Configurable token expiration and refresh
- Claims-based authentication
- Multiple signing key support

### 🔒 Cryptography
- AES-256-CBC encryption with PBKDF2 key derivation
- PBKDF2 password hashing (100,000 iterations)
- Secure random salt generation
- Key derivation from master keys

### 🔑 Key Management
- Azure Key Vault integration
- DefaultAzureCredential support
- Secret retrieval and caching
- Connection string management

### 👤 Authorization (RBAC)
- Permission-based access control
- Role-based access control with inheritance
- Policy-based authorization
- Claims transformation pipeline
- Explicit permission denials
- Extensible store interfaces

## Installation

```bash
dotnet add package Nalam360.Platform.Security
```

## Quick Start

### 1. JWT Authentication

```csharp
using Nalam360.Platform.Security.Tokens;

// Configure JWT
builder.Services.Configure<JwtOptions>(options =>
{
    options.SecretKey = "your-super-secret-key-min-32-chars";
    options.Issuer = "YourApp";
    options.Audience = "YourApp.Users";
    options.ExpirationMinutes = 60;
});

// Generate token
var tokenGenerator = serviceProvider.GetRequiredService<IJwtTokenGenerator>();
var claims = new Dictionary<string, object>
{
    ["sub"] = userId,
    ["email"] = "user@example.com",
    ["role"] = "Admin"
};
var token = tokenGenerator.Generate(claims);

// Validate token
var validationResult = await tokenValidator.ValidateAsync(token);
if (validationResult.IsValid)
{
    var userId = validationResult.Claims["sub"];
}
```

### 2. Password Hashing

```csharp
using Nalam360.Platform.Security.Cryptography;

var hasher = new Pbkdf2PasswordHasher();

// Hash password
string hashedPassword = hasher.HashPassword("user-password");

// Verify password
bool isValid = hasher.VerifyPassword("user-password", hashedPassword);
```

### 3. Encryption

```csharp
using Nalam360.Platform.Security.Cryptography;

// Configure
builder.Services.Configure<AesEncryptionOptions>(options =>
{
    options.MasterKey = "your-master-key";
    options.Salt = "your-salt";
});

// Encrypt
var encryptionService = serviceProvider.GetRequiredService<IEncryptionService>();
var data = Encoding.UTF8.GetBytes("sensitive data");
var encrypted = await encryptionService.EncryptAsync(data);

// Decrypt
var decrypted = await encryptionService.DecryptAsync(encrypted);
var original = Encoding.UTF8.GetString(decrypted.Value);
```

### 4. Azure Key Vault

```csharp
using Nalam360.Platform.Security.KeyVault;

// Configure
builder.Services.Configure<AzureKeyVaultOptions>(options =>
{
    options.VaultUri = "https://your-vault.vault.azure.net/";
    // Uses DefaultAzureCredential - supports managed identity, Azure CLI, etc.
});

// Use
var keyVault = serviceProvider.GetRequiredService<IKeyVaultService>();
var result = await keyVault.GetSecretAsync("database-connection-string");
if (result.IsSuccess)
{
    var connectionString = result.Value;
}
```

### 5. RBAC Authorization

```csharp
using Nalam360.Platform.Security.Authorization;

// Register services
builder.Services.AddNalam360Authorization(options =>
{
    // Add built-in policies
    options.AddPermissionPolicy("AdminOnly", requireAll: true, "admin.access");
    options.AddRolePolicy("ManagerOrAdmin", requireAll: false, "Manager", "Admin");
    
    // Enable claims transformation
    options.EnableClaimsTransformation = true;
});

// Add middleware (optional - for claims transformation)
app.UseClaimsTransformation();

// Use in controllers
public class OrdersController : ControllerBase
{
    private readonly IAuthorizationService _authz;
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        // Check permission
        if (!await _authz.HasPermissionAsync("orders.create"))
        {
            return Forbid();
        }
        
        // Process order...
        return Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        // Check policy
        if (!await _authz.SatisfiesPolicyAsync("AdminOnly"))
        {
            return Forbid();
        }
        
        // Delete order...
        return NoContent();
    }
}
```

## RBAC Deep Dive

### Permission Management

```csharp
// Create permissions
var permissionStore = serviceProvider.GetRequiredService<IPermissionStore>();

await permissionStore.SavePermissionAsync(new Permission
{
    Name = "orders.create",
    Description = "Create new orders",
    Resource = "Orders",
    Action = "Create",
    Category = "Sales"
});

await permissionStore.SavePermissionAsync(new Permission
{
    Name = "orders.delete",
    Description = "Delete orders",
    Resource = "Orders",
    Action = "Delete",
    Category = "Sales"
});
```

### Role Management with Inheritance

```csharp
var roleStore = serviceProvider.GetRequiredService<IRoleStore>();

// Base role
await roleStore.SaveRoleAsync(new Role
{
    Name = "User",
    Description = "Basic user",
    Permissions = new List<string> { "profile.read", "profile.update" },
    Priority = 1
});

// Manager inherits from User
await roleStore.SaveRoleAsync(new Role
{
    Name = "Manager",
    Description = "Team manager",
    Permissions = new List<string> { "orders.read", "orders.create", "team.view" },
    InheritsFrom = new List<string> { "User" }, // Inherits User permissions
    Priority = 5
});

// Admin inherits from Manager
await roleStore.SaveRoleAsync(new Role
{
    Name = "Admin",
    Description = "System administrator",
    Permissions = new List<string> { "orders.delete", "users.manage", "system.configure" },
    InheritsFrom = new List<string> { "Manager" }, // Inherits Manager + User permissions
    Priority = 10
});
```

### User Principal Assignment

```csharp
var principalStore = serviceProvider.GetRequiredService<IUserPrincipalStore>();

// Assign role to user
await principalStore.AssignRoleAsync(userId, "Manager");

// Grant additional permission
await principalStore.GrantPermissionAsync(userId, "reports.view");

// Explicitly deny permission (overrides role permissions)
await principalStore.DenyPermissionAsync(userId, "orders.delete");

// Check effective permissions
var permissions = await authzService.GetUserPermissionsAsync();
// Returns: profile.read, profile.update, orders.read, orders.create, 
//          team.view, reports.view
//          (orders.delete is excluded due to explicit denial)
```

### Policy-Based Authorization

```csharp
// Permission policy (requires ALL listed permissions)
var policy = new PermissionPolicy(
    name: "OrderManagement",
    requireAll: true,
    "orders.read", "orders.create", "orders.update"
);

// Role policy (requires ANY listed role)
var policy = new RolePolicy(
    name: "ManagerOrAdmin",
    requireAll: false,
    "Manager", "Admin"
);

// Custom policy with business logic
var policy = new CustomPolicy(
    name: "BusinessHoursOnly",
    async (principal, permissions, roles) =>
    {
        var now = DateTime.Now;
        if (now.Hour < 9 || now.Hour > 17)
        {
            return AuthorizationResult.Failure("Outside business hours");
        }
        return AuthorizationResult.Success();
    }
);

// Composite policy (combines multiple policies)
var policy = new CompositePolicy(
    name: "AdminDuringBusinessHours",
    requireAll: true,
    new RolePolicy("AdminRole", true, "Admin"),
    new CustomPolicy("BusinessHours", /* ... */)
);

// Register policies
builder.Services.AddNalam360Authorization(options =>
{
    options.AddCustomPolicy(policy);
});

// Use in code
if (await _authz.SatisfiesPolicyAsync("OrderManagement"))
{
    // User has all required permissions
}
```

### Claims Transformation

```csharp
// Automatically adds role and permission claims to user identity
// Enable in configuration:
builder.Services.AddNalam360Authorization(options =>
{
    options.EnableClaimsTransformation = true;
});

// Add middleware to pipeline
app.UseClaimsTransformation();

// Now you can use standard ASP.NET Core authorization
[Authorize(Roles = "Admin")] // Works with RBAC roles
public class AdminController : ControllerBase { }

// Or check claims directly
if (User.HasClaim("permission", "orders.delete"))
{
    // User has permission
}
```

## Advanced Configuration

### Custom Stores

```csharp
// Implement database-backed stores
public class DatabaseRoleStore : IRoleStore
{
    private readonly DbContext _db;
    
    public async Task<Role?> GetRoleAsync(string name, CancellationToken ct)
    {
        return await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == name, ct);
    }
    
    // Implement other methods...
}

// Register custom stores
builder.Services.AddNalam360Authorization(options =>
{
    options.UseRoleStore<DatabaseRoleStore>();
    options.UsePermissionStore<DatabasePermissionStore>();
    options.UseUserPrincipalStore<DatabaseUserPrincipalStore>();
});
```

### Multi-Tenancy Integration

```csharp
public class TenantAwareAuthorizationService : IAuthorizationService
{
    private readonly IAuthorizationService _inner;
    private readonly ITenantProvider _tenantProvider;
    
    public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct)
    {
        var tenantId = _tenantProvider.GetTenantId();
        var scopedPermission = $"{tenantId}:{permission}";
        return await _inner.HasPermissionAsync(scopedPermission, ct);
    }
}
```

## Security Best Practices

### ✅ DO

1. **Use Strong Keys**
   ```csharp
   // Minimum 32 characters for JWT secret
   options.SecretKey = "your-very-long-secret-key-at-least-32-chars";
   ```

2. **Store Secrets Securely**
   ```csharp
   // Use Azure Key Vault or User Secrets
   var keyVault = services.GetRequiredService<IKeyVaultService>();
   var jwtKey = await keyVault.GetSecretAsync("jwt-secret-key");
   ```

3. **Use Explicit Denials When Needed**
   ```csharp
   // Override inherited permissions
   await principalStore.DenyPermissionAsync(userId, "sensitive.action");
   ```

4. **Implement Least Privilege**
   ```csharp
   // Only grant necessary permissions
   role.Permissions = new List<string> { "resource.read" }; // Not "resource.*"
   ```

5. **Use Policy-Based Authorization**
   ```csharp
   // Centralize complex authorization logic
   [AuthorizePolicy("ComplexBusinessRule")]
   public async Task<IActionResult> SensitiveAction() { }
   ```

### ❌ DON'T

1. **Don't Hardcode Secrets**
   ```csharp
   // ❌ Bad
   options.SecretKey = "hardcoded-key";
   
   // ✅ Good
   options.SecretKey = configuration["Jwt:SecretKey"];
   ```

2. **Don't Use Weak Algorithms**
   ```csharp
   // ❌ Bad - MD5, SHA1
   // ✅ Good - PBKDF2, bcrypt, Argon2
   ```

3. **Don't Skip Token Validation**
   ```csharp
   // ❌ Bad - trusting tokens without validation
   // ✅ Good - always validate tokens
   var result = await _tokenValidator.ValidateAsync(token);
   ```

4. **Don't Grant ".*" Wildcard Permissions**
   ```csharp
   // ❌ Bad - too broad
   permissions.Add("*");
   
   // ✅ Good - specific permissions
   permissions.Add("orders.read");
   ```

## Testing

### Unit Tests

```csharp
[Fact]
public async Task HasPermission_ReturnsTrue_WhenUserHasDirectPermission()
{
    // Arrange
    var principalStore = new InMemoryUserPrincipalStore();
    await principalStore.GrantPermissionAsync("user-1", "orders.read");
    
    var authz = new DefaultAuthorizationService(
        httpContextAccessor, principalStore, roleStore, policyRegistry);
    
    // Act
    var hasPermission = await authz.HasPermissionAsync("orders.read");
    
    // Assert
    Assert.True(hasPermission);
}

[Fact]
public async Task RoleInheritance_ResolvesPermissionsFromParentRoles()
{
    // Arrange
    var roleStore = new InMemoryRoleStore(permissionStore);
    
    await roleStore.SaveRoleAsync(new Role
    {
        Name = "User",
        Permissions = new List<string> { "profile.read" }
    });
    
    await roleStore.SaveRoleAsync(new Role
    {
        Name = "Admin",
        Permissions = new List<string> { "users.delete" },
        InheritsFrom = new List<string> { "User" }
    });
    
    // Act
    var permissions = await roleStore.GetRolePermissionsAsync("Admin");
    
    // Assert
    Assert.Contains("profile.read", permissions); // From User
    Assert.Contains("users.delete", permissions); // From Admin
}
```

### Integration Tests

```csharp
[Fact]
public async Task Authorization_WorksWithClaimsTransformation()
{
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", validToken);
    
    var response = await client.GetAsync("/api/admin/users");
    
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

## Performance Considerations

- **Caching**: In-memory stores use `ConcurrentDictionary` for O(1) lookups
- **Claims Transformation**: Only executes once per request
- **Role Hierarchy**: Resolved recursively with cycle detection
- **Permission Resolution**: HashSet for efficient permission checking

## Examples

### Complete Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// Add authorization
builder.Services.AddNalam360Authorization(options =>
{
    options.AddPermissionPolicy("AdminAccess", true, "admin.access");
    options.AddRolePolicy("ManagerOrAbove", false, "Manager", "Admin");
    options.EnableClaimsTransformation = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseClaimsTransformation(); // Optional

app.MapControllers();
app.Run();
```

### Seeding Initial Data

```csharp
// Seed permissions and roles on startup
public static async Task SeedAuthorizationDataAsync(IServiceProvider services)
{
    var permissionStore = services.GetRequiredService<IPermissionStore>();
    var roleStore = services.GetRequiredService<IRoleStore>();
    
    // Create permissions
    await permissionStore.SavePermissionAsync(new Permission
    {
        Name = "users.read",
        Description = "View users",
        Resource = "Users",
        Action = "Read"
    });
    
    // Create roles
    await roleStore.SaveRoleAsync(new Role
    {
        Name = "Admin",
        Description = "System administrator",
        Permissions = new List<string> { "users.read", "users.write", "users.delete" },
        Priority = 10,
        IsSystem = true
    });
}
```

## API Reference

See inline XML documentation for complete API reference.

## License

MIT License - See LICENSE file for details
