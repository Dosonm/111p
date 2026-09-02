using System.Collections.Generic;
using UnityEngine;

public class ChunkView : MonoBehaviour
{
    private readonly List<Monster> monsters = new List<Monster>();
    private int staticMonsterCount = -1;
    private MonsterRegistry registry;
    private int chunkIndex;

    public ChunkView SourcePrefab { get; set; }

    public void Bind(int boundChunkIndex, MonsterRegistry monsterRegistry, float cellSize)
    {
        chunkIndex = boundChunkIndex;
        registry = monsterRegistry;

        transform.localPosition = new Vector3(0f, -chunkIndex * GridMath.Rows * cellSize, 0f);

        if (staticMonsterCount < 0)
        {
            GetComponentsInChildren(true, monsters);
            staticMonsterCount = monsters.Count;
        }

        foreach (var monster in monsters)
        {
            Vector3 local = monster.transform.localPosition;
            int col = GridMath.ColFromLocalX(local.x, cellSize);
            int localRow = GridMath.LocalRowFromLocalY(local.y, cellSize);

            float residualX = Mathf.Abs(local.x - GridMath.WorldX(col, cellSize));
            float residualY = Mathf.Abs(local.y - (-localRow * cellSize));
            if (residualX > cellSize * 0.2f || residualY > cellSize * 0.2f)
            {
                Debug.LogWarning($"{name}: 몬스터 '{monster.name}'의 위치({local})가 격자에 맞지 않습니다 (cellSize={cellSize}). 배치를 확인해주세요.", monster);
            }

            int globalRow = GridMath.GlobalRow(chunkIndex, localRow);
            monster.gameObject.SetActive(true);
            monster.Init(globalRow, col, registry);
        }

        gameObject.SetActive(true);
    }

    public void RegisterDynamicMonster(Monster monster)
    {
        monsters.Add(monster);
    }

    public void Unbind()
    {
        foreach (var monster in monsters)
        {
            if (monster != null && monster.gameObject.activeSelf)
            {
                monster.UnregisterFromChunkReturn();
            }
        }

        if (monsters.Count > staticMonsterCount)
        {
            monsters.RemoveRange(staticMonsterCount, monsters.Count - staticMonsterCount);
        }

        registry = null;
        gameObject.SetActive(false);
    }
}
