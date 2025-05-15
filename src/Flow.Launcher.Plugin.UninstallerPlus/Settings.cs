using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.UninstallerPlus;

public class Settings
{
    #region Filter Settings

    /// <summary>
    /// Show applications published by Microsoft
    /// </summary>
    public bool FilterShowMicrosoft { get; set; } = true;

    /// <summary>
    /// Show unregistered applications
    /// </summary>
    public bool AdvancedDisplayOrphans { get; set; } = true;

    /// <summary>
    /// Show system components
    /// </summary>
    public bool FilterShowSystemComponents { get; set; } = false;

    /// <summary>
    /// Show protected items
    /// </summary>
    public bool FilterShowProtected { get; set; } = false;

    /// <summary>
    /// Show tweaks
    /// </summary>
    public bool FilterShowTweaks { get; set; } = true;

    /// <summary>
    /// Show updates
    /// </summary>
    public bool FilterShowUpdates { get; set; } = false;

    /// <summary>
    /// Show Windows features
    /// </summary>
    public bool FilterShowWinFeatures { get; set; } = false;

    /// <summary>
    /// Show Windows Store apps
    /// </summary>
    public bool FilterShowStoreApps { get; set; } = true;

    #endregion

    public void RestoreToDefault()
    {
        var defaultSettings = new Settings();
        var type = GetType();
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (CheckJsonIgnoredOrKeyAttribute(prop))
            {
                continue;
            }
            var defaultValue = prop.GetValue(defaultSettings);
            prop.SetValue(this, defaultValue);
        }
    }

    public override string ToString()
    {
        var type = GetType();
        var props = type.GetProperties();
        var s = props.Aggregate(
            "Settings(\n",
            (current, prop) =>
            {
                if (CheckJsonIgnoredOrKeyAttribute(prop))
                {
                    return current;
                }
                return current + $"\t{prop.Name}: {prop.GetValue(this)}\n";
            }
        );
        s += ")";
        return s;
    }

    private static bool CheckJsonIgnoredOrKeyAttribute(PropertyInfo prop)
    {
        return
            // JsonIgnored
            prop.GetCustomAttribute<JsonIgnoreAttribute>() != null;
    }
}
