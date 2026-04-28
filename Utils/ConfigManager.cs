using System.IO;
using System.Text.Json;
using Keys = Process.NET.Native.Types.Keys;

namespace CS2Cheat.Utils;

public class ConfigManager
{
    private const string ConfigFile = "config.json";
    public bool AimBot { get; set; }
    public bool BombTimer { get; set; }
    public bool EspAimCrosshair { get; set; }
    public bool EspBox { get; set; }
    public bool SkeletonEsp { get; set; }
    public bool TriggerBot { get; set; }
    public Keys AimBotKey { get; set; }
    public Keys TriggerBotKey { get; set; }
    public bool TeamCheck { get; set; }

    // ESP style settings
    public int BoxStyle { get; set; }
    public int BoxThickness { get; set; }
    public int BoxColorIndex { get; set; }
    public int EspCornerRadius { get; set; }

    // Chams
    public bool GlowChams { get; set; }
    public int GlowColorIndex { get; set; }
    public float GlowIntensity { get; set; }

    // World changer
    public bool MotionBlur { get; set; }
    public float MotionBlurAmount { get; set; }

    public static ConfigManager Load()
    {
        try
        {
            if (!File.Exists(ConfigFile))
            {
                var def = Default();
                Save(def);
                return def;
            }
            var json = File.ReadAllText(ConfigFile);
            var options = JsonSerializer.Deserialize<ConfigManager>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return options ?? Default();
        }
        catch { return Default(); }
    }

    public static void Save(ConfigManager options)
    {
        try
        {
            var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, json);
        }
        catch { }
    }

    public static ConfigManager Default()
    {
        return new ConfigManager
        {
            AimBot = true,
            BombTimer = true,
            EspAimCrosshair = false,
            EspBox = true,
            SkeletonEsp = false,
            TriggerBot = true,
            AimBotKey = Keys.LButton,
            TriggerBotKey = Keys.LMenu,
            TeamCheck = true,
            BoxStyle = 0,
            BoxThickness = 1,
            BoxColorIndex = 0,
            EspCornerRadius = 5,
            GlowChams = false,
            GlowColorIndex = 0,
            GlowIntensity = 0.8f,
            MotionBlur = false,
            MotionBlurAmount = 0.5f
        };
    }
}