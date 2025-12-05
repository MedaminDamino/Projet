namespace BookDashboardBlazor.Core.Services.State;

/// <summary>
/// Centralized state management service for the application.
/// </summary>
public class AppStateService
{
    private readonly Dictionary<string, object> _state = new();
    
    /// <summary>
    /// Event raised when state changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Retrieves state value by key.
    /// </summary>
    public T? GetState<T>(string key)
    {
        if (_state.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default;
    }

    /// <summary>
    /// Sets state value and notifies subscribers.
    /// </summary>
    public void SetState<T>(string key, T value)
    {
        if (value == null)
        {
            _state.Remove(key);
        }
        else
        {
            _state[key] = value;
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears all state.
    /// </summary>
    public void ClearState()
    {
        _state.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
