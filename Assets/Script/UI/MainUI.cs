using TMPro;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    private PlayerData playerData;

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI pointText;

    [SerializeField] private TextMeshProUGUI statText;

    [SerializeField] private TextMeshProUGUI bestRecordText;

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

    private void Refresh()
    {
        goldText.text = playerData.Gold.ToString();
        pointText.text = playerData.LevelPoint.ToString();
        statText.text = $"체력: {playerData.MaxHealth}\n\n공격력: {playerData.AttackDamage}\n\n레벨: {playerData.Level}";
        bestRecordText.text = $"{playerData.BestMeters}M";
    }

    public void OpenUi(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    public void CloseUi(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
