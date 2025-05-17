using BulkCrapUninstaller.Functions;
using BulkCrapUninstaller.Properties;
using System;
using System.Collections.Generic;
using UninstallTools;

namespace BulkCrapUninstaller.Forms.Windows;

/// <summary>
/// Workaround for ..\..\source\BulkCrapUninstaller\Forms\Windows\MainWindow.cs
/// </summary>
public class MainWindow
{
    public static string ProgressFinishing => Localisable.Progress_Finishing;
    public static string ProgressFinishingIcons => Localisable.Progress_Finishing_Icons;

    private readonly AppUninstaller _appUninstaller;

    public MainWindow(Action listRefreshCallback)
    {
        _appUninstaller = new AppUninstaller(listRefreshCallback, b => { }, b => { });
    }

    public void RunLoudUninstall(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers,
        IEnumerable<ApplicationUninstallerEntry> allUninstallers)
    {
        _appUninstaller.RunUninstallW(selectedUninstallers, allUninstallers, false);
    }

    public void RunQuietUninstall(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers,
        IEnumerable<ApplicationUninstallerEntry> allUninstallers)
    {
        _appUninstaller.RunUninstallW(selectedUninstallers, allUninstallers, true);
    }

    public void RunModify(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers)
    {
        _appUninstaller.ModifyW(selectedUninstallers);
    }

    public void RunManualUninstall(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers,
        IEnumerable<ApplicationUninstallerEntry> allUninstallers)
    {
        _appUninstaller.AdvancedUninstallW(selectedUninstallers, allUninstallers);
    }
}
