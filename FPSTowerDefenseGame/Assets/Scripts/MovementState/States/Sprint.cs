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

        if (movement.controls.Player.Aim.IsPressed()) ExitState(movement, movement.aimingWalk);
        else if (!movement.controls.Player.Sprint.IsPressed()) ExitState(movement, movement.walk);
        else if (!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
        if (movement.controls.Player.Jump.WasPressedThisFrame()) ExitState(movement, movement.jump);
        if (movement.controls.Player.Crouch.IsPressed() &&  movement.controls.Player.Sprint.IsPressed() && movement.isGrounded) ExitState(movement, movement.sliding);

        if (movement.slopeAngle > movement.controller.slopeLimit) return;
        if (movement.targetSpeed != movement.sprintSpeed)
        {
            movement.targetSpeed = movement.sprintSpeed;
        }

    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {

        movement.isSprinting = false;
        movement.SwitchState(state);
    }
}
