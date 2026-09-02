using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDetailPanel : MonoBehaviour
{
    public static WeaponDetailPanel Instance { get; private set; }

    [SerializeField] private GameObject panelRoot;

    [SerializeField] private Image iconImage;

    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private TextMeshProUGUI optionText;

    private WeaponId currentWeapon;

    private void Awake()
    {
        Instance = this;
    }

    public void Open(WeaponData data)
    {
        currentWeapon = data.weaponId;

        iconImage.sprite = data.icon;

        int indexInRarity = (int)data.weaponId % 4;
        levelText.text = $"Lv. {indexInRarity + 1}";

        optionText.text = data.description;

        panelRoot.SetActive(true);
    }

    public void HandleEquipClick()
    {
        PlayerData.Instance.TryEquipWeapon(currentWeapon);
        AudioManager.Instance.PlaySfx(SfxId.Skill3);
    }

    public void Close()
    {
        panelRoot.SetActive(false);
    }
}
