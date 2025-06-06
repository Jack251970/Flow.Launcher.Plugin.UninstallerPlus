/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using Klocman.Native;
using Klocman.Tools;
using UninstallTools.Properties;

namespace UninstallTools.Factory
{
    public class StoreAppFactory : IIndependantUninstallerFactory
    {
        private static string HelperPath { get; } = Path.Combine(UninstallToolsGlobalConfig.AssemblyLocation, @"StoreAppHelper.exe");
        private static bool IsHelperAvailable() => File.Exists(HelperPath);

        public static string GetPowerShellRemoveCommand(string fullName)
        {
            return $"Remove-AppxPackage -package {fullName} -confirm:$false";
        }

        /// <summary>
        /// Convert supplied store app entries to power shell uninstall commands. Non-store app entries are ignored.
        /// </summary>
        public static string[] ToPowerShellRemoveCommands(IEnumerable<ApplicationUninstallerEntry> appEntries)
        {
            return appEntries.Where(x => x.UninstallerKind == UninstallerType.StoreApp)
                .Select(x => GetPowerShellRemoveCommand(x.Comment))
                .ToArray();
        }

        public IList<ApplicationUninstallerEntry> GetUninstallerEntries(
            ListGenerationProgress.ListGenerationCallback progressCallback)
        {
            var results = new List<ApplicationUninstallerEntry>();
            if (!IsHelperAvailable()) return results;

            static bool IsAdministrator()
            {
                using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    var principal = new System.Security.Principal.WindowsPrincipal(identity);
                    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
            }
            if (!IsAdministrator()) return results;

            var output = FactoryTools.StartHelperAndReadOutput(HelperPath, "/query");
            if (string.IsNullOrEmpty(output)) return results;

            var windowsPath = WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_WINDOWS);

            foreach (var data in FactoryTools.ExtractAppDataSetsFromHelperOutput(output))
            {
                if (!data.ContainsKey("InstalledLocation") || !Directory.Exists(data["InstalledLocation"])) continue;

                var fullName = data["FullName"];
                var uninstallStr = $"\"{HelperPath}\" /uninstall \"{fullName}\"";
                var isProtected = data.ContainsKey("IsProtected") && Convert.ToBoolean(data["IsProtected"], CultureInfo.InvariantCulture);
                var result = new ApplicationUninstallerEntry
                {
                    Comment = fullName,
                    CacheIdOverride = fullName,
                    RatingId = fullName.Substring(0, fullName.IndexOf("_", StringComparison.Ordinal)),
                    UninstallString = uninstallStr,
                    QuietUninstallString = uninstallStr,
                    RawDisplayName = string.IsNullOrEmpty(data["DisplayName"]) ? fullName : data["DisplayName"],
                    Publisher = data["PublisherDisplayName"],
                    IsValid = true,
                    UninstallerKind = UninstallerType.StoreApp,
                    InstallLocation = data["InstalledLocation"],
                    InstallDate = Directory.GetCreationTime(data["InstalledLocation"]),
                    IsProtected = isProtected,
                    SystemComponent = isProtected
                };

                if (File.Exists(data["Logo"]))
                {
                    try
                    {
                        result.DisplayIcon = data["Logo"];
                        result.IconBitmap = DrawingTools.IconFromImage(new Bitmap(data["Logo"]));
                    }
                    catch
                    {
                        result.DisplayIcon = null;
                        result.IconBitmap = null;
                    }
                }

                if (result.InstallLocation.StartsWith(windowsPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    result.SystemComponent = true;
                    //result.IsProtected = true;
                }

                results.Add(result);
            }

            return results;
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanStoreApps;
        public string DisplayName => Localisation.Progress_AppStores_WinStore;
    }
}