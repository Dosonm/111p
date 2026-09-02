using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState currentState { get; private set; }

    public float ActionCooldown { get; private set; }

    public void initialize(PlayerState _startState)
    {
        currentState = _startState;
        currentState.enter();
    }

    public void changeState(PlayerState _newState)
    {
        currentState.exit();
        currentState = _newState;
        currentState.enter();
    }

    public void SetActionCooldown(float duration)
    {
        ActionCooldown = duration;
    }

    public void update()
    {
        if (ActionCooldown > 0f)
        {
            ActionCooldown -= Time.deltaTime;
        }

        currentState.update();
    }
}
