using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace SimpleMedWayDemo.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;

    public CustomAuthenticationStateProvider(AuthService authService)
    {
        _authService = authService;
        _authService.OnAuthStateChanged += NotifyAuthenticationStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        
        if (string.IsNullOrEmpty(user))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user),
        }, "Custom Authentication");

        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationState(principal);
    }

    private void NotifyAuthenticationStateChanged()
    {
        var authState = GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(authState);
    }

    public void TriggerAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged();
    }
}
