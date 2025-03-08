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

    // For Functions/MessageBoxes.cs
    public YesNoAsk BackupLeftovers { get; set; } = YesNoAsk.Ask;
    public YesNoAsk MessagesRemoveJunk { get; set; } = YesNoAsk.Ask;
    public bool MessagesAskRemoveLoudItems { get; set; } = true;
    public YesNoAsk MessagesRestorePoints { get; set; } = YesNoAsk.Ask;

    // For Functions/AppUninstaller.cs
    public bool AdvancedDisableProtection { get; set; } = false;
    public bool CreateRestorePoint { get; set; } = true;
    public bool ExternalEnable { get; set; } = false;
    public string ExternalPreCommands { get; set; } = string.Empty;
    public bool UninstallConcurrentOneLoud { get; set; } = true;
    public bool UninstallConcurrency { get; set; } = true;
    public int UninstallConcurrentMaxCount { get; set; } = 0;
    public string ExternalPostCommands { get; set; } = string.Empty;
    public bool MessagesShowAllBadJunk { get; set; } = false;
    public bool AdvancedSimulate { get; set; } = false;
    public bool QuietAutoKillStuck { get; set; } = true;
    public bool QuietRetryFailedOnce { get; set; } = true;

    // For Forms/Wizards/BeginUninstallTaskWizard.cs
    public bool AdvancedIntelligentUninstallerSorting { get; set; } = true;

    // For Forms/Windows/JunkRemoveWindow.cs
    public string BackupLeftoversDirectory { get; set; } = string.Empty;

    // For Controls/Settings/UninstallationSettings.cs
    public bool UninstallPreventShutdown { get; set; } = true;
    public bool UninstallConcurrentDisableManualCollisionProtection { get; set; } = false;
    public bool QuietAutomatization { get; set; } = true;
    public bool QuietAutomatizationKillStuck { get; set; } = true;
    public bool QuietUseDaemon { get; set; } = true;
}
