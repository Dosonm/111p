using UnityEngine;

public abstract class PlayerSkill : MonoBehaviour
{
    [SerializeField] private float cooldown = 10f;

    protected Player player;

    private float cooldownRemaining;

    public bool IsReady => cooldownRemaining <= 0f;

    public float CooldownRatio => cooldown > 0f ? Mathf.Clamp01(cooldownRemaining / cooldown) : 0f;

    protected virtual void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }
    }

    public bool TryUse()
    {
        if (!GameManager.Instance.IsPlaying || GameManager.Instance.IsPaused)
        {
            return false;
        }

        if (!IsReady)
        {
            return false;
        }

        if (!Activate())
        {
            return false;
        }

        cooldownRemaining = cooldown;
        return true;
    }

    protected abstract bool Activate();
}
