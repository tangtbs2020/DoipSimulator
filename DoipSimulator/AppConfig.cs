using System.Text.Json;

namespace DoipSimulator
{
    internal class AppConfig
    {
        public string? DataDB { get; set; }
        public AutoAnsSection? AutoAns { get; set; }

        public class AutoAnsSection
        {
            public Dictionary<string, List<string>> General { get; set; } = new();
            public Dictionary<string, List<List<string>>> Special { get; set; } = new();
        }

        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
                return new AppConfig();

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public static (Dictionary<string, byte[]> general, Dictionary<string, List<byte[]>> special)?
            BuildAutoReplyConfig()
        {
            var config = Load();
            if (config.AutoAns == null)
                return null;

            var general = new Dictionary<string, byte[]>();
            if (config.AutoAns.General != null)
            {
                foreach (var kv in config.AutoAns.General)
                {
                    try
                    {
                        general[kv.Key.ToUpperInvariant()] = kv.Value
                            .Select(ParseHexByte).ToArray();
                    }
                    catch { /* skip malformed entries */ }
                }
            }

            var special = new Dictionary<string, List<byte[]>>();
            if (config.AutoAns.Special != null)
            {
                foreach (var kv in config.AutoAns.Special)
                {
                    try
                    {
                        special[kv.Key.ToUpperInvariant()] = kv.Value
                            .Select(list => list.Select(ParseHexByte).ToArray())
                            .ToList();
                    }
                    catch { /* skip malformed entries */ }
                }
            }

            return (general, special);
        }

        private static byte ParseHexByte(string hexStr)
        {
            if (hexStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return Convert.ToByte(hexStr[2..], 16);
            return Convert.ToByte(hexStr, 16);
        }
    }
}
