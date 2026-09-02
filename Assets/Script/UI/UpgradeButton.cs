using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    private PlayerData playerData;

    [SerializeField] private UpgradeType upgradeType;

    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private TextMeshProUGUI costText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        playerData = PlayerData.Instance;

        playerData.OnDataChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        playerData.OnDataChanged -= Refresh;
    }

    public void HandleClick()
    {
        if (playerData.TryUpgrade(upgradeType))
        {
            AudioManager.Instance.PlaySfx(SfxId.Upgrade);
        }
    }

    private void Refresh()
    {
        levelText.text = playerData.IsUpgradeMaxLevel(upgradeType)
            ? "Lv. max"
            : $"Lv. {playerData.GetUpgradeLevel(upgradeType)}";

        if (costText != null)
        {
            costText.text = $"{playerData.GetUpgradeGoldCost(upgradeType)}G";
        }

        button.interactable = !playerData.IsUpgradeMaxLevel(upgradeType);
    }
}
