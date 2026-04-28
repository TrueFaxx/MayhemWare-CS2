using System;
using System.Threading.Tasks;
using CS2Cheat.Graphics;
using CS2Cheat.Utils;
using SharpDX;
using Keys = Process.NET.Native.Types.Keys;
using RectangleF = System.Drawing.RectangleF;
using Color = SharpDX.Color;
using static CS2Cheat.Core.User32;   // <-- IMPORTANT

namespace CS2Cheat.Features;

public static class Menu
{
    private static bool _visible = false;
    private static int _selectedIndex = 0;
    private static readonly string[] Options = 
    {
        "ESP Box Style",      // 0
        "ESP Box Thickness",  // 1
        "ESP Box Color",      // 2
        "ESP Corner Radius",  // 3
        "Skeleton ESP",       // 4
        "Team Check",         // 5
        "Glow Chams",         // 6
        "Motion Blur",        // 7
        "Motion Blur Amount", // 8
        "AimBot",             // 9
        "TriggerBot",         // 10
        "Bomb Timer",         // 11
        "Exit Menu"           // 12
    };

    private static int _boxStyle;
    private static int _boxThickness;
    private static int _boxColorIndex;
    private static int _cornerRadius;
    private static bool _skeleton;
    private static bool _teamCheck;
    private static bool _glowChams;
    private static int _glowColorIndex;
    private static bool _motionBlur;
    private static float _motionBlurAmount;
    private static bool _aimBot;
    private static bool _triggerBot;
    private static bool _bombTimer;

    private static readonly Color[] ColorPalette = 
    { 
        Color.Red, Color.Green, Color.Blue, 
        Color.Yellow, Color.Purple 
    };
    private static readonly string[] ColorNames = { "Red", "Green", "Blue", "Yellow", "Purple" };

    public static bool IsVisible => _visible;
    public static void Toggle() => _visible = !_visible;
    public static Color GetBoxColor() => ColorPalette[_boxColorIndex];
    public static Color GetGlowColor() => ColorPalette[_glowColorIndex];

    public static void UpdateInput()
    {
        if (!_visible) return;

        if (GetAsyncKeyState((int)Keys.Up) != 0) { _selectedIndex = (_selectedIndex - 1 + Options.Length) % Options.Length; Task.Delay(150).Wait(); }
        if (GetAsyncKeyState((int)Keys.Down) != 0) { _selectedIndex = (_selectedIndex + 1) % Options.Length; Task.Delay(150).Wait(); }

        if (GetAsyncKeyState((int)Keys.Left) != 0) { ChangeValue(-1); Task.Delay(150).Wait(); }
        if (GetAsyncKeyState((int)Keys.Right) != 0) { ChangeValue(1); Task.Delay(150).Wait(); }

        if (GetAsyncKeyState((int)Keys.Enter) != 0)
        {
            if (_selectedIndex == 12) _visible = false;
            else if (_selectedIndex == 4) ToggleSkeleton();
            else if (_selectedIndex == 5) ToggleTeamCheck();
            else if (_selectedIndex == 6) ToggleGlowChams();
            else if (_selectedIndex == 7) ToggleMotionBlur();
            else if (_selectedIndex == 9) ToggleAimBot();
            else if (_selectedIndex == 10) ToggleTriggerBot();
            else if (_selectedIndex == 11) ToggleBombTimer();
            Task.Delay(200).Wait();
        }

        LoadFromConfig();
    }

    private static void ChangeValue(int delta)
    {
        var cfg = ConfigManager.Load();
        switch (_selectedIndex)
        {
            case 0: _boxStyle = (_boxStyle + delta + 2) % 2; cfg.BoxStyle = _boxStyle; break;
            case 1: _boxThickness = Math.Clamp(_boxThickness + delta, 1, 5); cfg.BoxThickness = _boxThickness; break;
            case 2: _boxColorIndex = (_boxColorIndex + delta + ColorPalette.Length) % ColorPalette.Length; cfg.BoxColorIndex = _boxColorIndex; break;
            case 3: _cornerRadius = Math.Clamp(_cornerRadius + delta * 2, 0, 20); cfg.EspCornerRadius = _cornerRadius; break;
            case 8: _motionBlurAmount = Math.Clamp(_motionBlurAmount + delta * 0.05f, 0f, 1f); cfg.MotionBlurAmount = _motionBlurAmount; break;
            default: return;
        }
        ConfigManager.Save(cfg);
    }

