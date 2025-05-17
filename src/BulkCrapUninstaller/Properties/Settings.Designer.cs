using Klocman;
using System.ComponentModel;
using System.Configuration;

namespace BulkCrapUninstaller.Properties;

/// <summary>
/// ..\..\source\BulkCrapUninstaller\Properties\Settings.Designer.cs
/// </summary>
public sealed partial class Settings : ApplicationSettingsBase
{
    private static Settings defaultInstance;
    public static Settings Default => defaultInstance ??= new();

    // For Functions/MessageBoxes.cs
    [global::System.Configuration.DefaultSettingValueAttribute("Ask")]
    public global::Klocman.YesNoAsk BackupLeftovers
    {
        get
        {
            return ((global::Klocman.YesNoAsk)(this["BackupLeftovers"]));
        }
        set
        {
            this["BackupLeftovers"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("Ask")]
    public global::Klocman.YesNoAsk MessagesRemoveJunk
    {
        get
        {
            return ((global::Klocman.YesNoAsk)(this["MessagesRemoveJunk"]));
        }
        set
        {
            this["MessagesRemoveJunk"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool MessagesAskRemoveLoudItems
    {
        get
        {
            return ((bool)(this["MessagesAskRemoveLoudItems"]));
        }
        set
        {
            this["MessagesAskRemoveLoudItems"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("Ask")]
    public global::Klocman.YesNoAsk MessagesRestorePoints
    {
        get
        {
            return ((global::Klocman.YesNoAsk)(this["MessagesRestorePoints"]));
        }
        set
        {
            this["MessagesRestorePoints"] = value;
        }
    }

    // For Functions/AppUninstaller.cs
    [global::System.Configuration.DefaultSettingValueAttribute("False")]
    public bool ExternalEnable
    {
        get
        {
            return ((bool)(this["ExternalEnable"]));
        }
        set
        {
            this["ExternalEnable"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("")]
    public string ExternalPreCommands
    {
        get
        {
            return ((string)(this["ExternalPreCommands"]));
        }
        set
        {
            this["ExternalPreCommands"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("../BleachBit/bleachbit_console.exe --clean system.tmp system.logs system.memory_d" +
            "ump system.muicache system.prefetch system.recycle_bin")]
    public string ExternalPostCommands
    {
        get
        {
            return ((string)(this["ExternalPostCommands"]));
        }
        set
        {
            this["ExternalPostCommands"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("False")]
    public bool MessagesShowAllBadJunk
    {
        get
        {
            return ((bool)(this["MessagesShowAllBadJunk"]));
        }
        set
        {
            this["MessagesShowAllBadJunk"] = value;
        }
    }

    // For Forms/Windows/JunkRemoveWindow.cs
    public string BackupLeftoversDirectory { get; set; } = string.Empty;

    // For Controls/Settings/UninstallationSettings.cs
    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool UninstallPreventShutdown
    {
        get
        {
            return ((bool)(this["UninstallPreventShutdown"]));
        }
        set
        {
            this["UninstallPreventShutdown"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool CreateRestorePoint
    {
        get
        {
            return ((bool)(this["CreateRestorePoint"]));
        }
        set
        {
            this["CreateRestorePoint"] = value;
        }
    }
    
    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool UninstallConcurrency
    {
        get
        {
            return ((bool)(this["UninstallConcurrency"]));
        }
        set
        {
            this["UninstallConcurrency"] = value;
        }
    }
    
    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool UninstallConcurrentOneLoud
    {
        get
        {
            return ((bool)(this["UninstallConcurrentOneLoud"]));
        }
        set
        {
            this["UninstallConcurrentOneLoud"] = value;
        }
    }
    
    [global::System.Configuration.DefaultSettingValueAttribute("False")]
    public bool UninstallConcurrentDisableManualCollisionProtection
    {
        get
        {
            return ((bool)(this["UninstallConcurrentDisableManualCollisionProtection"]));
        }
        set
        {
            this["UninstallConcurrentDisableManualCollisionProtection"] = value;
        }
    }
    
    [global::System.Configuration.DefaultSettingValueAttribute("2")]
    public int UninstallConcurrentMaxCount
    {
        get
        {
            return ((int)(this["UninstallConcurrentMaxCount"]));
        }
        set
        {
            this["UninstallConcurrentMaxCount"] = value;
        }
    }
    
    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool AdvancedIntelligentUninstallerSorting
    {
        get
        {
            return ((bool)(this["AdvancedIntelligentUninstallerSorting"]));
        }
        set
        {
            this["AdvancedIntelligentUninstallerSorting"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("False")]
    public bool AdvancedDisableProtection
    {
        get
        {
            return ((bool)(this["AdvancedDisableProtection"]));
        }
        set
        {
            this["AdvancedDisableProtection"] = value;
        }
    }
    
    [global::System.Configuration.DefaultSettingValueAttribute("False")]
    public bool AdvancedSimulate
    {
        get
        {
            return ((bool)(this["AdvancedSimulate"]));
        }
        set
        {
            this["AdvancedSimulate"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool QuietAutoKillStuck
    {
        get
        {
            return ((bool)(this["QuietAutoKillStuck"]));
        }
        set
        {
            this["QuietAutoKillStuck"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool QuietRetryFailedOnce
    {
        get
        {
            return ((bool)(this["QuietRetryFailedOnce"]));
        }
        set
        {
            this["QuietRetryFailedOnce"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool QuietAutomatization
    {
        get
        {
            return ((bool)(this["QuietAutomatization"]));
        }
        set
        {
            this["QuietAutomatization"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool QuietAutomatizationKillStuck
    {
        get
        {
            return ((bool)(this["QuietAutomatizationKillStuck"]));
        }
        set
        {
            this["QuietAutomatizationKillStuck"] = value;
        }
    }

    [global::System.Configuration.DefaultSettingValueAttribute("True")]
    public bool QuietUseDaemon
    {
        get
        {
            return ((bool)(this["QuietUseDaemon"]));
        }
        set
        {
            this["QuietUseDaemon"] = value;
        }
    }
}
