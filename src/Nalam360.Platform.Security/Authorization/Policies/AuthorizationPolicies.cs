using Nalam360.Platform.Security.Authorization.Models;

namespace Nalam360.Platform.Security.Authorization.Policies;

/// <summary>
/// Represents an authorization policy.
/// </summary>
public interface IAuthorizationPolicy
{
    /// <summary>
    /// Policy name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates whether the user satisfies this policy.
    /// </summary>
    Task<AuthorizationResult> EvaluateAsync(
        UserPrincipal principal,
        IReadOnlyList<string> userPermissions,
        IReadOnlyList<string> userRoles,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Policy requiring specific permissions.
/// </summary>
public class PermissionPolicy : IAuthorizationPolicy
{
    private readonly string[] _requiredPermissions;
    private readonly bool _requireAll;

    public string Name { get; }

    public PermissionPolicy(string name, bool requireAll = true, params string[] permissions)
    {
        Name = name;
        _requiredPermissions = permissions;
        _requireAll = requireAll;
    }

    public Task<AuthorizationResult> EvaluateAsync(
        UserPrincipal principal,
        IReadOnlyList<string> userPermissions,
        IReadOnlyList<string> userRoles,
        CancellationToken cancellationToken = default)
    {
        if (_requireAll)
        {
            // Require ALL permissions
            var missing = _requiredPermissions
                .Where(p => !userPermissions.Contains(p))
                .ToArray();

            if (missing.Length > 0)
            {
                return Task.FromResult(AuthorizationResult.MissingPermission(missing));
            }
        }
        else
        {
            // Require ANY permission
            if (!_requiredPermissions.Any(p => userPermissions.Contains(p)))
            {
                return Task.FromResult(AuthorizationResult.MissingPermission(_requiredPermissions));
            }
        }

        return Task.FromResult(AuthorizationResult.Success());
    }
}

/// <summary>
/// Policy requiring specific roles.
/// </summary>
public class RolePolicy : IAuthorizationPolicy
{
    private readonly string[] _requiredRoles;
    private readonly bool _requireAll;

    public string Name { get; }

    public RolePolicy(string name, bool requireAll = true, params string[] roles)
    {
        Name = name;
        _requiredRoles = roles;
        _requireAll = requireAll;
    }

    public Task<AuthorizationResult> EvaluateAsync(
        UserPrincipal principal,
        IReadOnlyList<string> userPermissions,
        IReadOnlyList<string> userRoles,
        CancellationToken cancellationToken = default)
    {
        if (_requireAll)
        {
            // Require ALL roles
            var missing = _requiredRoles
                .Where(r => !userRoles.Contains(r))
                .ToArray();

            if (missing.Length > 0)
            {
                return Task.FromResult(AuthorizationResult.MissingRole(missing));
            }
        }
        else
        {
            // Require ANY role
            if (!_requiredRoles.Any(r => userRoles.Contains(r)))
            {
                return Task.FromResult(AuthorizationResult.MissingRole(_requiredRoles));
            }
        }

        return Task.FromResult(AuthorizationResult.Success());
    }
}

/// <summary>
/// Policy based on custom logic.
/// </summary>
public class CustomPolicy : IAuthorizationPolicy
{
    private readonly Func<UserPrincipal, IReadOnlyList<string>, IReadOnlyList<string>, Task<AuthorizationResult>> _evaluator;

    public string Name { get; }

    public CustomPolicy(
        string name,
        Func<UserPrincipal, IReadOnlyList<string>, IReadOnlyList<string>, Task<AuthorizationResult>> evaluator)
    {
        Name = name;
        _evaluator = evaluator;
    }

    public Task<AuthorizationResult> EvaluateAsync(
        UserPrincipal principal,
        IReadOnlyList<string> userPermissions,
        IReadOnlyList<string> userRoles,
        CancellationToken cancellationToken = default)
    {
        return _evaluator(principal, userPermissions, userRoles);
    }
}

/// <summary>
/// Composite policy combining multiple policies.
/// </summary>
public class CompositePolicy : IAuthorizationPolicy
{
    private readonly IAuthorizationPolicy[] _policies;
    private readonly bool _requireAll;

    public string Name { get; }

    public CompositePolicy(string name, bool requireAll = true, params IAuthorizationPolicy[] policies)
    {
        Name = name;
        _policies = policies;
        _requireAll = requireAll;
    }

    public async Task<AuthorizationResult> EvaluateAsync(
        UserPrincipal principal,
        IReadOnlyList<string> userPermissions,
        IReadOnlyList<string> userRoles,
        CancellationToken cancellationToken = default)
    {
        if (_requireAll)
        {
            // All policies must pass
            foreach (var policy in _policies)
            {
                var result = await policy.EvaluateAsync(principal, userPermissions, userRoles, cancellationToken);
                if (!result.IsAuthorized)
                {
                    return result;
                }
            }
            return AuthorizationResult.Success();
        }
        else
        {
            // Any policy can pass
            foreach (var policy in _policies)
            {
                var result = await policy.EvaluateAsync(principal, userPermissions, userRoles, cancellationToken);
                if (result.IsAuthorized)
                {
                    return AuthorizationResult.Success();
                }
            }
            return AuthorizationResult.Failure("No policies satisfied");
        }
    }
}

/// <summary>
/// Registry for managing authorization policies.
/// </summary>
public interface IPolicyRegistry
{
    /// <summary>
    /// Registers a policy.
    /// </summary>
    void RegisterPolicy(IAuthorizationPolicy policy);

    /// <summary>
    /// Gets a policy by name.
    /// </summary>
    IAuthorizationPolicy? GetPolicy(string name);

    /// <summary>
    /// Gets all registered policies.
    /// </summary>
    IReadOnlyList<IAuthorizationPolicy> GetAllPolicies();
}

/// <summary>
/// Default policy registry implementation.
/// </summary>
public class PolicyRegistry : IPolicyRegistry
{
    private readonly Dictionary<string, IAuthorizationPolicy> _policies = new();

    public void RegisterPolicy(IAuthorizationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        _policies[policy.Name] = policy;
    }

    public IAuthorizationPolicy? GetPolicy(string name)
    {
        _policies.TryGetValue(name, out var policy);
        return policy;
    }

    public IReadOnlyList<IAuthorizationPolicy> GetAllPolicies()
    {
        return _policies.Values.ToList();
    }
}