    private static void ToggleSkeleton() { var cfg = ConfigManager.Load(); cfg.SkeletonEsp = !cfg.SkeletonEsp; ConfigManager.Save(cfg); _skeleton = cfg.SkeletonEsp; }
    private static void ToggleTeamCheck() { var cfg = ConfigManager.Load(); cfg.TeamCheck = !cfg.TeamCheck; ConfigManager.Save(cfg); _teamCheck = cfg.TeamCheck; }
    private static void ToggleGlowChams() { var cfg = ConfigManager.Load(); cfg.GlowChams = !cfg.GlowChams; ConfigManager.Save(cfg); _glowChams = cfg.GlowChams; }
    private static void ToggleMotionBlur() { var cfg = ConfigManager.Load(); cfg.MotionBlur = !cfg.MotionBlur; ConfigManager.Save(cfg); _motionBlur = cfg.MotionBlur; }
    private static void ToggleAimBot() { var cfg = ConfigManager.Load(); cfg.AimBot = !cfg.AimBot; ConfigManager.Save(cfg); _aimBot = cfg.AimBot; }
    private static void ToggleTriggerBot() { var cfg = ConfigManager.Load(); cfg.TriggerBot = !cfg.TriggerBot; ConfigManager.Save(cfg); _triggerBot = cfg.TriggerBot; }
    private static void ToggleBombTimer() { var cfg = ConfigManager.Load(); cfg.BombTimer = !cfg.BombTimer; ConfigManager.Save(cfg); _bombTimer = cfg.BombTimer; }

    private static void LoadFromConfig()
    {
        var cfg = ConfigManager.Load();
        _boxStyle = cfg.BoxStyle;
        _boxThickness = cfg.BoxThickness;
        _boxColorIndex = cfg.BoxColorIndex;
        _cornerRadius = cfg.EspCornerRadius;
        _skeleton = cfg.SkeletonEsp;
        _teamCheck = cfg.TeamCheck;
        _glowChams = cfg.GlowChams;
        _glowColorIndex = cfg.GlowColorIndex;
        _motionBlur = cfg.MotionBlur;
        _motionBlurAmount = cfg.MotionBlurAmount;
        _aimBot = cfg.AimBot;
        _triggerBot = cfg.TriggerBot;
        _bombTimer = cfg.BombTimer;
    }

    public static void Draw(Graphics.Graphics graphics)
    {
        if (!_visible) return;

        var screenSize = graphics.GameProcess.WindowRectangleClient.Size;
        int menuWidth = 350, menuHeight = Options.Length * 26 + 40;
        int startX = (screenSize.Width - menuWidth) / 2;
        int startY = (screenSize.Height - menuHeight) / 2;
        var bgRect = new RectangleF(startX, startY, menuWidth, menuHeight);
        graphics.DrawFilledRectangle(new Color(0, 0, 0, 200), bgRect);

        graphics.FontConsolas32?.DrawText(null, "[ ESP / World Settings ]", startX + 10, startY + 8, Color.White);

        for (int i = 0; i < Options.Length; i++)
        {
            int y = startY + 35 + i * 24;
            var color = (i == _selectedIndex) ? Color.Yellow : Color.White;
            string display = Options[i];
            switch (i)
            {
                case 0: display += $": {(_boxStyle == 0 ? "Full Box" : "Corner Only")}"; break;
                case 1: display += $": {_boxThickness}px"; break;
                case 2: display += $": {ColorNames[_boxColorIndex]}"; break;
                case 3: display += $": {_cornerRadius}px"; break;
                case 4: display += $": {(_skeleton ? "ON" : "OFF")}"; break;
                case 5: display += $": {(_teamCheck ? "ON" : "OFF")}"; break;
                case 6: display += $": {(_glowChams ? "ON" : "OFF")}"; break;
                case 7: display += $": {(_motionBlur ? "ON" : "OFF")}"; break;
                case 8: display += $": {_motionBlurAmount:F2}"; break;
                case 9: display += $": {(_aimBot ? "ON" : "OFF")}"; break;
                case 10: display += $": {(_triggerBot ? "ON" : "OFF")}"; break;
                case 11: display += $": {(_bombTimer ? "ON" : "OFF")}"; break;
            }
            graphics.FontConsolas32?.DrawText(null, display, startX + 15, y, color);
        }
        graphics.FontConsolas32?.DrawText(null, "Use ARROWS to change, ENTER to toggle", startX + 15, startY + menuHeight - 20, Color.Gray);
    }
}