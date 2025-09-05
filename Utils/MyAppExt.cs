using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace TwitchChatTools.Utils
{
    public static class MyAppExt
    {
        public static byte[] HWID = Guid.Parse(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001", "HwProfileGuid", string.Empty)?.ToString() ?? string.Empty).ToByteArray();
        public static DispatcherOperation? InvokeUI(Action act) => Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Background, act);
        public static void RunCMD(string scripd)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(scripd);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
        }
        public static int TrueRandom(int min, int max) => TrueRandom(min, max, 1)[0];
        public static int[] TrueRandom(int min, int max, int count = 1)
        {
            List<int> vs = new List<int>();
            if (min > max)
            {
                int p = max;
                max = min;
                min = p;
            }
            if (max > min)
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string lol = client.DownloadString($"https://www.random.org/integers/?num={count}&min={min}&max={max}&col=1&base=10&format=plain&rnd=new");
                    foreach (string num in lol.Split('\n'))
                    {
                        if (string.IsNullOrEmpty(num)) continue;
                        int.TryParse(num.Trim(), out int ret);
                        while (vs.Contains(ret))
                        {
                            ret++;
                            if (ret > max && max - min >= count)
                                ret = min;
                        }
                        vs.Add(ret);
                    }
                }
            else
            {
                for (int s = 0; s < count; s++)
                {
                    while (vs.Contains(min))
                    {
                        min++;
                        //if (min > max && max - min >= count)
                        //   min = min;
                    }
                    vs.Add(min);
                }
            }

            return vs.ToArray();
        }

        public static string GetTimeFromMilliseconds(int milliseconds, bool allowEmpty = false)
        {
            if (milliseconds == 0 && allowEmpty) return "";
            if (milliseconds < 1000)
            {
                return $"{milliseconds}ms";
            }
            if (milliseconds < 60000)
            {
                return $"{milliseconds / 1000}s {GetTimeFromMilliseconds(milliseconds % 1000, true)}";
            }
            if (milliseconds < 3600000)
            {
                return $"{milliseconds / 60000:N0}min {GetTimeFromMilliseconds(milliseconds % 60000, true)}";
            }
            return $"{milliseconds / 3600000:N0}h {GetTimeFromMilliseconds(milliseconds % 3600000, true)}";
        }
        public static async Task CopyToAsync(this Stream source, Stream destination, Action<float>? progress, long totalBytes, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));

            int bufferSize = 81920;

            if (source.CanSeek)
            {
                long length = totalBytes;
                long position = source.Position;
                if (length <= position) // Handles negative overflows
                {
                    // There are no bytes left in the stream to copy.
                    // However, because CopyTo{Async} is virtual, we need to
                    // ensure that any override is still invoked to provide its
                    // own validation, so we use the smallest legal buffer size here.
                    bufferSize = 1;
                }
                else
                {
                    long remaining = length - position;
                    if (remaining > 0)
                    {
                        // In the case of a positive overflow, stick to the default size
                        bufferSize = (int)Math.Min(bufferSize, remaining);
                    }
                }
            }

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;

            float totalBytesAll = totalBytes;
            float oldProgressValue = 0;
            float progressValue = 0;

            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;

                progressValue = totalBytesRead / totalBytesAll;
                if (progressValue - oldProgressValue > 0.01)
                {
                    oldProgressValue = progressValue;
                    progress?.Invoke(progressValue);
                }
            }
        }
    }
}
