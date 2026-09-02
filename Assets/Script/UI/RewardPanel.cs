using System.Text;
using TMPro;
using UnityEngine;

public class RewardPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;

    [SerializeField] private TextMeshProUGUI goldText;

    [SerializeField] private TextMeshProUGUI expText;

    [SerializeField] private GameObject weaponRow;
    [SerializeField] private TextMeshProUGUI weaponText;

    public void Open(GameManager.RunResult result)
    {
        goldText.text = result.GoldEarned.ToString();
        expText.text = result.ExpEarned.ToString();

        if (result.WeaponsEarned.Count == 0)
        {
            weaponRow.SetActive(false);
        }
        else
        {
            weaponRow.SetActive(true);

            var builder = new StringBuilder();
            for (int i = 0; i < result.WeaponsEarned.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(result.WeaponsEarned[i]);
            }

            weaponText.text = builder.ToString();
        }

        panelRoot.SetActive(true);
    }

    public void Close()
    {
        panelRoot.SetActive(false);
        GameManager.Instance.ShowLobby();
    }
}
