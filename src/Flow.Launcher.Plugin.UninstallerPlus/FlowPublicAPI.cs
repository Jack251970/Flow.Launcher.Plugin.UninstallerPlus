using BulkCrapUninstaller.Functions;
using System;
using System.Runtime.CompilerServices;

namespace Flow.Launcher.Plugin.UninstallerPlus;

internal class FlowPublicAPI : IFlowPublicAPI
{
    private readonly IPublicAPI _api;

    public FlowPublicAPI(IPublicAPI api)
    {
        _api = api;
    }

    public void LogDebug(string className, string message, [CallerMemberName] string methodName = "")
    {
        _api.LogDebug(className, message, methodName);
    }

    public void LogError(string className, string message, [CallerMemberName] string methodName = "")
    {
        _api.LogError(className, message, methodName);
    }

    public void LogException(string className, string message, Exception e, [CallerMemberName] string methodName = "")
    {
        _api.LogException(className, message, e, methodName);
    }

    public void LogInfo(string className, string message, [CallerMemberName] string methodName = "")
    {
        _api.LogInfo(className, message, methodName);
    }

    public void LogWarn(string className, string message, [CallerMemberName] string methodName = "")
    {
        _api.LogWarn(className, message, methodName);
    }
}
