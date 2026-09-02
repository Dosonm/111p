using UnityEngine;

public class PlayerDashState : PlayerState
{
    private int rowsRemaining;
    private float tickInterval;
    private float tickTimer;

    public PlayerDashState(PlayerStateMachine _PSM, Player _player) : base(_PSM, _player)
    {
    }

    public void Begin(int rows, float tickInterval)
    {
        rowsRemaining = rows;
        this.tickInterval = tickInterval;
    }

    public override void enter()
    {
        base.enter();

        tickTimer = 0f;
        DashTick();
    }

    public override void update()
    {
        base.update();

        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f)
        {
            return;
        }

        if (rowsRemaining <= 0)
        {
            PSM.changeState(player.IdleState);
            return;
        }

        DashTick();
    }

    private void DashTick()
    {
        int targetRow = player.PlayerRow + 1;
        int centerCol = player.PlayerX;

        bool centerDied = player.TryAttackCell(targetRow, centerCol, out Monster centerTarget);
        AttackIfNotCenter(targetRow, centerCol - 1, centerTarget);
        AttackIfNotCenter(targetRow, centerCol + 1, centerTarget);

        player.PlayDashAnimation();

        rowsRemaining--;
        tickTimer = tickInterval;

        if (centerTarget != null && !centerTarget.IsPickup && !centerDied)
        {
            return;
        }

        player.Advance(Vector2Int.down);
    }

    private void AttackIfNotCenter(int row, int col, Monster centerTarget)
    {
        Monster occupant = player.PeekCell(row, col);
        if (occupant == null || occupant == centerTarget)
        {
            return;
        }

        player.TryAttackCell(row, col);
    }
}
