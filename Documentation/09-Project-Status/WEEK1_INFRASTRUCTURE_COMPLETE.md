# Week 1 Critical Infrastructure - Implementation Complete

This document tracks the implementation of Week 1 critical infrastructure components for the Nalam360 Enterprise Platform.

## ✅ Completed Components (10/10)

### 1. N360ErrorBoundary Component ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Components/Utility/N360ErrorBoundary.razor`

**Purpose:** Catches component errors and displays user-friendly error messages with recovery options.

**Features:**
- Inherits from Blazor's ErrorBoundary
- Three severity levels: Warning, Error, Critical
- Customizable error display with custom render fragments
- Built-in action buttons: Reload, Retry, Contact Support
- Automatic audit logging of errors
- Stack trace display (configurable)
- Responsive design with modern error UI

**Usage Example:**
```razor
<N360ErrorBoundary ShowDetails="true" EnableAudit="true" AuditResource="Dashboard">
    <ChildContent>
        <YourComponent />
    </ChildContent>
    <ErrorContent Context="exception">
        <h3>Custom Error: @exception.Message</h3>
    </ErrorContent>
</N360ErrorBoundary>
```

**Key Parameters:**
- `Severity`: Warning, Error, Critical (affects styling)
- `ShowDetails`: Show error details
- `ShowStackTrace`: Show full stack trace
- `ShowReloadButton`, `ShowRetryButton`, `ShowContactSupportButton`: Button visibility
- `OnError`, `OnReload`, `OnRetry`, `OnContactSupport`: Event callbacks
- `EnableAudit`: Automatic error logging

---

### 2. JavaScript Interop Helper ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/wwwroot/js/nalam360-interop.js`

**Purpose:** Centralized JavaScript interop module for browser API interactions.

**Features:**
- **Clipboard**: Copy/read clipboard text
- **Storage**: LocalStorage and SessionStorage operations
- **Focus**: Element focus management
- **Scroll**: Smooth scrolling, scroll-to-element, position tracking
- **File Downloads**: Download text files or base64-encoded files
- **Print**: Print page or specific elements
- **Browser Info**: Detect browser type, mobile, touch support
- **Element Operations**: Add/remove/toggle classes, get bounding rect
- **Event Listeners**: Add/remove event listeners with cleanup

**Usage Example (from Blazor):**
```csharp
// Copy to clipboard
await JSRuntime.InvokeAsync<object>("Nalam360Interop.clipboard.copyToClipboard", "Hello World");

// Download file
await JSRuntime.InvokeVoidAsync("Nalam360Interop.file.downloadFile", "data.json", jsonData, "application/json");

// Scroll to element
await JSRuntime.InvokeVoidAsync("Nalam360Interop.scroll.scrollToElement", "#top-section", "smooth");

// Get browser info
var info = await JSRuntime.InvokeAsync<object>("Nalam360Interop.browser.getBrowserInfo");
```

**API Sections:**
- `clipboard.*` - Clipboard operations
- `localStorage.*` / `sessionStorage.*` - Browser storage
- `focus.*` - Focus management
- `scroll.*` - Scroll operations
- `file.*` - File download
- `print.*` - Print operations
- `browser.*` - Browser detection
- `element.*` - DOM manipulation
- `events.*` - Event handling

---

### 3. StateManager Service ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/State/`

**Purpose:** Generic state management service for sharing state across components.

**Features:**
- Thread-safe state updates
- State change notifications
- State snapshots (deep cloning)
- Reset to initial state
- Generic type support
- JSON serialization for cloning

**Usage Example:**
```csharp
// Create state manager
var stateManager = StateManagerFactory.Create(new UserState 
{ 
    Name = "John", 
    IsLoggedIn = false 
});

// Subscribe to changes
stateManager.OnStateChanged += () => 
{
    Console.WriteLine("State changed!");
    StateHasChanged();
};

// Update state
stateManager.UpdateState(state => state.IsLoggedIn = true);

// Or replace entire state
stateManager.UpdateState(new UserState { Name = "Jane", IsLoggedIn = true });

// Get snapshot
var snapshot = stateManager.GetSnapshot();

// Reset to initial
stateManager.ResetState();
```

**Interface:**
```csharp
public interface IStateManager<T> where T : class
{
    T State { get; }
    event Action? OnStateChanged;
    void UpdateState(T newState);
    void UpdateState(Action<T> updateAction);
    void ResetState();
    T GetSnapshot();
}
```

