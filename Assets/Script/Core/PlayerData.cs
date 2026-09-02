using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private static readonly int UpgradeTypeCount = Enum.GetValues(typeof(UpgradeType)).Length;

    private static int[] NewUpgradeLevels()
    {
        int[] levels = new int[UpgradeTypeCount];
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i] = 1;
        }

        return levels;
    }

    [Serializable]
    private class SaveData
    {
        [BoxGroup("재화")] public int gold;
        [BoxGroup("재화")] public int exp;
        [BoxGroup("재화")] public int level = 1;

        [BoxGroup("재화")] public int levelPoint;

        [BoxGroup("재화")] public int bestMeters;

        [BoxGroup("무기")] public List<WeaponId> ownedWeaponIds = new();
        [BoxGroup("무기")] public WeaponId equippedWeaponId = WeaponId.Normal0;

        [BoxGroup("업그레이드 레벨")] public int[] upgradeLevels;
    }

    private const string SaveKey = "SaveData";

    public static PlayerData Instance { get; private set; }

    [BoxGroup("세이브 데이터 (직접 수정 가능)")]
    [ShowInInspector, HideInEditorMode]
    [OnValueChanged("OnEditedInInspector", IncludeChildren = true)]
    private SaveData data;

    private void OnEditedInInspector()
    {
        Save();
    }

    public const int MaxHealthBase = 1111;
    public const int HealthPerLevel = 100;
    public const int AttackDamageBase = 101;
    public const int AttackDamagePerLevel = 10;
    public const int UpgradeBaseCost = 100;
    //public const int UpgradeCostStep = 10;

    public const int DashRowsBase = 3;
    public const int DashRowsPerLevel = 1;
    public const int BarrierReducePercentBase = 30;
    public const int BarrierReducePercentPerLevel = 10;
    public const int DrillChargesBase = 5;
    public const int DrillChargesPerLevel = 1;

    public const int SkillUpgradeMaxLevel = 5;

    private const float DamageVarianceMin = 0.7f;
    private const float DamageVarianceMax = 1.3f;

    public static float RollDamageVariance()
    {
        return UnityEngine.Random.Range(DamageVarianceMin, DamageVarianceMax);
    }

    public int Gold => data.gold;
    public int Exp => data.exp;
    public int Level => data.level;
    public int LevelPoint => data.levelPoint;
    public int BestMeters => data.bestMeters;

    [BoxGroup("파생 스탯 (읽기 전용)")]
    [ShowInInspector, HideInEditorMode, ReadOnly]
    public int MaxHealth => data == null ? 0
        : MaxHealthBase + (GetUpgradeLevel(UpgradeType.Health) - 1) * HealthPerLevel;

    [BoxGroup("파생 스탯 (읽기 전용)")]
    [ShowInInspector, HideInEditorMode, ReadOnly]
    public int AttackDamage => data == null ? 0 : Mathf.RoundToInt(
        (AttackDamageBase + (GetUpgradeLevel(UpgradeType.AttackDamage) - 1) * AttackDamagePerLevel)
        * WeaponStats.GetAttackMultiplier(EquippedWeapon));

    public WeaponId EquippedWeapon => data.equippedWeaponId;

    public float BlockDamageMultiplier => WeaponStats.GetBlockDamageMultiplier(EquippedWeapon);
    public float MonsterDamageMultiplier => WeaponStats.GetMonsterDamageMultiplier(EquippedWeapon);
    public float AttackSpeedMultiplier => WeaponStats.GetAttackSpeedMultiplier(EquippedWeapon);

    public int GetDashRows() => DashRowsBase + (GetUpgradeLevel(UpgradeType.Skill1) - 1) * DashRowsPerLevel;

    public int GetBarrierReducePercent() => BarrierReducePercentBase + (GetUpgradeLevel(UpgradeType.Skill2) - 1) * BarrierReducePercentPerLevel;

    public int GetDrillCharges() => DrillChargesBase + (GetUpgradeLevel(UpgradeType.Skill3) - 1) * DrillChargesPerLevel;

    public event Action OnDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    private void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);

        data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);

        if (data.ownedWeaponIds.Count == 0)
        {
            data.ownedWeaponIds.Add(WeaponId.Normal0);
        }

        if (data.upgradeLevels == null || data.upgradeLevels.Length != UpgradeTypeCount)
        {
            int[] resized = NewUpgradeLevels();
            if (data.upgradeLevels != null)
            {
                int copyLength = Mathf.Min(data.upgradeLevels.Length, resized.Length);
                Array.Copy(data.upgradeLevels, resized, copyLength);
            }

            data.upgradeLevels = resized;
        }
    }

    private void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));

        PlayerPrefs.Save();

        OnDataChanged?.Invoke();
    }

    public const int LevelUpExpBase = 300;
    public const int LevelUpExpStep = 100;

    public int GetExpToNextLevel(int level) => LevelUpExpBase + (level - 1) * LevelUpExpStep;

    public void AddRunResult(int goldEarned, int expEarned, int metersTravelled)
    {
        data.gold += goldEarned;
        data.exp += expEarned;

        while (data.exp >= GetExpToNextLevel(data.level))
        {
            data.exp -= GetExpToNextLevel(data.level);
            data.level++;
            data.levelPoint++;
        }

        if (metersTravelled > data.bestMeters)
        {
            data.bestMeters = metersTravelled;
        }

        Save();
    }

    public bool TrySpendGold(int amount)
    {
        if (!TrySpendGoldNoSave(amount))
        {
            return false;
        }

        Save();
        return true;
    }

    private bool TrySpendGoldNoSave(int amount)
    {
        if (amount <= 0 || data.gold < amount)
        {
            return false;
        }

        data.gold -= amount;
        return true;
    }

    private bool TrySpendLevelPoint(int amount)
    {
        if (amount <= 0 || data.levelPoint < amount)
        {
            return false;
        }

        data.levelPoint -= amount;
        return true;
    }

    public bool IsWeaponOwned(WeaponId weapon)
    {
        return data.ownedWeaponIds.Contains(weapon);
    }

    public bool AddWeapon(WeaponId weapon)
    {
        if (IsWeaponOwned(weapon))
        {
            return false;
        }

        data.ownedWeaponIds.Add(weapon);
        Save();
        return true;
    }

    public WeaponId? OpenChest(System.Random random)
    {
        WeaponId? result = WeaponStats.RollChest(random);
        if (result == null)
        {
            return null;
        }

        return AddWeapon(result.Value) ? result : null;
    }

    public bool TryEquipWeapon(WeaponId weapon)
    {
        if (!IsWeaponOwned(weapon))
        {
            return false;
        }

        data.equippedWeaponId = weapon;
        Save();
        return true;
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
        return data.upgradeLevels[(int)type];
    }

    private static bool UsesGold(UpgradeType type)
    {
        return type == UpgradeType.Health || type == UpgradeType.AttackDamage;
    }

    public int GetUpgradeMaxLevel(UpgradeType type)
    {
        return UsesGold(type) ? int.MaxValue : SkillUpgradeMaxLevel;
    }

    public bool IsUpgradeMaxLevel(UpgradeType type)
    {
        return GetUpgradeLevel(type) >= GetUpgradeMaxLevel(type);
    }

    public int GetUpgradeGoldCost(UpgradeType type)
    {
        int level = GetUpgradeLevel(type);
        return UpgradeBaseCost;// + (level - 1) * UpgradeCostStep;
    }

    public bool TryUpgrade(UpgradeType type)
    {
        if (IsUpgradeMaxLevel(type))
        {
            return false;
        }

        if (UsesGold(type))
        {
            if (!TrySpendGoldNoSave(GetUpgradeGoldCost(type)))
            {
                return false;
            }
        }
        else if (!TrySpendLevelPoint(1))
        {
            return false;
        }

        data.upgradeLevels[(int)type]++;
        Save();
        return true;
    }

    [BoxGroup("디버그")]
    [Button("레벨포인트 +5 (디버그)"), HideInEditorMode]
    private void GrantDebugLevelPoints()
    {
        data.levelPoint += 5;
        Save();
    }

    [BoxGroup("디버그")]
    [Button("세이브 초기화"), HideInEditorMode]
    private void ResetSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);

        Load();

        StartCoroutine(InvokeDataChangedNextFrame());
    }

    private IEnumerator InvokeDataChangedNextFrame()
    {
        yield return null;
        OnDataChanged?.Invoke();
    }
}
