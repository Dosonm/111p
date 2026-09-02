using TMPro;
using UnityEngine;

public class DepthText : MonoBehaviour
{
    private TextMeshProUGUI text;

    [SerializeField] private ScrollController scrollController;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        scrollController.OnDepthChanged += HandleDepthChanged;
        HandleDepthChanged(scrollController.RowsAdvanced);
    }

    private void OnDestroy()
    {
        scrollController.OnDepthChanged -= HandleDepthChanged;
    }

    private void HandleDepthChanged(int depth)
    {
        text.text = $"{depth}M";
    }
}
