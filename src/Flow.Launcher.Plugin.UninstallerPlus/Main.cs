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

public class UninstallerPlus : IAsyncPlugin, IContextMenu, IReloadable, IResultUpdated, IPluginI18n,
    ISettingProvider, IDisposable
{
    internal static PluginInitContext Context { get; private set; } = null!;

    internal static Settings Settings { get; private set; } = null!;

    internal static FlowPublicAPI FlowAPI { get; private set; } = null!;

    #region Private Fileds

    private readonly static string ClassName = nameof(UninstallerPlus);

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

    private bool UninstallersInitialized => _allUninstallers != null && _allUninstallers.Count > 0;

    private List<ApplicationUninstallerEntry> FilteredUninstallers = null!;

    private CancellationTokenSource? _refreshSource;
    private CancellationToken _refreshToken;

    private const string FlowLauncherDisplayName = "Flow Launcher";
    private const string FlowLauncherPublisher = "Flow-Launcher Team";

    private const string MicrosoftPublisher = "Microsoft";
    private const string TweakRateId = "tweak";

    private static readonly BCUSettings _bCUSettings = BCUSettings.Default;

    private readonly SemaphoreSlim _queryUpdateSemaphore = new(1, 1);

    private MainWindow _mainWindow = null!;

    #endregion

    #region IAsyncPlugin Interface

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        return await Task.Run(() => Query(query, token), token);
    }

    public async Task<List<Result>> Query(Query query, CancellationToken token)
    {
        var results = InitList(query);

        var initialized = UninstallersInitialized;
        if (!initialized)
        {
            // If the list is not initialized, we need to wait for the list to be refreshed before querying
            await _queryUpdateSemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                return QueryList(results, query, token);
            }
            finally
            {
                _queryUpdateSemaphore.Release();
            }
        }
        else
        {
            return QueryList(results, query, token);
        }
    }

    public async Task InitAsync(PluginInitContext context)
    {
        Context = context;

        // Init Flow api
        FlowAPI = new(context.API);
        
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
        _mainWindow = new MainWindow(InitiateListRefresh, FlowAPI);

        // Init program list
        InitiateListRefresh();

        await Task.CompletedTask;
    }

    #endregion

    #region Initialize List

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
        var initialized = UninstallersInitialized;

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

            if (token.IsCancellationRequested) return;

            /*await InvokeAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                TextBlock.Visibility = Visibility.Collapsed;
                SubProgressBar.Visibility = Visibility.Collapsed;
                SubTextBlock.Visibility = Visibility.Collapsed;
            }, DispatcherPriority.Normal, token);*/

            Context.API.LogDebug(ClassName, $"Loaded {uninstallerEntries.Count} uninstallers");

            if (token.IsCancellationRequested) return;

            // For the first time initialization, we need to update the text insides the query update semaphore
            // to ensure the query before the uninstall initialization work correctly 
            if (!initialized)
            {
                UpdateText(token);
            }
        }
        finally
        {
            _queryUpdateSemaphore.Release();
        }

        if (initialized)
        {
            _ = Task.Run(() => UpdateText(token), token).ConfigureAwait(false);
        }
    }

    private void UpdateText(CancellationToken token)
    {
        var allUninstallers = AllUninstallers;
        var filteredUninstallers = new List<ApplicationUninstallerEntry>(allUninstallers.Count);

        foreach (var uninstaller in allUninstallers)
        {
            if (token.IsCancellationRequested) return;

            if (ListViewFilter(uninstaller))
            {
                filteredUninstallers.Add(uninstaller);
            }
        }

        FilteredUninstallers = filteredUninstallers;
        Context.API.LogDebug(ClassName, $"Filtered {filteredUninstallers.Count} uninstallers");
    }

    #endregion

    #region Query List

    private List<Result> InitList(Query query)
    {
        var results = new List<Result>();
        var reloadResult = new Result
        {
            Title = Localize.flowlauncher_plugin_uninstallerplus_reload(),
            SubTitle = Localize.flowlauncher_plugin_uninstallerplus_reload_subtitle(),
            IcoPath = "Images/reload.png",
            Glyph = new(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\uE72C"),
            Score = Result.MaxScore, // Set to the topmost
            Action = _ =>
            {
                InitiateListRefresh();
                return true;
            }
        };
        results.Add(reloadResult);

        ResultsUpdated?.Invoke(this, new ResultUpdatedEventArgs
        {
            Results = results,
            Query = query
        });

        return results;
    }

    private List<Result> QueryList(List<Result> results, Query query, CancellationToken token)
    {
        var searchTerm = query.Search;
        var filteredUninstallers = FilteredUninstallers;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            foreach (var uninstaller in filteredUninstallers)
            {
                token.ThrowIfCancellationRequested();

                var result = new Result
                {
                    Title = uninstaller.DisplayName,
                    AutoCompleteText = uninstaller.DisplayName,
                    SubTitle = !string.IsNullOrEmpty(uninstaller.DisplayVersion) ?
                        $"{uninstaller.Publisher} v{uninstaller.DisplayVersion}" :
                        $"{uninstaller.Publisher}",
                    ContextData = uninstaller,
                    IcoPath = uninstaller.DisplayIcon,
                    ShowBadge = true,
                    BadgeIcoPath = "Images/badge.png",
                    Score = 0,
                    AddSelectedCount = false,
                    Action = _ =>
                    {
                        var allUninstallers = AllUninstallers;
                        Context.API.HideMainWindow();
                        _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers, FlowAPI);
                        return true;
                    }
                };
                results.Add(result);
            }
        }
        else
        {
            foreach (var uninstaller in filteredUninstallers)
            {
                token.ThrowIfCancellationRequested();

                var match = Context.API.FuzzySearch(searchTerm, uninstaller.DisplayName);

                if (!match.IsSearchPrecisionScoreMet()) continue;

                var result = new Result
                {
                    Title = uninstaller.DisplayName,
                    AutoCompleteText = uninstaller.DisplayName,
                    SubTitle = !string.IsNullOrEmpty(uninstaller.DisplayVersion) ?
                        $"{uninstaller.Publisher} v{uninstaller.DisplayVersion}" :
                        $"{uninstaller.Publisher}",
                    ContextData = uninstaller,
                    IcoPath = uninstaller.DisplayIcon,
                    ShowBadge = true,
                    BadgeIcoPath = "Images/badge.png",
                    TitleHighlightData = match.MatchData,
                    Score = match.Score,
                    AddSelectedCount = false,
                    Action = _ =>
                    {
                        var allUninstallers = AllUninstallers;
                        Context.API.HideMainWindow();
                        _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers, FlowAPI);
                        return true;
                    }
                };
                results.Add(result);
            }
        }

        return results;
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
            Title = Localize.flowlauncher_plugin_uninstallerplus_context_menu_uninstall(),
            IcoPath = "Images/uninstall.png",
            Glyph = new(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\uE835"),
            Score = 4,
            AddSelectedCount = false,
            Action = _ =>
            {
                var allUninstallers = AllUninstallers;
                Context.API.HideMainWindow();
                _mainWindow.RunLoudUninstall(new[] { uninstaller }, allUninstallers, FlowAPI);
                return true;
            }
        };
        results.Add(uninstallResult);

        var quietlyUninstallResult = new Result
        {
            Title = Localize.flowlauncher_plugin_uninstallerplus_context_menu_uninstall_quietly(),
            IcoPath = "Images/quiet.png",
            Glyph = new(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\uF71D"),
            Score = 3,
            AddSelectedCount = false,
            Action = _ =>
            {
                var allUninstallers = AllUninstallers;
                Context.API.HideMainWindow();
                _mainWindow.RunQuietUninstall(new[] { uninstaller }, allUninstallers, FlowAPI);
                return true;
            }
        };
        results.Add(quietlyUninstallResult);

        if (!string.IsNullOrEmpty(uninstaller.ModifyPath))
        {
            var modifyResult = new Result
            {
                Title = Localize.flowlauncher_plugin_uninstallerplus_context_menu_modify(),
                IcoPath = "Images/modify.png",
                Glyph = new(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\uE932"),
                Score = 2,
                AddSelectedCount = false,
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
            Title = Localize.flowlauncher_plugin_uninstallerplus_context_menu_uninstall_manually(),
            IcoPath = "Images/manual.png",
            Glyph = new(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\uE736"),
            Score = 1,
            AddSelectedCount = false,
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

    #region IReloadable Interface

    public void ReloadData()
    {
        InitiateListRefresh();
    }

    #endregion

    #region IResultUpdated Interface

    public event ResultUpdatedEventHandler? ResultsUpdated;

    #endregion

    #region IPluginI18n Interface

    public string GetTranslatedPluginTitle()
    {
        return Localize.flowlauncher_plugin_uninstallerplus_plugin_name();
    }

    public string GetTranslatedPluginDescription()
    {
        return Localize.flowlauncher_plugin_uninstallerplus_plugin_description();
    }

    public void OnCultureInfoChanged(CultureInfo cultureInfo)
    {

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
                if (UninstallersInitialized)
                {
                    _ = Task.Run(() => UpdateText(_refreshToken), _refreshToken).ConfigureAwait(false);
                }
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
