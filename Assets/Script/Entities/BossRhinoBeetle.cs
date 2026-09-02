using System.Collections;
using UnityEngine;

public class BossRhinoBeetle : Boss
{
    [Header("방어 스킬")]
    [SerializeField] private float defenseDuration = 5f;

    [SerializeField] private float defenseDamageMultiplier = 0.3f;

    [SerializeField] private GameObject barrierEffect;

    protected override SfxId DeathSfx => SfxId.Boss1Death;

    protected override void OnEnable()
    {
        base.OnEnable();

        monster.IncomingDamageMultiplier = 1f;
        barrierEffect.SetActive(false);
    }

    protected override void UseSkill()
    {
        if (Random.value < 0.5f)
        {
            StartCoroutine(DefenseSkillRoutine());
        }
        else
        {
            StartCoroutine(LaserSkillRoutine());
        }
    }

    private IEnumerator DefenseSkillRoutine()
    {
        monster.IncomingDamageMultiplier = defenseDamageMultiplier;
        barrierEffect.SetActive(true);

        yield return new WaitForSeconds(defenseDuration);

        monster.IncomingDamageMultiplier = 1f;
        barrierEffect.SetActive(false);
    }

    public override void OnDeath()
    {
        base.OnDeath();

        monster.IncomingDamageMultiplier = 1f;
        barrierEffect.SetActive(false);
    }
}
