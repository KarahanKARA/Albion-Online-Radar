using AlbionRadar.Drawing;
using AlbionRadar.Harvestable;
using AlbionRadar.Player;
using AlbionRadar.Mobs;
using Overlay.NET.Common;
using Overlay.NET.Directx;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AlbionRadar;

public sealed class Overlay : DirectXOverlayPlugin
{
    private readonly TickEngine _tickEngine = new();
    private const int _updateRate = 1000 / 60;

    private const string WindowName = "Albion Online Client";
    private const float MinimapBorderWidth = 2.0f;

    private int _interiorBrush;
    private int _borderBrush;
    private int _gridBrush;
    private int _localPlayerBrush;
    private int _playerBrush;
    private int _textBrush;
    private int _yellowBrush;
    private int _redBrush;
    public override void Initialize(IntPtr targetWindowHandle)
    {
        while (targetWindowHandle == IntPtr.Zero)
        {
            targetWindowHandle = GetTargetWindow();
            Thread.Sleep(10);
        }

        base.Initialize(targetWindowHandle);

        OverlayWindow = new DirectXOverlayWindow(targetWindowHandle, false);

        ImageHandler.Load(OverlayWindow.Graphics.GetDevice());

        _interiorBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.FromArgb(128, System.Drawing.Color.Black));
        _borderBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.Black);
        _gridBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.FromArgb(128, 45, 45, 45));

        _localPlayerBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.Blue);
        _playerBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.Red);
        _textBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.White); 
        _yellowBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.Yellow);
        _redBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.Red);
        _tickEngine.PreTick += OnPreTick;
        _tickEngine.Tick += OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!OverlayWindow.IsVisible)
            return;

        OverlayWindow.Update();
        InternalRender();
    }

    private void OnPreTick(object sender, EventArgs e)
    {
        var targetWindowIsActivated = IsActive();

        if (!targetWindowIsActivated)
        {
            IntPtr gameWindowHandle = GetTargetWindow();

            if (gameWindowHandle != IntPtr.Zero)
            {
                if (OverlayWindow.ParentWindow != gameWindowHandle)
                {
                    base.Initialize(gameWindowHandle);
                    OverlayWindow.UpdateParentWindow(gameWindowHandle);
                    targetWindowIsActivated = IsActive();
                }

                if (!targetWindowIsActivated)
                    targetWindowIsActivated = MainForm.CanShowOverlay();
            }
        }

        if (!targetWindowIsActivated && OverlayWindow.IsVisible)
        {
            ClearScreen();
            OverlayWindow.Hide();
        }
        else if (targetWindowIsActivated && !OverlayWindow.IsVisible)
            OverlayWindow.Show();
    }

    public override void Enable()
    {
        _tickEngine.Interval = TimeSpan.FromMicroseconds(_updateRate);
        _tickEngine.IsTicking = true;
        base.Enable();
    }

    public override void Disable()
    {
        _tickEngine.IsTicking = false;
        base.Disable();
    }

    public override void Update() => _tickEngine.Pulse();


    private void InternalRender()
    {
        float xOffset = Config.Instance.RadarParams.XOffset;
        float yOffset = Config.Instance.RadarParams.YOffset;
        float minimapSize = Config.Instance.RadarParams.Size;
        float gridWidth = minimapSize / 10;

        RawMatrix3x2 originalTransform = OverlayWindow.Graphics.GetDevice().Transform;
        Matrix3x2 translateMatrix = Matrix3x2.Translation(xOffset + minimapSize / 2, yOffset + minimapSize / 2);

        float maxAllowedCoordinate = minimapSize / 2 - MinimapBorderWidth;

        OverlayWindow.Graphics.BeginScene();
        OverlayWindow.Graphics.ClearScene();

        int nearbyPKPlayers = PlayerHandler.GetNearbyEnemyCount();
        bool hasPKPlayers = nearbyPKPlayers > 0;
        
        int borderColor = hasPKPlayers ? 
            OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.Red) : 
            _borderBrush;
        int gridColor = hasPKPlayers ? 
            OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)) : 
            _gridBrush;

        OverlayWindow.Graphics.DrawRectangle(xOffset, yOffset, minimapSize, minimapSize, MinimapBorderWidth, borderColor);
        OverlayWindow.Graphics.FillRectangle(xOffset, yOffset, minimapSize, minimapSize, _interiorBrush);

        for (float x = gridWidth; x < minimapSize; x += gridWidth)
            OverlayWindow.Graphics.DrawLine(ShiftX(x), ShiftY(0), ShiftX(x), yOffset + minimapSize - MinimapBorderWidth / 2, 1, gridColor);

        for (float y = gridWidth; y < minimapSize; y += gridWidth)
            OverlayWindow.Graphics.DrawLine(ShiftX(0), ShiftY(y), xOffset + minimapSize - MinimapBorderWidth / 2, ShiftY(y), 1, gridColor);

        OverlayWindow.Graphics.GetDevice().Transform = translateMatrix;

        float lpX = PlayerHandler.GetLocalPlayerPosX();
        float lpY = PlayerHandler.GetLocalPlayerPosY();

        foreach (var pair in HarvestableHandler.Harvestables)
        {
            var h = pair.Value;

            if (!Config.Instance.CanShowHarvestable((HarvestableType)h.Type, h.Tier, h.Charges))
                continue;

            if (h.Size == 0)
                continue;

            float hX = -1 * h.PosX + lpX;
            float hY = h.PosY - lpY;

            TransformPoint(ref hX, ref hY);

            if (Math.Abs(hX) > maxAllowedCoordinate || Math.Abs(hY) > maxAllowedCoordinate)
                continue;

            string iconName = string.Empty;

            if (h.Type is >= (byte)HarvestableType.FIBER and <= (byte)HarvestableType.FIBER_GUARDIAN_DEAD)
                iconName = "fiber_" + h.Tier + "_" + h.Charges;
            else if (h.Type <= (byte)HarvestableType.WOOD_GUARDIAN_RED)
                iconName = "logs_" + h.Tier + "_" + h.Charges;
            else if (h.Type is >= (byte)HarvestableType.ROCK and <= (byte)HarvestableType.ROCK_GUARDIAN_RED)
                iconName = "rock_" + h.Tier + "_" + h.Charges;
            else if (h.Type is >= (byte)HarvestableType.HIDE and <= (byte)HarvestableType.HIDE_GUARDIAN)
                iconName = "hide_" + h.Tier + "_" + h.Charges;
            else if (h.Type is >= (byte)HarvestableType.ORE and <= (byte)HarvestableType.ORE_GUARDIAN_RED)
                iconName = "ore_" + h.Tier + "_" + h.Charges;

            if (string.IsNullOrEmpty(iconName))
                continue;

            DrawIcon(OverlayWindow.Graphics, iconName, new Vector2(hX, hY));
        }

        foreach (var pair in MobsHandler.Mobs)
        {
            var mob = pair.Value;

            if (mob.TypeId == 442)
                continue;

            float mobX = -1 * mob.PosX + lpX;
            float mobY = mob.PosY - lpY;

            TransformPoint(ref mobX, ref mobY);

            if (Math.Abs(mobX) > maxAllowedCoordinate || Math.Abs(mobY) > maxAllowedCoordinate)
                continue;

            if (mob.TypeId == 424 || mob.TypeId == 426 || mob.TypeId == 428 || (mob.MobInfo?.MobType == MobType.SKINNABLE && mob.TypeId != 420))
            {
                string iconName = string.Empty;
                int tier = mob.TypeId switch
                {
                    424 => 4,
                    425 => 5,
                    426 => 5,
                    427 => 6,
                    428 => 6,
                    _ => mob.MobInfo?.Tier ?? 0
                };
                
                iconName = $"hide_{tier}_{mob.EnchantmentLevel}";
                DrawIcon(OverlayWindow.Graphics, iconName, new Vector2(mobX, mobY));
                continue;
            }

            if (mob.TypeId == 534 || mob.TypeId == 535 || mob.TypeId == 536 || mob.TypeId == 537)
            {
                string iconName = string.Empty;
                int tier = mob.TypeId switch
                {
                    534 => 4,
                    535 => 5,
                    536 => 6,
                    537 => 7,
                    _ => 0
                };

                if (mob.MobInfo?.MobType == MobType.HARVESTABLE && 
                    !Config.Instance.CanShowHarvestableMob(mob.MobInfo.HarvestableMobType, mob.MobInfo.Tier, mob.EnchantmentLevel))
                    continue;

                iconName = $"ore_{tier}_{mob.EnchantmentLevel}";
                DrawIcon(OverlayWindow.Graphics, iconName, new Vector2(mobX, mobY));
                continue;
            }

            if (mob.TypeId == 554 || mob.TypeId == 555 || mob.TypeId == 557)
            {
                if (mob.MobInfo?.MobType == MobType.HARVESTABLE && 
                    !Config.Instance.CanShowHarvestableMob(mob.MobInfo.HarvestableMobType, mob.MobInfo.Tier, mob.EnchantmentLevel))
                    continue;

                string iconName = string.Empty;
                int tier = mob.MobInfo?.Tier ?? 0;
                
                if (tier == 0 || mob.EnchantmentLevel == 0)
                    continue;
                
                iconName = $"rock_{tier}_{mob.EnchantmentLevel}";
                DrawIcon(OverlayWindow.Graphics, iconName, new Vector2(mobX, mobY));
                continue;
            }

            if (mob.TypeId == 92 || mob.TypeId == 87)
            {
                DrawFilledCircle(OverlayWindow.Graphics, new Vector2(mobX, mobY), 7, _yellowBrush);
                string mistType = mob.TypeId == 92 ? "DUO" : "SOLO";
                DrawText(OverlayWindow.Graphics, $"MIST {mistType} T{mob.MobInfo?.Tier ?? 0}", new Vector2(mobX, mobY), _yellowBrush);
                continue;
            }

            DrawFilledCircle(OverlayWindow.Graphics, new Vector2(mobX, mobY), 7, _redBrush);
        }

        OverlayWindow.Graphics.FillCircle(0, 0, 5, _localPlayerBrush);
        OverlayWindow.Graphics.DrawCircle(0, 0, 5, 1.5f, _borderBrush);

        OverlayWindow.Graphics.GetDevice().Transform = originalTransform;
        int textBackgroundBrush = OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.FromArgb(150, 0, 0, 0));
        string statusText = $"Nearby: {nearbyPKPlayers}";
        float textWidth = statusText.Length * 12;
        float textHeight = 35;
        float centerX = xOffset + minimapSize / 2;
        float textPosY = yOffset - 35;
        int fontId = OverlayWindow.Graphics.CreateFont("Arial", 22);

        int textColor = hasPKPlayers ? 
            OverlayWindow.Graphics.CreateBrush(System.Drawing.Color.Red) : 
            _textBrush;

        OverlayWindow.Graphics.FillRectangle(centerX - textWidth / 2, textPosY, textWidth, textHeight, textBackgroundBrush);
        OverlayWindow.Graphics.DrawText(
            statusText,
            fontId,
            textColor, 
            (int)(centerX - textWidth / 2 + 5),
            (int)textPosY
        );
        OverlayWindow.Graphics.EndScene();
    }


    public override void Dispose()
    {
        OverlayWindow.Dispose();
        base.Dispose();
    }

    private void ClearScreen()
    {
        OverlayWindow.Graphics.BeginScene();
        OverlayWindow.Graphics.ClearScene();
        OverlayWindow.Graphics.EndScene();
    }

    public static IntPtr GetTargetWindow()
    {
        return Native.FindWindow(null, WindowName);
    }

    private static float ShiftX(float x) { return Config.Instance.RadarParams.XOffset + x + MinimapBorderWidth / 2; }
    private static float ShiftY(float y) { return Config.Instance.RadarParams.YOffset + y + MinimapBorderWidth / 2; }

    private static void TransformPoint(ref float x, ref float y)
    {
        const float angle = 225.0f * (float)Math.PI / 180.0f;

        float newX = x * (float)Math.Cos(angle) - y * (float)Math.Sin(angle);
        float newY = x * (float)Math.Sin(angle) + y * (float)Math.Cos(angle);

        newX *= Config.Instance.RadarParams.Scale;
        newY *= Config.Instance.RadarParams.Scale;

        x = newX;
        y = newY;
    }

    private void DrawFilledCircle(Direct2DRenderer graphics, Vector2 position, float radius, int brush)
    {
        graphics.FillCircle(position.X, position.Y, (int)radius, brush);
        graphics.DrawCircle(position.X, position.Y, (int)radius, 1.5f, _borderBrush);
    }

    private void DrawText(Direct2DRenderer graphics, string text, Vector2 position, int brush)
    {
        int fontId = graphics.CreateFont("Arial", 12);
        graphics.DrawText(text, fontId, brush, (int)position.X - 25, (int)position.Y - 20);
    }

    private void DrawIcon(Direct2DRenderer graphics, string iconName, Vector2 position)
    {
        try
        {
            Bitmap icon = ImageHandler.GetImage(iconName);
            if (icon != null)
            {
                float scale = Config.Instance.RadarParams.IconScale;
                float width = icon.Size.Width * scale;
                float height = icon.Size.Height * scale;
                float iconX = position.X - width / 2;
                float iconY = position.Y - height / 2;
                graphics.DrawBitmap(iconX, iconY, icon, scale, BitmapInterpolationMode.Linear);
            }
        }
        catch (Exception e)
        {
            MainForm.Log($"DrawIcon Error: {e}");
        }
    }
}
