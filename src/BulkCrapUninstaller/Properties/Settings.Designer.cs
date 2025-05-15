using Klocman;
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
    public YesNoAsk BackupLeftovers { get; set; } = YesNoAsk.Ask;
    public YesNoAsk MessagesRemoveJunk { get; set; } = YesNoAsk.Ask;
    public bool MessagesAskRemoveLoudItems { get; set; } = true;
    public YesNoAsk MessagesRestorePoints { get; set; } = YesNoAsk.Ask;

    // For Functions/AppUninstaller.cs
    public bool ExternalEnable { get; set; } = false;
    public string ExternalPreCommands { get; set; } = string.Empty;
    public string ExternalPostCommands { get; set; } = string.Empty;
    public bool MessagesShowAllBadJunk { get; set; } = false;

    // For Forms/Windows/JunkRemoveWindow.cs
    public string BackupLeftoversDirectory { get; set; } = string.Empty;

    // For Controls/Settings/UninstallationSettings.cs
    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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

    [global::System.Configuration.UserScopedSettingAttribute()]
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
