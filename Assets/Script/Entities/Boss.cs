using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Monster))]
public abstract class Boss : MonoBehaviour
{
    [Header("스킬 공통")]
    [SerializeField] private float skillInterval = 10f;

    [Header("레이저 스킬")]
    [SerializeField] private int laserDamage = 20;

    [SerializeField] private float laserWarningDuration = 1f;

    [SerializeField] private float laserBlinkInterval = 0.1f;

    [SerializeField] private float laserActiveDuration = 0.2f;

    [SerializeField] private BossLaser laserPrefab;

    protected Monster monster;
    protected Player player;
    protected ChunkView chunkView;

    protected PlayerStat playerStat => player.Stat;

    private float skillTimer;
    private bool activated;
    private BossLaser activeLaser;

    protected virtual void Awake()
    {
        monster = GetComponent<Monster>();
        player = FindFirstObjectByType<Player>();
        chunkView = GetComponentInParent<ChunkView>();
    }

    protected virtual void OnEnable()
    {
        activated = false;
        skillTimer = skillInterval;

        TryShowBossBar();
    }

    private void Start()
    {
        TryShowBossBar();
    }

    private void TryShowBossBar()
    {
        if (MonsterHealthBarSpawner.Instance != null)
        {
            MonsterHealthBarSpawner.Instance.ShowBossBar(monster);
        }
    }

    public void OnFirstHit()
    {
        if (activated)
        {
            return;
        }

        activated = true;
        skillTimer = skillInterval;
    }

    private void Update()
    {
        if (!activated)
        {
            return;
        }

        skillTimer -= Time.deltaTime;

        if (skillTimer <= 0f)
        {
            skillTimer = skillInterval;
            UseSkill();
        }
    }

    protected abstract void UseSkill();

    protected IEnumerator LaserSkillRoutine()
    {
        Vector3 lowerCellPosition = player.transform.position;
        Vector3 upperCellPosition = lowerCellPosition + new Vector3(0f, player.CellSize, 0f);
        int targetCol = player.PlayerX;

        BossLaser laser = Instantiate(laserPrefab);
        laser.transform.position = (lowerCellPosition + upperCellPosition) * 0.5f;
        activeLaser = laser;

        yield return laser.PlayWarningThenFire(
            laserWarningDuration,
            laserBlinkInterval,
            laserActiveDuration,
            () => TryHitPlayer(targetCol));

        activeLaser = null;
        Destroy(laser.gameObject);
    }

    private void TryHitPlayer(int targetCol)
    {
        if (player.PlayerX != targetCol)
        {
            return;
        }

        int appliedDamage = playerStat.TakeDamage(laserDamage, fromBlock: false);
        DamageTextSpawner.Instance.PlayTaken(appliedDamage, player.transform.position);
        AudioManager.Instance.PlaySfx(SfxId.PlayerHit);
    }

    protected abstract SfxId DeathSfx { get; }

    public virtual void OnDeath()
    {
        if (activeLaser != null)
        {
            activeLaser.Cleanup();
            Destroy(activeLaser.gameObject);
            activeLaser = null;
        }

        AudioManager.Instance.PlaySfx(DeathSfx);

        GameManager.Instance.PlayBossKillSlowMotion();
    }
}
