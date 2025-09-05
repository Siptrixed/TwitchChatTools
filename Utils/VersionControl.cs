using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace TwitchChatTools.Utils
{
    internal static class VersionControl
    {
        public const string AppRepository = "Siptrixed/TwitchChatTools";

        public const string VersionStage = "Pre-Alpha";
        public static string? Version => System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();

        public static string VersionTitle => $"{VersionStage} {Version}";

        public static VersionInfo? NewestVersion = null;
        public static async Task<VersionInfo?> CheckVersionAsync(bool silentUpdate = false)
        {
#if DEBUG
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", $"{AppRepository.Replace("/", ".")} VersionControl");
                    string GitHubData = await client.GetStringAsync($"https://api.github.com/repos/{AppRepository}/releases");
                    var Json = JToken.Parse(GitHubData);
                    var AllReleases = Json.ToList();
                    if (AllReleases.Count > 0)
                    {
                        var LastRelease = AllReleases[0];
                        string NewestVersion = LastRelease["tag_name"]?.ToString() ?? string.Empty;
                        string NewestVersionTitle = LastRelease["name"]?.ToString() ?? string.Empty;
                        if (CheckVersion(NewestVersion, Version))
                        {
                            var NewVer = new VersionInfo();
                            NewVer.Version = NewestVersion;
                            NewVer.Title = NewestVersionTitle;
                            foreach (var asset in LastRelease["assets"]?.ToList() ?? [])
                            {
                                if (asset["content_type"]?.ToString() == "application/x-zip-compressed")
                                {
                                    NewVer.Name = asset["name"]?.ToString() ?? string.Empty;
                                    if (CheckIsCorrectOS(NewVer.Name))
                                    {
                                        NewVer.DownloadURL = asset["browser_download_url"]?.ToString() ?? string.Empty;
                                        VersionControl.NewestVersion = NewVer;
                                        return NewVer;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
#endif
            return null;
        }

        public static async Task<bool> DownloadUpdateAsync(Action<float> progress)
        {
            if (NewestVersion == null) return false;
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(NewestVersion.DownloadURL, HttpCompletionOption.ResponseHeadersRead);
                    var totalBytes = response.Content.Headers.ContentLength ?? 0;

                    if (totalBytes <= 0) return false;
                    
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var stream = File.Create("Update.zip"))
                    {
                        await contentStream.CopyToAsync(stream, progress, totalBytes);
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public static void ReplaceAndRelaunchApp()
        {
            MyAppExt.InvokeUI(() =>
            {
                Application.Current.Shutdown();
                var cmd = string.Empty;
                if (File.Exists("Update.zip"))
                {
                    cmd = "timeout 2 & tar -xf Update.zip & del Update.zip & TwitchChatTools.exe";
                }
                else
                {
                    cmd = "TwitchChatTools.exe";
                }
                Process.Start(new ProcessStartInfo($"cmd") { 
                    Arguments = $"/c \"{cmd}\"", 
                    UseShellExecute = false, 
                    CreateNoWindow = true 
                });
            });
        }

        private static bool CheckIsCorrectOS(string PackName)
        {
            PackName = PackName.ToLower();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (!PackName.Contains("win")) return false;
                if (RuntimeInformation.OSArchitecture == Architecture.X86)
                {
                    if (!PackName.Contains("x86") && !PackName.Contains("32bit")) return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private static bool CheckVersion(string newest, string? current)
        {
            if (string.IsNullOrEmpty(current)) return true;
            string[] VFc = current.Split('.');
            string[] VFl = newest.Split('.');
            bool oldVer = false;
            for (int i = 0; i < VFc.Length; i++)
            {
                if (VFc[i] != VFl[i])
                {
                    int.TryParse(VFc[i], out int VFci);
                    int.TryParse(VFl[i], out int VFli);
                    if (VFli > VFci) oldVer = true;
                }
            }
            return oldVer;
        }

        public class VersionInfo
        {
            public string Version { get; set; } = VersionControl.Version ?? "0.0.0.0";
            public string Title { get; set; } = string.Empty;
            public string Name { get; set; } = "Undefined";
            public string DownloadURL { get; set; } = string.Empty;

        }
    }
}
