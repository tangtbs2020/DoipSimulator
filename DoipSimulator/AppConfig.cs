using System.Text.Json;

namespace DoipSimulator
{
    internal class AppConfig
    {
        public string? DataDB { get; set; }

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
    }
}
