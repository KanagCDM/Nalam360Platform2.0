namespace Nalam360Enterprise.UI.Components.Healthcare;

/// <summary>
/// Represents the state of a video call
/// </summary>
public enum CallState
{
    /// <summary>
    /// No active call
    /// </summary>
    Idle,
    
    /// <summary>
    /// Call is being established
    /// </summary>
    Connecting,
    
    /// <summary>
    /// Call is active and connected
    /// </summary>
    Connected,
    
    /// <summary>
    /// Call was disconnected
    /// </summary>
    Disconnected
}
