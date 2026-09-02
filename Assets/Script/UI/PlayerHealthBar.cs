using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    private Slider slider;

    [SerializeField] private Player player;

    private void Start()
    {
        slider = GetComponent<Slider>();

        player.Stat.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(player.Stat.CurrentHealth, player.Stat.MaxHealth);
    }

    private void OnDestroy()
    {
        player.Stat.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        slider.value = max > 0 ? current / (float)max : 0f;
    }
}
