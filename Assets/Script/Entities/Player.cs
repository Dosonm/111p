using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PlayerStat))]
public class Player : MonoBehaviour
{
    #region 입력

    [BoxGroup("입력")]
    [SerializeField] private Joystick joystick;

    [BoxGroup("입력")]
    [SerializeField] private float inputThreshold = 0.4f;

    #endregion

    #region 이동 판정

    [BoxGroup("이동 판정")]
    [SerializeField] private float actionInterval = 0.3f;

    [BoxGroup("이동 판정")]
    [SerializeField] private int playerX = 4;

    [BoxGroup("이동 판정")]
    [SerializeField] private float slideDuration = 0.12f;

    #endregion

    #region 이펙트

    [BoxGroup("이펙트")]
    [SerializeField] private PlayerAttackVfx attackVfx;

    [BoxGroup("이펙트")]
    [SerializeField] private BlockBreakVfx blockBreakVfx;

    #endregion

    #region 애니메이션 스테이트 이름

    [BoxGroup("애니메이션")]
    [SerializeField] private string sideAttackState = "PlayerAttackSide";

    [BoxGroup("애니메이션")]
    [SerializeField] private string downAttackState = "PlayerAttackDown";

    [BoxGroup("애니메이션")]
    [SerializeField] private string idleState = "PlayerIdle";

    [BoxGroup("애니메이션")]
    [SerializeField] private string moveState = "PlayerMove";

    [BoxGroup("애니메이션")]
    [SerializeField] private string moveDownState = "PlayerMoveDown";

    #endregion

    #region 런타임 캐시(컴포넌트/해시)

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private int sideAttackHash;
    private int downAttackHash;
    private int idleHash;
    private int moveHash;
    private int moveDownHash;

    #endregion

    #region 런타임 상태

    private MonsterRegistry registry;
    private ScrollController scrollController;
    private float cellSize;
    private float targetX;

    private int startPlayerX;

    private PlayerStateMachine stateMachine;
    private Vector2Int currentDirectionInput;
    private DrillSkill drillSkill;

    private bool runEnded;

    #endregion

    #region 공개 프로퍼티

    public int PlayerX => playerX;
    public int PlayerRow => scrollController.CurrentPlayerRow;

    public float ActionInterval => actionInterval / PlayerData.Instance.AttackSpeedMultiplier;
    public float CellSize => cellSize;

    public PlayerIdleState IdleState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerStat Stat { get; private set; }

    public bool CanStartSkill => stateMachine.currentState != DashState;

    #endregion

    #region 초기화

    public void Init(MonsterRegistry monsterRegistry, ScrollController controller, float appliedCellSize)
    {
        registry = monsterRegistry;
        scrollController = controller;
        cellSize = appliedCellSize;

        stateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(stateMachine, this);
        AttackState = new PlayerAttackState(stateMachine, this);
        MoveState = new PlayerMoveState(stateMachine, this);
        DashState = new PlayerDashState(stateMachine, this);
    }

    private void Awake()
    {
        startPlayerX = playerX;

        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Stat = GetComponent<PlayerStat>();

        drillSkill = GetComponentInChildren<DrillSkill>();

        sideAttackHash = Animator.StringToHash(sideAttackState);
        downAttackHash = Animator.StringToHash(downAttackState);
        idleHash = Animator.StringToHash(idleState);
        moveHash = Animator.StringToHash(moveState);
        moveDownHash = Animator.StringToHash(moveDownState);
    }

    private void Start()
    {
        ApplyX(playerX);
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

        stateMachine.initialize(IdleState);

        Stat.OnHealthChanged += HandleHealthChanged;
        GameManager.Instance.OnPlayStarted += HandleRunRestarted;
    }

    private void OnDestroy()
    {
        Stat.OnHealthChanged -= HandleHealthChanged;
        GameManager.Instance.OnPlayStarted -= HandleRunRestarted;
    }

    #endregion

    #region 판 시작/종료

    public void BeginDash(int rows, float tickInterval)
    {
        DashState.Begin(rows, tickInterval);
        stateMachine.changeState(DashState);
    }

    private void HandleRunRestarted()
    {
        runEnded = false;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth > 0 || runEnded)
        {
            return;
        }

        runEnded = true;

        AudioManager.Instance.PlaySfx(SfxId.PlayerDeath);

