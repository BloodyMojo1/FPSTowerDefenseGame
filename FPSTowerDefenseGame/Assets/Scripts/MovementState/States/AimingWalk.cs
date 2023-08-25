using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingWalk : PlayerBaseState
{
    public override void EnterState(PlayerStateManager movement)
    {
    }

    public override void UpdateState(PlayerStateManager movement)
    {
        if (movement.controls.Player.Jump.WasPressedThisFrame()) ExitState(movement, movement.jump);
        if (movement.controls.Player.Crouch.IsPressed()) ExitState(movement, movement.crouch);
        else if (movement.controls.Player.Movment.IsPressed() && !movement.controls.Player.Aim.IsPressed()) ExitState(movement, movement.walk);
        else if (movement.controls.Player.Sprint.IsPressed() && !movement.controls.Player.Aim.IsPressed()) ExitState(movement, movement.sprint);
        else if (!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);

        if (movement.slopeAngle > movement.controller.slopeLimit) return;
        if (movement.targetSpeed != movement.aimingWalkSpeed)
        {
            movement.targetSpeed = movement.aimingWalkSpeed;
        }
    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {

        movement.SwitchState(state);
    }

}
