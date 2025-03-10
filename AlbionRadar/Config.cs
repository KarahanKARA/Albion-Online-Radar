using AlbionRadar.Harvestable;
using AlbionRadar.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace AlbionRadar;

public sealed class Config
{
    public Radar RadarParams { get; set; } = new();
    public PlayerSettings Players { get; set; } = new();
    public HarvestableSettings Wood { get; set; } = new();
    public HarvestableSettings Stone { get; set; } = new();
    public HarvestableSettings Hide { get; set; } = new();
    public HarvestableSettings Ore { get; set; } = new();
    public HarvestableSettings Fiber { get; set; } = new();
    public DisplaySettings Display { get; set; } = new();

    static Config()
    {
        try
        {
            if (File.Exists(FileName))
            {
                Instance = Serializer.DeserializeFromFile<Config>(FileName);
                if (!ValidateConfig(Instance))
                {
                    MainForm.Log("Config Err");
                    Default();
                }
            }
            else
            {
                Default();
            }
        }
        catch (Exception ex)
        {
            MainForm.Log($"Config Err: {ex.Message}");
            Default();
        }
    }

    private static bool ValidateConfig(Config config)
    {
        try
        {
            foreach (var settings in new[] { config.Wood, config.Stone, config.Hide, config.Ore, config.Fiber })
            {
                if (settings?.Tier == null || settings.Tier.Length != 8)
                    return false;

                foreach (var tier in settings.Tier)
                {
                    if (tier?.Enchants == null || tier.Enchants.Length != 4)
                        return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public const string FileName = "config.json";
    public static Config Instance;

    public class Radar
    {
        public float XOffset { get; set; } = 10;
        public float YOffset { get; set; } = 160;
        public float Size { get; set; } = 480;
        public float Scale { get; set; } = 2.0f;
        public float IconScale { get; set; } = 1.0f;
        public float IconSize { get; set; } = 4.0f;
    }

    public class HarvestableSettings
    {
        public class TierSettings
        {
            public bool Enabled { get; set; } = true;
            public bool[] Enchants { get; set; } = [true, true, true, true];
        }

        public HarvestableSettings()
        {
            for (int i = 0; i < Tier.Length; ++i)
                Tier[i] = new TierSettings();
        }

        public TierSettings[] Tier { get; set; } = new TierSettings[8];
    }

    public class PlayerSettings
    {
        public bool ShowPlayers { get; set; } = true;
        public bool PlaySound { get; set; } = false;

        public List<string> EnemyGuilds { get; set; } = new List<string>();

    }

    public class DisplaySettings
    {
        public bool ShowMobs { get; set; } = true;
        public bool ShowMists { get; set; } = true;
        public bool ShowDynamicGather { get; set; } = true;
        public bool ShowStaticGather { get; set; } = true;
    }

    private static void Default()
    {
        Instance = new Config();
        
        Instance.RadarParams = new Radar
        {
            XOffset = 10,
            YOffset = 160,
            Size = 480,
            Scale = 2.0f,
            IconScale = 1.0f,
            IconSize = 4.0f
        };

        Instance.Players = new PlayerSettings
        {
            ShowPlayers = true,
            PlaySound = false,
            EnemyGuilds = new List<string>()
        };

        Instance.Display = new DisplaySettings
        {
            ShowMobs = true,
            ShowMists = true,
            ShowDynamicGather = true,
            ShowStaticGather = true
        };

        Instance.Wood = CreateDefaultHarvestableSettings();
        Instance.Stone = CreateDefaultHarvestableSettings();
        Instance.Hide = CreateDefaultHarvestableSettings();
        Instance.Ore = CreateDefaultHarvestableSettings();
        Instance.Fiber = CreateDefaultHarvestableSettings();

        Save();
        MainForm.Log("New Config File Created.");
    }

    private static HarvestableSettings CreateDefaultHarvestableSettings()
    {
        var settings = new HarvestableSettings();
        for (int i = 0; i < settings.Tier.Length; i++)
        {
            settings.Tier[i] = new HarvestableSettings.TierSettings
            {
                Enabled = true,
                Enchants = [true, true, true, true]
            };
        }
        return settings;
    }

    public static void ResetConfig()
    {
        try
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
            
            Default();
        }
        catch (Exception ex)
        {
            MainForm.Log($"Config error: {ex.Message}");
        }
    }

    public static void Save()
    {
        try
        {
            var fullPath = Path.GetFullPath(FileName);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            Serializer.Serialize(Instance, FileName);
        }
        catch
        {
            Default();
        }
    }

    public bool CanShowHarvestable(HarvestableType type, byte tier, byte enchant)
    {
        if (tier <= 0 || tier > 8)
            return false;

        HarvestableSettings settings;

        switch (type)
        {
            case >= HarvestableType.FIBER and <= HarvestableType.FIBER_GUARDIAN_DEAD:
                settings = Fiber;
                break;
            case <= HarvestableType.WOOD_GUARDIAN_RED:
                settings = Wood;
                break;
            case >= HarvestableType.ROCK and <= HarvestableType.ROCK_GUARDIAN_RED:
                settings = Stone;
                break;
            case >= HarvestableType.HIDE and <= HarvestableType.HIDE_GUARDIAN:
                settings = Hide;
                break;
            case >= HarvestableType.ORE and <= HarvestableType.ORE_GUARDIAN_RED:
                settings = Ore;
                break;
            default:
                return false;
        }

        if (enchant == 0)
            return settings.Tier[tier - 1].Enabled;

        if (enchant > 0 && enchant <= 4)
            return settings.Tier[tier - 1].Enchants[enchant - 1];

        return false;
    }

    public bool CanShowHarvestableMob(Mobs.HarvestableMobType type, byte tier, byte enchant)
    {
        if (tier <= 0 || tier > 8)
            return false;

        HarvestableSettings settings;

        switch (type)
        {
            case Mobs.HarvestableMobType.FIBER:
                settings = Fiber;
                break;
            case Mobs.HarvestableMobType.WOOD:
                settings = Wood;
                break;
            case Mobs.HarvestableMobType.ROCK:
                settings = Stone;
                break;
            case Mobs.HarvestableMobType.HIDE:
                settings = Hide;
                break;
            case Mobs.HarvestableMobType.ORE:
                settings = Ore;
                break;
            default:
                return false;
        }

        if (enchant == 0)
            return settings.Tier[tier - 1].Enabled;

        if (enchant > 0 && enchant <= 4)
            return settings.Tier[tier - 1].Enchants[enchant - 1];

        return false;
    }
}
