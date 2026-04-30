using CS2Cheat.Data.Game;
using CS2Cheat.Features;
using CS2Cheat.Graphics;
using CS2Cheat.Utils;
using static CS2Cheat.Core.User32;
using Application = System.Windows.Application;

namespace CS2Cheat;

public class Program : Application, IDisposable
{
    private Program()
    {
        // Load offsets synchronously at start (async inside)
        Task.Run(async () => await Offsets.UpdateOffsets()).Wait();
        Startup += (_, _) => InitializeComponent();
        Exit += (_, _) => Dispose();
    }

    private GameProcess GameProcess { get; set; } = null!;
    private GameData GameData { get; set; } = null!;
    private WindowOverlay WindowOverlay { get; set; } = null!;
    private Graphics.Graphics Graphics { get; set; } = null!;
    private TriggerBot Trigger { get; set; } = null!;
    private AimBot AimBot { get; set; } = null!;
    private BombTimer BombTimer { get; set; } = null!;

    public void Dispose()
    {
        GameProcess?.Dispose();
        GameData?.Dispose();
        WindowOverlay?.Dispose();
        Graphics?.Dispose();
        Trigger?.Dispose();
        AimBot?.Dispose();
        BombTimer?.Dispose();
    }

    public static void Main()
    {
        new Program().Run();
    }

    private void InitializeComponent()
    {
        var features = ConfigManager.Load();
        GameProcess = new GameProcess();
        GameProcess.Start();

        GameData = new GameData(GameProcess);
        GameData.Start();

        WindowOverlay = new WindowOverlay(GameProcess);
        WindowOverlay.Start();

        Graphics = new Graphics.Graphics(GameProcess, GameData, WindowOverlay);
        Graphics.Start();

        Trigger = new TriggerBot(GameProcess, GameData);
        if (features.TriggerBot) Trigger.Start();

        AimBot = new AimBot(GameProcess, GameData);
        if (features.AimBot) AimBot.Start();

        BombTimer = new BombTimer(Graphics);
        if (features.BombTimer) BombTimer.Start();

        // Set window display affinity (OBS bypass)
        SetWindowDisplayAffinity(WindowOverlay!.Window.Handle, 0x00000011);
    }
}