        GameManager.Instance.EndRun(scrollController.RowsAdvanced);
    }

    public void ResetRun()
    {
        ApplyX(startPlayerX);
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

        stateMachine.changeState(IdleState);
    }

    #endregion

    #region 매 프레임 입력/이동

    private void Update()
    {
        if (!GameManager.Instance.IsPlaying || GameManager.Instance.IsPaused)
        {
            return;
        }

        currentDirectionInput = DirectionReader.Read(joystick.Direction, inputThreshold);

        stateMachine.update();

        SlideTowardTarget();
    }

    public Vector2Int ReadDirection()
    {
        return currentDirectionInput;
    }

    private void ApplyX(int newX)
    {
        playerX = Mathf.Clamp(newX, 0, GridMath.Columns - 1);
        targetX = GridMath.WorldX(playerX, cellSize);
    }

    private void SlideTowardTarget()
    {
        if (Mathf.Approximately(transform.position.x, targetX))
        {
            return;
        }

        float speed = cellSize / slideDuration;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    public void Advance(Vector2Int direction)
    {
        if (direction.x != 0)
        {
            ApplyX(playerX + direction.x);
        }

        if (direction.y != 0)
        {
            scrollController.StepDown();
        }
    }

    #endregion

    #region 격자 조회

    public bool IsBlocked(Vector2Int direction)
    {
        int targetCol = playerX + direction.x;
        return targetCol < 0 || targetCol >= GridMath.Columns;
    }

    public bool HasMonsterAt(Vector2Int direction)
    {
        int targetRow = scrollController.CurrentPlayerRow - direction.y;
        int targetCol = playerX + direction.x;
        return registry.TryGet(targetRow, targetCol, out _);
    }

    public bool IsPickupAt(Vector2Int direction)
    {
        int targetRow = scrollController.CurrentPlayerRow - direction.y;
        int targetCol = playerX + direction.x;
        return registry.TryGet(targetRow, targetCol, out Monster monster) && monster.IsPickup;
    }

    public Monster PeekCell(int row, int col)
    {
        registry.TryGet(row, col, out Monster monster);
        return monster;
    }

    #endregion

    #region 공격 진입점

    public void CollectPickupAt(Vector2Int direction)
    {
        int targetRow = scrollController.CurrentPlayerRow - direction.y;
        int targetCol = playerX + direction.x;

        if (!registry.TryGet(targetRow, targetCol, out Monster monster) || !monster.IsPickup)
        {
            return;
        }

        GameManager.Instance.AddReward(monster.boxType);
        AudioManager.Instance.PlaySfx(SfxId.Pickup);

        monster.TakeDamage(int.MaxValue, out _);
    }

    public void AttackInDirection(Vector2Int direction)
    {
        int targetRow = scrollController.CurrentPlayerRow - direction.y;
        int targetCol = playerX + direction.x;

        if (!registry.TryGet(targetRow, targetCol, out Monster monster))
        {
            return;
        }

        attackVfx.Play(direction, monster.boxType);
        PlayAttackAnimation(direction);
        AudioManager.Instance.PlaySfx(SfxId.PlayerAttack);

        int incomingDamage = monster.Damage;
        bool fromBlock = monster.boxType == Monster.BoxType.Block;

        bool died = ResolveHit(targetRow, targetCol, monster);

        if (drillSkill != null)
        {
            Vector2Int facing = direction.y != 0 ? Vector2Int.down : new Vector2Int(direction.x, 0);
            drillSkill.OnPlayerAttacked(monster, facing);
        }

        int appliedDamage = Stat.TakeDamage(incomingDamage, fromBlock);
        DamageTextSpawner.Instance.PlayTaken(appliedDamage, transform.position);

        if (died)
        {
            Advance(direction);
        }
    }

    public bool TryAttackCell(int row, int col)
    {
        return TryAttackCell(row, col, out _);
    }

    public bool TryAttackCell(int row, int col, out Monster hit)
    {
        if (!registry.TryGet(row, col, out Monster monster))
        {
            hit = null;
            return false;
        }

        hit = monster;
        return ResolveHit(row, col, monster);
    }

    private bool ResolveHit(int row, int col, Monster monster)
    {
        int dRow = scrollController.CurrentPlayerRow - row;
        int dCol = col - playerX;
        Vector3 hitPosition = transform.position + new Vector3(dCol * cellSize, dRow * cellSize, 0f);

        Vector3 monsterPosition = monster.transform.position;
        Monster.BoxType boxType = monster.boxType;
        bool wasBlock = boxType == Monster.BoxType.Block;

        bool isMonster = !wasBlock && !monster.IsPickup;
        int attackDamage = Stat.AttackDamage;
        if (isMonster)
        {
            attackDamage = Mathf.RoundToInt(attackDamage * PlayerData.Instance.MonsterDamageMultiplier);
        }

        attackDamage = Mathf.RoundToInt(attackDamage * PlayerData.RollDamageVariance());

        bool died = monster.TakeDamage(attackDamage, out int appliedAttackDamage);
        DamageTextSpawner.Instance.PlayDealt(appliedAttackDamage, hitPosition);

        if (monster.IsPickup)
        {
            if (died)
            {
                AudioManager.Instance.PlaySfx(SfxId.Pickup);
            }
        }
        else
        {
            SfxId sfx = wasBlock
                ? (died ? SfxId.BlockBreak : SfxId.BlockNonBreak)
                : (died ? SfxId.MonsterDeath : SfxId.MonsterNonDeath);
            AudioManager.Instance.PlaySfx(sfx);
        }

        if (died)
        {
            GameManager.Instance.AddReward(boxType);
            GameManager.Instance.HitFeedback(monsterPosition);

            if (wasBlock)
            {
                blockBreakVfx.Play(monsterPosition);
            }
        }

        return died;
    }

    #endregion

    #region 애니메이션 재생

    private void PlayAttackAnimation(Vector2Int direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        if (direction.y == 0)
        {
            animator.Play(sideAttackHash, 0, 0f);
            return;
        }

        animator.Play(downAttackHash, 0, 0f);
    }

    public void PlayIdleAnimation()
    {
        animator.Play(idleHash, 0, 0f);
    }

    public void PlayMoveAnimation(Vector2Int direction)
    {
        if (direction.x == 0)
        {
            animator.Play(moveDownHash, 0, 0f);
            return;
        }

        spriteRenderer.flipX = direction.x < 0;
        animator.Play(moveHash, 0, 0f);
    }

    public void PlayDashAnimation()
    {
        animator.Play(downAttackHash, 0, 0f);
    }

    #endregion
}
