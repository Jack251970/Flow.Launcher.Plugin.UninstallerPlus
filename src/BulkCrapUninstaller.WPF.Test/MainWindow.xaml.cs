using System.Diagnostics;
using System.Windows;
using UninstallTools;
using UninstallTools.Controls;
using UninstallTools.Factory;

namespace BulkCrapUninstaller.WPF.Test;

public partial class MainWindow : Window
{
    private readonly UninstallerIconGetter _iconGetter;

    private IList<ApplicationUninstallerEntry>? _allUninstallers;
    public IList<ApplicationUninstallerEntry> AllUninstallers
    {
        get => _allUninstallers ?? new List<ApplicationUninstallerEntry>();
        private set
        {
            _allUninstallers = value;
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        _iconGetter = new UninstallerIconGetter();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Task.Run(() => ListRefreshThread());
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _iconGetter?.Dispose();
    }

    private async Task ListRefreshThread(CancellationToken token = default)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ProgressBar.Visibility = Visibility.Visible;
            TextBlock.Visibility = Visibility.Visible;
            TextBlock.Text = string.Empty;
            SubProgressBar.Visibility = Visibility.Visible;
            SubTextBlock.Visibility = Visibility.Visible;
            SubTextBlock.Text = string.Empty;
        });

        var progressMax = 0;
        var uninstallerEntries = ApplicationUninstallerFactory.GetUninstallerEntries(async (x) =>
        {
            progressMax = x.TotalCount + 1;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProgressBar.Maximum = progressMax;
                ProgressBar.Value = x.CurrentCount;
                TextBlock.Text = x.Message;

                var inner = x.Inner;
                if (inner != null)
                {
                    SubProgressBar.Maximum = progressMax;
                    SubProgressBar.Value = x.CurrentCount;
                    SubTextBlock.Text = inner.Message;
                }
                else
                {
                    SubProgressBar.Maximum = -1;
                    SubProgressBar.Value = 0;
                    SubTextBlock.Text = string.Empty;
                }
            });

            token.ThrowIfCancellationRequested();
        });

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ProgressBar.Maximum = progressMax;
            TextBlock.Text = "Program loading finished";
            SubProgressBar.Maximum = 2;
            SubProgressBar.Value = 0;
            SubTextBlock.Text = string.Empty;
        });

        //if (!string.IsNullOrEmpty(Program.InstalledRegistryKeyName))
        //    uninstallerEntries.RemoveAll(
        //        x => PathTools.PathsEqual(x.RegistryKeyName, Program.InstalledRegistryKeyName));

        AllUninstallers = uninstallerEntries;

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SubProgressBar.Value = 1;
            SubTextBlock.Text = "Icon loading finished";
        });

        try
        {
            _iconGetter.UpdateIconList(AllUninstallers);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        /*await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ProgressBar.Visibility = Visibility.Hidden;
            TextBlock.Visibility = Visibility.Hidden;
            SubProgressBar.Visibility = Visibility.Hidden;
            SubTextBlock.Visibility = Visibility.Hidden;
        });*/
    }
}
