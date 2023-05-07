using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : PlayerBaseState
{
    public override void EnterState(PlayerStateManager movement)
    {
        if(movement.currentSpeed != 0) movement.currentSpeed = 0; //Enter Player State if its not moving
    }

    /// <summary>
    /// This is how each state get updated once the correct button are pressed
    /// Also limits what state can be performed in different states.
    /// </summary>

    public override void UpdateState(PlayerStateManager movement)
    {
        if (movement.controls.Player.Sprint.IsPressed())
        {
            movement.SwitchState(movement.sprint);
        }

        if (movement.controls.Player.Movment.IsPressed())
        {
            movement.SwitchState(movement.walk);
        }

        if (movement.controls.Player.Crouch.IsPressed())
        {
            movement.SwitchState(movement.crouch);
        }

        if (movement.controls.Player.Jump.WasPressedThisFrame())
        {
            movement.SwitchState(movement.jump);
        }


    }

}
