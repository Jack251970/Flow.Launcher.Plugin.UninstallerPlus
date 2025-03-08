/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using Klocman.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace BulkCrapUninstaller;

/// <summary>
/// Workaround for ..\..\source\BulkCrapUninstaller\Program.cs
/// </summary>
public static class Program
{
    // MYTODO: Check this.
    private static DirectoryInfo _assemblyLocation = null;
    public static DirectoryInfo AssemblyLocation
    {
        get
        {
            if (_assemblyLocation == null)
            {
                var location = (Assembly.GetAssembly(typeof(Program))?.Location) ?? throw new InvalidOperationException("Failed to get entry assembly location");
                if (location[location.LastIndexOf('\\')..].Contains('.'))
                    location = PathTools.GetDirectory(location);
                _assemblyLocation = new DirectoryInfo(location);
            }
            return _assemblyLocation;
        }
    }

    /// <summary>
    /// Don't use settings
    /// </summary>
    public static Uri ConnectionString { get; } = Debugger.IsAttached ? new Uri(@"http://localhost:7721") : new Uri(@"http://bugsklocman.ddns.net:7721");

#pragma warning disable CA1024
    public static HttpClient GetHttpClient() => new HttpClient
    {
        BaseAddress = ConnectionString
    };
#pragma warning restore CA1024
}
