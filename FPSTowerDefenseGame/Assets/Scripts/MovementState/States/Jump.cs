using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : PlayerBaseState, activeCooldown
{

    private int id;
    private float cooldownDuration;

    public int Id => id;

    public float CooldownDuration => cooldownDuration;

    public override void EnterState(PlayerStateManager movement)
    {
        //More Advance Jumping 
        //As players jump it needs to get weaker
        if (id == movement.jumpId) return; //Sets Id and duration from MovementManager 
        else
        {
            id = movement.jumpId;
            cooldownDuration = movement.JumpcooldownDuration;
        }
    }

    public override void UpdateState(PlayerStateManager movement)
    {
        if (!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
        else ExitState(movement, movement.walk);
        if (movement.controls.Player.Sprint.IsPressed()) ExitState(movement, movement.sprint);
        if (movement.controls.Player.Crouch.IsPressed()) ExitState(movement, movement.crouch);


        if (!movement.cooldownSystem.IsOnCooldown(id)) //Checks if there is cooldown
        {
            if (movement.currentJumpForce != movement.maxJumpForce) movement.currentJumpForce = movement.maxJumpForce; //Set max jump height if theres no cooldown
        }
        else
        {
            var lowJump = movement.currentJumpForce * movement.jumpForceReduction; //Reduces maxJumpforce my a percentage 1 = 100% 0.5 = 50% jump reduction
            if (lowJump <= movement.minJumpForce) movement.currentJumpForce = movement.minJumpForce; //Makes jump force have a min, that way player still have a little jump
            else movement.currentJumpForce = lowJump;
        }

        if (movement.isGrounded == true)
        {
            movement.velocity.y = Mathf.Sqrt(movement.currentJumpForce * -2 * movement.gravity); //Moves the character contoller up by jump force to create a jump
        }

        movement.cooldownSystem.PutOnCooldown(this); //Sets new cooldown
    }

    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        //if (movement.currentJumpForce != movement.highJumpForce) movement.currentJumpForce = movement.highJumpForce; //Sets jump force
        movement.SwitchState(state);
    }
}
