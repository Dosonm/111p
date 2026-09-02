using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LayerDefinition
{
    public string layerName = "Layer";

    public int minRow;

    public List<ChunkView> chunkPrefabs = new List<ChunkView>();

    [Min(0)]
    public int recentExcludeCount = 2;
}

[CreateAssetMenu(fileName = "LayerTable", menuName = "Map/Layer Table")]
public class LayerTable : ScriptableObject
{
    public List<LayerDefinition> layers = new List<LayerDefinition>();

    public LayerDefinition GetLayerForRow(int globalRow)
    {
        LayerDefinition result = null;
        foreach (var layer in layers)
        {
            if (layer.minRow <= globalRow)
            {
                result = layer;
            }
            else
            {
                break;
            }
        }

        return result ?? (layers.Count > 0 ? layers[0] : null);
    }

    private void OnValidate()
    {
        for (int i = 1; i < layers.Count; i++)
        {
            if (layers[i].minRow < layers[i - 1].minRow)
            {
                Debug.LogError($"{name}: 지층은 minRow 기준 오름차순으로 정렬되어야 합니다 ({i}번째 지층에서 순서가 어긋났습니다).", this);
            }
        }
    }
}
