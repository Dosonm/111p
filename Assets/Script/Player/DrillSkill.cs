using System.Collections.Generic;
using UnityEngine;

public class DrillSkill : PlayerSkill
{
    [SerializeField] private SkillEffectAnimation effect;

    public int Charges => PlayerData.Instance.GetDrillCharges();

    private int chargesRemaining;

    private static readonly Vector2Int[] DownOffsets =
    {
        new(0, 1),
        new(-1, 0), new(1, 0),
        new(-2, -1), new(2, -1),
    };

    private readonly List<Monster> hitThisProc = new();

    protected override bool Activate()
    {
        chargesRemaining = Charges;
        effect.Play();
        AudioManager.Instance.PlaySfx(SfxId.Skill3);
        return true;
    }

    public void OnPlayerAttacked(Monster primaryTarget, Vector2Int facing)
    {
        if (chargesRemaining <= 0)
        {
            return;
        }

        chargesRemaining--;

        int row = player.PlayerRow;
        int col = player.PlayerX;

        hitThisProc.Clear();
        if (primaryTarget != null)
        {
            hitThisProc.Add(primaryTarget);
        }

        foreach (Vector2Int offset in DownOffsets)
        {
            (int dRow, int dCol) = RotateOffset(offset, facing);
            AttackIfNotAlreadyHit(row + dRow, col + dCol);
        }
    }

    private static (int dRow, int dCol) RotateOffset(Vector2Int downOffset, Vector2Int facing)
    {
        if (facing.x == 0)
        {
            return (downOffset.y, downOffset.x);
        }

        int sign = facing.x > 0 ? 1 : -1;
        return (downOffset.x, sign * downOffset.y);
    }

    private void AttackIfNotAlreadyHit(int row, int col)
    {
        Monster occupant = player.PeekCell(row, col);
        if (occupant == null || hitThisProc.Contains(occupant))
        {
            return;
        }

        hitThisProc.Add(occupant);
        player.TryAttackCell(row, col);
    }
}
