using DG.Tweening;
using UnityEngine;

public class SkillEffectAnimation : MonoBehaviour
{
    [SerializeField] private Vector2 offset;

    [SerializeField] private float outDuration = 0.08f;

    [SerializeField] private float backDuration = 0.12f;

    [SerializeField] private Ease outEase = Ease.OutQuad;
    [SerializeField] private Ease backEase = Ease.InQuad;

    private RectTransform rectTransform;
    private Vector2 homePosition;
    private Sequence activeSequence;
    private bool initialized;

    public void Play()
    {
        if (!initialized)
        {
            rectTransform = GetComponent<RectTransform>();
            homePosition = rectTransform.anchoredPosition;
            initialized = true;
        }

        activeSequence?.Kill();
        rectTransform.anchoredPosition = homePosition;

        gameObject.SetActive(true);

        activeSequence = DOTween.Sequence()
            .Append(rectTransform.DOAnchorPos(homePosition + offset, outDuration).SetEase(outEase))
            .Append(rectTransform.DOAnchorPos(homePosition, backDuration).SetEase(backEase))
            .OnComplete(() => gameObject.SetActive(false))
            .SetUpdate(true);
    }
}
