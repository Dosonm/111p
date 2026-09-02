using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private Vector2Int direction;

    public PlayerAttackState(PlayerStateMachine _PSM, Player _player) : base(_PSM, _player)
    {
    }

    public void SetDirection(Vector2Int _direction)
    {
        direction = _direction;
    }

    public override void enter()
    {
        base.enter();

        player.AttackInDirection(direction);
        PSM.SetActionCooldown(player.ActionInterval);
    }

    public override void update()
    {
        base.update();

        Vector2Int nextDirection = player.ReadDirection();
        if (nextDirection == Vector2Int.zero)
        {
            PSM.changeState(player.IdleState);
            return;
        }

        if (PSM.ActionCooldown > 0f)
        {
            return;
        }

        DecideNext(nextDirection);
    }
}
