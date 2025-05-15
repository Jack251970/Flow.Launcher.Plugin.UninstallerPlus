using BulkCrapUninstaller.Forms.Windows;
using Klocman.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using UninstallTools;
using UninstallTools.Controls;
using UninstallTools.Factory;

namespace Flow.Launcher.Plugin.UninstallerPlus;

public class UninstallerPlus : IAsyncPlugin, IAsyncReloadable, IPluginI18n, ISettingProvider, IDisposable
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

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const string FlowLauncherDisplayName = "Flow Launcher";
    private const string FlowLauncherPublisher = "Flow-Launcher Team";

    private const string MicrosoftPublisher = "Microsoft";
    private const string TweakRateId = "tweak";

    private const int QueryUpdateSemaphoreMaxCount = 1;
    private readonly SemaphoreSlim _queryUpdateSemaphore = new(1, QueryUpdateSemaphoreMaxCount);

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
            // Ignore
        }
        return EmptyResults;
    }

    public async Task<List<Result>> Query(Query query, CancellationToken token)
    {
        var results = new List<Result>();

        await _queryUpdateSemaphore.WaitAsync(token);

        var searchTerm = query.Search;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            foreach (var uninstaller in FilteredUninstallers)
            {
                var result = new Result
                {
                    Title = uninstaller.DisplayName,
                    SubTitle = uninstaller.Publisher,
                    /*IcoPath = _iconGetter.ColumnImageGetter(uninstaller)?.ToString() ?? string.Empty,*/
                    Score = 0,
                    Action = _ =>
                    {
                        _mainWindow.RunLoudUninstall(new[] { uninstaller }, AllUninstallers.ToList());

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
                    SubTitle = uninstaller.Publisher,
                    /*IcoPath = _iconGetter.ColumnImageGetter(uninstaller)?.ToString() ?? string.Empty,*/
                    TitleHighlightData = match.MatchData,
                    Score = match.Score,
                    Action = _ =>
                    {
                        _mainWindow.RunLoudUninstall(new[] { uninstaller }, AllUninstallers.ToList());

                        return true;
                    }
                };
                results.Add(result);
            }
        }

        _queryUpdateSemaphore.Release();

        return results;
    }

    public async Task InitAsync(PluginInitContext context)
    {
        Context = context;
        
        // Init settings
        Settings = context.API.LoadSettingJsonStorage<Settings>();
        Context.API.LogDebug(ClassName, $"Init: {Settings}");

        // Init Bulk-Crap-Uninstaller instances
        _iconGetter = new UninstallerIconGetter();
        _mainWindow = new MainWindow(InitiateListRefresh);

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
            Task.Run(() => ListRefreshThread(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
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

        /*await InvokeAsync(() =>
        {
            ProgressBar.Visibility = Visibility.Visible;
            TextBlock.Visibility = Visibility.Visible;
            TextBlock.Text = string.Empty;
            SubProgressBar.Visibility = Visibility.Visible;
            SubTextBlock.Visibility = Visibility.Visible;
            SubTextBlock.Text = string.Empty;
        }, DispatcherPriority.Normal, token);*/

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

        /*await InvokeAsync(() =>
        {
            ProgressBar.Maximum = progressMax;
            TextBlock.Text = Forms.Windows.MainWindow.ProgressFinishing;
            SubProgressBar.Maximum = 2;
            SubProgressBar.Value = 0;
            SubTextBlock.Text = string.Empty;
        }, DispatcherPriority.Normal, token);*/

        // Remove Flow Launcher from the list so that it doesn't show up in the UI
        uninstallerEntries.RemoveAll(x =>
            (x.DisplayName == FlowLauncherDisplayName || x.DisplayNameTrimmed == FlowLauncherDisplayName) &&
            (x.Publisher == FlowLauncherPublisher || x.PublisherTrimmed == FlowLauncherPublisher));

        AllUninstallers = uninstallerEntries;

        /*await InvokeAsync(() =>
        {
            SubProgressBar.Value = 1;
            SubTextBlock.Text = Forms.Windows.MainWindow.ProgressFinishingIcons;
        }, DispatcherPriority.Normal, token);*/

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
            Context.API.LogException(ClassName, "Failed to load icons", ex);
        }

        /*await InvokeAsync(() =>
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            TextBlock.Visibility = Visibility.Collapsed;
            SubProgressBar.Visibility = Visibility.Collapsed;
            SubTextBlock.Visibility = Visibility.Collapsed;
        }, DispatcherPriority.Normal, token);*/

        _queryUpdateSemaphore.Release();

        _ = UpdateTextAsync(token);
    }

    private async Task UpdateTextAsync(CancellationToken token)
    {
        await _queryUpdateSemaphore.WaitAsync(token);

        await Task.Run(() =>
        {
            FilteredUninstallers.Clear();

            foreach (var uninstaller in AllUninstallers)
            {
                if (ListViewFilter(uninstaller))
                {
                    FilteredUninstallers.Add(uninstaller);
                }
            }
        }, token).ConfigureAwait(false);

        _queryUpdateSemaphore.Release();
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

    #region ISettingProvider Interface

    public Control CreateSettingPanel()
    {
        Context.API.LogDebug(ClassName, $"Settings Panel: {Settings}");
        return new UserControl(); // TODO
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
            _cancellationTokenSource?.Cancel();
            _queryUpdateSemaphore?.Dispose();
            _cancellationTokenSource?.Dispose();
            _iconGetter?.Dispose();
            _disposed = true;
        }
    }

    #endregion
}
