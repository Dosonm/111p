using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BossLaser : MonoBehaviour
{
    [SerializeField] private SpriteRenderer warningSprite;

    [SerializeField] private GameObject laserVisual;

    private Tween blinkTween;

    public IEnumerator PlayWarningThenFire(float warningDuration, float blinkInterval, float activeDuration, System.Action onFire)
    {
        warningSprite.gameObject.SetActive(true);
        laserVisual.SetActive(false);

        Color color = warningSprite.color;
        color.a = 1f;
        warningSprite.color = color;
        blinkTween = warningSprite
            .DOFade(0.2f, blinkInterval)
            .SetLoops(-1, LoopType.Yoyo);

        yield return new WaitForSeconds(warningDuration);

        blinkTween?.Kill();
        blinkTween = null;

        warningSprite.gameObject.SetActive(false);
        laserVisual.SetActive(true);

        onFire?.Invoke();

        yield return new WaitForSeconds(activeDuration);

        laserVisual.SetActive(false);
    }

    public void Cleanup()
    {
        blinkTween?.Kill();
        blinkTween = null;
    }
}
