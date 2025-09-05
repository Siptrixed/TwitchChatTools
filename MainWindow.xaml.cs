using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TwitchChatTools.Model;
using TwitchChatTools.PageTabs;
using TwitchChatTools.Utils;

namespace TwitchChatTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskbarIcon TaskBarIcon = new TaskbarIcon();

        public MainWindow()
        {
            InitializeComponent();

            if (MainApp.Instance.Settings.SelectedLanguage == 0)
            {
                MainApp.Instance.Settings.SelectedLanguage = Thread.CurrentThread.CurrentCulture.LCID;
            }
            LangSelect.ItemsSource = Settings.Langs;
            LangSelect.SelectedValue = MainApp.Instance.Settings.SelectedLanguage;

            ShowProgress(Lang.Bind("InitCaption"), withLangSelect: true);

            TaskBarIcon.IconSource = LogoImage.Source;
            TaskBarIcon.ToolTipText = Title;
            TaskBarIcon.TrayLeftMouseUp += (x, y) => { Show(); WindowState = WindowState.Normal; Activate(); };

            CheckVersion();
        }

        private void CheckVersion()
        {
            SetProgress(Lang.Bind("CheckVersion"));

            Task.Run(async () =>
            {
                var version = await VersionControl.CheckVersionAsync();
                _ = MyAppExt.InvokeUI(() =>
                {
                    if (version != null)
                    {
                        ShowUpdate(version.Title);
                    }
                    else
                    {
                        _ = MyAppExt.InvokeUI(() =>
                        {
                            OnLoginClick(null, null);
                        });
                    }
                });
            });
        }

        private void OnWindowClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(this);
                if (pos.Y < 21) this.DragMove();
            }
        }
        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            if (MainApp.Instance.Settings.MinimizeToTray)
            {
                Hide();
            }
            else WindowState = WindowState.Minimized;
        }
        private void OnLoginClick(object? sender, RoutedEventArgs? e)
        {
            ShowProgress(e == null ? Lang.Bind("CheckTokenCaption") : Lang.Bind("OAuthProgressCaption"), withLangSelect: true);
            _ = Task.Run(async () =>
            {
                var authResult = await MainApp.Instance.TryToAuth(e != null);
                _ = MyAppExt.InvokeUI(() =>
                {
                    if (authResult)
                    {
                        if (MainApp.Instance.Settings.ConnectToUsername == null)
                        {
                            MainApp.Instance.Settings.ConnectToUsername = MainApp.Instance.Account.Login;
                            ChannelToConnectUser.Text = MainApp.Instance.Settings.ConnectToUsername;
                            ShowChannelSelect();
                        }
                        else
                        {
                            ChannelToConnectUser.Text = MainApp.Instance.Settings.ConnectToUsername;
                            OnChannelSelectClick(null, null);
                        }
                    }
                    else
                    {
                        ShowLogin();
                    }
                });
            });
        }
        private void OnChannelSelectClick(object? sender, RoutedEventArgs? e)
        {
            MainApp.Instance.Settings.ConnectToUsername = ChannelToConnectUser.Text.Trim().ToLower();
            ShowProgress(Lang.Bind("CheckChannelCaption"), withLangSelect: true);
            _ = Task.Run(async () =>
            {
                var loginExists = await MainApp.Instance.GetConnectToUserId();

                if (loginExists)
                {
                    _ = MyAppExt.InvokeUI(() => SetProgress(Lang.Bind("Connecting")));
                    await MainApp.Instance.Connect((bind) => MyAppExt.InvokeUI(() => SetProgress(bind)));
                }

                _ = MyAppExt.InvokeUI(() =>
                {
                    if (loginExists)
                    {
                        HideFullScreen();
                        if (SettingsFrame.Content is Settings currentPage)
                        {
                            currentPage.UpdateSettingValues();
                        }
                    }
                    else
                    {
                        ShowChannelSelect();
                    }
                });
            });
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }
        private void HideFullScreen()
        {
            FullWindowBanner.Visibility =
            PleaseWaitGrid.Visibility =
            LoginGrid.Visibility =
            ChannelToConnectSelector.Visibility = Visibility.Collapsed;
        }
        private void ShowUpdate(string newVersion)
        {
            CurrentVersionLabel.Content = VersionControl.VersionTitle;
            NewVersionLabel.Content = newVersion;

            LangSelect.Visibility = Visibility.Visible;
            ChannelToConnectSelector.Visibility = PleaseWaitGrid.Visibility = LoginGrid.Visibility = Visibility.Collapsed;
            FullWindowBanner.Visibility = UpdateGrid.Visibility = Visibility.Visible;
        }
        private void ShowLogin()
        {
            LangSelect.Visibility = Visibility.Visible;
            ChannelToConnectSelector.Visibility = PleaseWaitGrid.Visibility = UpdateGrid.Visibility = Visibility.Collapsed;
            FullWindowBanner.Visibility = LoginGrid.Visibility = Visibility.Visible;
        }
        private void ShowChannelSelect()
        {
            LangSelect.Visibility = Visibility.Visible;
            LoginGrid.Visibility = PleaseWaitGrid.Visibility = UpdateGrid.Visibility = Visibility.Collapsed;
            FullWindowBanner.Visibility = ChannelToConnectSelector.Visibility = Visibility.Visible;
        }
        private void ShowProgress(object? text = null, float procentage = -1, bool withLangSelect = false)
        {
            LangSelect.Visibility = withLangSelect ? Visibility.Visible : Visibility.Collapsed;
            var margin = HeaderProgressBar.Margin;
            margin.Right = withLangSelect ? 138 : 39;
            HeaderProgressBar.Margin = margin;
            ChannelToConnectSelector.Visibility = LoginGrid.Visibility = UpdateGrid.Visibility = Visibility.Collapsed;
            FullWindowBanner.Visibility = PleaseWaitGrid.Visibility = Visibility.Visible;
            SetProgress(text, procentage);
        }

        private void SetProgress(object? text = null, float procentage = -1)
        {
            HeaderProgressBar.Value = procentage;
            HeaderProgressBar.IsIndeterminate = procentage == -1;
            if (text == null) return;

            if (text is string txt && !string.IsNullOrEmpty(txt))
            {
                WaitLabel.Content = text;
            }
            else if (text is Binding bind)
            {
                WaitLabel.SetBinding(Label.ContentProperty, bind);
            }
            else
            {
                WaitLabel.SetBinding(Label.ContentProperty, Lang.Bind("Loading"));
            }
        }


        private void LangSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LangSelect.SelectedIndex == -1) return;
            MainApp.Instance.Settings.SelectedLanguage = (int)LangSelect.SelectedValue;
            MainApp.Instance.Settings.ApplyLanguage();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
            e.Handled = true;
        }

        private void UpdateLaterButton_Click(object sender, RoutedEventArgs e)
        {
            OnLoginClick(null, null);
        }

        private void UpdateNowButton_Click(object sender, RoutedEventArgs e)
        {
            ShowProgress(Lang.Bind("Downloading"), 0, withLangSelect: true);
            _ = Task.Run(async () =>
            {

                var downloadSuccess = await VersionControl.DownloadUpdateAsync((progress) =>
                {
                    MyAppExt.InvokeUI(() => SetProgress(null, progress * 100));
                });

                _ = MyAppExt.InvokeUI(() =>
                {
                    if (downloadSuccess)
                    {
                        VersionControl.ReplaceAndRelaunchApp();
                    }
                    else
                    {
                        OnLoginClick(null, null);
                    }
                });
            });
        }
    }
}