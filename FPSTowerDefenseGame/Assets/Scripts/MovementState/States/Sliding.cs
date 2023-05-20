using UnityEngine;

public class Sliding : PlayerBaseState, activeCooldown

{

    private int id;
    private float cooldownDuration;

    public int Id => id;

    public float CooldownDuration => cooldownDuration;

    private bool forceApplied;

    private float CurrentY;
    private float lastY;

    public override void EnterState(PlayerStateManager movement)
    {
        //Only go into sliding
        //if player is sprinting or is on an step angle which auto slides them down with increasing speeds
        movement.isSliding = true;
        forceApplied = false;


        if (id == movement.slideID) return; //Sets Id and duration from MovementManager 
        else
        {
            id = movement.slideID;
            cooldownDuration = movement.slideCooldownDuration;
        }

    }

    public override void UpdateState(PlayerStateManager movement)
    {

        if (movement.controls.Player.Jump.WasPressedThisFrame()) ExitState(movement, movement.jump);
        if (!movement.controls.Player.Movment.IsPressed()) ExitState(movement, movement.idle);
        if (movement.controls.Player.Crouch.WasReleasedThisFrame()) ExitState(movement, movement.walk);
        if (movement.targetSpeed == movement.crouchSpeed) ExitState(movement, movement.crouch); //Makes player enter crouch state when speed is crouchspeed

        CurrentY = movement.transform.position.y;

        if (CurrentY < lastY)
        {
            movement.targetSpeed += movement.slopeAngle * movement.slopeMultiplier * Time.deltaTime; //Increase speed overtime
        }

        if (forceApplied != true && CurrentY <= lastY)
        {
            movement.targetSpeed += movement.currentAdditionalSlidingSpeed; //Applies additional speed
            forceApplied = true; //Make forceApplied true so it only get used once.
        }

        if (movement.isSlopeSliding == false)
        {
            movement.targetSpeed = Mathf.MoveTowards(movement.targetSpeed, movement.crouchSpeed, movement.slidingDistance * Time.deltaTime); //Slowly makes current speed lose value towards crouch speed
            forceApplied = true;
        }

        if (!movement.cooldownSystem.IsOnCooldown(id)) //Check if there no cooldown
        {
            if (movement.currentAdditionalSlidingSpeed != movement.maxAdditionalSlidingSpeed) movement.currentAdditionalSlidingSpeed = movement.maxAdditionalSlidingSpeed; //Resets Additional slide speed to max
        }
        else if (movement.isSliding != true)
        {
            float lowSlideForce = movement.currentAdditionalSlidingSpeed * movement.slideForceReduction; //Reduces additional force
            if (lowSlideForce <= movement.minAdditionalSlidingSpeed) movement.currentAdditionalSlidingSpeed = movement.minAdditionalSlidingSpeed; //Threshold to make it not constantly get worse
            else movement.currentAdditionalSlidingSpeed = lowSlideForce; //Applies reduced force
        }


        lastY = CurrentY;


        movement.cooldownSystem.PutOnCooldown(this); //Puts this script on cooldown
    }


    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        movement.isSliding = false;
        movement.SwitchState(state);
    }
}

