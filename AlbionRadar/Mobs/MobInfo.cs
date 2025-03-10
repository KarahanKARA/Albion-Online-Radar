using AlbionRadar.Harvestable;
using System.Collections.Generic;
using System.Linq;

namespace AlbionRadar.Mobs;

public sealed class MobInfo
{
    public static readonly List<MobInfo> MobsInfo =
    [
        // HARVESTABLE MOBS
        new(48, 3, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T3 Fiber
        new(92, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber
        new(1220, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber
        new(1230, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber
        new(518, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber (Monitor Lizard)
        new(4306, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber (Monitor Lizard)
        new(4285, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber (Monitor Lizard)
        new(4385, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber (Monitor Lizard)
        new(4286, 4, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T4 Fiber (Monitor Lizard)
        new(49, 5, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T5 Fiber
        new(795, 5, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T5 Fiber
        new(790, 5, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T5 Fiber
        new(1240, 5, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T5 Fiber
        new(89885, 5, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T5 Fiber
        new(51, 6, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T6 Fiber
        new(519, 6, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T6 Fiber
        new(521, 6, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T6 Fiber Enhanced
        new(50, 7, MobType.HARVESTABLE, HarvestableMobType.FIBER), // T7 Fiber

        new(52, 3, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T3 Hide
        new(1201, 4, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T4 Hide
        new(1202, 4, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T4 Hide
        new(1347, 4, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T4-1 Hide
        new(53, 5, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T5 Hide
        new(413, 5, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T5 Hide
        new(1203, 5, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T5 Hide
        new(55, 6, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T6 Hide
        new(415, 6, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T6 Hide
        new(54, 7, MobType.HARVESTABLE, HarvestableMobType.HIDE), // T7 Hide

        new(56, 3, MobType.HARVESTABLE, HarvestableMobType.ORE), // T3 Ore
        new(57, 3, MobType.HARVESTABLE, HarvestableMobType.ORE), // T3 Ore
        new(58, 5, MobType.HARVESTABLE, HarvestableMobType.ORE), // T5 Ore
        new(59, 5, MobType.HARVESTABLE, HarvestableMobType.ORE), // T5 Ore
        new(60, 7, MobType.HARVESTABLE, HarvestableMobType.ORE), // T7 Ore
        new(61, 6, MobType.HARVESTABLE, HarvestableMobType.ORE), // T6 Ore

        new(62, 3, MobType.HARVESTABLE, HarvestableMobType.WOOD), // T3 Wood
        new(63, 3, MobType.HARVESTABLE, HarvestableMobType.WOOD), // T3 Wood
        new(64, 5, MobType.HARVESTABLE, HarvestableMobType.WOOD), // T5 Wood
        new(65, 5, MobType.HARVESTABLE, HarvestableMobType.WOOD), // T5 Wood
        new(66, 7, MobType.HARVESTABLE, HarvestableMobType.WOOD), // T7 Wood
        new(67, 6, MobType.HARVESTABLE, HarvestableMobType.WOOD), // T6 Wood

        new(68, 3, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T3 Rock
        new(69, 3, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T3 Rock
        new(70, 5, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T5 Rock
        new(71, 5, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T5 Rock
        new(72, 7, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T7 Rock
        new(73, 6, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T6 Rock

        new(554, 4, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T4 Rock Mob
        new(557, 4, MobType.HARVESTABLE, HarvestableMobType.ROCK), // T4 Rock Mob

        // RESOURCE (MIST) MOBS
        new(75, 2, MobType.RESOURCE), // T2 Mist
        new(76, 3, MobType.RESOURCE), // T3 Mist
        new(77, 4, MobType.RESOURCE), // T4 Mist
        new(78, 5, MobType.RESOURCE), // T5 Mist
        new(79, 6, MobType.RESOURCE), // T6 Mist
        new(80, 7, MobType.RESOURCE), // T7 Mist
        new(81, 8, MobType.RESOURCE), // T8 Mist

        // SKINNABLE MOBS
        new(9, 1, MobType.SKINNABLE),
        new(16, 1, MobType.SKINNABLE),
        new(17, 1, MobType.SKINNABLE),
        new(18, 1, MobType.SKINNABLE),
        new(19, 1, MobType.SKINNABLE),
        new(20, 1, MobType.SKINNABLE),
        new(21, 1, MobType.SKINNABLE),
        new(22, 1, MobType.SKINNABLE),
        new(23, 2, MobType.SKINNABLE),
        new(24, 3, MobType.SKINNABLE),
        new(25, 4, MobType.SKINNABLE),
        new(26, 5, MobType.SKINNABLE),
        new(27, 6, MobType.SKINNABLE),
        new(28, 7, MobType.SKINNABLE),
        new(29, 8, MobType.SKINNABLE),
        new(30, 1, MobType.SKINNABLE),
        new(31, 2, MobType.SKINNABLE),
        new(32, 3, MobType.SKINNABLE),
        new(34, 5, MobType.SKINNABLE),
        new(36, 1, MobType.SKINNABLE),
        new(37, 2, MobType.SKINNABLE),
        new(38, 3, MobType.SKINNABLE),
        new(41, 6, MobType.SKINNABLE),
        new(42, 7, MobType.SKINNABLE),
        new(43, 8, MobType.SKINNABLE),
        new(44, 1, MobType.SKINNABLE),
        new(45, 1, MobType.SKINNABLE),
        new(419, 1, MobType.SKINNABLE),
        new(420, 1, MobType.SKINNABLE),
        
        new(1270, 5, MobType.SKINNABLE), // T5 Giant Snake
        new(1337, 6, MobType.SKINNABLE),  // T6 Swamp Dragon
        
        new(1227, 5, MobType.SKINNABLE), // T5 Mob
        new(1234, 5, MobType.SKINNABLE), // T5 Mob
        new(1237, 6, MobType.SKINNABLE), // T6 Mob
        new(1244, 6, MobType.SKINNABLE), // T6 Mob
        new(1277, 5, MobType.SKINNABLE), // T5 Mob
        new(1280, 5, MobType.SKINNABLE), // T5 Mob
        new(1284, 5, MobType.SKINNABLE), // T5 Mob
        new(1287, 6, MobType.SKINNABLE), // T6 Mob
        new(1290, 6, MobType.SKINNABLE), // T6 Mob
        new(1294, 6, MobType.SKINNABLE), // T6 Mob
        new(1297, 7, MobType.SKINNABLE), // T7 Mob
        new(1327, 6, MobType.SKINNABLE), // T6 Mob
        new(1330, 5, MobType.SKINNABLE), // T5 Mob
        new(1334, 5, MobType.SKINNABLE), // T5 Mob
        new(1340, 6, MobType.SKINNABLE), // T6 Mob
        new(1357, 7, MobType.SKINNABLE), // T7 Mob
        new(1370, 5, MobType.SKINNABLE), // T5 Mob
        new(1374, 5, MobType.SKINNABLE), // T5 Mob
        new(1377, 7, MobType.SKINNABLE),  // T7 Mob
        
        new(1220, 4, MobType.SKINNABLE), // T4 Mob
        new(1320, 5, MobType.SKINNABLE), // T5 Mob
        new(1284, 5, MobType.SKINNABLE), // T5 Mob
        new(1294, 6, MobType.SKINNABLE), // T6 Mob
        new(752, 8, MobType.SKINNABLE),  
        
        // T1 MOBS 20 HP (Filter)
        new(87, 4, MobType.OTHER), 
        new(411, 4, MobType.OTHER) 
    ];

    public int ID { get; }
    public byte Tier { get; }
    public MobType MobType { get; }
    public HarvestableMobType HarvestableMobType { get; }

    private MobInfo(int id, byte tier, MobType mobType)
    {
        ID = id;
        Tier = tier;
        MobType = mobType;
        HarvestableMobType = HarvestableMobType.NONE;
    }

    private MobInfo(int id, byte tier, MobType mobType, HarvestableMobType harvestableMobType)
    {
        ID = id;
        Tier = tier;
        MobType = mobType;
        HarvestableMobType = harvestableMobType;
    }

    public override string ToString()
    {
        string type = MobType == MobType.HARVESTABLE ? HarvestableMobType.ToString() : MobType.ToString();
        return $"T{Tier} {type}";
    }

    public static MobInfo GetMobInfo(int mobId)
    {
        return MobsInfo.FirstOrDefault(m => m.ID == mobId);
    }
}
