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
    public static bool Enabled => _enabled;
    public static void Toggle() => _enabled = !_enabled;

    public static void Apply(GameProcess gameProcess, IEnumerable<Entity> entities, Player localPlayer)
    {
        if (!_enabled || gameProcess?.Process == null || Offsets.dwGlowManager == 0) return;

        var glowManager = gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwGlowManager);
        if (glowManager == IntPtr.Zero) return;

        var glowArray = gameProcess.Process.Read<IntPtr>(glowManager + 0x8);
        if (glowArray == IntPtr.Zero) return;

        var cfg = ConfigManager.Load();
        var glowColor = Menu.GetGlowColor();
        int rgba = (glowColor.R << 0) | (glowColor.G << 8) | (glowColor.B << 16) | (255 << 24);

        foreach (var ent in entities)
        {
            if (!ent.IsAlive() || ent.Team == localPlayer.Team) continue;

            // Try to get glow index from entity (common offset 0xA48)
            var glowIndex = gameProcess.Process.Read<int>(ent.AddressBase + 0xA48);
            if (glowIndex < 0 || glowIndex > 63) continue;

            var glowObj = gameProcess.Process.Read<IntPtr>(glowArray + (glowIndex * 0x8));
            if (glowObj == IntPtr.Zero) continue;

            gameProcess.Process.Write(glowObj + Offsets.m_bGlowEnabled, (byte)1);
            gameProcess.Process.Write(glowObj + Offsets.m_glowColorOverride, rgba);
            gameProcess.Process.Write(glowObj + Offsets.m_iGlowType, 1);
        }
    }
}