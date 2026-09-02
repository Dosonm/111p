using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    [SerializeField] private WeaponData data;

    [SerializeField] private GameObject equippedBadge;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    private void Start()
    {
        PlayerData.Instance.OnDataChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        PlayerData.Instance.OnDataChanged -= Refresh;
    }

    public void HandleClick()
    {
        WeaponDetailPanel.Instance.Open(data);
    }

    private void Refresh()
    {
        button.interactable = PlayerData.Instance.IsWeaponOwned(data.weaponId);
        equippedBadge.SetActive(PlayerData.Instance.EquippedWeapon == data.weaponId);
    }
}
