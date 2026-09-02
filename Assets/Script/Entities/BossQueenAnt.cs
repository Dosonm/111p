using UnityEngine;

public class BossQueenAnt : Boss
{
    [Header("소환 스킬")]
    [SerializeField] private Monster summonPrefab;

    [SerializeField] private int summonRetryLimit = 9;

    protected override SfxId DeathSfx => SfxId.Boss2Death;

    protected override void UseSkill()
    {
        if (Random.value < 0.5f)
        {
            StartCoroutine(LaserSkillRoutine());
        }
        else
        {
            SummonMonster();
        }
    }

    private void SummonMonster()
    {
        if (summonPrefab == null)
        {
            return;
        }

        MonsterRegistry registry = monster.Registry;
        int targetRow = player.PlayerRow;
        int playerCol = player.PlayerX;
        int targetCol = -1;

        for (int attempt = 0; attempt < summonRetryLimit; attempt++)
        {
            int candidateCol = Random.Range(0, GridMath.Columns);
            if (candidateCol != playerCol && !registry.TryGet(targetRow, candidateCol, out _))
            {
                targetCol = candidateCol;
                break;
            }
        }

        if (targetCol < 0)
        {
            return;
        }

        Monster spawned = Instantiate(summonPrefab, chunkView.transform);
        spawned.IsDynamic = true;

        float worldX = GridMath.WorldX(targetCol, player.CellSize);
        float worldY = player.transform.position.y;
        spawned.transform.position = new Vector3(worldX, worldY, spawned.transform.position.z);

        spawned.Init(targetRow, targetCol, registry);
        chunkView.RegisterDynamicMonster(spawned);
    }
}
