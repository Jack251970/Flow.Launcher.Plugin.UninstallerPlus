using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
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

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public MainWindow()
    {
        InitializeComponent();
        _iconGetter = new UninstallerIconGetter();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            try
            {
                await ListRefreshThread(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to refresh list: {ex.Message}");
            }
        }, _cancellationTokenSource.Token);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Dispose();
        _iconGetter?.Dispose();
    }

    private async Task ListRefreshThread(CancellationToken token = default)
    {
        await InvokeAsync(() =>
        {
            ProgressBar.Visibility = Visibility.Visible;
            TextBlock.Visibility = Visibility.Visible;
            TextBlock.Text = string.Empty;
            SubProgressBar.Visibility = Visibility.Visible;
            SubTextBlock.Visibility = Visibility.Visible;
            SubTextBlock.Text = string.Empty;
        }, DispatcherPriority.Normal, token);

        var progressMax = 0;
        var uninstallerEntries = ApplicationUninstallerFactory.GetUninstallerEntries(async (x) =>
        {
            progressMax = x.TotalCount + 1;

            await InvokeAsync(() =>
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
            }, DispatcherPriority.Normal, token);
        }, token);

        await InvokeAsync(() =>
        {
            ProgressBar.Maximum = progressMax;
            TextBlock.Text = "Program loading finished";
            SubProgressBar.Maximum = 2;
            SubProgressBar.Value = 0;
            SubTextBlock.Text = string.Empty;
        }, DispatcherPriority.Normal, token);

        //if (!string.IsNullOrEmpty(Program.InstalledRegistryKeyName))
        //    uninstallerEntries.RemoveAll(
        //        x => PathTools.PathsEqual(x.RegistryKeyName, Program.InstalledRegistryKeyName));

        AllUninstallers = uninstallerEntries;

        await InvokeAsync(() =>
        {
            SubProgressBar.Value = 1;
            SubTextBlock.Text = "Icon loading finished";
        }, DispatcherPriority.Normal, token);

        try
        {
            _iconGetter.UpdateIconList(AllUninstallers, token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        await InvokeAsync(() =>
        {
            ProgressBar.Visibility = Visibility.Hidden;
            TextBlock.Visibility = Visibility.Hidden;
            SubProgressBar.Visibility = Visibility.Hidden;
            SubTextBlock.Visibility = Visibility.Hidden;
        }, DispatcherPriority.Normal, token);
    }

    private static async Task<bool> InvokeAsync(Action function, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken token = default)
    {
        try
        {
            // If the application is exitting, Application.Current will be null
            if (Application.Current?.Dispatcher is not null)
            {
                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(function, priority, token);
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message is not "Failed to enqueue the operation")
                    {
                        throw;
                    }
                }
            }

            return true;
        }
        catch (TaskCanceledException)
        {
            // Ignored - token is cancelled
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to invoke async: {ex.Message}");
            return false;
        }
    }
}
