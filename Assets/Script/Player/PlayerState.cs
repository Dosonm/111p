using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine PSM;
    protected Player player;

    public PlayerState(PlayerStateMachine _PSM, Player _player)
    {
        this.player = _player;
        this.PSM = _PSM;
    }

    public virtual void enter()
    {
    }

    public virtual void exit()
    {
    }

    public virtual void update()
    {
    }

    protected void DecideNext(Vector2Int direction)
    {
        if (player.IsBlocked(direction))
        {
            if (PSM.currentState != player.IdleState)
            {
                PSM.changeState(player.IdleState);
            }
            return;
        }

        bool hasMonster = player.HasMonsterAt(direction);
        bool isPickup = player.IsPickupAt(direction);

        if (hasMonster && !isPickup)
        {
            player.AttackState.SetDirection(direction);
            PSM.changeState(player.AttackState);
            return;
        }

        player.MoveState.SetDirection(direction);
        PSM.changeState(player.MoveState);
    }
}
