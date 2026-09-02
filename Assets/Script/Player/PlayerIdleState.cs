using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine _PSM, Player _player) : base(_PSM, _player)
    {
    }

    public override void enter()
    {
        base.enter();

        player.PlayIdleAnimation();
    }

    public override void update()
    {
        base.update();

        Vector2Int direction = player.ReadDirection();
        if (direction == Vector2Int.zero)
        {
            return;
        }

        if (PSM.ActionCooldown > 0f)
        {
            return;
        }

        DecideNext(direction);
    }
}
