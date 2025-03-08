using BulkCrapUninstaller.Properties;
using Klocman.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
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

    private readonly List<ApplicationUninstallerEntry> FilteredUninstallers = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const string FlowLauncherDisplayName = "Flow Launcher";
    private const string FlowLauncherPublisher = "Flow-Launcher Team";

    private const string MicrosoftPublisher = "Microsoft";
    private const string TweakRateId = "tweak";

    private readonly Settings _settings = Settings.Default;

    private readonly SemaphoreSlim _queryUpdateSemaphore = new(1, 1);

    private readonly Forms.Windows.MainWindow _mainWindow;

    public MainWindow()
    {
        InitializeComponent();
        _iconGetter = new UninstallerIconGetter();
        _mainWindow = new Forms.Windows.MainWindow(InitiateListRefresh);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        InitiateListRefresh();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _queryUpdateSemaphore?.Dispose();
        _cancellationTokenSource?.Dispose();
        _iconGetter?.Dispose();
    }

    private void InitiateListRefresh()
    {
        try
        {
            Task.Run(() => ListRefreshThread(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignored - token cancelled
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to refresh list: {ex.Message}");
        }
    }

    private async Task ListRefreshThread(CancellationToken token)
    {
        await _queryUpdateSemaphore.WaitAsync(token).ConfigureAwait(false);

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
            TextBlock.Text = Forms.Windows.MainWindow.ProgressFinishing;
            SubProgressBar.Maximum = 2;
            SubProgressBar.Value = 0;
            SubTextBlock.Text = string.Empty;
        }, DispatcherPriority.Normal, token);

        // Remove Flow Launcher from the list so that it doesn't show up in the UI
        uninstallerEntries.RemoveAll(x =>
            (x.DisplayName == FlowLauncherDisplayName || x.DisplayNameTrimmed == FlowLauncherDisplayName) &&
            (x.Publisher == FlowLauncherPublisher || x.PublisherTrimmed == FlowLauncherPublisher));

        AllUninstallers = uninstallerEntries;

        await InvokeAsync(() =>
        {
            SubProgressBar.Value = 1;
            SubTextBlock.Text = Forms.Windows.MainWindow.ProgressFinishingIcons;
        }, DispatcherPriority.Normal, token);

        try
        {
            _iconGetter.UpdateIconList(AllUninstallers, token);
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException("Icon loading cancelled");
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

        _queryUpdateSemaphore.Release();

        _ = UpdateTextAsync(token);
    }

    private static async Task InvokeAsync(Action function, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken token = default)
    {
        // If the application is exitting, Application.Current will be null
        if (Application.Current?.Dispatcher is not null)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(function, priority, token);
            }
            catch (TaskCanceledException)
            {
                // Ignored - token cancelled
            }
            catch (InvalidOperationException ex) when (ex.Message is "Failed to enqueue the operation")
            {
                // Ignored - enqueue failed
            }
        }
        else
        {
            // Ignored - application exitted
        }
    }

    private async Task UpdateTextAsync(CancellationToken token)
    {
        await _queryUpdateSemaphore.WaitAsync(token);

        var uninstallerListText = await Task.Run(() =>
        {
            FilteredUninstallers.Clear();

            foreach (var uninstaller in AllUninstallers)
            {
                if (ListViewFilter(uninstaller))
                {
                    FilteredUninstallers.Add(uninstaller);
                }
            }

            var stringBuilder = new StringBuilder();
            foreach (var uninstaller in FilteredUninstallers)
            {
                stringBuilder.AppendLine($"{uninstaller.DisplayName} By {uninstaller.Publisher}");
            }
            return stringBuilder.ToString();
        }).ConfigureAwait(false);

        await InvokeAsync(() =>
        {
            UninstallerListTextBlock.Text = uninstallerListText;
        }, DispatcherPriority.Normal, _cancellationTokenSource.Token);

        _queryUpdateSemaphore.Release();
    }

    private bool ListViewFilter(ApplicationUninstallerEntry entry)
    {
        if (!_settings.FilterShowMicrosoft && !string.IsNullOrEmpty(entry.Publisher) && entry.Publisher.Contains(MicrosoftPublisher))
        {
            return false;
        }

        if (!_settings.AdvancedDisplayOrphans && entry.IsOrphaned)
        {
            return false;
        }

        if (!_settings.FilterShowSystemComponents && entry.SystemComponent)
        {
            return false;
        }

        if (!_settings.FilterShowProtected && entry.IsProtected)
        {
            return false;
        }

        if (entry.RatingId != null && !_settings.FilterShowTweaks && entry.RatingId.StartsWith(TweakRateId, StringComparison.Ordinal))
        {
            return false;
        }

        if (!_settings.FilterShowUpdates && entry.IsUpdate)
        {
            return false;
        }

        if (!_settings.FilterShowWinFeatures && entry.UninstallerKind == UninstallerType.WindowsFeature)
        {
            return false;
        }

        if (!_settings.FilterShowStoreApps && entry.UninstallerKind == UninstallerType.StoreApp)
        {
            return false;
        }

        return true;
    }

    private void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        var uninstallText = UninstallTextBox.Text;
        if (!string.IsNullOrEmpty(uninstallText))
        {
            foreach (var uninstaller in FilteredUninstallers)
            {
                if (uninstaller.DisplayName.ToLower().Contains(uninstallText.ToLower()))
                {
                    if (MessageBox.Show($"Do you want to uninstall {uninstaller.DisplayName} by {uninstaller.Publisher}?", 
                        "Uninstall", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        _mainWindow.RunLoudUninstall(new[] { uninstaller }, AllUninstallers);
                    }
                }
            }
        }
    }
}
