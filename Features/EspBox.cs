using CS2Cheat.Core.Data;
using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using CS2Cheat.Utils;
using SharpDX;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class EspBox
{
    private const int OutlineThickness = 2;
    private static ConfigManager? _config;
    private static ConfigManager Config => _config ??= ConfigManager.Load();

    // Gun icons (original)
    private static readonly Dictionary<string, string> GunIcons = new()
    {
        ["knife_ct"] = "]", ["knife_t"] = "[", ["deagle"] = "A", ["elite"] = "B",
        ["fiveseven"] = "C", ["glock"] = "D", ["revolver"] = "J", ["hkp2000"] = "E",
        ["p250"] = "F", ["usp_silencer"] = "G", ["tec9"] = "H", ["cz75a"] = "I",
        ["mac10"] = "K", ["ump45"] = "L", ["bizon"] = "M", ["mp7"] = "N",
        ["mp9"] = "R", ["p90"] = "O", ["galilar"] = "Q", ["famas"] = "R",
        ["m4a1_silencer"] = "T", ["m4a1"] = "S", ["aug"] = "U", ["sg556"] = "V",
        ["ak47"] = "W", ["g3sg1"] = "X", ["scar20"] = "Y", ["awp"] = "Z",
        ["ssg08"] = "a", ["xm1014"] = "b", ["sawedoff"] = "c", ["mag7"] = "d",
        ["nova"] = "e", ["negev"] = "f", ["m249"] = "g", ["taser"] = "h",
        ["flashbang"] = "i", ["hegrenade"] = "j", ["smokegrenade"] = "k",
        ["molotov"] = "l", ["decoy"] = "m", ["incgrenade"] = "n", ["c4"] = "o"
    };

    public static void Draw(Graphics.Graphics graphics)
    {
        var player = graphics.GameData.Player;
        if (player == null || graphics.GameData.Entities == null) return;

        foreach (var entity in graphics.GameData.Entities)
        {
            if (!entity.IsAlive() || entity.AddressBase == player.AddressBase) continue;
            if (Config.TeamCheck && entity.Team == player.Team) continue;

            var boundingBox = GetEntityBoundingBox(graphics, entity);
            if (boundingBox == null) continue;

            var colorBox = entity.Team == Team.Terrorists ? Color.DarkRed : Color.DarkBlue;
            DrawEntityInfo(graphics, entity, colorBox, boundingBox.Value);
        }
    }

    private static void DrawEntityInfo(Graphics.Graphics graphics, Entity entity, Color color, (Vector2, Vector2) boundingBox)
    {
        var (topLeft, bottomRight) = boundingBox;
        if (topLeft.X > bottomRight.X || topLeft.Y > bottomRight.Y) return;

        var cfg = ConfigManager.Load();
        int thickness = cfg.BoxThickness;
        int radius = cfg.EspCornerRadius;
        Color boxColor = Menu.GetBoxColor();

        // Draw box with rounded corners
        for (int i = 0; i < thickness; i++)
        {
            var tl = new Vector2(topLeft.X - i, topLeft.Y - i);
            var br = new Vector2(bottomRight.X + i, bottomRight.Y + i);
            if (cfg.BoxStyle == 0)
                graphics.DrawRoundedRectangle(boxColor, tl, br, radius);
            else
                DrawCornerBox(graphics, boxColor, tl, br, radius);
        }

        // --- Health bar (left side) ---
        float healthPercent = Math.Clamp(entity.Health / 100f, 0f, 1f);
        var healthBarPos = new Vector2(topLeft.X - 10 - OutlineThickness, topLeft.Y);
        var healthBarSize = new Vector2(6, (bottomRight.Y - topLeft.Y) * healthPercent);
        graphics.DrawFilledRectangle(Color.Black, new RectangleF(healthBarPos.X, healthBarPos.Y, 6, bottomRight.Y - topLeft.Y));
        graphics.DrawFilledRectangle(Color.Green, new RectangleF(healthBarPos.X, healthBarPos.Y + (bottomRight.Y - topLeft.Y - healthBarSize.Y), 6, healthBarSize.Y));
        graphics.DrawRectangle(Color.Black, healthBarPos, new Vector2(healthBarPos.X + 6, bottomRight.Y));

        // Health number
        var healthText = entity.Health.ToString();
        var healthX = (int)(bottomRight.X + 2);
        var healthY = (int)(topLeft.Y + (bottomRight.Y - topLeft.Y - graphics.FontConsolas32.MeasureText(null, healthText, FontDrawFlags.Center).Bottom) / 2);
        graphics.FontConsolas32.DrawText(null, healthText, healthX, healthY, Color.White);

        // Weapon icon (from GunIcons)
        var weaponIcon = GetWeaponIcon(entity.CurrentWeaponName);
        if (!string.IsNullOrEmpty(weaponIcon))
        {
            var textSize = graphics.Undefeated.MeasureText(null, weaponIcon, FontDrawFlags.Center);
            var weaponX = (int)((topLeft.X + bottomRight.X - textSize.Right) / 2);
            var weaponY = (int)(bottomRight.Y + 2.5f);
            graphics.Undefeated.DrawText(null, weaponIcon, weaponX, weaponY, Color.White);
        }

        // Player name (enemies only)
        if (graphics.GameData.Player.Team != entity.Team && !string.IsNullOrEmpty(entity.Name))
        {
            var name = entity.Name ?? "UNKNOWN";
            var textWidth = graphics.FontConsolas32.MeasureText(null, name, FontDrawFlags.Center).Right + 10f;
            var nameX = (int)((topLeft.X + bottomRight.X) / 2 - textWidth / 2);
            var nameY = (int)(topLeft.Y - 15f);
            graphics.FontConsolas32.DrawText(null, name, nameX, nameY, Color.White);
        }

        // Status flags (scoped, flashed, shifting)
        var flagX = (int)(bottomRight.X + 5f);
        var flagY = (int)topLeft.Y;
        var spacing = 15;

        if (entity.IsInScope == 1)
            graphics.FontConsolas32.DrawText(null, "Scoped", flagX, flagY, Color.White);

        if (entity.FlashAlpha > 7)
            graphics.FontConsolas32.DrawText(null, "Flashed", flagX, flagY + spacing, Color.White);

        if (entity.IsInScope == 256)
            graphics.FontConsolas32.DrawText(null, "Shifting", flagX, flagY + spacing * 2, Color.White);
        else if (entity.IsInScope == 257)
            graphics.FontConsolas32.DrawText(null, "Shifting in scope", flagX, flagY + spacing * 3, Color.White);
    }

    private static void DrawCornerBox(Graphics.Graphics graphics, Color color, Vector2 tl, Vector2 br, int radius)
    {
        int len = 12;
        graphics.DrawLine(color, tl, new Vector2(tl.X + len, tl.Y));
        graphics.DrawLine(color, tl, new Vector2(tl.X, tl.Y + len));
        graphics.DrawLine(color, new Vector2(br.X, tl.Y), new Vector2(br.X - len, tl.Y));
        graphics.DrawLine(color, new Vector2(br.X, tl.Y), new Vector2(br.X, tl.Y + len));
        graphics.DrawLine(color, new Vector2(tl.X, br.Y), new Vector2(tl.X + len, br.Y));
        graphics.DrawLine(color, new Vector2(tl.X, br.Y), new Vector2(tl.X, br.Y - len));
        graphics.DrawLine(color, br, new Vector2(br.X - len, br.Y));
        graphics.DrawLine(color, br, new Vector2(br.X, br.Y - len));
    }

    private static (Vector2, Vector2)? GetEntityBoundingBox(Graphics.Graphics graphics, Entity entity)
    {
        const float padding = 5.0f;
        if (graphics.GameData.Player == null) return null;
        var matrix = graphics.GameData.Player.MatrixViewProjectionViewport;
        if (matrix == null || entity.BonePos == null || entity.BonePos.Count == 0) return null;

        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        bool anyValid = false;

        foreach (var bone in entity.BonePos.Values)
        {
            var transformed = matrix.Transform(bone);
            if (transformed.Z >= 1 || transformed.X < 0 || transformed.Y < 0) continue;
            anyValid = true;
            min.X = Math.Min(min.X, transformed.X);
            min.Y = Math.Min(min.Y, transformed.Y);
            max.X = Math.Max(max.X, transformed.X);
            max.Y = Math.Max(max.Y, transformed.Y);
        }

        if (!anyValid) return null;

        var sizeMultiplier = 2f - entity.Health / 100f;
        var paddingVec = new Vector2(padding * sizeMultiplier);
        return (min - paddingVec, max + paddingVec);
    }

    private static string GetWeaponIcon(string weapon)
    {
        return GunIcons.TryGetValue(weapon?.ToLower() ?? string.Empty, out var icon) ? icon : string.Empty;
    }
}