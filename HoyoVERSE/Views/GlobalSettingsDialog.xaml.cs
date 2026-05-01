using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using HoyoVERSE.Models;
using HoyoVERSE.ViewModels;
using ModernWpf.Controls;

namespace HoyoVERSE.Views
{
    public partial class GlobalSettingsDialog : ContentDialog
    {
        readonly MainViewModel _vm;
        bool _populating;
        bool _dirty;

        public GlobalSettingsDialog(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            Loaded += (s, e) => Populate();
            Closed += OnClosed;
        }

        void Populate()
        {
            _populating = true;
            try
            {
                var current = _vm.Language ?? "en-us";
                LangBox.SelectedIndex = 0;
                foreach (ComboBoxItem item in LangBox.Items)
                {
                    if (string.Equals(item.Tag as string, current, StringComparison.OrdinalIgnoreCase))
                    {
                        LangBox.SelectedItem = item;
                        break;
                    }
                }
                RegionGlobalBtn.IsChecked = _vm.Region == HypRegion.Global;
                RegionChinaBtn.IsChecked = _vm.Region == HypRegion.China;
            }
            finally
            {
                _populating = false;
            }
        }

        void LangBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_populating) return;
            var lang = LangBox.SelectedValue as string;
            if (string.IsNullOrEmpty(lang) || lang == _vm.Language) return;
            _vm.Language = lang;
            _dirty = true;
        }

        void RegionGlobal_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (_vm.Region == HypRegion.Global)
            {
                btn.IsChecked = true;
                return;
            }
            _vm.Region = HypRegion.Global;
            RegionGlobalBtn.IsChecked = true;
            RegionChinaBtn.IsChecked = false;
            _dirty = true;
        }

        void RegionChina_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (_vm.Region == HypRegion.China)
            {
                btn.IsChecked = true;
                return;
            }
            _vm.Region = HypRegion.China;
            RegionChinaBtn.IsChecked = true;
            RegionGlobalBtn.IsChecked = false;
            _dirty = true;
        }

        async void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            // Reload the catalog/news once after the dialog closes so we don't
            // re-fetch on every toggle while the dialog is still open.
            if (_dirty)
            {
                _dirty = false;
                await _vm.InitializeAsync();
            }
        }
    }
}
