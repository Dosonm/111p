using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamageTextSpawner : MonoBehaviour
{
    public static DamageTextSpawner Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI dealtPrefab;

    [SerializeField] private TextMeshProUGUI takenPrefab;

    [SerializeField] private float jumpHeight = .8f;

    [SerializeField] private float fallDistance = 3f;

    [SerializeField] private float maxLaunchAngle = 45f;

    [SerializeField] private float jumpHeightVariance = 0.3f;

    [SerializeField] private float riseDuration = 0.25f;

    [SerializeField] private float fallDuration = 0.55f;

    private ObjectPool<TextMeshProUGUI> dealtPool;
    private ObjectPool<TextMeshProUGUI> takenPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        dealtPool = CreatePool(dealtPrefab);
        takenPool = CreatePool(takenPrefab);
    }

    private ObjectPool<TextMeshProUGUI> CreatePool(TextMeshProUGUI prefab)
    {
        Vector3 prefabScale = prefab.transform.localScale;

        return new ObjectPool<TextMeshProUGUI>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: instance =>
            {
                DOTween.Kill(instance);
                instance.transform.localScale = prefabScale;
                instance.alpha = 1f;
                instance.gameObject.SetActive(true);
            },
            actionOnRelease: instance => instance.gameObject.SetActive(false),
            actionOnDestroy: instance => Destroy(instance.gameObject));
    }

    public void PlayDealt(int amount, Vector3 worldPosition)
    {
        Play(dealtPool, dealtPrefab, amount, worldPosition);
    }

    public void PlayTaken(int amount, Vector3 worldPosition)
    {
        Play(takenPool, takenPrefab, amount, worldPosition);
    }

    private void Play(ObjectPool<TextMeshProUGUI> pool, TextMeshProUGUI prefab, int amount, Vector3 worldPosition)
    {
        if (prefab == null || amount <= 0)
        {
            return;
        }

        TextMeshProUGUI instance = pool.Get();
        instance.text = amount.ToString();
        instance.rectTransform.position = worldPosition;

        float angleDeg = Random.Range(-maxLaunchAngle, maxLaunchAngle);
        float launchHeight = jumpHeight + Random.Range(-jumpHeightVariance, jumpHeightVariance);
        float horizontal = launchHeight * Mathf.Tan(angleDeg * Mathf.Deg2Rad);
        Vector3 peak = worldPosition + new Vector3(horizontal, launchHeight, 0f);
        Vector3 landing = peak + new Vector3(horizontal * 0.6f, -(launchHeight + fallDistance), 0f);

        Sequence sequence = DOTween.Sequence();
        sequence.SetId(instance);
        sequence.Append(instance.transform.DOMove(peak, riseDuration).SetEase(Ease.OutQuad));
        sequence.Append(instance.transform.DOMove(landing, fallDuration).SetEase(Ease.InQuad));

        sequence.Insert(riseDuration + fallDuration * 0.5f, instance.DOFade(0f, fallDuration * 0.5f).SetEase(Ease.InQuad));
        sequence.OnComplete(() => pool.Release(instance));
    }
}
