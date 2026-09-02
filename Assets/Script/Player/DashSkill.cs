using UnityEngine;

public class DashSkill : PlayerSkill
{
    [SerializeField] private float tickInterval = 0.1f;

    [SerializeField] private SkillEffectAnimation effect;

    public int Rows => PlayerData.Instance.GetDashRows();

    public float TickInterval { get => tickInterval; set => tickInterval = value; }

    protected override bool Activate()
    {
        if (!player.CanStartSkill)
        {
            return false;
        }

        player.BeginDash(Rows, tickInterval);
        effect.Play();
        AudioManager.Instance.PlaySfx(SfxId.Skill1);
        return true;
    }
}
