using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : PlayerBaseState
{
    public override void EnterState(PlayerStateManager movement)
    {
        //More Advance Jumping 
        //As players jump it needs to get weaker

        if(movement.currentJumpForce != movement.highJumpForce) movement.currentJumpForce = movement.highJumpForce; //Sets jump force
    }

    public override void UpdateState(PlayerStateManager movement)
    {
        if (!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
        else ExitState(movement, movement.walk);
        if (movement.controls.Player.Sprint.IsPressed()) ExitState(movement, movement.sprint);
        if (movement.controls.Player.Crouch.IsPressed()) ExitState(movement, movement.crouch);

        if (movement.isGrounded == true)
        {
            movement.velocity.y = Mathf.Sqrt(movement.currentJumpForce * -2 * movement.gravity); //Moves the character contoller up by jump force to create a jump
        }
    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        movement.SwitchState(state);
    }
}
