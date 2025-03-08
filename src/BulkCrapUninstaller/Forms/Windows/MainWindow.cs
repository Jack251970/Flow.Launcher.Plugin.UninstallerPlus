using BulkCrapUninstaller.Functions;
using BulkCrapUninstaller.Properties;
using System;

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

    public void RunLoudUninstall()
    {
        // MYTODO
        //_appUninstaller.RunUninstall(_listView.SelectedUninstallers, _listView.AllUninstallers, false);
    }

    public void RunQuietUninstall()
    {
        // MYTODO
        //_appUninstaller.RunUninstall(_listView.SelectedUninstallers, _listView.AllUninstallers, true);
    }
}
