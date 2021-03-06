﻿using Astrarium.Algorithms;
using Astrarium.Config.Controls;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium
{
    public class UIElementsIntegration
    {
        public UIElementsConfig<string, ToolbarButtonBase> ToolbarButtons { get; } = new UIElementsConfig<string, ToolbarButtonBase>();
        public UIElementsConfig<MenuItemPosition, MenuItem> MenuItems { get; } = new UIElementsConfig<MenuItemPosition, MenuItem>();
        public UIElementsConfig<string, SettingItem> SettingItems { get; } = new UIElementsConfig<string, SettingItem>();

        public UIElementsIntegration()
        {
            // Default language
            SettingItems.Add("UI", new SettingItem("Language", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower(), typeof(LanguageSettingControl)));

            // Flag indicating the app should be started with maximized main window
            SettingItems.Add("UI", new SettingItem("StartMaximized", false));

            // Type of application menu
            SettingItems.Add("UI", new SettingItem("IsCompactMenu", false));

            // Toolbar visibility
            SettingItems.Add("UI", new SettingItem("IsToolbarVisible", true));

            // Status bar visibility
            SettingItems.Add("UI", new SettingItem("IsStatusBarVisible", true));

            // Default observer location.
            // Has no section, so not displayed in settings window.
            SettingItems.Add(null, new SettingItem("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod")));

            // Default color schema
            SettingItems.Add("Colors", new SettingItem("Schema", ColorSchema.Night));
        }
    }
}
