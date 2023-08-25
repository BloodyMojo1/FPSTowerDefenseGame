using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : PlayerBaseState
{
    public override void EnterState(PlayerStateManager movement)
    {
        movement.isWalking = true;
    }

    public override void UpdateState(PlayerStateManager movement)
    {
        if (movement.controls.Player.Jump.WasPressedThisFrame()) ExitState(movement, movement.jump);
        if  (movement.controls.Player.Crouch.IsPressed()) ExitState(movement, movement.crouch);
        else if (movement.controls.Player.Sprint.IsPressed()) ExitState(movement, movement.sprint);
        else if(!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
        else if (movement.controls.Player.Aim.IsPressed()) ExitState(movement, movement.aimingWalk);

        if (movement.slopeAngle > movement.controller.slopeLimit) return;
        if (movement.targetSpeed != movement.baseSpeed) movement.targetSpeed = movement.baseSpeed;

    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        movement.isWalking = false;
        movement.SwitchState(state);
    }
}
