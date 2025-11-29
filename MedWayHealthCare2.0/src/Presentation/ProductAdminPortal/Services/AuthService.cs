namespace ProductAdminPortal.Services;

public class AuthService
{
    private string? _currentUser;
    private List<string> _selectedModules = new();

    public event Action? OnAuthStateChanged;

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_currentUser));
    }

    public Task<string?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    public Task<List<string>> GetSelectedModulesAsync()
    {
        return Task.FromResult(_selectedModules);
    }

    public Task LoginAsync(string username, string password, List<string> selectedModules)
    {
        // Simple authentication - in production, validate against a real user store
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            _currentUser = username;
            _selectedModules = selectedModules;
            OnAuthStateChanged?.Invoke();
            return Task.CompletedTask;
        }
        throw new UnauthorizedAccessException("Invalid credentials");
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        _selectedModules.Clear();
        OnAuthStateChanged?.Invoke();
        return Task.CompletedTask;
    }
}
