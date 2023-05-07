using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprint : PlayerBaseState
{
    public override void EnterState(PlayerStateManager movement)
    {
        movement.isSprinting = true;
    }

    public override void UpdateState(PlayerStateManager movement)
    {
        if (!movement.controls.Player.Sprint.WasReleasedThisFrame()) ExitState(movement, movement.walk);
        else if (!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
        if (movement.controls.Player.Jump.WasPressedThisFrame()) ExitState(movement, movement.jump);
        if(movement.controls.Player.Crouch.IsPressed()) ExitState(movement, movement.crouch);

        if (movement.isGrounded == false) return;

        if(movement.currentSpeed != movement.sprintSpeed)
        {
            movement.currentSpeed = movement.sprintSpeed;
        }
    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        movement.isSprinting = false;
        movement.SwitchState(state);
    }
}
