using System.Text.Json;

namespace Nalam360Enterprise.UI.Core.State;

/// <summary>
/// Default implementation of IStateManager for component state management
/// </summary>
/// <typeparam name="T">The type of state to manage</typeparam>
public class StateManager<T> : IStateManager<T> where T : class
{
    private T _state;
    private readonly T _initialState;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the StateManager class
    /// </summary>
    /// <param name="initialState">The initial state</param>
    public StateManager(T initialState)
    {
        _initialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
        _state = CloneState(initialState);
    }

    /// <inheritdoc/>
    public T State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    /// <inheritdoc/>
    public event Action? OnStateChanged;

    /// <inheritdoc/>
    public void UpdateState(T newState)
    {
        if (newState == null)
        {
            throw new ArgumentNullException(nameof(newState));
        }

        lock (_lock)
        {
            _state = newState;
        }

        NotifyStateChanged();
    }

    /// <inheritdoc/>
    public void UpdateState(Action<T> updateAction)
    {
        if (updateAction == null)
        {
            throw new ArgumentNullException(nameof(updateAction));
        }

        lock (_lock)
        {
            updateAction(_state);
        }

        NotifyStateChanged();
    }

    /// <inheritdoc/>
    public void ResetState()
    {
        lock (_lock)
        {
            _state = CloneState(_initialState);
        }

        NotifyStateChanged();
    }

    /// <inheritdoc/>
    public T GetSnapshot()
    {
        lock (_lock)
        {
            return CloneState(_state);
        }
    }

    /// <summary>
    /// Notifies subscribers that the state has changed
    /// </summary>
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Creates a deep clone of the state using JSON serialization
    /// </summary>
    /// <param name="state">The state to clone</param>
    /// <returns>A cloned copy of the state</returns>
    private T CloneState(T state)
    {
        try
        {
            var json = JsonSerializer.Serialize(state);
            return JsonSerializer.Deserialize<T>(json) ?? state;
        }
        catch
        {
            // If serialization fails, return the original state
            // This handles non-serializable types
            return state;
        }
    }
}

/// <summary>
/// Factory for creating StateManager instances
/// </summary>
public static class StateManagerFactory
{
    /// <summary>
    /// Creates a new StateManager with the specified initial state
    /// </summary>
    /// <typeparam name="T">The type of state to manage</typeparam>
    /// <param name="initialState">The initial state</param>
    /// <returns>A new StateManager instance</returns>
    public static IStateManager<T> Create<T>(T initialState) where T : class
    {
        return new StateManager<T>(initialState);
    }

    /// <summary>
    /// Creates a new StateManager with a default initial state
    /// </summary>
    /// <typeparam name="T">The type of state to manage</typeparam>
    /// <returns>A new StateManager instance</returns>
    public static IStateManager<T> Create<T>() where T : class, new()
    {
        return new StateManager<T>(new T());
    }
}
