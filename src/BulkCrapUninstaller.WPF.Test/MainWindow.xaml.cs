using Klocman.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UninstallTools;
using UninstallTools.Controls;
using UninstallTools.Factory;
using BCUSettings = BulkCrapUninstaller.Properties.Settings;

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

    private bool UninstallersInitialized => _allUninstallers != null && _allUninstallers.Count > 0;

    private List<ApplicationUninstallerEntry> FilteredUninstallers = null!;

    private CancellationTokenSource? _refreshSource;
    private CancellationToken _refreshToken;

    private const string FlowLauncherDisplayName = "Flow Launcher";
    private const string FlowLauncherPublisher = "Flow-Launcher Team";

    private const string MicrosoftPublisher = "Microsoft";
    private const string TweakRateId = "tweak";

    private static readonly Settings Settings = new();
    private static readonly BCUSettings _bCUSettings = BCUSettings.Default;

    private const int QueryUpdateSemaphoreMaxCount = 1;
    private readonly SemaphoreSlim _queryUpdateSemaphore = new(1, QueryUpdateSemaphoreMaxCount);

    private readonly Forms.Windows.MainWindow _mainWindow;

    public MainWindow()
    {
        InitializeComponent();
        _iconGetter = new UninstallerIconGetter();
        _mainWindow = new Forms.Windows.MainWindow(InitiateListRefresh, (e) => { Debugger.Break(); });
        UninstallToolsGlobalConfig.Initialize();

        Settings.BackupLeftovers = Klocman.YesNoAsk.Yes;
        Settings.PropertyChanged += Settings_PropertyChanged;

        InitBCUSettings();
        
        _bCUSettings.PropertyChanged += BCUSettings_PropertyChanged;
    }

    private static void InitBCUSettings()
    {
        _bCUSettings.BackupLeftovers = Settings.BackupLeftovers;
        _bCUSettings.MessagesRemoveJunk = Settings.MessagesRemoveJunk;
        _bCUSettings.MessagesAskRemoveLoudItems = Settings.MessagesAskRemoveLoudItems;
        _bCUSettings.MessagesRestorePoints = Settings.MessagesRestorePoints;
        _bCUSettings.ExternalEnable = Settings.ExternalEnable;
        _bCUSettings.ExternalPreCommands = Settings.ExternalPreCommands;
        _bCUSettings.ExternalPostCommands = Settings.ExternalPostCommands;
        _bCUSettings.MessagesShowAllBadJunk = Settings.MessagesShowAllBadJunk;
        _bCUSettings.BackupLeftoversDirectory = Settings.BackupLeftoversDirectory;
        _bCUSettings.UninstallPreventShutdown = Settings.UninstallPreventShutdown;
        _bCUSettings.CreateRestorePoint = Settings.CreateRestorePoint;
        _bCUSettings.UninstallConcurrency = Settings.UninstallConcurrency;
        _bCUSettings.UninstallConcurrentOneLoud = Settings.UninstallConcurrentOneLoud;
        _bCUSettings.UninstallConcurrentDisableManualCollisionProtection = Settings.UninstallConcurrentDisableManualCollisionProtection;
        _bCUSettings.UninstallConcurrentMaxCount = Settings.UninstallConcurrentMaxCount;
        _bCUSettings.AdvancedIntelligentUninstallerSorting = Settings.AdvancedIntelligentUninstallerSorting;
        _bCUSettings.AdvancedDisableProtection = Settings.AdvancedDisableProtection;
        _bCUSettings.AdvancedSimulate = Settings.AdvancedSimulate;
        _bCUSettings.QuietAutoKillStuck = Settings.QuietAutoKillStuck;
        _bCUSettings.QuietRetryFailedOnce = Settings.QuietRetryFailedOnce;
        _bCUSettings.QuietAutomatization = Settings.QuietAutomatization;
        _bCUSettings.QuietAutomatizationKillStuck = Settings.QuietAutomatizationKillStuck;
        _bCUSettings.QuietUseDaemon = Settings.QuietUseDaemon;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // Refresh filtered list
            case nameof(Settings.FilterShowMicrosoft):
            case nameof(Settings.AdvancedDisplayOrphans):
            case nameof(Settings.FilterShowSystemComponents):
            case nameof(Settings.FilterShowProtected):
            case nameof(Settings.FilterShowTweaks):
            case nameof(Settings.FilterShowUpdates):
            case nameof(Settings.FilterShowWinFeatures):
            case nameof(Settings.FilterShowStoreApps):
                _ = UpdateTextAsync(_refreshToken);
                break;
            // Synchronize Flow settings with BCU settings
            case nameof(Settings.BackupLeftovers):
                Debug.WriteLine("Settings BackupLeftovers changed to " + Settings.BackupLeftovers);
                _bCUSettings.BackupLeftovers = Settings.BackupLeftovers;
                break;
            case nameof(Settings.MessagesRemoveJunk):
                _bCUSettings.MessagesRemoveJunk = Settings.MessagesRemoveJunk;
                break;
            case nameof(Settings.MessagesAskRemoveLoudItems):
                _bCUSettings.MessagesAskRemoveLoudItems = Settings.MessagesAskRemoveLoudItems;
                break;
            case nameof(Settings.MessagesRestorePoints):
                _bCUSettings.MessagesRestorePoints = Settings.MessagesRestorePoints;
                break;
            case nameof(Settings.ExternalEnable):
                _bCUSettings.ExternalEnable = Settings.ExternalEnable;
                break;
            case nameof(Settings.ExternalPreCommands):
                _bCUSettings.ExternalPreCommands = Settings.ExternalPreCommands;
                break;
            case nameof(Settings.ExternalPostCommands):
                _bCUSettings.ExternalPostCommands = Settings.ExternalPostCommands;
                break;
            case nameof(Settings.MessagesShowAllBadJunk):
                _bCUSettings.MessagesShowAllBadJunk = Settings.MessagesShowAllBadJunk;
                break;
            case nameof(Settings.BackupLeftoversDirectory):
                _bCUSettings.BackupLeftoversDirectory = Settings.BackupLeftoversDirectory;
                break;
            case nameof(Settings.UninstallPreventShutdown):
                _bCUSettings.UninstallPreventShutdown = Settings.UninstallPreventShutdown;
                break;
            case nameof(Settings.CreateRestorePoint):
                _bCUSettings.CreateRestorePoint = Settings.CreateRestorePoint;
                break;
            case nameof(Settings.UninstallConcurrency):
                _bCUSettings.UninstallConcurrency = Settings.UninstallConcurrency;
                break;
            case nameof(Settings.UninstallConcurrentOneLoud):
                _bCUSettings.UninstallConcurrentOneLoud = Settings.UninstallConcurrentOneLoud;
                break;
            case nameof(Settings.UninstallConcurrentDisableManualCollisionProtection):
                _bCUSettings.UninstallConcurrentDisableManualCollisionProtection = Settings.UninstallConcurrentDisableManualCollisionProtection;
                break;
            case nameof(Settings.UninstallConcurrentMaxCount):
                _bCUSettings.UninstallConcurrentMaxCount = Settings.UninstallConcurrentMaxCount;
                break;
            case nameof(Settings.AdvancedIntelligentUninstallerSorting):
                _bCUSettings.AdvancedIntelligentUninstallerSorting = Settings.AdvancedIntelligentUninstallerSorting;
                break;
            case nameof(Settings.AdvancedDisableProtection):
                _bCUSettings.AdvancedDisableProtection = Settings.AdvancedDisableProtection;
                break;
            case nameof(Settings.AdvancedSimulate):
                _bCUSettings.AdvancedSimulate = Settings.AdvancedSimulate;
                break;
            case nameof(Settings.QuietAutoKillStuck):
                _bCUSettings.QuietAutoKillStuck = Settings.QuietAutoKillStuck;
                break;
            case nameof(Settings.QuietRetryFailedOnce):
                _bCUSettings.QuietRetryFailedOnce = Settings.QuietRetryFailedOnce;
                break;
            case nameof(Settings.QuietAutomatization):
                _bCUSettings.QuietAutomatization = Settings.QuietAutomatization;
                break;
            case nameof(Settings.QuietAutomatizationKillStuck):
                _bCUSettings.QuietAutomatizationKillStuck = Settings.QuietAutomatizationKillStuck;
                break;
            case nameof(Settings.QuietUseDaemon):
                _bCUSettings.QuietUseDaemon = Settings.QuietUseDaemon;
                break;
        }
    }

    private void BCUSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // Synchronize settings with Flow settings
            case nameof(BCUSettings.BackupLeftovers):
                Debug.WriteLine("BCUSettings BackupLeftovers changed to " + _bCUSettings.BackupLeftovers);
                Settings.BackupLeftovers = _bCUSettings.BackupLeftovers;
                break;
            case nameof(BCUSettings.MessagesRemoveJunk):
                Settings.MessagesRemoveJunk = _bCUSettings.MessagesRemoveJunk;
                break;
            case nameof(BCUSettings.MessagesAskRemoveLoudItems):
                Settings.MessagesAskRemoveLoudItems = _bCUSettings.MessagesAskRemoveLoudItems;
                break;
            case nameof(BCUSettings.MessagesRestorePoints):
                Settings.MessagesRestorePoints = _bCUSettings.MessagesRestorePoints;
                break;
            case nameof(BCUSettings.ExternalEnable):
                Settings.ExternalEnable = _bCUSettings.ExternalEnable;
                break;
            case nameof(BCUSettings.ExternalPreCommands):
                Settings.ExternalPreCommands = _bCUSettings.ExternalPreCommands;
                break;
            case nameof(BCUSettings.ExternalPostCommands):
                Settings.ExternalPostCommands = _bCUSettings.ExternalPostCommands;
                break;
            case nameof(BCUSettings.MessagesShowAllBadJunk):
                Settings.MessagesShowAllBadJunk = _bCUSettings.MessagesShowAllBadJunk;
                break;
            case nameof(BCUSettings.BackupLeftoversDirectory):
                Settings.BackupLeftoversDirectory = _bCUSettings.BackupLeftoversDirectory;
                break;
            case nameof(BCUSettings.UninstallPreventShutdown):
                Settings.UninstallPreventShutdown = _bCUSettings.UninstallPreventShutdown;
                break;
            case nameof(BCUSettings.CreateRestorePoint):
                Settings.CreateRestorePoint = _bCUSettings.CreateRestorePoint;
                break;
            case nameof(BCUSettings.UninstallConcurrency):
                Settings.UninstallConcurrency = _bCUSettings.UninstallConcurrency;
                break;
            case nameof(BCUSettings.UninstallConcurrentOneLoud):
                Settings.UninstallConcurrentOneLoud = _bCUSettings.UninstallConcurrentOneLoud;
                break;
            case nameof(BCUSettings.UninstallConcurrentDisableManualCollisionProtection):
                Settings.UninstallConcurrentDisableManualCollisionProtection = _bCUSettings.UninstallConcurrentDisableManualCollisionProtection;
                break;
            case nameof(BCUSettings.UninstallConcurrentMaxCount):
                Settings.UninstallConcurrentMaxCount = _bCUSettings.UninstallConcurrentMaxCount;
                break;
            case nameof(BCUSettings.AdvancedIntelligentUninstallerSorting):
                Settings.AdvancedIntelligentUninstallerSorting = _bCUSettings.AdvancedIntelligentUninstallerSorting;
                break;
            case nameof(BCUSettings.AdvancedDisableProtection):
                Settings.AdvancedDisableProtection = _bCUSettings.AdvancedDisableProtection;
                break;
            case nameof(BCUSettings.AdvancedSimulate):
                Settings.AdvancedSimulate = _bCUSettings.AdvancedSimulate;
                break;
            case nameof(BCUSettings.QuietAutoKillStuck):
                Settings.QuietAutoKillStuck = _bCUSettings.QuietAutoKillStuck;
                break;
            case nameof(BCUSettings.QuietRetryFailedOnce):
                Settings.QuietRetryFailedOnce = _bCUSettings.QuietRetryFailedOnce;
                break;
            case nameof(BCUSettings.QuietAutomatization):
                Settings.QuietAutomatization = _bCUSettings.QuietAutomatization;
                break;
            case nameof(BCUSettings.QuietAutomatizationKillStuck):
                Settings.QuietAutomatizationKillStuck = _bCUSettings.QuietAutomatizationKillStuck;
                break;
            case nameof(BCUSettings.QuietUseDaemon):
                Settings.QuietUseDaemon = _bCUSettings.QuietUseDaemon;
                break;
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Test synchronization of settings & PropertyChanged event
        Debug.WriteLine("");
        _bCUSettings.BackupLeftovers = Klocman.YesNoAsk.Yes;
        _bCUSettings.BackupLeftovers = Klocman.YesNoAsk.Ask;
        _bCUSettings.BackupLeftovers = Klocman.YesNoAsk.Ask;
        Debug.WriteLine("");
        Settings.BackupLeftovers = Klocman.YesNoAsk.Yes;
        Settings.BackupLeftovers = Klocman.YesNoAsk.Ask;
        Settings.BackupLeftovers = Klocman.YesNoAsk.Ask;

        // Initialize the list of uninstallers
        InitiateListRefresh();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _refreshSource?.Cancel();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _refreshSource?.Dispose();
        _queryUpdateSemaphore.Dispose();
        _iconGetter.Dispose();
    }

    private void InitiateListRefresh()
    {
        try
        {
            _refreshSource?.Cancel();
            _refreshSource?.Dispose();

            var refreshSource = new CancellationTokenSource();
            _refreshSource = refreshSource;
            var refreshToken = refreshSource.Token;
            _refreshToken = refreshToken;

            Task.Run(() => ListRefreshThread(refreshToken), refreshToken);
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
        var initialized = UninstallersInitialized;

        await _queryUpdateSemaphore.WaitAsync(token).ConfigureAwait(false);

        try
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

            if (token.IsCancellationRequested) return;

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

            if (token.IsCancellationRequested) return;

            await InvokeAsync(() =>
            {
                ProgressBar.Maximum = progressMax;
                TextBlock.Text = Forms.Windows.MainWindow.ProgressFinishing;
                SubProgressBar.Maximum = 2;
                SubProgressBar.Value = 0;
                SubTextBlock.Text = string.Empty;
            }, DispatcherPriority.Normal, token);

            if (token.IsCancellationRequested) return;

            // Remove Flow Launcher from the list so that it doesn't show up in the UI
            uninstallerEntries.RemoveAll(x =>
                (x.DisplayName == FlowLauncherDisplayName || x.DisplayNameTrimmed == FlowLauncherDisplayName) &&
                (x.Publisher == FlowLauncherPublisher || x.PublisherTrimmed == FlowLauncherPublisher));

            AllUninstallers = uninstallerEntries;

            if (token.IsCancellationRequested) return;

            await InvokeAsync(() =>
            {
                SubProgressBar.Value = 1;
                SubTextBlock.Text = Forms.Windows.MainWindow.ProgressFinishingIcons;
            }, DispatcherPriority.Normal, token);

            if (token.IsCancellationRequested) return;

            try
            {
                _iconGetter.UpdateIconList(uninstallerEntries, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (token.IsCancellationRequested) return;

            await InvokeAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                TextBlock.Visibility = Visibility.Collapsed;
                SubProgressBar.Visibility = Visibility.Collapsed;
                SubTextBlock.Visibility = Visibility.Collapsed;
            }, DispatcherPriority.Normal, token);

            if (token.IsCancellationRequested) return;

            if (!initialized)
            {
                await UpdateTextAsync(token).ConfigureAwait(false);
            }
        }
        finally
        {
            _queryUpdateSemaphore.Release();
        }

        if (initialized)
        {
            _ = UpdateTextAsync(token).ConfigureAwait(false);
        }
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
        var allUninstallers = AllUninstallers;
        var filteredUninstallers = new List<ApplicationUninstallerEntry>(allUninstallers.Count);

        var uninstallerListText = await Task.Run(() =>
        {
            foreach (var uninstaller in allUninstallers)
            {
                if (token.IsCancellationRequested) return string.Empty;

                if (ListViewFilter(uninstaller))
                {
                    filteredUninstallers.Add(uninstaller);
                }
            }

            FilteredUninstallers = filteredUninstallers;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{filteredUninstallers.Count}/{allUninstallers.Count} Uninstallers:");
            foreach (var uninstaller in filteredUninstallers)
            {
                if (!string.IsNullOrEmpty(uninstaller.DisplayVersion))
                {
                    stringBuilder.AppendLine($"{uninstaller.DisplayName} By {uninstaller.Publisher} v{uninstaller.DisplayVersion}");
                }
                else
                {
                    stringBuilder.AppendLine($"{uninstaller.DisplayName} By {uninstaller.Publisher}");
                }
            }
            return stringBuilder.ToString();
        }).ConfigureAwait(false);

        await InvokeAsync(() =>
        {
            var firstUninstaller = filteredUninstallers.FirstOrDefault();
            if (firstUninstaller != null)
            {
                UninstallerListTextBlock.Text = uninstallerListText;
                Image.Source = GetIcon(firstUninstaller);
            }
        }, DispatcherPriority.Normal, _refreshToken);

        // Local function
        static BitmapSource GetIcon(ApplicationUninstallerEntry uninstaller)
        {
            var icon = uninstaller.GetIcon();
            if (icon != null)
            {
                using var bitmap = icon.ToBitmap();
                var hIcon = bitmap.GetHicon();
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                        hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    imageSource.Freeze(); // Allows cross-thread use if needed
                    return imageSource;
                });
            }

            return null!;
        }
    }

    private static bool ListViewFilter(ApplicationUninstallerEntry entry)
    {
        if (!Settings.FilterShowMicrosoft && !string.IsNullOrEmpty(entry.Publisher) && entry.Publisher.Contains(MicrosoftPublisher))
        {
            return false;
        }

        if (!Settings.AdvancedDisplayOrphans && entry.IsOrphaned)
        {
            return false;
        }

        if (!Settings.FilterShowSystemComponents && entry.SystemComponent)
        {
            return false;
        }

        if (!Settings.FilterShowProtected && entry.IsProtected)
        {
            return false;
        }

        if (entry.RatingId != null && !Settings.FilterShowTweaks && entry.RatingId.StartsWith(TweakRateId, StringComparison.Ordinal))
        {
            return false;
        }

        if (!Settings.FilterShowUpdates && entry.IsUpdate)
        {
            return false;
        }

        if (!Settings.FilterShowWinFeatures && entry.UninstallerKind == UninstallerType.WindowsFeature)
        {
            return false;
        }

        if (!Settings.FilterShowStoreApps && entry.UninstallerKind == UninstallerType.StoreApp)
        {
            return false;
        }

        return true;
    }

    private async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        var uninstallText = UninstallTextBox.Text;
        if (!string.IsNullOrEmpty(uninstallText))
        {
            var initialized = UninstallersInitialized;
            if (!initialized)
            {
                if (_queryUpdateSemaphore.CurrentCount < QueryUpdateSemaphoreMaxCount)
                {
                    // If the semaphore is not available which means that the list is being updated, just return empty results
                    return;
                }

                try
                {
                    await _queryUpdateSemaphore.WaitAsync(_refreshToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                try
                {
                    var filteredUninstallers = FilteredUninstallers;
                    foreach (var uninstaller in filteredUninstallers)
                    {
                        if (uninstaller.DisplayName.ToLower().Contains(uninstallText.ToLower()))
                        {
                            if (MessageBox.Show($"Do you want to uninstall {uninstaller.DisplayName} by {uninstaller.Publisher}?",
                                "Uninstall", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                var allUninstallers = AllUninstallers;
                                _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers);
                            }
                        }
                    }
                }
                finally
                {
                    _queryUpdateSemaphore.Release();
                }
            }
            else
            {
                var filteredUninstallers = FilteredUninstallers;
                foreach (var uninstaller in filteredUninstallers)
                {
                    if (uninstaller.DisplayName.ToLower().Contains(uninstallText.ToLower()))
                    {
                        if (MessageBox.Show($"Do you want to uninstall {uninstaller.DisplayName} by {uninstaller.Publisher}?",
                            "Uninstall", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            var allUninstallers = AllUninstallers;
                            _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers);
                        }
                    }
                }
            }
        }
    }
}
