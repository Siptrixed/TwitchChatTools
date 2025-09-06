using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwitchChatTools.Model;
using TwitchChatTools.Model.Utils;
using TwitchChatTools.Model.WinApi;
using TwitchChatTools.Model.Twitch;

namespace TwitchChatTools.PageTabs
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        int keyMode = -1;
        public static readonly Dictionary<int, string> Langs = new() {
            { 9 , "English" },
            { 1049 , "Русский" }
        };

        public Settings()
        {
            InitializeComponent();

            LangSelect.ItemsSource = Langs;
            VersionLabel.Content = VersionControl.VersionTitle;
            //UpdateSettingValues();
        }

        private (Key key, KeyModifier mod)? HotkeySelectorKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift
                        || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl
                        || e.Key == Key.LeftAlt || e.Key == Key.RightAlt) keyMode = -1;
            else
            {
                Key key = e.Key;
                if (key == Key.System)
                {
                    key = e.SystemKey;
                    keyMode = 0;
                }
                else if (keyMode == 0)
                    keyMode = -1;

                try
                {
                    KeyModifier mod = KeyModifier.None;
                    switch (keyMode)
                    {
                        case 0:
                            mod = KeyModifier.Alt;
                            break;
                        case 1:
                            mod = KeyModifier.Ctrl;
                            break;
                        case 2:
                            mod = KeyModifier.Shift;
                            break;
                    }
                    return (key, mod);
                }
                catch
                {

                }
            }
            return null;
        }
        private void HotkeySelectorKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) keyMode = 2;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) keyMode = 1;
        }

        public void UpdateSettingValues()
        {
            StopSoundHotkeySelector.Text = MainApp.Instance?.Settings.StopSoundHotkey.ToString();
            Settings_MinimizeToTray.IsChecked = MainApp.Instance?.Settings.MinimizeToTray;
            LangSelect.SelectedValue = MainApp.Instance?.Settings.SelectedLanguage;
            TwitchDataLabel.Text = $"{Lang.Instance["Account"]}: {MainApp.Instance?.Account.Login} ({MainApp.Instance?.Account.UserID})\r\n" +
                $"{Lang.Instance["Channel"]}: {MainApp.Instance?.Settings.ConnectToUsername} ({MainApp.Instance?.Settings.ConnectToUserId})";
        }

        public void ExitFromTwitchAndClose(object sender, RoutedEventArgs e)
        {
            if (MainApp.Instance != null)
            {
                MainApp.Instance.Account = new TwitchAccount();
            }

            Process.Start(ObjectFileSystem.AppFile);
            Application.Current.Shutdown();
        }

        private void ChangeConnectedChannel(object sender, RoutedEventArgs e)
        {
            if (MainApp.Instance != null)
            {
                MainApp.Instance.Settings.ConnectToUsername = null;
                MainApp.Instance.Settings.ConnectToUserId = null;
            }

            Process.Start(ObjectFileSystem.AppFile);
            Application.Current.Shutdown();
        }
        public void MinimizeToTrayChanged(object sender, RoutedEventArgs e)
        {
            if (MainApp.Instance != null)
            {
                MainApp.Instance.Settings.MinimizeToTray = Settings_MinimizeToTray.IsChecked!.Value;
            }
        }
        public void StopSoundHotkeySettingKeyDown(object sender, KeyEventArgs e)
        {
            HotkeySelectorKeyDown(e);
        }

        public void StopSoundHotkeySettingKeyUp(object sender, KeyEventArgs e)
        {
            var selected = HotkeySelectorKeyUp(sender, e);
            if (selected != null && selected.HasValue)
            {
                HotKeyManager.SetStopSound(selected.Value.key, selected.Value.mod);
                StopSoundHotkeySelector.Text = MainApp.Instance?.Settings.StopSoundHotkey?.ToString() ?? string.Empty;
            }
        }
        public void StopSoundClearHotkey(object sender, RoutedEventArgs e)
        {
            HotKeyManager.ClearStopSound();
            StopSoundHotkeySelector.Text = MainApp.Instance?.Settings.StopSoundHotkey?.ToString() ?? string.Empty;
        }

        private void LangSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainApp.Instance == null) return;

            if (LangSelect.SelectedIndex == -1) return;

            MainApp.Instance.Settings.SelectedLanguage = (int)LangSelect.SelectedValue;
            MainApp.Instance.Settings.ApplyLanguage();

            UpdateSettingValues();
        }
    }
}
