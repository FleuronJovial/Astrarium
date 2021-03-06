﻿using Astrarium.Types.Themes;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class PhenomenaSettingsVM : ViewModelBase
    {
        private readonly ISky sky;

        public ObservableCollection<Node> Nodes { get; private set; } = new ObservableCollection<Node>();
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public double UtcOffset { get; private set; }

        private static Cache cache;

        public IEnumerable<string> Categories
        {
            get
            {
                return
                    AllNodes(Nodes.First())
                        .Where(n => n.IsChecked ?? false)
                        .Select(n => n.Id);
            }
        }

        public bool OkButtonEnabled
        {
            get
            {
                return Nodes.Any() && Nodes.First().IsChecked != false;
            }
        }

        public PhenomenaSettingsVM(ISky sky)
        {
            this.sky = sky;

            JulianDayFrom = cache?.JdFrom ?? sky.Context.JulianDay;
            JulianDayTo = cache?.JdTo ?? sky.Context.JulianDay + 30;

            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);

            BuildCategoriesTree();
        }

        public void Ok()
        {
            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("$PhenomenaSettingsWindow.WarningTitle", "$PhenomenaSettingsWindow.WarningText", System.Windows.MessageBoxButton.OK);
                return;
            }

            // save selected values to cache
            cache = new Cache() 
            { 
                JdFrom = JulianDayFrom,
                JdTo = JulianDayTo,
                Categories = Categories.ToArray()
            };

            Close(true);
        }

        private void BuildCategoriesTree()
        {
            Nodes.Clear();

            var categories = sky.GetEventsCategories();
            var groups = categories.GroupBy(cat => cat.Split('.').First());

            Node root = new Node(Text.Get("PhenomenaSettingsWindow.Phenomena.All"));
            root.CheckedChanged += Root_CheckedChanged;

            foreach (var group in groups)
            {
                Node node = new Node(Text.Get(group.Key), group.Key);
                foreach (var item in group)
                {
                    if (item != group.Key)
                    {
                        node.Children.Add(new Node(Text.Get(item), item));
                    }
                }
                root.Children.Add(node);
            }

            Nodes.Add(root);

            if (cache != null)
            {
                foreach (var node in AllNodes(Nodes.First()))
                {
                    node.IsChecked = cache.Categories.Contains(node.Id);
                }
            }
        }

        private IEnumerable<Node> AllNodes(Node node)
        {
            yield return node;

            foreach (Node child in node.Children)
            {
                foreach (Node n in AllNodes(child))
                {
                    yield return n;
                }
            }
        }

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }

        private class Cache
        {
            public double JdFrom { get; set; }
            public double JdTo { get; set; }
            public string[] Categories { get; set; }
        }
    }
}
