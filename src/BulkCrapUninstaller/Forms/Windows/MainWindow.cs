using BulkCrapUninstaller.Functions;
using BulkCrapUninstaller.Properties;
using Klocman.Forms.Tools;
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

    public MainWindow(Action listRefreshCallback, Action<Exception> sendErrorAction)
    {
        _appUninstaller = new AppUninstaller(listRefreshCallback, b => { }, b => { });
        PremadeDialogs.SendErrorAction = sendErrorAction;
    }

    public void RunLoudUninstall(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers,
        IEnumerable<ApplicationUninstallerEntry> allUninstallers, IFlowPublicAPI api = null)
    {
        _appUninstaller.RunUninstallW(selectedUninstallers, allUninstallers, false, api);
    }

    public void RunQuietUninstall(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers,
        IEnumerable<ApplicationUninstallerEntry> allUninstallers, IFlowPublicAPI api = null)
    {
        _appUninstaller.RunUninstallW(selectedUninstallers, allUninstallers, true, api);
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
