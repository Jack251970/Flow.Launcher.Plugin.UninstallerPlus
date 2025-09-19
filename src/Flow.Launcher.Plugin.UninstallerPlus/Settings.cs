using Klocman;
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

    #region Uninstallation Settings

    // For Functions/MessageBoxes.cs
    private YesNoAsk _backupLeftovers = YesNoAsk.Ask;
    public YesNoAsk BackupLeftovers
    {
        get => _backupLeftovers;
        set
        {
            if (_backupLeftovers != value)
            {
                _backupLeftovers = value;
                OnPropertyChanged();
            }
        }
    }

    private YesNoAsk _messagesRemoveJunk = YesNoAsk.Ask;
    public YesNoAsk MessagesRemoveJunk
    {
        get => _messagesRemoveJunk;
        set
        {
            if (_messagesRemoveJunk != value)
            {
                _messagesRemoveJunk = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _messagesAskRemoveLoudItems = true;
    public bool MessagesAskRemoveLoudItems
    {
        get => _messagesAskRemoveLoudItems;
        set
        {
            if (_messagesAskRemoveLoudItems != value)
            {
                _messagesAskRemoveLoudItems = value;
                OnPropertyChanged();
            }
        }
    }

    private YesNoAsk _messagesRestorePoints = YesNoAsk.Ask;
    public YesNoAsk MessagesRestorePoints
    {
        get => _messagesRestorePoints;
        set
        {
            if (_messagesRestorePoints != value)
            {
                _messagesRestorePoints = value;
                OnPropertyChanged();
            }
        }
    }

    // For Functions/AppUninstaller.cs
    private bool _externalEnable = false;
    public bool ExternalEnable
    {
        get => _externalEnable;
        set
        {
            if (_externalEnable != value)
            {
                _externalEnable = value;
                OnPropertyChanged();
            }
        }
    }

    private string _externalPreCommands = string.Empty;
    public string ExternalPreCommands
    {
        get => _externalPreCommands;
        set
        {
            if (_externalPreCommands != value)
            {
                _externalPreCommands = value;
                OnPropertyChanged();
            }
        }
    }

    private string _externalPostCommands = "../BleachBit/bleachbit_console.exe --clean system.tmp system.logs system.memory_dump system.muicache system.prefetch system.recycle_bin";
    public string ExternalPostCommands
    {
        get => _externalPostCommands;
        set
        {
            if (_externalPostCommands != value)
            {
                _externalPostCommands = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _messagesShowAllBadJunk = false;
    public bool MessagesShowAllBadJunk
    {
        get => _messagesShowAllBadJunk;
        set
        {
            if (_messagesShowAllBadJunk != value)
            {
                _messagesShowAllBadJunk = value;
                OnPropertyChanged();
            }
        }
    }

    // For Forms/Windows/JunkRemoveWindow.cs
    private string _backupLeftoversDirectory = string.Empty;
    public string BackupLeftoversDirectory
    {
        get => _backupLeftoversDirectory;
        set
        {
            if (_backupLeftoversDirectory != value)
            {
                _backupLeftoversDirectory = value;
                OnPropertyChanged();
            }
        }
    }

    // For Controls/Settings/UninstallationSettings.cs
    private bool _uninstallPreventShutdown = true;
    public bool UninstallPreventShutdown
    {
        get => _uninstallPreventShutdown;
        set
        {
            if (_uninstallPreventShutdown != value)
            {
                _uninstallPreventShutdown = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _uninstallConcurrency = true;
    public bool UninstallConcurrency
    {
        get => _uninstallConcurrency;
        set
        {
            if (_uninstallConcurrency != value)
            {
                _uninstallConcurrency = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _uninstallConcurrentOneLoud = true;
    public bool UninstallConcurrentOneLoud
    {
        get => _uninstallConcurrentOneLoud;
        set
        {
            if (_uninstallConcurrentOneLoud != value)
            {
                _uninstallConcurrentOneLoud = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _uninstallConcurrentDisableManualCollisionProtection = false;
    public bool UninstallConcurrentDisableManualCollisionProtection
    {
        get => _uninstallConcurrentDisableManualCollisionProtection;
        set
        {
            if (_uninstallConcurrentDisableManualCollisionProtection != value)
            {
                _uninstallConcurrentDisableManualCollisionProtection = value;
                OnPropertyChanged();
            }
        }
    }

    private int _uninstallConcurrentMaxCount = 2;
    public int UninstallConcurrentMaxCount
    {
        get => _uninstallConcurrentMaxCount;
        set
        {
            if (_uninstallConcurrentMaxCount != value)
            {
                _uninstallConcurrentMaxCount = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _advancedIntelligentUninstallerSorting = true;
    public bool AdvancedIntelligentUninstallerSorting
    {
        get => _advancedIntelligentUninstallerSorting;
        set
        {
            if (_advancedIntelligentUninstallerSorting != value)
            {
                _advancedIntelligentUninstallerSorting = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _advancedDisableProtection = false;
    public bool AdvancedDisableProtection
    {
        get => _advancedDisableProtection;
        set
        {
            if (_advancedDisableProtection != value)
            {
                _advancedDisableProtection = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _advancedSimulate = false;
    public bool AdvancedSimulate
    {
        get => _advancedSimulate;
        set
        {
            if (_advancedSimulate != value)
            {
                _advancedSimulate = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _quietAutoKillStuck = true;
    public bool QuietAutoKillStuck
    {
        get => _quietAutoKillStuck;
        set
        {
            if (_quietAutoKillStuck != value)
            {
                _quietAutoKillStuck = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _quietRetryFailedOnce = true;
    public bool QuietRetryFailedOnce
    {
        get => _quietRetryFailedOnce;
        set
        {
            if (_quietRetryFailedOnce != value)
            {
                _quietRetryFailedOnce = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _quietAutomatization = true;
    public bool QuietAutomatization
    {
        get => _quietAutomatization;
        set
        {
            if (_quietAutomatization != value)
            {
                _quietAutomatization = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _quietAutomatizationKillStuck = true;
    public bool QuietAutomatizationKillStuck
    {
        get => _quietAutomatizationKillStuck;
        set
        {
            if (_quietAutomatizationKillStuck != value)
            {
                _quietAutomatizationKillStuck = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _quietUseDaemon = true;
    public bool QuietUseDaemon
    {
        get => _quietUseDaemon;
        set
        {
            if (_quietUseDaemon != value)
            {
                _quietUseDaemon = value;
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
