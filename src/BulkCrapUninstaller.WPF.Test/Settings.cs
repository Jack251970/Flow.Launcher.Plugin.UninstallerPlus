namespace BulkCrapUninstaller.WPF.Test;

public class Settings
{
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
}
