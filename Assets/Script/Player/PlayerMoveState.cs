using UnityEngine;

public class PlayerMoveState : PlayerState
{
    private Vector2Int direction;

    public PlayerMoveState(PlayerStateMachine _PSM, Player _player) : base(_PSM, _player)
    {
    }

    public void SetDirection(Vector2Int _direction)
    {
        direction = _direction;
    }

    public override void enter()
    {
        base.enter();

        player.PlayMoveAnimation(direction);

        player.CollectPickupAt(direction);
        player.Advance(direction);
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
