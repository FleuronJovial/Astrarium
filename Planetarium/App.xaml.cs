﻿using ADK;
using Ninject;
using Planetarium.Calculators;
using Planetarium.Config;
using Planetarium.Config.ControlBuilders;
using Planetarium.Renderers;
using Planetarium.Types;
using Planetarium.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Baml2006;
using System.Xaml;

namespace Planetarium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel kernel = new StandardKernel();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureContainer();

            Dispatcher.UnhandledException += (s, ea) =>
            {
                kernel.Get<IViewManager>().ShowMessageBox("Error", $"An unhandled exception occurred:\n\n{ea.Exception.Message}\nStack trace:\n\n{ea.Exception.StackTrace}", MessageBoxButton.OK);
                ea.Handled = true;
            };

            kernel.Get<IViewManager>().ShowWindow<MainVM>();
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ConfigureContainer()
        {
            SettingsConfig settingsConfig = new SettingsConfig();

            settingsConfig.Add("EquatorialGrid", true).WithSection("Grid");
            settingsConfig.Add("LabelEquatorialPoles", false).EnabledWhenTrue("EquatorialGrid").WithSection("Grid");

            settingsConfig.Add("HorizontalGrid", true).WithSection("Grid");
            settingsConfig.Add("LabelHorizontalPoles", false).EnabledWhenTrue("HorizontalGrid").WithSection("Grid");
            settingsConfig.Add("EclipticLine", true).WithSection("Grid");
            settingsConfig.Add("LabelEquinoxPoints", false).EnabledWhenTrue("EclipticLine").WithSection("Grid");
            settingsConfig.Add("LabelLunarNodes", false).EnabledWhenTrue("EclipticLine").WithSection("Grid");
            settingsConfig.Add("GalacticEquator", true).WithSection("Grid");
            settingsConfig.Add("MilkyWay", true).WithSection("Grid");
            settingsConfig.Add("Ground", true).WithSection("Grid");
            settingsConfig.Add("HorizonLine", true).WithSection("Grid");
            settingsConfig.Add("LabelCardinalDirections", true).EnabledWhenTrue("HorizonLine").WithSection("Grid");

            settingsConfig.Add("Sun", true).WithSection("Sun");
            settingsConfig.Add("LabelSun", true).EnabledWhenTrue("Sun").WithSection("Sun");
            settingsConfig.Add("SunLabelFont", new Font("Arial", 12)).EnabledWhen(s => s.Get<bool>("Sun") && s.Get<bool>("LabelSun")).WithSection("Sun");
            settingsConfig.Add("TextureSun", true).EnabledWhenTrue("Sun").WithSection("Sun");
            settingsConfig.Add("TextureSunPath", "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/hmiigr/{yyyy}{MM}{dd}/{yyyy}{MM}{dd}_0000_hmiigr_512.jpg").EnabledWhen(s => s.Get<bool>("Sun") && s.Get<bool>("TextureSun")).WithSection("Sun");

            settingsConfig.Add("ConstLabels", true).WithSection("Constellations");
            settingsConfig.Add("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName).EnabledWhenTrue("ConstLabels").WithSection("Constellations");
            settingsConfig.Add("ConstLines", true).WithSection("Constellations");
            settingsConfig.Add("ConstBorders", true).WithSection("Constellations");

            settingsConfig.Add("Stars", true).WithSection("Stars");
            settingsConfig.Add("StarsLabels", true).EnabledWhenTrue("Stars").WithSection("Stars");
            settingsConfig.Add("StarsProperNames", true).EnabledWhen(s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels")).WithSection("Stars");

            settingsConfig.Add("EclipticColorNight", Color.FromArgb(0xC8, 0x80, 0x80, 0x00)).WithSection("Colors");
            settingsConfig.Add("HorizontalGridColorNight", Color.FromArgb(0xC8, 0x00, 0x40, 0x00)).WithSection("Colors");
            settingsConfig.Add("CardinalDirectionsColor", Color.FromArgb(0x00, 0x99, 0x99)).WithSection("Colors");

            settingsConfig.Add("UseTextures", true).WithSection("Misc");

            settingsConfig.Add("Planets", true).WithSection("Planets");
            settingsConfig.Add("JupiterMoonsShadowOutline", true).WithSection("Planets");
            settingsConfig.Add("ShowRotationAxis", false).WithSection("Planets");

            settingsConfig.Add("GRSLongitude", 
                new GreatRedSpotSettings()
                {
                    Epoch = 2458150.5000179596,
                    MonthlyDrift = 2,
                    Longitude = 283
                })
                .WithSection("Planets")
                .WithBuilder(typeof(GRSSettingBuilder));

            settingsConfig.Add("Comets", true).WithSection("Comets");
            settingsConfig.Add("CometsLabels", true).WithSection("Comets").EnabledWhenTrue("Comets");
            settingsConfig.Add("Asteroids", true).WithSection("Asteroids");
            settingsConfig.Add("AsteroidsLabels", true).WithSection("Asteroids").EnabledWhenTrue("Asteroids");

            settingsConfig.Add("DeepSky", true).WithSection("Deep Sky");
            settingsConfig.Add("DeepSkyLabels", true).WithSection("Deep Sky").EnabledWhenTrue("DeepSky");
            settingsConfig.Add("DeepSkyOutlines", true).WithSection("Deep Sky").EnabledWhenTrue("DeepSky");

            settingsConfig.Add("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod"));

            kernel.Bind<ISettingsConfig, SettingsConfig>().ToConstant(settingsConfig).InSingletonScope();

            var settings = new Settings();
            settings.SetDefaults(settingsConfig.GetDefaultSettings());

            kernel.Bind<ISettings, Settings>().ToConstant(settings).InSingletonScope();

            kernel.Get<Settings>().Load();

            SkyContext context = new SkyContext(
                new Date(DateTime.Now).ToJulianEphemerisDay(),
                new CrdsGeographical(settings.Get<CrdsGeographical>("ObserverLocation")));

            // TODO: consider more proper way to load plugins
            string homeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            IEnumerable<string> pluginPaths = Directory.EnumerateFiles(homeFolder, "*.dll");

            foreach (string path in pluginPaths)
            {
                try
                {
                    Assembly.LoadFrom(path);
                }
                catch (Exception ex)
                {
                    // TODO: log
                }
            }

            var alltypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

            // collect all calculators implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for calculators there
            Type[] calcTypes = alltypes
                .Where(t => typeof(BaseCalc).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();

            foreach (Type calcType in calcTypes)
            {
                var types = calcType.GetInterfaces().ToList();
                if (types.Any())
                {
                    // each interface that calculator implements
                    // should be bound to the calc instance
                    types.Add(calcType);
                    kernel.Bind(types.ToArray()).To(calcType).InSingletonScope();
                }
            }

            // collect all calculators implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for renderers there
            Type[] rendererTypes = alltypes
                .Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            foreach (Type rendererType in rendererTypes)
            {
                kernel.Bind(rendererType).ToSelf().InSingletonScope();
            }

            // collect all event provider implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for providers there
            Type[] eventProviderTypes = alltypes
                .Where(t => typeof(BaseAstroEventsProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();

            foreach (Type eventProviderType in eventProviderTypes)
            {
                kernel.Bind(eventProviderType).ToSelf().InSingletonScope();
            }

            var calculators = calcTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseCalc>()
                .ToArray();

            var eventProviders = eventProviderTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseAstroEventsProvider>()
                .ToArray();

            kernel.Bind<Sky, ISearcher, IEphemerisProvider>().ToConstant(new Sky(context, calculators, eventProviders)).InSingletonScope();

            var renderers = rendererTypes
                .Select(r => kernel.Get(r))
                .Cast<BaseRenderer>()
                .OrderBy(r => r.ZOrder)
                .ToArray();

            kernel.Bind<ISkyMap>().ToConstant(new SkyMap(context, renderers)).InSingletonScope();
            kernel.Bind<IViewManager>().ToConstant(new ViewManager(t => kernel.Get(t))).InSingletonScope();
        }

        private static T LoadBaml<T>(Stream stream)
        {
            var reader = new Baml2006Reader(stream);
            var writer = new XamlObjectWriter(reader.SchemaContext);
            while (reader.Read())
            {
                writer.WriteNode(reader);
            }

            if (writer.Result is T)
            {
                return (T)writer.Result;
            }
            else { return default(T); }
        }

    }
}
