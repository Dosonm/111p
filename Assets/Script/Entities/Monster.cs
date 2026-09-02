using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public enum BoxType
    {
        Block,
        Monster,
        Gold,

        Chest,
        Exp
    }

    #region 인스펙터 설정값

    [BoxGroup("기본 스탯")]
    [SerializeField] private int maxHealth = 10;

    [BoxGroup("기본 스탯")]
    [SerializeField] private int damage = 1;

    [BoxGroup("기본 스탯")]
    [EnumToggleButtons]
    public BoxType boxType;

    [BoxGroup("차지 영역(Footprint)")]
    [MinValue(1)]
    [SerializeField] private int footprintWidth = 1;

    [BoxGroup("차지 영역(Footprint)")]
    [MinValue(1)]
    [SerializeField] private int footprintHeight = 1;

    #endregion

    #region 공개 프로퍼티 / 이벤트

    public float IncomingDamageMultiplier { get; set; } = 1f;

    public int Damage => damage;

    public bool IsPickup => boxType == BoxType.Gold || boxType == BoxType.Chest || boxType == BoxType.Exp;

    public bool IsDynamic { get; set; }

    public bool IsBoss => boss != null;

    public MonsterRegistry Registry => registry;

    public SpriteRenderer BarAnchorSprite => spriteRenderer;

    public event Action<int, int> OnHealthChanged;

    #endregion

    #region 런타임 상태 (플레이 모드 전용, 디버그용)

    [ShowInInspector, ReadOnly, HideInEditorMode, BoxGroup("런타임 상태")]
    public bool IsDead { get; private set; }

    [ShowInInspector, ReadOnly, HideInEditorMode, BoxGroup("런타임 상태")]
    public int CurrentHealth => currentHealth;

    [ShowInInspector, ReadOnly, HideInEditorMode, BoxGroup("런타임 상태")]
    public int MaxHealth => maxHealth;

    #endregion

    #region 내부 상태

    private int currentHealth;
    private int row;
    private int col;
    private MonsterRegistry registry;
    private Boss boss;
    private SpriteRenderer spriteRenderer;

    #endregion

    #region Unity 생명주기

    private void Awake()
    {
        boss = GetComponent<Boss>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnDisable()
    {
        if (MonsterHealthBarSpawner.Instance != null)
        {
            MonsterHealthBarSpawner.Instance.Release(this);
        }
    }

    #endregion

    #region 격자 등록 / 초기화

    public void Init(int row, int col, MonsterRegistry registry)
    {
        currentHealth = maxHealth;
        IsDead = false;
        IncomingDamageMultiplier = 1f;
        this.registry = registry;
        this.row = row;
        this.col = col;

        ForEachFootprintCell((r, c) => registry.Register(r, c, this));

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void UnregisterFromChunkReturn()
    {
        ForEachFootprintCell((r, c) => registry.Unregister(r, c, this));

        if (IsDynamic)
        {
            Destroy(gameObject);
        }
    }

    private void ForEachFootprintCell(Action<int, int> action)
    {
        for (int r = row; r < row + footprintHeight; r++)
        {
            for (int c = col; c < col + footprintWidth; c++)
            {
                action(r, c);
            }
        }
    }

    #endregion

    #region 전투

    public bool TakeDamage(int damage, out int appliedDamage)
    {
        if (boss != null)
        {
            boss.OnFirstHit();
        }

        appliedDamage = Mathf.RoundToInt(damage * IncomingDamageMultiplier);
        if (damage > 0 && appliedDamage < 1)
        {
            appliedDamage = 1;
        }
        currentHealth -= appliedDamage;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (boss == null && currentHealth > 0 && MonsterHealthBarSpawner.Instance != null)
        {
            MonsterHealthBarSpawner.Instance.ShowMonsterBar(this);
        }

        if (currentHealth <= 0 && !IsDead)
        {
            IsDead = true;

            if (boss != null)
            {
                boss.OnDeath();
            }

            ForEachFootprintCell((r, c) => registry.Unregister(r, c, this));

            if (IsDynamic)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        return false;
    }

    #endregion
}
