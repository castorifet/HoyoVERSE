using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HoyoVERSE.Models;
using HoyoVERSE.Services;
using HoyoVERSE.ViewModels;
using ModernWpf.Controls;

namespace HoyoVERSE.Views
{
    public partial class GameSettingsDialog : ContentDialog
    {
        readonly GameInfo _info;
        readonly MainViewModel _vm;
        bool _removeRequested;

        public GameSettingsDialog(GameInfo info, MainViewModel vm)
        {
            InitializeComponent();
            _info = info;
            _vm = vm;
            Loaded += (s, e) => Populate();
            PrimaryButtonClick += OnPrimary;
            Closed += OnClosed;
        }

        void Populate()
        {
            Title = Loc.Instance.Format("SettingsForFmt", _info.Name ?? string.Empty);

            // Name field — only for custom games
            NameSection.Visibility = _info.IsCustom ? Visibility.Visible : Visibility.Collapsed;
            NameBox.Text = _info.Name ?? string.Empty;

            ExeBox.Text = _info.ExePath ?? string.Empty;
            BrowseBtn.IsEnabled = _info.IsCustom; // built-in games use the main "Locate..." button

            // Unity status hint
            UnityHint.Text = _info.IsUnity
                ? Loc.Instance.Format("UnityDetectedFmt",
                                _info.UnityCompany ?? "?", _info.UnityProduct ?? "?")
                : Loc.Instance["UnityNotDetected"];

            FillEnumCombo<GraphicsApi>(GraphicsApiBox, _info.Settings.GraphicsApi);
            FillEnumCombo<WindowMode>(WindowModeBox, _info.Settings.WindowMode);
            FillEnumCombo<VsyncMode>(VsyncBox, _info.Settings.VSync);

            WidthBox.Value = _info.Settings.Width;
            HeightBox.Value = _info.Settings.Height;

            FpsSection.Visibility = _info.SupportsFpsUnlock ? Visibility.Visible : Visibility.Collapsed;
            if (_info.SupportsFpsUnlock)
            {
                var existing = _info.Settings.TargetFps;
                if (existing == 0) existing = HoyoTweaks.TryReadHsrFps();
                FpsBox.Value = existing;
            }

            CustomArgsBox.Text = _info.Settings.CustomArgs ?? string.Empty;

            WarpHistoryBtn.Visibility = WarpHistoryService.IsSupported(_info)
                ? Visibility.Visible : Visibility.Collapsed;

            RemoveBtn.Visibility = _info.IsCustom ? Visibility.Visible : Visibility.Collapsed;
        }

        async void WarpHistory_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WarpHistoryDialog(_info);
            await dlg.ShowAsync();
        }

        static void FillEnumCombo<T>(ComboBox box, T current)
        {
            box.Items.Clear();
            foreach (var v in Enum.GetValues(typeof(T)))
                box.Items.Add(v);
            box.SelectedItem = current;
        }

        void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = Loc.Instance["ExeFilter"],
                Title = Loc.Instance["ChooseExeTitle"]
            };
            if (dlg.ShowDialog() == true)
            {
                ExeBox.Text = dlg.FileName;
                UnityHint.Text = UnityHelper.IsUnityGame(dlg.FileName)
                    ? Loc.Instance["UnityDetectedSimple"]
                    : Loc.Instance["NotUnityGame"];
            }
        }

        void Remove_Click(object sender, RoutedEventArgs e)
        {
            _removeRequested = true;
            Hide();
        }

        void OnPrimary(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_removeRequested) return;

            // Persist name + exe (custom games only)
            if (_info.IsCustom)
            {
                if (!string.IsNullOrWhiteSpace(NameBox.Text) && NameBox.Text != _info.Name)
                    _vm.RenameCustomGame(_info, NameBox.Text);

                if (!string.IsNullOrWhiteSpace(ExeBox.Text) && ExeBox.Text != _info.ExePath && File.Exists(ExeBox.Text))
                {
                    _info.ExePath = ExeBox.Text;
                    _info.IsUnity = UnityHelper.IsUnityGame(ExeBox.Text);
                    var ai = UnityHelper.TryReadAppInfo(ExeBox.Text);
                    if (ai != null) { _info.UnityCompany = ai.Company; _info.UnityProduct = ai.Product; }
                    var saved = SettingsStore.GetCustomGames().Find(c => c.Id == _info.Id);
                    if (saved != null) { saved.ExePath = ExeBox.Text; SettingsStore.UpdateCustomGame(saved); }
                }
            }

            _info.Settings.GraphicsApi = (GraphicsApi)(GraphicsApiBox.SelectedItem ?? GraphicsApi.Auto);
            _info.Settings.WindowMode = (WindowMode)(WindowModeBox.SelectedItem ?? WindowMode.Default);
            _info.Settings.VSync = (VsyncMode)(VsyncBox.SelectedItem ?? VsyncMode.Default);
            _info.Settings.Width = (int)Math.Round(WidthBox.Value);
            _info.Settings.Height = (int)Math.Round(HeightBox.Value);
            if (_info.SupportsFpsUnlock)
                _info.Settings.TargetFps = (int)Math.Round(FpsBox.Value);
            _info.Settings.CustomArgs = CustomArgsBox.Text ?? string.Empty;

            _vm.SaveSettings(_info);
        }

        void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (_removeRequested)
                _vm.RemoveCustomGame(_info);
        }
    }
}
