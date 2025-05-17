using Klocman;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace BulkCrapUninstaller.Properties;

/// <summary>
/// ..\..\source\BulkCrapUninstaller\Properties\Settings.Designer.cs
/// </summary>
public sealed partial class Settings : ApplicationSettingsBase
{
    private static Settings defaultInstance;
    public static Settings Default => defaultInstance ??= new();

    // Helper Methods
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

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

    private bool _createRestorePoint = true;
    public bool CreateRestorePoint
    {
        get => _createRestorePoint;
        set
        {
            if (_createRestorePoint != value)
            {
                _createRestorePoint = value;
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
}
