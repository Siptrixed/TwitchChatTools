using Hardcodet.Wpf.TaskbarNotification;
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
            LangSelect.SelectedIndex = Array.IndexOf(Settings.Langs, MainApp.Instance.Settings.SelectedLanguage);

            ShowProgress(Lang.Bind("InitCaption"), withLangSelect: true);

            TaskBarIcon.IconSource = LogoImage.Source;
            TaskBarIcon.ToolTipText = Title;
            TaskBarIcon.TrayLeftMouseUp += (x, y) => { Show(); WindowState = WindowState.Normal; Activate(); };

            OnLoginClick(null, null);
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
                    _ = MyAppExt.InvokeUI(() => SetProgress("Подключение..."));
                    await MainApp.Instance.Connect();
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
        private void ShowLogin()
        {
            LangSelect.Visibility = Visibility.Visible;
            ChannelToConnectSelector.Visibility = PleaseWaitGrid.Visibility = Visibility.Collapsed;
            FullWindowBanner.Visibility = LoginGrid.Visibility = Visibility.Visible;
        }
        private void ShowChannelSelect()
        {
            LangSelect.Visibility = Visibility.Visible;
            LoginGrid.Visibility = PleaseWaitGrid.Visibility = Visibility.Collapsed;
            FullWindowBanner.Visibility = ChannelToConnectSelector.Visibility = Visibility.Visible;
        }
        private void ShowProgress(object? text = null, float procentage = 0, bool withLangSelect = false)
        {
            LangSelect.Visibility = withLangSelect ? Visibility.Visible : Visibility.Collapsed;
            var margin = HeaderProgressBar.Margin;
            margin.Right = withLangSelect ? 138 : 39;
            HeaderProgressBar.Margin = margin;
            ChannelToConnectSelector.Visibility = LoginGrid.Visibility = Visibility.Collapsed;
            FullWindowBanner.Visibility = PleaseWaitGrid.Visibility = Visibility.Visible;
            SetProgress(text, procentage);
        }

        private void SetProgress(object? text = null, float procentage = 0)
        {
            HeaderProgressBar.Value = procentage;
            HeaderProgressBar.IsIndeterminate = procentage == 0 || procentage == 100;
            if (text is string txt && !string.IsNullOrEmpty(txt))
            {
                WaitLabel.Content = text;
            }else if(text is Binding bind)
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
            MainApp.Instance.Settings.SelectedLanguage = Settings.Langs[LangSelect.SelectedIndex];
            MainApp.Instance.Settings.ApplyLanguage();
        }
    }
}