using BulkCrapUninstaller.Functions;
using BulkCrapUninstaller.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
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
        _appUninstaller.RunUninstall(selectedUninstallers, allUninstallers, false);
    }

    public void RunQuietUninstall(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers,
        IEnumerable<ApplicationUninstallerEntry> allUninstallers)
    {
        _appUninstaller.RunUninstall(selectedUninstallers, allUninstallers, true);
    }

    public void RunModify(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers)
    {
        _appUninstaller.Modify(selectedUninstallers);
    }

    public void RunManualUninstall(IEnumerable<ApplicationUninstallerEntry> selectedUninstallers,
        IEnumerable<ApplicationUninstallerEntry> allUninstallers)
    {
        var items = selectedUninstallers.ToArray();
        var protectedItems = items.Where(x => x.IsProtected).ToArray();

        if (!Settings.Default.AdvancedDisableProtection && protectedItems.Any())
        {
            var affectedKeyNames = protectedItems.Select(x => x.DisplayName).ToArray();
            if (MessageBoxes.ProtectedItemsWarningQuestion(affectedKeyNames) == MessageBoxes.PressedButton.Cancel)
                return;

            items = selectedUninstallers.Where(x => !x.IsProtected).ToArray();
        }

        if (!items.Any())
        {
            MessageBoxes.NoUninstallersSelectedInfo();
            return;
        }

        _appUninstaller.AdvancedUninstall(items, allUninstallers);
    }
}
