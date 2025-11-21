namespace Nalam360Enterprise.UI.Core.State;

/// <summary>
/// State management service for sharing state across components
/// </summary>
/// <typeparam name="T">The type of state to manage</typeparam>
public interface IStateManager<T> where T : class
{
    /// <summary>
    /// Gets the current state
    /// </summary>
    T State { get; }
    
    /// <summary>
    /// Event triggered when state changes
    /// </summary>
    event Action? OnStateChanged;
    
    /// <summary>
    /// Updates the entire state
    /// </summary>
    /// <param name="newState">The new state</param>
    void UpdateState(T newState);
    
    /// <summary>
    /// Updates the state using an update action
    /// </summary>
    /// <param name="updateAction">Action to update the state</param>
    void UpdateState(Action<T> updateAction);
    
    /// <summary>
    /// Resets the state to its initial value
    /// </summary>
    void ResetState();
    
    /// <summary>
    /// Gets a snapshot of the current state
    /// </summary>
    /// <returns>A copy of the current state</returns>
    T GetSnapshot();
}
