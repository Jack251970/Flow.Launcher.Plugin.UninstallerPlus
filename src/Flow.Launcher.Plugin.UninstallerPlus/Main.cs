using BulkCrapUninstaller.Forms.Windows;
using Klocman.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using UninstallTools;
using UninstallTools.Controls;
using UninstallTools.Factory;
using BCUSettings = BulkCrapUninstaller.Properties.Settings;

namespace Flow.Launcher.Plugin.UninstallerPlus;

public class UninstallerPlus : IAsyncPlugin, IAsyncReloadable, IContextMenu, IPluginI18n, ISettingProvider, IDisposable
{
    internal static PluginInitContext Context { get; private set; } = null!;

    internal static Settings Settings { get; private set; } = null!;

    private readonly static string ClassName = nameof(UninstallerPlus);

    private readonly List<Result> EmptyResults = new();

    private UninstallerIconGetter _iconGetter = null!;

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

    private CancellationTokenSource? _refreshSource;
    private CancellationToken _refreshToken;

    private const string FlowLauncherDisplayName = "Flow Launcher";
    private const string FlowLauncherPublisher = "Flow-Launcher Team";

    private const string MicrosoftPublisher = "Microsoft";
    private const string TweakRateId = "tweak";

    private static readonly BCUSettings _bCUSettings = BCUSettings.Default;

    private readonly SemaphoreSlim _queryUpdateSemaphore = new(1, 1);

    private MainWindow _mainWindow = null!;

