namespace BulkCrapUninstaller.WPF.Test;

public class Settings
{
    public bool FilterShowMicrosoft { get; set; } = true;

    public bool AdvancedDisplayOrphans { get; set; } = true;

    public bool FilterShowSystemComponents { get; set; } = false;

    public bool FilterShowProtected { get; set; } = false;

    public bool FilterShowTweaks { get; set; } = true;

    public bool FilterShowUpdates { get; set; } = false;

    public bool FilterShowWinFeatures { get; set; } = false;

    public bool FilterShowStoreApps { get; set; } = true;
}
