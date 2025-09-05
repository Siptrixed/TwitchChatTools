using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace TwitchChatTools.Utils
{
    public class Lang : INotifyPropertyChanged
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("TwitchChatTools.Properties.Resources", typeof(Lang).Assembly);

        private static CultureInfo _currentCulture = CultureInfo.CurrentCulture;

        public static Lang Instance { get; } = new Lang();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string this[string key] => _resourceManager.GetString(key, _currentCulture) ?? string.Empty;

        public void SetCulture(CultureInfo culture)
        {
            _currentCulture = culture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public static Binding Bind(string path)
        {
            return new Binding()
            {
                Source = Instance,
                Path = new System.Windows.PropertyPath($"[{path}]")
            };
        }
        public static string Get(string path)
        {
            return Instance[path];
        }
    }
}
