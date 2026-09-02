public static class WeaponStats
{
    private static readonly int[] RarityAttackBonusPercent = { 10, 30, 50, 80 };

    private static readonly int[] BlockDamageReducePercentByRarity = { 10, 20, 30, 50 };
    private static readonly int[] MonsterDamageBonusPercentByRarity = { 15, 30, 45, 70 };
    private static readonly int[] AttackSpeedBonusPercentByRarity = { 10, 20, 30, 45 };

    private const int ChestMissPercent = 60;
    private static readonly int[] RarityWeightPercent = { 50, 30, 15, 5 };

    public static WeaponId? RollChest(System.Random random)
    {
        if (random.Next(100) < ChestMissPercent)
        {
            return null;
        }

        int rarityRoll = random.Next(100);
        int rarityIndex = 0;
        int cumulative = 0;
        for (int i = 0; i < RarityWeightPercent.Length; i++)
        {
            cumulative += RarityWeightPercent[i];
            if (rarityRoll < cumulative)
            {
                rarityIndex = i;
                break;
            }
        }

        int indexInRarity = random.Next(4);
        return (WeaponId)(rarityIndex * 4 + indexInRarity);
    }

    private static int RarityIndex(WeaponId weapon) => (int)weapon / 4;
    private static int IndexInRarity(WeaponId weapon) => (int)weapon % 4;

    public static float GetAttackMultiplier(WeaponId weapon)
    {
        return 1f + RarityAttackBonusPercent[RarityIndex(weapon)] / 100f;
    }

    public static float GetBlockDamageMultiplier(WeaponId weapon)
    {
        return IndexInRarity(weapon) == 1 ? 1f - BlockDamageReducePercentByRarity[RarityIndex(weapon)] / 100f : 1f;
    }

    public static float GetMonsterDamageMultiplier(WeaponId weapon)
    {
        return IndexInRarity(weapon) == 2 ? 1f + MonsterDamageBonusPercentByRarity[RarityIndex(weapon)] / 100f : 1f;
    }

    public static float GetAttackSpeedMultiplier(WeaponId weapon)
    {
        return IndexInRarity(weapon) == 3 ? 1f + AttackSpeedBonusPercentByRarity[RarityIndex(weapon)] / 100f : 1f;
    }
}
