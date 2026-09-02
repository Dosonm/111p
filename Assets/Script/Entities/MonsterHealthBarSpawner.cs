using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthBarSpawner : MonoBehaviour
{
    public static MonsterHealthBarSpawner Instance { get; private set; }

    [SerializeField] private Slider monsterBarPrefab;

    [SerializeField] private Slider bossBarPrefab;

    [SerializeField] private float monsterBarBottomPadding = 0.1f;

    private class ActiveBar
    {
        public Slider Slider;
        public Monster Target;
        public bool IsBossBar;
        public System.Action<int, int> HealthChangedHandler;
    }

    private ObjectPool<Slider> monsterPool;
    private ObjectPool<Slider> bossPool;

    private readonly Dictionary<Monster, ActiveBar> activeByMonster = new Dictionary<Monster, ActiveBar>();
    private readonly List<ActiveBar> activeBars = new List<ActiveBar>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        monsterPool = new ObjectPool<Slider>(monsterBarPrefab, transform);
        bossPool = new ObjectPool<Slider>(bossBarPrefab, transform);
    }

    private class ObjectPool<T> where T : Component
    {
        private readonly UnityEngine.Pool.ObjectPool<T> pool;

        public ObjectPool(T prefab, Transform parent)
        {
            pool = new UnityEngine.Pool.ObjectPool<T>(
                createFunc: () => Instantiate(prefab, parent),
                actionOnGet: instance => instance.gameObject.SetActive(true),
                actionOnRelease: instance => instance.gameObject.SetActive(false),
                actionOnDestroy: instance => Destroy(instance.gameObject));
        }

        public T Get() => pool.Get();
        public void Release(T instance) => pool.Release(instance);
    }

    public void ShowMonsterBar(Monster monster)
    {
        Show(monster, monsterPool, isBossBar: false);
    }

    public void ShowBossBar(Monster monster)
    {
        Show(monster, bossPool, isBossBar: true);
    }

    private void Show(Monster monster, ObjectPool<Slider> pool, bool isBossBar)
    {
        if (activeByMonster.ContainsKey(monster))
        {
            return;
        }

        Slider slider = pool.Get();
        ActiveBar bar = new ActiveBar { Slider = slider, Target = monster, IsBossBar = isBossBar };
        bar.HealthChangedHandler = (current, max) => UpdateValue(bar);

        activeByMonster.Add(monster, bar);
        activeBars.Add(bar);

        monster.OnHealthChanged += bar.HealthChangedHandler;
        UpdateValue(bar);
        UpdatePosition(bar);
    }

    public void Release(Monster monster)
    {
        if (!activeByMonster.TryGetValue(monster, out ActiveBar bar))
        {
            return;
        }

        monster.OnHealthChanged -= bar.HealthChangedHandler;

        activeByMonster.Remove(monster);
        activeBars.Remove(bar);

        ObjectPool<Slider> pool = bar.IsBossBar ? bossPool : monsterPool;
        pool.Release(bar.Slider);
    }

    private void UpdateValue(ActiveBar bar)
    {
        int max = bar.Target.MaxHealth;
        bar.Slider.value = max > 0 ? bar.Target.CurrentHealth / (float)max : 0f;
    }

    private void LateUpdate()
    {
        foreach (ActiveBar bar in activeBars)
        {
            UpdatePosition(bar);
        }
    }

    private void UpdatePosition(ActiveBar bar)
    {
        Bounds bounds = bar.Target.BarAnchorSprite.bounds;
        Vector3 worldPosition = bar.IsBossBar
            ? bounds.center
            : new Vector3(bounds.center.x, bounds.min.y - monsterBarBottomPadding, bounds.center.z);

        bar.Slider.transform.position = worldPosition;
    }
}
