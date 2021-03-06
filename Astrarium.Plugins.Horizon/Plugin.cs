﻿using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Horizon
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            SettingItems.Add("Grids", new SettingItem("Ground", true));
            SettingItems.Add("Grids", new SettingItem("HorizonLine", true));
            SettingItems.Add("Grids", new SettingItem("LabelCardinalDirections", true, s => s.Get<bool>("HorizonLine")));
            SettingItems.Add("Colors", new SettingItem("ColorCardinalDirections", Color.FromArgb(0x00, 0x99, 0x99)));
            SettingItems.Add("Colors", new SettingItem("ColorHorizon", Color.FromArgb(0xC8, 0x00, 0x40, 0x00)));

            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconGround", "$Settings.Ground", new SimpleBinding(settings, "Ground", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
