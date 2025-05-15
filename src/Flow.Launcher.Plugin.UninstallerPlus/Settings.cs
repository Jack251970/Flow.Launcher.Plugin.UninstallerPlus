using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.UninstallerPlus;

public class Settings : BaseModel
{
    #region Filter Settings

    /// <summary>
    /// Show applications published by Microsoft
    /// </summary>
    private bool _filterShowMicrosoft = true;
    public bool FilterShowMicrosoft
    {
        get => _filterShowMicrosoft;
        set
        {
            if (_filterShowMicrosoft != value)
            {
                _filterShowMicrosoft = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show unregistered applications
    /// </summary>
    private bool _advancedDisplayOrphans = true;
    public bool AdvancedDisplayOrphans
    {
        get => _advancedDisplayOrphans;
        set
        {
            if (_advancedDisplayOrphans != value)
            {
                _advancedDisplayOrphans = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show system components
    /// </summary>
    private bool _filterShowSystemComponents = false;
    public bool FilterShowSystemComponents
    {
        get => _filterShowSystemComponents;
        set
        {
            if (_filterShowSystemComponents != value)
            {
                _filterShowSystemComponents = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show protected items
    /// </summary>
    private bool _filterShowProtected = false;
    public bool FilterShowProtected
    {
        get => _filterShowProtected;
        set
        {
            if (_filterShowProtected != value)
            {
                _filterShowProtected = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show tweaks
    /// </summary>
    private bool _filterShowTweaks = true;
    public bool FilterShowTweaks
    {
        get => _filterShowTweaks;
        set
        {
            if (_filterShowTweaks != value)
            {
                _filterShowTweaks = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show updates
    /// </summary>
    private bool _filterShowUpdates = false;
    public bool FilterShowUpdates
    {
        get => _filterShowUpdates;
        set
        {
            if (_filterShowUpdates != value)
            {
                _filterShowUpdates = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show Windows features
    /// </summary>
    private bool _filterShowWinFeatures = false;
    public bool FilterShowWinFeatures
    {
        get => _filterShowWinFeatures;
        set
        {
            if (_filterShowWinFeatures != value)
            {
                _filterShowWinFeatures = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show Windows Store apps
    /// </summary>
    private bool _filterShowStoreApps = true;
    public bool FilterShowStoreApps
    {
        get => _filterShowStoreApps;
        set
        {
            if (_filterShowStoreApps != value)
            {
                _filterShowStoreApps = value;
                OnPropertyChanged();
            }
        }
    }

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
