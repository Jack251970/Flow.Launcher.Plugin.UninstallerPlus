﻿/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using Klocman.Binding.Settings;

namespace BulkCrapUninstaller.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
#if WPF_TEST
    public sealed partial class Settings
#else
    internal sealed partial class Settings
#endif
    {
        private SettingBinder<Settings> _settingManager;

        public SettingBinder<Settings> SettingBinder
            => _settingManager ??= new SettingBinder<Settings>(this);
    }
}