---

### 4. ApiClient Service ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/Http/`

**Purpose:** HTTP client wrapper with Result pattern, automatic retry logic, and error handling.

**Features:**
- Railway-Oriented Programming with `Result<T>`
- Automatic retry on network failures (configurable, default: 3 retries)
- Exponential backoff between retries
- Supports GET, POST, PUT, DELETE, PATCH
- Bearer token authentication
- Custom header management
- Automatic JSON serialization/deserialization
- Comprehensive error mapping to Result.Failure

**Usage Example:**
```csharp
@inject IApiClient ApiClient

// GET request
var result = await ApiClient.GetAsync<List<Order>>("/api/orders");
if (result.IsSuccess)
{
    var orders = result.Value;
    // Process orders
}
else
{
    // Handle error
    Console.WriteLine(result.Error.Message);
}

// POST request
var createResult = await ApiClient.PostAsync<Order>("/api/orders", new CreateOrderRequest
{
    CustomerId = customerId,
    Total = 100.50m
});

// Set auth token
ApiClient.SetAuthorizationToken(jwtToken);

// Custom headers
ApiClient.SetHeader("X-Tenant-Id", tenantId);
```

**Configuration:**
```csharp
services.AddHttpClient<IApiClient, ApiClient>();
services.AddScoped<IApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(IApiClient));
    httpClient.BaseAddress = new Uri("https://api.example.com");
    httpClient.Timeout = TimeSpan.FromSeconds(30);
    
    var logger = sp.GetRequiredService<ILogger<ApiClient>>();
    return new ApiClient(httpClient, logger, maxRetries: 3, retryDelaySeconds: 1);
});
```

**Error Mapping:**
- 404 → `Result.Failure(Error.NotFound(...))`
- 401 → `Result.Failure(Error.Failure("Unauthorized", ...))`
- 403 → `Result.Failure(Error.Failure("Forbidden", ...))`
- 400 → `Result.Failure(Error.Validation(...))`
- Others → `Result.Failure(Error.Failure("ApiError", ...))`

---

### 5. NotificationQueue Service ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/Notifications/`

**Purpose:** Manages a queue of toast/notification messages for display to users.

**Features:**
- Thread-safe notification queue
- Four notification types: Success, Error, Warning, Info
- Auto-dismiss with configurable duration
- Event-driven notifications (OnNotificationAdded, OnNotificationRemoved)
- Persistent notifications (null duration)
- Unique IDs for tracking
- Ordered by creation time

**Usage Example:**
```csharp
@inject INotificationQueue NotificationQueue

// Add notifications
var id = NotificationQueue.Success("Order created successfully!", duration: 5000);
NotificationQueue.Error("Failed to save changes", duration: 8000);
NotificationQueue.Warning("This action cannot be undone", duration: 6000);
NotificationQueue.Info("New message received", duration: 5000);

// Persistent notification (manual dismiss)
var persistentId = NotificationQueue.Add("Important: Server maintenance scheduled", NotificationType.Warning, duration: null);

// Remove specific notification
NotificationQueue.Remove(persistentId);

// Clear all
NotificationQueue.Clear();

// Subscribe to events
protected override void OnInitialized()
{
    NotificationQueue.OnNotificationAdded += HandleNotificationAdded;
    NotificationQueue.OnNotificationRemoved += HandleNotificationRemoved;
}

private void HandleNotificationAdded(NotificationMessage message)
{
    Console.WriteLine($"New {message.Type}: {message.Message}");
    StateHasChanged();
}

// Get all current notifications
var notifications = NotificationQueue.GetAll();
```

**NotificationMessage Properties:**
- `Id`: Unique identifier (Guid)
- `Message`: The notification text
- `Type`: Success, Error, Warning, Info
- `Duration`: Auto-dismiss time in milliseconds (null = persistent)
- `CreatedAt`: Timestamp (UTC)

---

### 6. StorageService ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/Storage/StorageService.cs`

**Purpose:** Service for interacting with browser LocalStorage and SessionStorage.

**Features:**
- Generic type support with automatic JSON serialization
- Separate LocalStorage and SessionStorage APIs
- Async operations
- Error handling with null returns on failure
- Type-safe get/set operations

