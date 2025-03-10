using AlbionRadar.Harvestable;

namespace AlbionRadar.Mobs;

public sealed class Mob(int id, int typeId, float posX, float posY, int health, byte enchantmentLevel)
{
    public int ID { get; } = id;
    public int TypeId { get; } = typeId;
    public float PosX { get; } = posX;
    public float PosY { get; } = posY;
    public int Health { get; } = health;
    public byte EnchantmentLevel { get; set; } = enchantmentLevel;
    public MobInfo MobInfo { get; } = MobInfo.GetMobInfo(typeId);

    public override string ToString()
    {
        return $"MobData: ID={ID}, TypeID={TypeId}";
    }
}
