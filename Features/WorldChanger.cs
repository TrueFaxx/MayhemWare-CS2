using CS2Cheat.Data.Game;
using CS2Cheat.Utils;

namespace CS2Cheat.Features;

public static class WorldChanger
{
    private static bool _motionBlur;
    private static float _blurAmount;

    public static void Update(GameProcess gameProcess)
    {
        var cfg = ConfigManager.Load();
        _motionBlur = cfg.MotionBlur;
        _blurAmount = cfg.MotionBlurAmount;

        if (gameProcess?.Process == null) return;
        if (Offsets.pMotionBlur != IntPtr.Zero)
            gameProcess.Process.Write(Offsets.pMotionBlur, _motionBlur ? _blurAmount : 0f);
    }
}