using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private CinemachineImpulseSource impulseSource;

    [SerializeField] private float hitStopDuration = 0.03f;

    [SerializeField] private float bossKillSlowMotionScale = 0.05f;

    [SerializeField] private float bossKillSlowMotionDuration = 1.2f;

    [SerializeField] private RewardPanel rewardPanel;

    [SerializeField] private TitleIntro titleIntro;

    public const float BaseGoldPerMeter = 0.3f;
    public const float BaseExpPerMeter = 0.5f;
    public const int GoldPerPickup = 2;
    public const int ExpPerPickup = 10;

    public int RunGoldPickupCount { get; private set; }
    public int RunExpPickupCount { get; private set; }
    public int RunChestPickupCount { get; private set; }

    public bool IsPlaying { get; private set; }

    public bool IsPaused { get; private set; }

    public event Action OnPlayStarted;

    public void StartPlay()
    {
        IsPlaying = true;

        RunGoldPickupCount = 0;
        RunExpPickupCount = 0;
        RunChestPickupCount = 0;

        AudioManager.Instance.PlayBgm(BgmId.Battle);

        OnPlayStarted?.Invoke();
    }

    public event Action<RunResult> OnRunEnded;

    public event Action OnBackPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (!IsPlaying || IsPaused)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnBackPressed?.Invoke();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (timeScaleRoutine != null)
        {
            StopCoroutine(timeScaleRoutine);
            timeScaleRoutine = null;
        }

        isSlowMotionActive = false;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void AbandonRun()
    {
        IsPlaying = false;
        IsPaused = false;
        Time.timeScale = 1f;

        if (timeScaleRoutine != null)
        {
            StopCoroutine(timeScaleRoutine);
            timeScaleRoutine = null;
        }

        isSlowMotionActive = false;

        ShowLobby();
    }

    public void ShowLobby()
    {
        titleIntro.ShowTitle();
        AudioManager.Instance.PlayBgm(BgmId.Ui);
    }

    public void AddReward(Monster.BoxType boxType)
    {
        switch (boxType)
        {
            case Monster.BoxType.Gold:
                RunGoldPickupCount++;
                break;
            case Monster.BoxType.Chest:
                RunChestPickupCount++;
                break;
            case Monster.BoxType.Exp:
                RunExpPickupCount++;
                break;
        }
    }

    public readonly struct RunResult
    {
        public readonly int GoldEarned;
        public readonly int ExpEarned;
        public readonly IReadOnlyList<WeaponId> WeaponsEarned;

        public RunResult(int goldEarned, int expEarned, IReadOnlyList<WeaponId> weaponsEarned)
        {
            GoldEarned = goldEarned;
            ExpEarned = expEarned;
            WeaponsEarned = weaponsEarned;
        }
    }

    public RunResult EndRun(int metersTravelled)
    {
        IsPlaying = false;

        int goldEarned = Mathf.RoundToInt(metersTravelled * BaseGoldPerMeter) + RunGoldPickupCount * GoldPerPickup;
        int expEarned = Mathf.RoundToInt(metersTravelled * BaseExpPerMeter) + RunExpPickupCount * ExpPerPickup;

        PlayerData.Instance.AddRunResult(goldEarned, expEarned, metersTravelled);

        var weaponsEarned = new List<WeaponId>();
        var random = new System.Random();
        for (int i = 0; i < RunChestPickupCount; i++)
        {
            WeaponId? chestResult = PlayerData.Instance.OpenChest(random);
            if (chestResult != null)
            {
                weaponsEarned.Add(chestResult.Value);
            }
        }

        var runResult = new RunResult(goldEarned, expEarned, weaponsEarned);
        AudioManager.Instance.PlaySfx(SfxId.GameEnd);
        OnRunEnded?.Invoke(runResult);
        rewardPanel.Open(runResult);
        return runResult;
    }

    private Coroutine timeScaleRoutine;

    private bool isSlowMotionActive;

    public void HitFeedback(Vector3 position)
    {
        impulseSource.GenerateImpulseAtPositionWithVelocity(position, impulseSource.DefaultVelocity);

        if (hitStopDuration <= 0f || isSlowMotionActive)
        {
            return;
        }

        if (timeScaleRoutine != null)
        {
            StopCoroutine(timeScaleRoutine);
        }

        timeScaleRoutine = StartCoroutine(RunTimeScale(0f, hitStopDuration));
    }

    public void PlayBossKillSlowMotion()
    {
        if (timeScaleRoutine != null)
        {
            StopCoroutine(timeScaleRoutine);
        }

        timeScaleRoutine = StartCoroutine(RunTimeScale(bossKillSlowMotionScale, bossKillSlowMotionDuration));
    }

    private IEnumerator RunTimeScale(float scale, float realtimeDuration)
    {
        isSlowMotionActive = scale > 0f && scale < 1f;
        Time.timeScale = scale;

        yield return new WaitForSecondsRealtime(realtimeDuration);

        Time.timeScale = 1f;
        isSlowMotionActive = false;
        timeScaleRoutine = null;
    }
}