**Usage Example:**
```csharp
@inject IStorageService StorageService

// LocalStorage operations
await StorageService.SetItemAsync("user", new User { Name = "John", Id = 123 });
var user = await StorageService.GetItemAsync<User>("user");
await StorageService.RemoveItemAsync("user");
await StorageService.ClearAsync();

// SessionStorage operations
await StorageService.SetSessionItemAsync("tempData", myData);
var data = await StorageService.GetSessionItemAsync<MyData>("tempData");
await StorageService.RemoveSessionItemAsync("tempData");
await StorageService.ClearSessionAsync();

// Store complex objects
await StorageService.SetItemAsync("settings", new AppSettings 
{ 
    Theme = "dark",
    Language = "en",
    Notifications = true 
});

// Retrieve with type safety
var settings = await StorageService.GetItemAsync<AppSettings>("settings");
if (settings != null)
{
    ApplySettings(settings);
}
```

**Interface:**
```csharp
public interface IStorageService
{
    // LocalStorage
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
    Task ClearAsync();
    
    // SessionStorage
    Task<T?> GetSessionItemAsync<T>(string key);
    Task SetSessionItemAsync<T>(string key, T value);
    Task RemoveSessionItemAsync(string key);
    Task ClearSessionAsync();
}
```

---

### 7. BrowserService ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/Browser/BrowserService.cs`

**Purpose:** Service for browser-specific operations (clipboard, downloads, print, scroll, focus).

**Features:**
- Clipboard operations (copy/read)
- File downloads (text and base64)
- Print operations (page or specific element)
- Browser detection (Chrome, Firefox, Safari, Edge, mobile)
- Viewport size detection
- Scroll operations (smooth/auto)
- Element focus management

**Usage Example:**
```csharp
@inject IBrowserService BrowserService

// Clipboard
var copied = await BrowserService.CopyToClipboardAsync("Hello World");
var clipboardText = await BrowserService.ReadFromClipboardAsync();

// File download
await BrowserService.DownloadFileAsync("report.csv", csvData, "text/csv");
await BrowserService.DownloadBase64FileAsync("image.png", base64ImageData, "image/png");

// Print
await BrowserService.PrintPageAsync();
await BrowserService.PrintElementAsync("#report-section");

// Browser detection
var browserInfo = await BrowserService.GetBrowserInfoAsync();
if (browserInfo.IsMobile)
{
    // Show mobile UI
}

// Viewport
var viewport = await BrowserService.GetViewportSizeAsync();
if (viewport.Width < 768)
{
    // Mobile layout
}

// Scroll
await BrowserService.ScrollToTopAsync();
await BrowserService.ScrollToElementAsync("#section-2", "smooth");

// Focus
await BrowserService.FocusElementAsync("#first-name-input");
```

**BrowserInfo Properties:**
- `UserAgent`: Full user agent string
- `IsChrome`, `IsFirefox`, `IsSafari`, `IsEdge`, `IsOpera`: Browser detection
- `IsMobile`: Mobile device detection
- `IsTouch`: Touch support detection
- `Language`: Browser language
- `Platform`: Operating system platform

---

### 8. StringExtensions ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/Utilities/StringExtensions.cs`

**Purpose:** Extension methods for common string manipulation operations.

**Features:**
- **Case Conversion**: ToTitleCase, ToKebabCase, ToCamelCase, ToPascalCase
- **Truncation**: Truncate with custom suffix
- **Validation**: IsNumeric, IsValidEmail
- **Encoding**: ToBase64, FromBase64
- **Formatting**: ToSlug (URL-friendly), RemoveWhitespace
- **Pluralization**: Pluralize based on count

**Usage Example:**
```csharp
using Nalam360Enterprise.UI.Core.Utilities;

// Truncate
var text = "This is a very long string".Truncate(10); // "This is..."

// Case conversion
"hello world".ToTitleCase(); // "Hello World"
"HelloWorld".ToKebabCase(); // "hello-world"
"hello-world".ToCamelCase(); // "helloWorld"
"hello-world".ToPascalCase(); // "HelloWorld"

// Validation
"12345".IsNumeric(); // true
"test@example.com".IsValidEmail(); // true

// Encoding
"Hello".ToBase64(); // "SGVsbG8="
"SGVsbG8=".FromBase64(); // "Hello"

// Slug
"My Blog Post Title!".ToSlug(); // "my-blog-post-title"

// Pluralize
"item".Pluralize(1); // "item"
"item".Pluralize(5); // "items"
"category".Pluralize(3); // "categories"
"box".Pluralize(2); // "boxes"
"child".Pluralize(2, "children"); // "children" (custom plural)

// Remove whitespace
"Hello   World".RemoveWhitespace(); // "HelloWorld"
```

