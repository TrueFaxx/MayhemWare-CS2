using System;
using System.Threading.Tasks;
using CS2Cheat.Graphics;
using CS2Cheat.Utils;
using SharpDX;
using Keys = Process.NET.Native.Types.Keys;
using Color = SharpDX.Color;
using static CS2Cheat.Core.User32;

namespace CS2Cheat.Features;

public static class Menu
{
    private static bool _visible = false;
    private static int _selectedIndex = 0;

    // Options: toggles + settings
    private static readonly string[] Options = 
    {
        "ESP Box",
        "Skeleton ESP",
        "Team Check",
        "Glow Chams",
        "Motion Blur",
        "AimBot",
        "TriggerBot",
        "Bomb Timer",
        "Grenade Predictor",   // new
        "ESP Box Style",       // 0=full, 1=corner
        "ESP Box Thickness",   // 1-5
        "ESP Corner Radius",   // 0-20
        "Exit Menu"
    };

    // Internal state
    private static bool _espBox;
    private static bool _skeleton;
    private static bool _teamCheck;
    private static bool _glowChams;
    private static bool _motionBlur;
    private static bool _aimBot;
    private static bool _triggerBot;
    private static bool _bombTimer;
    private static bool _grenadePrediction;

    private static int _boxStyle;
    private static int _boxThickness;
    private static int _cornerRadius;

    public static bool IsVisible => _visible;
    public static void Toggle() => _visible = !_visible;

    // For ESP features – reads config every time (cached in menu)
    public static Color GetBoxColor()
    {
        var cfg = ConfigManager.Load();
        return cfg.BoxColorIndex switch
        {
            0 => Color.Red,
            1 => Color.Green,
            2 => Color.Blue,
            3 => Color.Yellow,
            4 => Color.Purple,
            _ => Color.White
        };
    }

    public static Color GetGlowColor()
    {
        var cfg = ConfigManager.Load();
        return cfg.GlowColorIndex switch
        {
            0 => Color.Red,
            1 => Color.Green,
            2 => Color.Blue,
            3 => Color.Yellow,
            4 => Color.Purple,
            _ => Color.White
        };
    }

    public static int GetBoxStyle() => _boxStyle;
    public static int GetBoxThickness() => _boxThickness;
    public static int GetCornerRadius() => _cornerRadius;

    public static void UpdateInput()
    {
        if (!_visible) return;

        // Navigation
        if (GetAsyncKeyState((int)Keys.Up) != 0)
        {
            _selectedIndex = (_selectedIndex - 1 + Options.Length) % Options.Length;
            Task.Delay(120).Wait();
        }
        if (GetAsyncKeyState((int)Keys.Down) != 0)
        {
            _selectedIndex = (_selectedIndex + 1) % Options.Length;
            Task.Delay(120).Wait();
        }

        // Adjust values with left/right
        if (GetAsyncKeyState((int)Keys.Left) != 0)
        {
            AdjustOption(-1);
            Task.Delay(120).Wait();
        }
        if (GetAsyncKeyState((int)Keys.Right) != 0)
        {
            AdjustOption(1);
            Task.Delay(120).Wait();
        }

        // Enter toggles boolean options
        if (GetAsyncKeyState((int)Keys.Enter) != 0)
        {
            if (_selectedIndex == Options.Length - 1) // Exit
                _visible = false;
            else if (_selectedIndex < 8) // boolean toggles (0-7 + grenade predictor at index 8)
                ToggleBoolean(_selectedIndex);
            Task.Delay(200).Wait();
        }

        // Load latest config states
        var cfg = ConfigManager.Load();
        _espBox = cfg.EspBox;
        _skeleton = cfg.SkeletonEsp;
        _teamCheck = cfg.TeamCheck;
        _glowChams = cfg.GlowChams;
        _motionBlur = cfg.MotionBlur;
        _aimBot = cfg.AimBot;
        _triggerBot = cfg.TriggerBot;
        _bombTimer = cfg.BombTimer;
        _grenadePrediction = cfg.GrenadePrediction;
        _boxStyle = cfg.BoxStyle;
        _boxThickness = cfg.BoxThickness;
        _cornerRadius = cfg.EspCornerRadius;
    }

