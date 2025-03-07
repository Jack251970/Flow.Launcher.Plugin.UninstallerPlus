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

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const string FlowLauncherDisplayName = "Flow Launcher";
    private const string FlowLauncherPublisher = "Flow-Launcher Team";

    private const string MicrosoftPublisher = "Microsoft";
    private const string TweakRateId = "tweak";

    private readonly Settings _settings = new();
    
    public MainWindow()
    {
        InitializeComponent();
        _iconGetter = new UninstallerIconGetter();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            Task.Run(() => ListRefreshThread(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Ignored - token cancelled
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to refresh list: {ex.Message}");
        }
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

        // Remove Flow Launcher from the list so that it doesn't show up in the UI
        uninstallerEntries.RemoveAll(x =>
            (x.DisplayName == FlowLauncherDisplayName || x.DisplayNameTrimmed == FlowLauncherDisplayName) &&
            (x.Publisher == FlowLauncherPublisher || x.PublisherTrimmed == FlowLauncherPublisher));

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
        catch (TaskCanceledException)
        {
            throw new TaskCanceledException("Icon loading cancelled");
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

        _ = UpdateTextAsync();
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

    private async Task UpdateTextAsync()
    {
        var uninstallerListText = await Task.Run(() =>
        {
            var stringBuilder = new StringBuilder();
            foreach (var uninstaller in AllUninstallers)
            {
                if (ListViewFilter(uninstaller))
                {
                    stringBuilder.AppendLine($"Name: {uninstaller.DisplayName} Publisher: {uninstaller.Publisher}");
                }
            }
            return stringBuilder.ToString();
        }).ConfigureAwait(false);

        await InvokeAsync(() =>
        {
            UninstallerListTextBlock.Text = uninstallerListText;
        }, DispatcherPriority.Normal, _cancellationTokenSource.Token);
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
}
