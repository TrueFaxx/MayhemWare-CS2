using System;
using System.Collections.Generic;
using CS2Cheat.Core.Data;
using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using CS2Cheat.Utils;
using SharpDX;
using RectangleF = System.Drawing.RectangleF;
using Color = SharpDX.Color;   // <-- resolves ambiguity

namespace CS2Cheat.Features;

public static class EspBox
{
    private const int OutlineThickness = 2;

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

        var cfg = ConfigManager.Load();
        if (!cfg.EspBox) return;

        foreach (var entity in graphics.GameData.Entities)
        {
            if (!entity.IsAlive() || entity.AddressBase == player.AddressBase) continue;
            if (cfg.TeamCheck && entity.Team == player.Team) continue;

            var boundingBox = GetEntityBoundingBox(graphics, entity);
            if (boundingBox == null) continue;

            DrawEntityInfo(graphics, entity, boundingBox.Value);
        }
    }

    private static void DrawEntityInfo(Graphics.Graphics graphics, Entity entity, (Vector2, Vector2) boundingBox)
    {
        var (topLeft, bottomRight) = boundingBox;
        if (topLeft.X >= bottomRight.X || topLeft.Y >= bottomRight.Y) return;

        int thickness = Menu.GetBoxThickness();
        int radius = Menu.GetCornerRadius();
        Color boxColor = Menu.GetBoxColor();
        int boxStyle = Menu.GetBoxStyle();

        for (int i = 0; i < thickness; i++)
        {
            var tl = new Vector2(topLeft.X - i, topLeft.Y - i);
            var br = new Vector2(bottomRight.X + i, bottomRight.Y + i);
            if (boxStyle == 0)
                graphics.DrawRoundedRectangle(boxColor, tl, br, radius);
            else
                DrawCornerBox(graphics, boxColor, tl, br);
        }

        // Health bar
        float healthPercent = Math.Clamp(entity.Health / 100f, 0f, 1f);
        var healthBarPos = new Vector2(topLeft.X - 10 - OutlineThickness, topLeft.Y);
        graphics.DrawFilledRectangle(Color.Black, new RectangleF(healthBarPos.X, healthBarPos.Y, 6, bottomRight.Y - topLeft.Y));
        graphics.DrawFilledRectangle(Color.Green, new RectangleF(healthBarPos.X, healthBarPos.Y + (bottomRight.Y - topLeft.Y) * (1 - healthPercent), 6, (bottomRight.Y - topLeft.Y) * healthPercent));
        graphics.DrawRectangle(Color.Black, healthBarPos, new Vector2(healthBarPos.X + 6, bottomRight.Y));

        // Health number
        var healthText = entity.Health.ToString();
        var healthX = (int)(bottomRight.X + 2);
        var healthY = (int)(topLeft.Y + (bottomRight.Y - topLeft.Y - 12) / 2);
        graphics.FontConsolas32?.DrawText(null, healthText, healthX, healthY, Color.White);

        // Weapon icon
        var weaponIcon = GetWeaponIcon(entity.CurrentWeaponName);
        if (!string.IsNullOrEmpty(weaponIcon))
        {
            var weaponX = (int)((topLeft.X + bottomRight.X) / 2 - 6);
            var weaponY = (int)(bottomRight.Y + 2);
            graphics.Undefeated?.DrawText(null, weaponIcon, weaponX, weaponY, Color.White);
        }

        // Player name (enemies only)
        if (graphics.GameData.Player!.Team != entity.Team && !string.IsNullOrEmpty(entity.Name))
        {
            var name = entity.Name;
            var nameX = (int)((topLeft.X + bottomRight.X) / 2 - (name.Length * 4));
            var nameY = (int)(topLeft.Y - 15);
            graphics.FontConsolas32?.DrawText(null, name, nameX, nameY, Color.White);
        }

        // Status flags
        int flagX = (int)(bottomRight.X + 5), flagY = (int)topLeft.Y;
        int spacing = 15;
        if (entity.IsInScope == 1)
            graphics.FontConsolas32?.DrawText(null, "Scoped", flagX, flagY, Color.White);
        if (entity.FlashAlpha > 7)
            graphics.FontConsolas32?.DrawText(null, "Flashed", flagX, flagY + spacing, Color.White);
        if (entity.IsInScope == 256)
            graphics.FontConsolas32?.DrawText(null, "Shifting", flagX, flagY + spacing * 2, Color.White);
        else if (entity.IsInScope == 257)
            graphics.FontConsolas32?.DrawText(null, "Shifting in scope", flagX, flagY + spacing * 3, Color.White);
    }

    private static void DrawCornerBox(Graphics.Graphics graphics, Color color, Vector2 tl, Vector2 br)
    {
        float width = br.X - tl.X;
        float height = br.Y - tl.Y;
        float minDim = Math.Min(width, height);
        int len = (int)Math.Clamp(minDim * 0.2f, 5f, 25f);
        if (len < 2) len = 12;

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
        const float padding = 5f;
        var player = graphics.GameData.Player;
        if (player == null) return null;

        var matrix = player.MatrixViewProjectionViewport;
        if (!entity.BonePos.TryGetValue("head", out var headPos)) return null;

        var screenHead = matrix.Transform(headPos);
        var screenFoot = matrix.Transform(entity.Origin);
        if (screenHead.Z >= 1 || screenFoot.Z >= 1 || screenHead.Y > screenFoot.Y) return null;

        float height = screenFoot.Y - screenHead.Y;
        if (height <= 1f) return null;

        float width = height * 0.5f;
        var topLeft = new Vector2(screenHead.X - width / 2, screenHead.Y) - new Vector2(padding, padding);
        var bottomRight = new Vector2(screenHead.X + width / 2, screenFoot.Y) + new Vector2(padding, padding);

        var screenSize = graphics.GameProcess.WindowRectangleClient.Size;
        topLeft.X = Math.Clamp(topLeft.X, 0, screenSize.Width);
        topLeft.Y = Math.Clamp(topLeft.Y, 0, screenSize.Height);
        bottomRight.X = Math.Clamp(bottomRight.X, 0, screenSize.Width);
        bottomRight.Y = Math.Clamp(bottomRight.Y, 0, screenSize.Height);

        return (topLeft.X < bottomRight.X && topLeft.Y < bottomRight.Y) ? (topLeft, bottomRight) : null;
    }

    private static string GetWeaponIcon(string weapon)
    {
        return GunIcons.TryGetValue(weapon?.ToLower() ?? "", out var icon) ? icon : "";
    }
}