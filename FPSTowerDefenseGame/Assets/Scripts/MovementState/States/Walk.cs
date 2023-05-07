using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : PlayerBaseState
{
    public override void EnterState(PlayerStateManager movement)
    {
        if (movement.currentSpeed == 0) movement.currentSpeed = movement.baseSpeed;
    }

    public override void UpdateState(PlayerStateManager movement)
    {
        if(!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        movement.SwitchState(state);
    }
}
