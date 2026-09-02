using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ChunkStreamer : MonoBehaviour
{
    [Serializable]
    private class BossChunkEntry
    {
        public int chunkIndex = -1;

        public ChunkView prefab;
    }

    [SerializeField] private Transform mapRoot;

    [SerializeField] private LayerTable layerTable;

    [SerializeField] private int activeChunkCount = 4;

    [SerializeField] private int keepAboveChunks = 1;

    [SerializeField] private List<BossChunkEntry> bossChunks = new List<BossChunkEntry>();

    private MonsterRegistry registry;
    private float cellSize;
    private PatternPicker patternPicker;
    private readonly Dictionary<ChunkView, ObjectPool<ChunkView>> poolsByPrefab = new Dictionary<ChunkView, ObjectPool<ChunkView>>();
    private readonly Dictionary<int, ChunkView> activeChunks = new Dictionary<int, ChunkView>();
    private int currentMinIndex = -1;
    private int currentMaxIndex = -1;

    public void Init(MonsterRegistry monsterRegistry, float appliedCellSize)
    {
        registry = monsterRegistry;
        cellSize = appliedCellSize;
        patternPicker = new PatternPicker();
    }

    public void Warmup(int playerRow)
    {
        Tick(playerRow);
    }

    public void ResetRun(int playerRow)
    {
        var indexesToRelease = new List<int>(activeChunks.Keys);
        foreach (int index in indexesToRelease)
        {
            ReleaseChunkAt(index);
        }

        currentMinIndex = -1;
        currentMaxIndex = -1;

        Warmup(playerRow);
    }

    public void Tick(int playerRow)
    {
        int desiredMin = Mathf.Max(0, Mathf.FloorToInt(playerRow / (float)GridMath.Rows) - keepAboveChunks);
        int desiredMax = desiredMin + activeChunkCount - 1;

        if (currentMinIndex < 0)
        {
            for (int index = desiredMin; index <= desiredMax; index++)
            {
                SpawnChunkAt(index);
            }
            currentMinIndex = desiredMin;
            currentMaxIndex = desiredMax;
            return;
        }

        for (int index = currentMinIndex; index < desiredMin; index++)
        {
            ReleaseChunkAt(index);
        }

        if (currentMaxIndex < desiredMax)
        {
            SpawnChunkAt(currentMaxIndex + 1);
            currentMaxIndex += 1;
        }

        currentMinIndex = desiredMin;
    }

    private void SpawnChunkAt(int chunkIndex)
    {
        ChunkView bossPrefab = FindBossChunkPrefab(chunkIndex);
        ChunkView prefab;

        if (bossPrefab != null)
        {
            prefab = bossPrefab;
        }
        else
        {
            int globalTopRow = GridMath.GlobalRow(chunkIndex, 0);
            LayerDefinition layer = layerTable.GetLayerForRow(globalTopRow);
            prefab = patternPicker.Pick(layer);
        }

        if (prefab == null)
        {
            return;
        }

        ObjectPool<ChunkView> pool = GetOrCreatePool(prefab);
        ChunkView view = pool.Get();
        view.SourcePrefab = prefab;
        view.Bind(chunkIndex, registry, cellSize);
        activeChunks[chunkIndex] = view;
    }

    private ChunkView FindBossChunkPrefab(int chunkIndex)
    {
        foreach (BossChunkEntry entry in bossChunks)
        {
            if (entry.chunkIndex == chunkIndex && entry.prefab != null)
            {
                return entry.prefab;
            }
        }

        return null;
    }

    private void ReleaseChunkAt(int chunkIndex)
    {
        if (activeChunks.TryGetValue(chunkIndex, out ChunkView view))
        {
            ObjectPool<ChunkView> pool = GetOrCreatePool(view.SourcePrefab);
            pool.Release(view);
            activeChunks.Remove(chunkIndex);
        }
    }

    private ObjectPool<ChunkView> GetOrCreatePool(ChunkView prefab)
    {
        if (!poolsByPrefab.TryGetValue(prefab, out ObjectPool<ChunkView> pool))
        {
            pool = new ObjectPool<ChunkView>(
                createFunc: () => Instantiate(prefab, mapRoot),
                actionOnGet: view => view.gameObject.SetActive(true),
                actionOnRelease: view => view.Unbind(),
                actionOnDestroy: view => Destroy(view.gameObject),
                defaultCapacity: activeChunkCount + 2);
            poolsByPrefab[prefab] = pool;
        }

        return pool;
    }
}