**Available Methods:**
- `Truncate(maxLength, suffix = "...")`
- `ToTitleCase()`
- `ToKebabCase()`
- `ToCamelCase()`
- `ToPascalCase()`
- `RemoveWhitespace()`
- `IsNumeric()`
- `IsValidEmail()`
- `ToBase64()`
- `FromBase64()`
- `Pluralize(count, pluralForm = null)`
- `ToSlug()`

---

### 9. DateTimeExtensions ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/Utilities/DateTimeExtensions.cs`

**Purpose:** Extension methods for DateTime manipulation and formatting.

**Features:**
- **Relative Time**: "2 hours ago", "just now", "3 days ago"
- **Date Checks**: IsToday, IsYesterday, IsFuture, IsPast, IsWeekend, IsWeekday
- **Range Operations**: StartOfDay, EndOfDay, StartOfWeek, EndOfWeek, StartOfMonth, EndOfMonth
- **Business Days**: AddBusinessDays (skips weekends)
- **Age Calculation**: GetAge from birth date
- **Formatting**: ToIso8601, ToUnixTimestamp

**Usage Example:**
```csharp
using Nalam360Enterprise.UI.Core.Utilities;

// Relative time
var posted = DateTime.UtcNow.AddHours(-2);
posted.ToRelativeTime(); // "2 hours ago"

DateTime.UtcNow.AddSeconds(-30).ToRelativeTime(); // "just now"
DateTime.UtcNow.AddDays(-5).ToRelativeTime(); // "5 days ago"

// Date checks
DateTime.Today.IsToday(); // true
DateTime.Today.AddDays(-1).IsYesterday(); // true
DateTime.Now.AddDays(1).IsFuture(); // true
DateTime.Now.AddDays(-1).IsPast(); // true
new DateTime(2024, 11, 23).IsWeekend(); // true (Saturday)

// Date ranges
var today = DateTime.Today;
today.StartOfDay(); // Today at 00:00:00
today.EndOfDay(); // Today at 23:59:59.999
today.StartOfWeek(); // Monday of current week
today.EndOfWeek(); // Sunday of current week
today.StartOfMonth(); // First day of month
today.EndOfMonth(); // Last day of month
today.StartOfYear(); // January 1
today.EndOfYear(); // December 31

// Business days
var dueDate = DateTime.Today.AddBusinessDays(5); // 5 business days from today

// Age calculation
var birthDate = new DateTime(1990, 5, 15);
var age = birthDate.GetAge(); // 34 (as of 2024)

// Formatting
DateTime.UtcNow.ToIso8601(); // "2024-11-21T14:30:00.000Z"
DateTime.UtcNow.ToUnixTimestamp(); // 1700576400
DateTimeExtensions.FromUnixTimestamp(1700576400); // DateTime
```

**Available Methods:**
- `ToRelativeTime()`
- `IsToday()`, `IsYesterday()`, `IsFuture()`, `IsPast()`
- `StartOfDay()`, `EndOfDay()`
- `StartOfWeek()`, `EndOfWeek()`
- `StartOfMonth()`, `EndOfMonth()`
- `StartOfYear()`, `EndOfYear()`
- `AddBusinessDays(days)`
- `IsWeekend()`, `IsWeekday()`
- `GetAge()`
- `ToIso8601()`
- `ToUnixTimestamp()`
- `FromUnixTimestamp(timestamp)` (static)

---

### 10. DebounceHelper & ThrottleHelper ✅
**Location:** `src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Core/Utilities/DebounceHelper.cs`

**Purpose:** Helpers for debouncing and throttling rapid function calls.

**Features:**
- **DebounceHelper**: Delays execution until calls stop coming
- **ThrottleHelper**: Limits execution to maximum rate
- Async support
- Cancellation support
- Thread-safe

