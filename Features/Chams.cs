using System.Collections.Generic;
using System.Linq;
using CS2Cheat.Data.Entity;
using CS2Cheat.Data.Game;
using CS2Cheat.Utils;
using SharpDX;

namespace CS2Cheat.Features;

public static class Chams
{
    private static bool _enabled;
    private static Color _color = Color.Red;

    public static bool Enabled => _enabled;
    public static void Toggle() => _enabled = !_enabled;
    public static void SetColor(Color color) => _color = color;

    public static void Apply(GameProcess gameProcess, IEnumerable<Entity> entities, Player localPlayer)
    {
        if (!_enabled || gameProcess?.Process == null) return;

        var cfg = ConfigManager.Load();
        var glowColor = Menu.GetGlowColor();
        int rgba = (glowColor.R << 0) | (glowColor.G << 8) | (glowColor.B << 16) | (255 << 24);

        foreach (var ent in entities)
        {
            if (!ent.IsAlive() || ent.Team == localPlayer.Team) continue;

            var glowProp = gameProcess.Process.Read<IntPtr>(ent.AddressBase + Offsets.m_pGlowProperty);
            if (glowProp == IntPtr.Zero) continue;

            gameProcess.Process.Write(glowProp + Offsets.m_bGlowEnabled, (byte)1);
            gameProcess.Process.Write(glowProp + Offsets.m_glowColorOverride, rgba);
        }
    }
}