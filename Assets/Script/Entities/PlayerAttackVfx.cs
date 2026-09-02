using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttackVfx : MonoBehaviour
{
    [SerializeField] private ParticleSystem BlockVfxPrefab;
    [SerializeField] private ParticleSystem MonsterVfxPrefab;

    [SerializeField] private float offsetDistance = 0.2f;

    [SerializeField] private float lifetime = 0.5f;

    private ObjectPool<ParticleSystem> blockPool;
    private ObjectPool<ParticleSystem> monsterPool;

    private void Awake()
    {
        blockPool = CreatePool(BlockVfxPrefab);
        monsterPool = CreatePool(MonsterVfxPrefab);
    }

    private ObjectPool<ParticleSystem> CreatePool(ParticleSystem prefab)
    {
        return new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: instance => instance.gameObject.SetActive(true),
            actionOnRelease: instance => instance.gameObject.SetActive(false),
            actionOnDestroy: instance => Destroy(instance.gameObject));
    }

    public void Play(Vector2Int direction, Monster.BoxType boxType)
    {
        bool isBlock = boxType == Monster.BoxType.Block;
        ObjectPool<ParticleSystem> pool = isBlock ? blockPool : monsterPool;

        ParticleSystem instance = pool.Get();
        instance.transform.localPosition = new Vector3(direction.x * offsetDistance, direction.y * offsetDistance, 0f);
        instance.Play();
        StartCoroutine(ReleaseAfter(pool, instance, lifetime));
    }

    private IEnumerator ReleaseAfter(ObjectPool<ParticleSystem> pool, ParticleSystem instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(instance);
    }
}
