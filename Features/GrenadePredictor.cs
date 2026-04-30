using System;
using System.Collections.Generic;
using CS2Cheat.Data.Entity;          // for Player class
using CS2Cheat.Graphics;              // for Transform, GetNormalized extension methods
using CS2Cheat.Utils;
using SharpDX;
using Color = SharpDX.Color;                // resolve ambiguity
using RectangleF = System.Drawing.RectangleF;  // resolve ambiguity

namespace CS2Cheat.Features;

public static class GrenadePredictor
{
    private const float Gravity = 800f;           // units/s²
    private const float TimeStep = 0.02f;         // seconds
    private const int MaxSteps = 300;
    private const float MaxDistance = 4000f;

    public static void Draw(Graphics.Graphics graphics)
    {
        var cfg = ConfigManager.Load();
        if (!cfg.GrenadePrediction) return;
        if (graphics?.GameData?.Player == null) return;

        var player = graphics.GameData.Player;
        if (!player.IsAlive()) return;

        // Only when holding a grenade
        var weapon = player.CurrentWeaponName.ToLower();
        if (!IsGrenade(weapon)) return;

        var points = PredictTrajectory(player);
        if (points.Length < 2) return;

        // Draw path
        for (int i = 0; i < points.Length - 1; i++)
        {
            graphics.DrawLineWorld(Color.White, points[i], points[i + 1]);
        }

        // Draw impact point (last position)
        var last = points[points.Length - 1];
        var screenLast = player.MatrixViewProjectionViewport.Transform(last);
        if (screenLast.Z < 1)
        {
            graphics.DrawFilledRectangle(Color.Red, new RectangleF(screenLast.X - 3, screenLast.Y - 3, 6, 6));
        }
    }

    private static bool IsGrenade(string weapon)
    {
        return weapon.Contains("grenade") || weapon == "molotov" || weapon == "incgrenade" ||
               weapon == "flashbang" || weapon == "smokegrenade" || weapon == "decoy";
    }

    private static Vector3[] PredictTrajectory(Player player)
    {
        // Get throw velocity (simplified: use view direction + player velocity)
        var viewDir = player.AimDirection.GetNormalized();
        var throwVelocity = viewDir * 1100f;      // average throw strength
        throwVelocity += player.Velocity;          // inherit player movement

        var pos = player.EyePosition;
        var vel = throwVelocity;
        var points = new List<Vector3>();

        for (int step = 0; step < MaxSteps; step++)
        {
            points.Add(pos);
            // Apply gravity and air drag
            vel.Y -= Gravity * TimeStep;
            vel *= 0.998f;

            var nextPos = pos + vel * TimeStep;

            // Simple ground collision (y <= 0)
            if (nextPos.Y <= 0)
                break;
            if (Vector3.Distance(player.EyePosition, nextPos) > MaxDistance)
                break;

            pos = nextPos;
        }
        return points.ToArray();
    }
}