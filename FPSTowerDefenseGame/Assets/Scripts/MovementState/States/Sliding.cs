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

        //need to make player automatically go into slide if anagle too step 
        //When going down a slope add more speed as player go down

        //another cooldown like the jump where the force gets worse.
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

        if(CurrentY < lastY)
        {
            //movement.targetSpeed = movement.slideSpeed;
            movement.targetSpeed += Time.deltaTime * movement.slopeMultiplier * movement.slopeAngle;
        }


        //Whole cooldown system is bugged
        if (!movement.cooldownSystem.IsOnCooldown(id))
        {
            if (movement.currentAdditionalSlidingSpeed != movement.maxAdditionalSlidingSpeed) movement.currentAdditionalSlidingSpeed = movement.maxAdditionalSlidingSpeed;
        }
        else
        {
            float lowSlideForce = movement.currentAdditionalSlidingSpeed * movement.slideForceReduction;
            if (lowSlideForce <= movement.minAdditionalSlidingSpeed) movement.currentAdditionalSlidingSpeed = movement.minAdditionalSlidingSpeed;
            else movement.currentAdditionalSlidingSpeed = lowSlideForce;
        }

        if (forceApplied != true && CurrentY <= lastY)
        {
            movement.targetSpeed += movement.currentAdditionalSlidingSpeed;
            //Applies additional speed
            forceApplied = true; //Make forceApplied true so it only get used once.
        }

        
        lastY = CurrentY;
        //Would prefer this a timer but it works for now.
        movement.targetSpeed = Mathf.MoveTowards(movement.targetSpeed, movement.crouchSpeed, movement.slidingDistance * Time.deltaTime); //Slowly makes current speed lose value towards crouch speed
        movement.cooldownSystem.PutOnCooldown(this);
    }


    void ExitState(PlayerStateManager movement, PlayerBaseState state)
    {
        movement.isSliding = false;
        movement.SwitchState(state);
    }
}

