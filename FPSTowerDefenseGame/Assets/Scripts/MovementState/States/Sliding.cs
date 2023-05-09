using UnityEngine;

public class Sliding : PlayerBaseState

{
    private bool forceApplied;

    public override void EnterState(PlayerStateManager movement)
    {
        //Only go into sliding
        //if player is sprinting or is on an step angle which auto slides them down with increasing speeds
        movement.isSliding = true;
        forceApplied = false;

        //need to make player automatically go into slide if anagle too step 
        //When going down a slope add more speed as player go down

        //another cooldown like the jump where the force gets worse.
    }



    public override void UpdateState(PlayerStateManager movement)
    {

        if (movement.controls.Player.Jump.WasPressedThisFrame()) ExitState(movement, movement.jump);
        if (!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
        if (movement.controls.Player.Crouch.WasReleasedThisFrame()) ExitState(movement, movement.walk);
        if (movement.currentSpeed == movement.crouchSpeed) ExitState(movement, movement.crouch); //Makes player enter crouch state when speed is crouchspeed




        if (forceApplied != true)
        {
            movement.currentSpeed += movement.aditionalSlidingSpeed; //Applies additional speed
            forceApplied = true; //Make forceApplied true so it only get used once.
        }

        //Would prefer this a timer but it works for now.
        movement.currentSpeed = Mathf.MoveTowards(movement.currentSpeed, movement.crouchSpeed, movement.slidingDistance * Time.deltaTime); //Slowly makes current speed lose value towards crouch speed
    }


    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        movement.isSliding = false;
        movement.SwitchState(state);
    }
}

