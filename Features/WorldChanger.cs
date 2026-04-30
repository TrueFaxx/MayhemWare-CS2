using CS2Cheat.Data.Game;
using CS2Cheat.Utils;

namespace CS2Cheat.Features;

public static class WorldChanger
{
    public static void Update(GameProcess gameProcess)
    {
        var cfg = ConfigManager.Load();
        if (Offsets.pMotionBlur == IntPtr.Zero) return;
        float value = cfg.MotionBlur ? cfg.MotionBlurAmount : 0f;
        gameProcess.Process.Write(Offsets.pMotionBlur, value);
    }
}