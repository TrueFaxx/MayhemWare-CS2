using System.IO;
using System.Text.Json;
using Keys = Process.NET.Native.Types.Keys;

namespace CS2Cheat.Utils;

public class ConfigManager
{
    private const string ConfigFile = "config.json";

    // Feature toggles
    public bool AimBot { get; set; }
    public bool BombTimer { get; set; }
    public bool EspAimCrosshair { get; set; }
    public bool EspBox { get; set; }
    public bool SkeletonEsp { get; set; }
    public bool TriggerBot { get; set; }
    public bool TeamCheck { get; set; }
    public bool GlowChams { get; set; }
    public bool MotionBlur { get; set; }
    public bool GrenadePrediction { get; set; }

    // ESP styling
    public int BoxStyle { get; set; }          // 0 = full, 1 = corner
    public int BoxThickness { get; set; }      // 1-5
    public int EspCornerRadius { get; set; }   // 0-20
    public int BoxColorIndex { get; set; }     // 0-4 (Red, Green, Blue, Yellow, Purple)
    public int GlowColorIndex { get; set; }    // 0-4

    // World changer
    public float MotionBlurAmount { get; set; }

    // Hotkeys
    public Keys AimBotKey { get; set; }
    public Keys TriggerBotKey { get; set; }

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
            TeamCheck = true,
            GlowChams = false,
            MotionBlur = false,
            GrenadePrediction = false,
            BoxStyle = 0,
            BoxThickness = 1,
            EspCornerRadius = 5,
            BoxColorIndex = 0,
            GlowColorIndex = 0,
            MotionBlurAmount = 0.5f,
            AimBotKey = Keys.LButton,
            TriggerBotKey = Keys.LMenu
        };
    }
}