**Usage Example:**
```csharp
using Nalam360Enterprise.UI.Core.Utilities;

// Debounce - waits 300ms after last call
private readonly DebounceHelper _searchDebouncer = new(300);

private async Task OnSearchInputChanged(string searchText)
{
    await _searchDebouncer.DebounceAsync(async () =>
    {
        // This executes only after user stops typing for 300ms
        var results = await SearchApi.SearchAsync(searchText);
        UpdateResults(results);
    });
}

// Throttle - executes at most once per 1000ms
private readonly ThrottleHelper _scrollThrottler = new(1000);

private async Task OnScroll()
{
    var executed = await _scrollThrottler.ThrottleAsync(async () =>
    {
        // This executes at most once per second
        await LoadMoreData();
    });
    
    if (!executed)
    {
        Console.WriteLine("Throttled - too soon");
    }
}

// Synchronous versions
await _debouncer.DebounceAsync(() => Console.WriteLine("Debounced!"));
await _throttler.ThrottleAsync(() => Console.WriteLine("Throttled!"));

// Reset throttle timer
_throttler.Reset();

// Dispose when done (DebounceHelper implements IDisposable)
_searchDebouncer.Dispose();
```

**Common Use Cases:**
- **Debounce**: Search input, form validation, window resize
- **Throttle**: Scroll events, mouse move, API rate limiting

---

## Service Registration

All services are automatically registered when you call `AddNalam360EnterpriseUI()`:

```csharp
builder.Services.AddNalam360EnterpriseUI();
```

**Registered Services:**
- `IStateManager<T>` - Generic state management (Scoped)
- `IApiClient` - HTTP client wrapper (Scoped)
- `INotificationQueue` - Notification queue (Singleton)
- `IStorageService` - Browser storage (Scoped)
- `IBrowserService` - Browser operations (Scoped)
- Extension methods (StringExtensions, DateTimeExtensions) - Static, no registration needed
- DebounceHelper, ThrottleHelper - Create instances as needed

**JavaScript Module:**
The `nalam360-interop.js` file is automatically included and available at:
```
wwwroot/js/nalam360-interop.js
```

Reference in your app:
```html
<script src="_content/Nalam360Enterprise.UI/js/nalam360-interop.js"></script>
```

---

## Testing

All components and services should be tested:

**Unit Tests:**
```powershell
dotnet test tests/Nalam360Enterprise.UI.Tests/ --filter "Category=Infrastructure"
```

**Component Tests (bUnit):**
```csharp
[Fact]
public void ErrorBoundary_CatchesException_DisplaysError()
{
    // Arrange
    var cut = RenderComponent<N360ErrorBoundary>(parameters => parameters
        .Add(p => p.ChildContent, (RenderFragment)(_ => throw new Exception("Test error"))));
    
    // Assert
    cut.Find(".n360-error-boundary__title").TextContent.Should().Contain("Something went wrong");
}
```

**Service Tests:**
```csharp
[Fact]
public async Task ApiClient_WithValidRequest_ReturnsSuccess()
{
    // Arrange
    var apiClient = CreateApiClient();
    
    // Act
    var result = await apiClient.GetAsync<List<Order>>("/api/orders");
    
    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

---

## Next Steps

**Week 2 (Performance Components):**
- N360VirtualScroll (10k+ row optimization)
- N360Signature (healthcare consent)
- EnumerableExtensions (LINQ helpers)
- ColorExtensions (hex/rgb conversion)

**Week 3 (Healthcare Essentials):**
- N360VideoCall (WebRTC telemedicine)
- ValidationHelper (common validation rules)
- N360LazyLoad, N360InfiniteScroll

**Week 4 (Utility Services):**
- FileHelper, ExportHelper
- CacheService
- Remaining 8 utility services

---

## Breaking Changes

None - these are all new additions.

---

## Migration Guide

If you have custom implementations of any of these services, you can:

1. **Use built-in services:**
   ```csharp
   builder.Services.AddNalam360EnterpriseUI();
   ```

2. **Keep your custom implementation:**
   ```csharp
   // Register built-in services
   builder.Services.AddNalam360EnterpriseUI();
   
   // Override specific service
   builder.Services.Replace(ServiceDescriptor.Scoped<IApiClient, MyCustomApiClient>());
   ```

3. **Mix and match:**
   ```csharp
   builder.Services.AddNalam360EnterpriseUI();
   
   // Use built-in for most, custom for specific needs
   builder.Services.AddScoped<IStorageService, MyEncryptedStorageService>();
   ```

---

## Documentation

Full API documentation is available in the interactive docs site:
```powershell
dotnet run --project docs/Nalam360Enterprise.Docs.Web
```

Navigate to: http://localhost:5032

---

## Support

For issues or questions:
- Create an issue in the repository
- Check the documentation at `Documentation/03-Components/`
- Review examples in `examples/` folder

---

**Status:** ✅ Production Ready
**Last Updated:** November 21, 2024
**Version:** 1.0.0
