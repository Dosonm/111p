using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class BlockBreakVfx : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfxPrefab;

    [SerializeField] private float lifetime = 0.5f;

    private ObjectPool<ParticleSystem> pool;

    private void Awake()
    {
        pool = new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(vfxPrefab, transform),
            actionOnGet: instance => instance.gameObject.SetActive(true),
            actionOnRelease: instance => instance.gameObject.SetActive(false),
            actionOnDestroy: instance => Destroy(instance.gameObject));
    }

    public void Play(Vector3 worldPosition)
    {
        ParticleSystem instance = pool.Get();
        instance.transform.position = worldPosition;
        instance.Play();
        StartCoroutine(ReleaseAfter(instance, lifetime));
    }

    private IEnumerator ReleaseAfter(ParticleSystem instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(instance);
    }
}
