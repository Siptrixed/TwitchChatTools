using MessagePack;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Input;

namespace TwitchChatTools.Utils
{
    internal static class ObjectFileSystem
    {
        public static string AppFile => $"{AppContext.BaseDirectory}TwitchChatTools.exe";
        public static string AppDataDirectory => $"{AppContext.BaseDirectory}Data";
        public static string AppDirectory => Path.GetDirectoryName(AppContext.BaseDirectory) ?? AppContext.BaseDirectory;

        private static MessagePackSerializerOptions MessagePackOptions = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        static ObjectFileSystem()
        {
            if (!Directory.Exists(AppDataDirectory))
            {
                Directory.CreateDirectory(AppDataDirectory);
            }
        }

        private static bool TrySaveObjToFile<T>(string filename, T obj, bool protect = false)
        {
            try
            {
                if (File.Exists(filename)) File.Delete(filename);

                if (protect)
                {
                    var bytes = MessagePackSerializer.Serialize(obj, MessagePackOptions);
                    bytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                    File.WriteAllBytes(filename, bytes);
                }
                else
                {
                    File.WriteAllText(filename, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static bool TryReadObjFromFile<T>(string filename, out T? obj, bool protect = false)
        {
            try
            {

                if (File.Exists(filename))
                {
                    if (protect)
                    {
                        var bytes = File.ReadAllBytes(filename);
                        bytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
                        obj = MessagePackSerializer.Deserialize<T>(bytes, MessagePackOptions);
                    }
                    else
                    {
                        obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
                    }
                    return true;
                }
                else
                {
                    obj = default;
                    return false;
                }
            }
            catch
            {
                obj = default;
                return false;
            }
        }
        public static bool SaveObject<T>(T obj, bool protect = false, string? name = null, bool backup = false)
        {
            if (name == null) name = typeof(T).Name;
            var ext = protect ? "bin" : "json";

            var saveFile = $"{AppDataDirectory}\\{name}.{ext}";

            if (backup && File.Exists(saveFile))
            {
                var backupFile = $"{AppDataDirectory}\\{name}.{ext}.bak";
                File.Move(saveFile, backupFile);
            }

            return TrySaveObjToFile(saveFile, obj, protect);
        }
        public static T LoadObject<T>(bool protect = false, string? name = null) where T : new()
        {
            if (name == null) name = typeof(T).Name;
            var ext = protect ? "bin" : "json";

            var saveFile = $"{AppDataDirectory}\\{name}.{ext}";
            bool success = TryReadObjFromFile<T>(saveFile, out T? Obj, protect);
            if (success) return Obj!;
            else if(File.Exists(saveFile)) File.Delete(saveFile);

            var backupFile = $"{AppDataDirectory}\\{name}.{ext}.bak";
            success = TryReadObjFromFile<T>(backupFile, out T? Obj2, protect);
            if (success) return Obj2!;

            return new T();
        }
    }
}