    private static void AdjustOption(int delta)
    {
        switch (_selectedIndex)
        {
            case 9:  // Box Style
                _boxStyle = (_boxStyle + delta + 2) % 2;
                var cfg1 = ConfigManager.Load();
                cfg1.BoxStyle = _boxStyle;
                ConfigManager.Save(cfg1);
                break;
            case 10: // Box Thickness
                _boxThickness = Math.Clamp(_boxThickness + delta, 1, 5);
                var cfg2 = ConfigManager.Load();
                cfg2.BoxThickness = _boxThickness;
                ConfigManager.Save(cfg2);
                break;
            case 11: // Corner Radius
                _cornerRadius = Math.Clamp(_cornerRadius + delta * 2, 0, 20);
                var cfg3 = ConfigManager.Load();
                cfg3.EspCornerRadius = _cornerRadius;
                ConfigManager.Save(cfg3);
                break;
        }
    }

    private static void ToggleBoolean(int idx)
    {
        var cfg = ConfigManager.Load();
        switch (idx)
        {
            case 0: cfg.EspBox = !cfg.EspBox; break;
            case 1: cfg.SkeletonEsp = !cfg.SkeletonEsp; break;
            case 2: cfg.TeamCheck = !cfg.TeamCheck; break;
            case 3: cfg.GlowChams = !cfg.GlowChams; break;
            case 4: cfg.MotionBlur = !cfg.MotionBlur; break;
            case 5: cfg.AimBot = !cfg.AimBot; break;
            case 6: cfg.TriggerBot = !cfg.TriggerBot; break;
            case 7: cfg.BombTimer = !cfg.BombTimer; break;
            case 8: cfg.GrenadePrediction = !cfg.GrenadePrediction; break;
        }
        ConfigManager.Save(cfg);
    }

    public static void Draw(Graphics.Graphics graphics)
    {
        if (!_visible) return;
        try
        {
            if (graphics?.GameProcess == null || !graphics.GameProcess.IsValid) return;
            var font = graphics.FontConsolas32;
            if (font == null) return;

            var screenSize = graphics.GameProcess.WindowRectangleClient.Size;
            if (screenSize.Width <= 0 || screenSize.Height <= 0) return;

            int menuX = 20, menuY = 20, lineHeight = 20, width = 260;

            // Background
            for (int y = menuY - 5; y < menuY + Options.Length * lineHeight + 10; y++)
                graphics.DrawLine(new Color(0, 0, 0, 180), new Vector2(menuX - 5, y), new Vector2(menuX + width, y));

            for (int i = 0; i < Options.Length; i++)
            {
                Color color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                string display = Options[i];
                string value = "";

                switch (i)
                {
                    case 0: value = _espBox ? "[ON]" : "[OFF]"; break;
                    case 1: value = _skeleton ? "[ON]" : "[OFF]"; break;
                    case 2: value = _teamCheck ? "[ON]" : "[OFF]"; break;
                    case 3: value = _glowChams ? "[ON]" : "[OFF]"; break;
                    case 4: value = _motionBlur ? "[ON]" : "[OFF]"; break;
                    case 5: value = _aimBot ? "[ON]" : "[OFF]"; break;
                    case 6: value = _triggerBot ? "[ON]" : "[OFF]"; break;
                    case 7: value = _bombTimer ? "[ON]" : "[OFF]"; break;
                    case 8: value = _grenadePrediction ? "[ON]" : "[OFF]"; break;
                    case 9: value = _boxStyle == 0 ? "[Full]" : "[Corner]"; break;
                    case 10: value = $"[{_boxThickness}]"; break;
                    case 11: value = $"[{_cornerRadius}px]"; break;
                }
                font.DrawText(null, $"{display,-22} {value}", menuX, menuY + i * lineHeight, color);
            }
            font.DrawText(null, "↑ ↓ ← → | ENTER | DEL/RSHIFT", menuX, menuY + Options.Length * lineHeight + 5, Color.Gray);
        }
        catch (Exception ex) { Console.WriteLine($"[Menu Draw] {ex.Message}"); }
    }
}