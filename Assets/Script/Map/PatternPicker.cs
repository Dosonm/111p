using System;
using System.Collections.Generic;

public class PatternPicker
{
    private readonly Random rng;
    private readonly Dictionary<LayerDefinition, Queue<ChunkView>> recentByLayer = new Dictionary<LayerDefinition, Queue<ChunkView>>();

    private readonly List<ChunkView> candidates = new List<ChunkView>();

    public PatternPicker(int? seed = null)
    {
        rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public ChunkView Pick(LayerDefinition layer)
    {
        if (layer.chunkPrefabs.Count == 0)
        {
            return null;
        }

        if (!recentByLayer.TryGetValue(layer, out Queue<ChunkView> recent))
        {
            recent = new Queue<ChunkView>();
            recentByLayer[layer] = recent;
        }

        candidates.Clear();
        foreach (ChunkView prefab in layer.chunkPrefabs)
        {
            if (!recent.Contains(prefab))
            {
                candidates.Add(prefab);
            }
        }

        List<ChunkView> pickFrom = candidates.Count > 0 ? candidates : layer.chunkPrefabs;
        ChunkView chosen = pickFrom[rng.Next(pickFrom.Count)];

        int effectiveWindow = Math.Min(layer.recentExcludeCount, layer.chunkPrefabs.Count - 1);

        recent.Enqueue(chosen);
        while (recent.Count > effectiveWindow)
        {
            recent.Dequeue();
        }

        return chosen;
    }
}
