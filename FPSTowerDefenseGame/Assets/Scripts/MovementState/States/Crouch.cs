using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : PlayerBaseState
{
    public override void EnterState(PlayerStateManager movement)
    {
        if(movement.targetHeight != movement.crouchHeight)
        {
            movement.targetHeight = movement.crouchHeight;
        }

        movement.isCrouching = true;
    }

    public override void UpdateState(PlayerStateManager movement)
    {
        if (!movement.controls.Player.Crouch.IsPressed())
        {
            ExitState(movement, movement.walk);
            if(!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
            if (movement.controls.Player.Sprint.IsPressed()) ExitState(movement, movement.sprint);
        }
        if(movement.controls.Player.Jump.WasPressedThisFrame()) ExitState(movement, movement.jump);
    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        if (movement.controls.Player.Crouch.WasReleasedThisFrame())
        {
            if (movement.targetHeight != movement.baseHeight) movement.targetHeight = movement.baseHeight;
        }

        movement.isCrouching = false;

        movement.SwitchState(state);
    }
}
