using UnityEngine;

/// <summary>
/// Persistent debug controller. Add to a GameObject in your scene.
/// Provides a single toggle for all debug logs with automatic build exclusion.
/// </summary>
public class DebugController : MonoBehaviour
{
    private static DebugController _instance;
    
    [Header("Debug Settings")]
    [SerializeField] 
    [Tooltip("Enable/disable all debug logs in the game")]
    private bool _enableDebugLogs = true;
    
    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Replacement for Debug.Log - only logs if debug is enabled and not in release build
    /// </summary>
    public static void Log(string message)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_instance != null && _instance._enableDebugLogs)
        {
            Debug.Log(message);
        }
        #endif
    }
    
    /// <summary>
    /// Replacement for Debug.LogWarning - only logs if debug is enabled and not in release build
    /// </summary>
    public static void LogWarning(string message)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_instance != null && _instance._enableDebugLogs)
        {
            Debug.LogWarning(message);
        }
        #endif
    }
    
    /// <summary>
    /// Replacement for Debug.LogError - always logs (errors should always be visible)
    /// </summary>
    public static void LogError(string message)
    {
        Debug.LogError(message);
    }
    
    /// <summary>
    /// Runtime toggle for debug logs
    /// </summary>
    public static void SetDebugEnabled(bool enabled)
    {
        if (_instance != null)
        {
            _instance._enableDebugLogs = enabled;
        }
    }
}
