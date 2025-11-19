using Microsoft.Maui.Storage;

namespace SweetShopMa.Services;

/// <summary>
/// Manages feature flags for enabling/disabling modules
/// </summary>
public static class FeatureFlags
{
    private const string AttendanceTrackerKey = "Feature_AttendanceTracker";
    
    /// <summary>
    /// Gets or sets whether the Attendance Tracker module is enabled
    /// </summary>
    public static bool IsAttendanceTrackerEnabled
    {
        get => Preferences.Get(AttendanceTrackerKey, true); // Default to enabled
        set => Preferences.Set(AttendanceTrackerKey, value);
    }
}

