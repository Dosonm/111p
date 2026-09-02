using DG.Tweening;
using UnityEngine;

public class TitleIntro : MonoBehaviour
{
    [SerializeField] private RectTransform titlePanel;

    [SerializeField] private float duration = 0.6f;

    [SerializeField] private float overshootMultiplier = 1.2f;

    [SerializeField] private Ease ease = Ease.InOutQuad;

    private Vector2 homePosition;
    private float canvasHeight;

    private void Awake()
    {
        homePosition = titlePanel.anchoredPosition;

        canvasHeight = ((RectTransform)titlePanel.root).rect.height;
    }

    public void OnPlayButtonClicked()
    {
        GameManager.Instance.StartPlay();

        float targetY = homePosition.y + canvasHeight * overshootMultiplier;
        titlePanel.DOAnchorPosY(targetY, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .OnComplete(() => titlePanel.gameObject.SetActive(false));
    }

    public void ShowTitle()
    {
        titlePanel.anchoredPosition = homePosition;
        titlePanel.gameObject.SetActive(true);
    }
}
