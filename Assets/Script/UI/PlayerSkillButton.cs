using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillButton : MonoBehaviour
{
    [SerializeField] private PlayerSkill skill;

    [SerializeField] private Image cooldownGauge;

    private Button button;
    private float lastRatio = -1f;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void HandleClick()
    {
        skill.TryUse();
    }

    private void Update()
    {
        float ratio = 1f - skill.CooldownRatio;
        if (ratio == lastRatio)
        {
            return;
        }

        lastRatio = ratio;
        cooldownGauge.fillAmount = ratio;
        button.interactable = skill.IsReady;
    }
}