    #region IAsyncPlugin Interface

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        try
        {
            return await Task.Run(() => Query(query, token), token);
        }
        catch (OperationCanceledException)
        {
            // Ignored - token cancelled
        }
        return EmptyResults;
    }

    public async Task<List<Result>> Query(Query query, CancellationToken token)
    {
        var initialized = _allUninstallers != null && _allUninstallers.Count > 0;
        if (!initialized)
        {
            await _queryUpdateSemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                return QueryList(query.Search);
            }
            finally
            {
                _queryUpdateSemaphore.Release();
            }
        }
        else
        {
            return QueryList(query.Search);
        }
    }

    public async Task InitAsync(PluginInitContext context)
    {
        Context = context;
        
        // Init settings
        Settings = context.API.LoadSettingJsonStorage<Settings>();
        Context.API.LogDebug(ClassName, $"Init: {Settings}");

        // Init settings property changed event
        Settings.PropertyChanged += Settings_PropertyChanged;

        // Init Bulk-Crap-Uninstaller settings
        InitBCUSettings();

        // Init Bulk-Crap-Uninstaller settings property changed event
        _bCUSettings.PropertyChanged += BCUSettings_PropertyChanged;

        // Init plugin context
        UninstallToolsGlobalConfig.Initialize(context.CurrentPluginMetadata.PluginDirectory);

        // Init Bulk-Crap-Uninstaller instances
        _iconGetter = new UninstallerIconGetter();
        _mainWindow = new MainWindow(InitiateListRefresh, (e) => Context.API.LogException(ClassName, "Exception happened in Bulk-Crap-Uninstaller", e));

        // Init program list
        InitiateListRefresh();

        await Task.CompletedTask;
    }

    #endregion

    #region Initialize List

    private List<Result> QueryList(string searchTerm)
    {
        var results = new List<Result>();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            foreach (var uninstaller in FilteredUninstallers)
            {
                var result = new Result
                {
                    Title = uninstaller.DisplayName,
                    AutoCompleteText = uninstaller.DisplayName,
                    SubTitle = uninstaller.Publisher,
                    ContextData = uninstaller,
                    IcoPath = uninstaller.DisplayIcon,
                    Score = 0,
                    Action = _ =>
                    {
                        var allUninstallers = AllUninstallers;
                        Context.API.HideMainWindow();
                        _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers);
                        return true;
                    }
                };
                results.Add(result);
            }
        }
        else
        {
            foreach (var uninstaller in FilteredUninstallers)
            {
                var match = Context.API.FuzzySearch(searchTerm, uninstaller.DisplayName);

                if (!match.IsSearchPrecisionScoreMet()) continue;

                var result = new Result
                {
                    Title = uninstaller.DisplayName,
                    AutoCompleteText = uninstaller.DisplayName,
                    SubTitle = uninstaller.Publisher,
                    ContextData = uninstaller,
                    IcoPath = uninstaller.DisplayIcon,
                    TitleHighlightData = match.MatchData,
                    Score = match.Score,
                    Action = _ =>
                    {
                        var allUninstallers = AllUninstallers;
                        Context.API.HideMainWindow();
                        _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers);
                        return true;
                    }
                };
                results.Add(result);
            }
        }

        return results;
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
            Context.API.LogException(ClassName, "Failed to refresh list", ex);
        }
    }

    private async Task ListRefreshThread(CancellationToken token)
    {
        await _queryUpdateSemaphore.WaitAsync(token).ConfigureAwait(false);

        try
        {
            /*await InvokeAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                TextBlock.Visibility = Visibility.Visible;
                TextBlock.Text = string.Empty;
                SubProgressBar.Visibility = Visibility.Visible;
                SubTextBlock.Visibility = Visibility.Visible;
                SubTextBlock.Text = string.Empty;
            }, DispatcherPriority.Normal, token);*/

            if (token.IsCancellationRequested) return;

            var progressMax = 0;
            var uninstallerEntries = ApplicationUninstallerFactory.GetUninstallerEntries((x) =>
            {
                progressMax = x.TotalCount + 1;

                /*await InvokeAsync(() =>
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
                }, DispatcherPriority.Normal, token);*/

            }, token);

            if (token.IsCancellationRequested) return;

            /*await InvokeAsync(() =>
            {
                ProgressBar.Maximum = progressMax;
                TextBlock.Text = Forms.Windows.MainWindow.ProgressFinishing;
                SubProgressBar.Maximum = 2;
                SubProgressBar.Value = 0;
                SubTextBlock.Text = string.Empty;
            }, DispatcherPriority.Normal, token);*/

            if (token.IsCancellationRequested) return;

            // Remove Flow Launcher from the list so that it doesn't show up in the UI
            uninstallerEntries.RemoveAll(x =>
                (x.DisplayName == FlowLauncherDisplayName || x.DisplayNameTrimmed == FlowLauncherDisplayName) &&
                (x.Publisher == FlowLauncherPublisher || x.PublisherTrimmed == FlowLauncherPublisher));

            AllUninstallers = uninstallerEntries;

            if (token.IsCancellationRequested) return;

            /*await InvokeAsync(() =>
            {
                SubProgressBar.Value = 1;
                SubTextBlock.Text = Forms.Windows.MainWindow.ProgressFinishingIcons;
            }, DispatcherPriority.Normal, token);*/

            if (token.IsCancellationRequested) return;

            try
            {
                _iconGetter.UpdateIconList(uninstallerEntries, token);
            }
            catch (Exception ex)
            {
                Context.API.LogException(ClassName, "Failed to load icons", ex);
            }

            /*await InvokeAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                TextBlock.Visibility = Visibility.Collapsed;
                SubProgressBar.Visibility = Visibility.Collapsed;
                SubTextBlock.Visibility = Visibility.Collapsed;
            }, DispatcherPriority.Normal, token);*/

            if (token.IsCancellationRequested) return;

            Context.API.LogDebug(ClassName, $"Loaded {uninstallerEntries.Count} uninstallers");
        }
        finally
        {
            _queryUpdateSemaphore.Release();
        }

        _ = UpdateTextAsync(token);
    }

    private async Task UpdateTextAsync(CancellationToken token)
    {
        var allUninstallers = AllUninstallers;

        await Task.Run(() =>
        {
            FilteredUninstallers.Clear();

            foreach (var uninstaller in allUninstallers)
            {
                if (token.IsCancellationRequested) return;

                if (ListViewFilter(uninstaller))
                {
                    FilteredUninstallers.Add(uninstaller);
                }
            }
        }, token).ConfigureAwait(false);

        Context.API.LogDebug(ClassName, $"Filtered {FilteredUninstallers.Count} uninstallers");
    }

    #endregion

    #region List View Filter

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

    #endregion

    #region IAsyncReloadable Interface

    public async Task ReloadDataAsync()
    {
        InitiateListRefresh();

        await Task.CompletedTask;
    }

    #endregion

    #region IPluginI18n Interface

    public string GetTranslatedPluginTitle()
    {
        return Context.API.GetTranslation("flowlauncher_plugin_uninstallerplus_plugin_name");
    }

    public string GetTranslatedPluginDescription()
    {
        return Context.API.GetTranslation("flowlauncher_plugin_uninstallerplus_plugin_description");
    }

    public void OnCultureInfoChanged(CultureInfo cultureInfo)
    {

    }

    #endregion

    #region IContextMenu Interface

    public List<Result> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult.ContextData is not ApplicationUninstallerEntry uninstaller)
        {
            return new List<Result>();
        }

        var results = new List<Result>();

        var uninstallResult = new Result
        {
            Title = Context.API.GetTranslation("flowlauncher_plugin_uninstallerplus_context_menu_uninstall"),
            IcoPath = "Images/uninstall.png",
            Action = _ =>
            {
                var allUninstallers = AllUninstallers;
                Context.API.HideMainWindow();
                _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers);
                return true;
            }
        };
        results.Add(uninstallResult);

        var quietlyUninstallResult = new Result
        {
            Title = Context.API.GetTranslation("flowlauncher_plugin_uninstallerplus_context_menu_uninstall_quietly"),
            IcoPath = "Images/quiet.png",
            Action = _ =>
            {
                var allUninstallers = AllUninstallers;
                Context.API.HideMainWindow();
                _mainWindow.RunQuietUninstall(new[] { uninstaller }, allUninstallers);
                return true;
            }
        };
        results.Add(quietlyUninstallResult);

        if (!string.IsNullOrEmpty(uninstaller.ModifyPath))
        {
            var modifyResult = new Result
            {
                Title = Context.API.GetTranslation("flowlauncher_plugin_uninstallerplus_context_menu_modify"),
                IcoPath = "Images/modify.png",
                Action = c =>
                {
                    Context.API.HideMainWindow();
                    _mainWindow.RunModify(new[] { uninstaller });
                    return true;
                }
            };
            results.Add(modifyResult);
        }

        var manualUninstallResult = new Result
        {
            Title = Context.API.GetTranslation("flowlauncher_plugin_uninstallerplus_context_menu_uninstall_manually"),
            IcoPath = "Images/manual.png",
            Action = _ =>
            {
                var allUninstallers = AllUninstallers;
                Context.API.HideMainWindow();
                _mainWindow.RunManualUninstall(new[] { uninstaller }, allUninstallers);
                return true;
            }
        };
        results.Add(manualUninstallResult);

        return results;
    }

    #endregion

    #region ISettingProvider Interface

    public Control CreateSettingPanel()
    {
        Context.API.LogDebug(ClassName, $"Settings Panel: {Settings}");
        return new SettingsPanel(Settings);
    }

    #endregion

    #region Settings

    private static void InitBCUSettings()
    {
        // Load BCU settings from Flow settings
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

    #endregion

    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshSource?.Cancel();
            _refreshSource?.Dispose();
            _queryUpdateSemaphore.Dispose();
            _iconGetter?.Dispose();
            _disposed = true;
        }
    }

    #endregion
}
