using System.Collections;
using UnityEngine;

public class BarrierSkill : PlayerSkill
{
    [SerializeField] private float duration = 7f;

    [SerializeField] private GameObject auraVisual;

    public float Duration { get => duration; set => duration = value; }

    public float IncomingDamageMultiplier => 1f - PlayerData.Instance.GetBarrierReducePercent() / 100f;

    private Coroutine activeRoutine;

    protected override bool Activate()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(BarrierRoutine());
        AudioManager.Instance.PlaySfx(SfxId.Skill2);
        return true;
    }

    private IEnumerator BarrierRoutine()
    {
        auraVisual.SetActive(true);
        player.Stat.IncomingDamageMultiplier = IncomingDamageMultiplier;

        yield return new WaitForSeconds(duration);

        auraVisual.SetActive(false);
        player.Stat.IncomingDamageMultiplier = 1f;
        activeRoutine = null;
    }
}
