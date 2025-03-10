using AlbionRadar.Drawing;
using AlbionRadar.Utils;
using Overlay.NET.Directx;
using System;
using System.Windows.Forms;

namespace AlbionRadar;

public partial class SettingForm : Form
{
    private bool CanChangeSettings;
    private DarkModeCS DM;
    private System.Windows.Forms.Timer iconSizeTimer;
    private decimal lastIconSize;

    public SettingForm()
    {
        InitializeComponent();
        DM = new DarkModeCS(this);
        this.FormClosing += SettingForm_FormClosing;

        // Timer'ı ayarla
        iconSizeTimer = new System.Windows.Forms.Timer();
        iconSizeTimer.Interval = 250; // 250ms bekle
        iconSizeTimer.Tick += IconSizeTimer_Tick;

        // Yeni checkbox'lar için click event'lerini ekle
        mobCheckbox.Click += CheckBox_Click;
        mistCheckbox.Click += CheckBox_Click;
        dynamicGatherCheckbox.Click += CheckBox_Click;
        staticGatherCheckbox.Click += CheckBox_Click;
    }

    private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        UpdateSettings();
        Config.Save();
    }

    private void SettingForm_Load(object sender, EventArgs e)
    {
        var config = Config.Instance;

        nRadarX.Value = (decimal)config.RadarParams.XOffset;
        nRadarY.Value = (decimal)config.RadarParams.YOffset;
        nRadarSize.Value = (decimal)config.RadarParams.Size;
        nRadarScale.Value = (decimal)config.RadarParams.Scale;
        nIconSize.Value = (decimal)config.RadarParams.IconSize;

        mobCheckbox.Checked = config.Display.ShowMobs;
        mistCheckbox.Checked = config.Display.ShowMists;
        dynamicGatherCheckbox.Checked = config.Display.ShowDynamicGather;
        staticGatherCheckbox.Checked = config.Display.ShowStaticGather;

        // Wood
        for (int i = 0; i < config.Wood.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbWoodT{i + 1}", true);

            if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                throw new();

            ((CheckBox)cbs[0]).Checked = config.Wood.Tier[i].Enabled;

            // Wood has enchants starting from tier 4
            if (i < 3)
                continue;

            for (int x = 0; x < config.Wood.Tier[i].Enchants.Length; ++x)
            {
                cbs = Controls.Find($"cbWoodT{i + 1}E{x + 1}", true);

                if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                    throw new();

                ((CheckBox)cbs[0]).Checked = config.Wood.Tier[i].Enchants[x];
            }
        }

        // Stone
        for (int i = 0; i < config.Stone.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbStoneT{i + 1}", true);

            if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                throw new();

            ((CheckBox)cbs[0]).Checked = config.Stone.Tier[i].Enabled;

            // Stone has enchants starting from tier 4
            if (i < 3)
                continue;

            for (int x = 0; x < config.Stone.Tier[i].Enchants.Length; ++x)
            {
                cbs = Controls.Find($"cbStoneT{i + 1}E{x + 1}", true);

                if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                    throw new();

                ((CheckBox)cbs[0]).Checked = config.Stone.Tier[i].Enchants[x];
            }
        }

        // Hide
        for (int i = 0; i < config.Hide.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbHideT{i + 1}", true);

            if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                throw new();

            ((CheckBox)cbs[0]).Checked = config.Hide.Tier[i].Enabled;

            // Hide has enchants starting from tier 4
            if (i < 3)
                continue;

            for (int x = 0; x < config.Hide.Tier[i].Enchants.Length; ++x)
            {
                cbs = Controls.Find($"cbHideT{i + 1}E{x + 1}", true);

                if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                    throw new();

                ((CheckBox)cbs[0]).Checked = config.Hide.Tier[i].Enchants[x];
            }
        }

        // Ore
        for (int i = 0; i < config.Ore.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbOreT{i + 1}", true);

            if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                throw new();

            ((CheckBox)cbs[0]).Checked = config.Ore.Tier[i].Enabled;

            // Ore has enchants starting from tier 4
            if (i < 3)
                continue;

            for (int x = 0; x < config.Ore.Tier[i].Enchants.Length; ++x)
            {
                cbs = Controls.Find($"cbOreT{i + 1}E{x + 1}", true);

                if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                    throw new();

                ((CheckBox)cbs[0]).Checked = config.Ore.Tier[i].Enchants[x];
            }
        }

        // Fiber
        for (int i = 0; i < config.Fiber.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbFiberT{i + 1}", true);

            if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                throw new();

            ((CheckBox)cbs[0]).Checked = config.Fiber.Tier[i].Enabled;

            // Fiber has enchants starting from tier 4
            if (i < 3)
                continue;

            for (int x = 0; x < config.Fiber.Tier[i].Enchants.Length; ++x)
            {
                cbs = Controls.Find($"cbFiberT{i + 1}E{x + 1}", true);

                if (cbs.Length != 1 || cbs[0].GetType() != typeof(CheckBox))
                    throw new();

                ((CheckBox)cbs[0]).Checked = config.Fiber.Tier[i].Enchants[x];
            }
        }

        CanChangeSettings = true;
    }

    private void CheckBox_Click(object sender, EventArgs e)
    {
        if (!CanChangeSettings)
            return;

        var config = Config.Instance;
        if (sender is CheckBox cb)
        {
            if (cb == mobCheckbox)
                config.Display.ShowMobs = cb.Checked;
            else if (cb == mistCheckbox)
                config.Display.ShowMists = cb.Checked;
            else if (cb == dynamicGatherCheckbox)
                config.Display.ShowDynamicGather = cb.Checked;
            else if (cb == staticGatherCheckbox)
                config.Display.ShowStaticGather = cb.Checked;

            Config.Save();
        }
    }

    private void PropertyChanged(object sender, EventArgs e)
    {
        UpdateSettings();
    }

    private void UpdateSettings()
    {
        if (!CanChangeSettings)
            return;

        var config = Config.Instance;

        config.RadarParams.XOffset = (float)nRadarX.Value;
        config.RadarParams.YOffset = (float)nRadarY.Value;
        config.RadarParams.Size = (float)nRadarSize.Value;
        config.RadarParams.Scale = (float)nRadarScale.Value;
        config.RadarParams.IconSize = (float)nIconSize.Value;
        config.Display.ShowMobs = mobCheckbox.Checked;
        config.Display.ShowMists = mistCheckbox.Checked;
        config.Display.ShowDynamicGather = dynamicGatherCheckbox.Checked;
        config.Display.ShowStaticGather = staticGatherCheckbox.Checked;

        // Wood
        for (int i = 0; i < config.Wood.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbWoodT{i + 1}", true);
            if (cbs.Length == 1 && cbs[0] is CheckBox cb)
            {
                config.Wood.Tier[i].Enabled = cb.Checked;

                if (i >= 3)
                {
                    for (int x = 0; x < config.Wood.Tier[i].Enchants.Length; ++x)
                    {
                        var enchantCbs = Controls.Find($"cbWoodT{i + 1}E{x + 1}", true);
                        if (enchantCbs.Length == 1 && enchantCbs[0] is CheckBox enchantCb)
                        {
                            config.Wood.Tier[i].Enchants[x] = enchantCb.Checked;
                        }
                    }
                }
            }
        }

        // Stone
        for (int i = 0; i < config.Stone.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbStoneT{i + 1}", true);
            if (cbs.Length == 1 && cbs[0] is CheckBox cb)
            {
                config.Stone.Tier[i].Enabled = cb.Checked;

                if (i >= 3)
                {
                    for (int x = 0; x < config.Stone.Tier[i].Enchants.Length; ++x)
                    {
                        var enchantCbs = Controls.Find($"cbStoneT{i + 1}E{x + 1}", true);
                        if (enchantCbs.Length == 1 && enchantCbs[0] is CheckBox enchantCb)
                        {
                            config.Stone.Tier[i].Enchants[x] = enchantCb.Checked;
                        }
                    }
                }
            }
        }

        // Hide
        for (int i = 0; i < config.Hide.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbHideT{i + 1}", true);
            if (cbs.Length == 1 && cbs[0] is CheckBox cb)
            {
                config.Hide.Tier[i].Enabled = cb.Checked;

                if (i >= 3)
                {
                    for (int x = 0; x < config.Hide.Tier[i].Enchants.Length; ++x)
                    {
                        var enchantCbs = Controls.Find($"cbHideT{i + 1}E{x + 1}", true);
                        if (enchantCbs.Length == 1 && enchantCbs[0] is CheckBox enchantCb)
                        {
                            config.Hide.Tier[i].Enchants[x] = enchantCb.Checked;
                        }
                    }
                }
            }
        }

        // Ore
        for (int i = 0; i < config.Ore.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbOreT{i + 1}", true);
            if (cbs.Length == 1 && cbs[0] is CheckBox cb)
            {
                config.Ore.Tier[i].Enabled = cb.Checked;

                if (i >= 3)
                {
                    for (int x = 0; x < config.Ore.Tier[i].Enchants.Length; ++x)
                    {
                        var enchantCbs = Controls.Find($"cbOreT{i + 1}E{x + 1}", true);
                        if (enchantCbs.Length == 1 && enchantCbs[0] is CheckBox enchantCb)
                        {
                            config.Ore.Tier[i].Enchants[x] = enchantCb.Checked;
                        }
                    }
                }
            }
        }

        // Fiber
        for (int i = 0; i < config.Fiber.Tier.Length; ++i)
        {
            var cbs = Controls.Find($"cbFiberT{i + 1}", true);
            if (cbs.Length == 1 && cbs[0] is CheckBox cb)
            {
                config.Fiber.Tier[i].Enabled = cb.Checked;

                if (i >= 3)
                {
                    for (int x = 0; x < config.Fiber.Tier[i].Enchants.Length; ++x)
                    {
                        var enchantCbs = Controls.Find($"cbFiberT{i + 1}E{x + 1}", true);
                        if (enchantCbs.Length == 1 && enchantCbs[0] is CheckBox enchantCb)
                        {
                            config.Fiber.Tier[i].Enchants[x] = enchantCb.Checked;
                        }
                    }
                }
            }
        }

        Config.Save();
    }

    private void IconSizeTimer_Tick(object sender, EventArgs e)
    {
        iconSizeTimer.Stop();
        try
        {
            if (!CanChangeSettings)
                return;

            var config = Config.Instance;
            if (lastIconSize != nIconSize.Value)
            {
                lastIconSize = nIconSize.Value;
                config.RadarParams.IconSize = (float)nIconSize.Value;
                Config.Save();
                ImageHandler.Reload();
            }
        }
        catch (Exception ex)
        {
            MainForm.Log($"Error updating icon size: {ex.Message}");
        }
    }

    private void nIconSize_ValueChanged(object sender, EventArgs e)
    {
        if (!CanChangeSettings)
            return;

        // Timer'ı yeniden başlat
        iconSizeTimer.Stop();
        iconSizeTimer.Start();
    }

    private void btnResetConfig_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(
            "Tüm ayarları sıfırlamak istediğinize emin misiniz?",
            "Ayarları Sıfırla",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            CanChangeSettings = false;
            Config.ResetConfig();
            MessageBox.Show(
                "Ayarlar sıfırlandı. Program yeniden başlatılacak.",
                "Bilgi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Application.Restart();
        }
    }
}
