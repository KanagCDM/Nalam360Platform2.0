namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// Helper for debouncing rapid function calls
/// </summary>
public class DebounceHelper : IDisposable
{
    private CancellationTokenSource? _cts;
    private readonly int _delayMilliseconds;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the DebounceHelper class
    /// </summary>
    /// <param name="delayMilliseconds">Delay in milliseconds</param>
    public DebounceHelper(int delayMilliseconds = 300)
    {
        _delayMilliseconds = delayMilliseconds;
    }

    /// <summary>
    /// Debounces an action - cancels previous call if new call comes within delay
    /// </summary>
    /// <param name="action">The action to debounce</param>
    public async Task DebounceAsync(Func<Task> action)
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        try
        {
            await Task.Delay(_delayMilliseconds, _cts.Token);
            await action();
        }
        catch (TaskCanceledException)
        {
            // Expected when debounced
        }
    }

    /// <summary>
    /// Debounces an action (synchronous version)
    /// </summary>
    /// <param name="action">The action to debounce</param>
    public async Task DebounceAsync(Action action)
    {
        await DebounceAsync(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Disposes the debounce helper
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Helper for throttling function calls to maximum rate
/// </summary>
public class ThrottleHelper
{
    private DateTime _lastExecution = DateTime.MinValue;
    private readonly int _intervalMilliseconds;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the ThrottleHelper class
    /// </summary>
    /// <param name="intervalMilliseconds">Minimum interval between executions in milliseconds</param>
    public ThrottleHelper(int intervalMilliseconds = 1000)
    {
        _intervalMilliseconds = intervalMilliseconds;
    }

    /// <summary>
    /// Throttles an action - only executes if interval has passed since last execution
    /// </summary>
    /// <param name="action">The action to throttle</param>
    /// <returns>True if action was executed, false if throttled</returns>
    public async Task<bool> ThrottleAsync(Func<Task> action)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastExecution).TotalMilliseconds;

            if (elapsed < _intervalMilliseconds)
            {
                return false;
            }

            _lastExecution = now;
        }

        await action();
        return true;
    }

    /// <summary>
    /// Throttles an action (synchronous version)
    /// </summary>
    /// <param name="action">The action to throttle</param>
    /// <returns>True if action was executed, false if throttled</returns>
    public async Task<bool> ThrottleAsync(Action action)
    {
        return await ThrottleAsync(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Resets the throttle timer
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _lastExecution = DateTime.MinValue;
        }
    }
}
