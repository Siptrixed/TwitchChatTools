using System.Windows;
using TwitchChatTools.Model;
using TwitchChatTools.Model.WebServer;

namespace TwitchChatTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainApp.InitializeInstance();

            MainApp.Instance?.Settings.ApplyLanguage();
            
            CustomWebServer.InitializeAndRunInstance();
        }
    }

}
