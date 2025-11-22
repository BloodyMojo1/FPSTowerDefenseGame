using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    // === Input & Control ===
    public InputMaster controls;
    private Vector2 move;
    private Rigidbody rb;

    // === Movement State ===
    private float lastMoveY;
    private float lastTurnInput = 0f;
    private float currentTurnSpeed = 0f;
    private bool occurredThisFrame = false;
    private bool hasReleasedTurnInput = false;
    private bool targetTurnSpeedSet = false;

    // === Acceleration & Speed ===
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelerationSpeed;
    [SerializeField] private float idleDeceleration; // Deceleration when no input is given

    // === Turning & Steering ===
    [SerializeField] private float turnSpeed;
    [SerializeField] private float turnAcceleration;
    [SerializeField] private float turnDeceleration = 0.1f;
    [SerializeField] private float idleTurningMultiplier;
    [SerializeField] private AnimationCurve steeringCurve;

    [Range(0f, 1f), SerializeField] private float counterSteerWeight;
    [Range(0f, 1f), SerializeField] private float burnoutTurningPenalty = 0.5f;

    private bool burnoutPenalty = false;
    private bool idleBurnoutPenalty = false;
    private float penaltySpeed = 0f;
    private float peakPenaltySpeed = 0f;

    // === Braking ===
    private float brakeHoldTime = 0f;
    [SerializeField] private float baseBrakeForce;
    [SerializeField] private float maxBrakeForce;
    [SerializeField] private float brakeMultiplier;
    [SerializeField] private float increaseBrakeForce;

    // === Drifting ===
    [SerializeField] private float driftingParameter;
    [SerializeField] private float forwardsSlippingThreshold;
    [SerializeField] private float timeSinceLastDriftInput = 1.5f;

    private bool isDrifting = false;
    private float driftInputTimer = 0f;
    private float normalDriftingExtremum;
    private float normalDriftingAsymptote;

    // === Wheel Colliders ===
    private List<WheelCollider> motorWheels = new List<WheelCollider>();
    private List<WheelCollider> steeringWheels = new List<WheelCollider>();

    // === Pivot Points & COM ===
    [SerializeField] private GameObject centerOfMass;
    [SerializeField] private Transform pivot_1;
    [SerializeField] private Transform pivot_2;
    private Transform mainPivotPoint;
    private Transform lastPivotPoint;
    private float previousPivotPoint;

    // === Vehicle Type ===
    public enum TireType { Tire, Tracked }
    public TireType tireType;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new InputMaster(); // Sets up controls with Input Manager

        //Get object the holds wheelCollider should always be last of vehicle parent 
        GameObject lastChild = transform.GetChild(transform.childCount - 1).gameObject;

        //Loop through wheelHolder and gets all wheelColliders 
        foreach (Transform child in lastChild.transform)
        {
            //Finds WheelCollider on child
            WheelCollider wheelCollider = child.GetComponent<WheelCollider>();

            //Sort wheel by tags and add to appropriate list
            if (wheelCollider != null)
            {
                if (child.CompareTag("SteeringWheel"))
                {
                    steeringWheels.Add(wheelCollider);
                }
                else if (child.CompareTag("MotorWheel"))
                {
                    motorWheels.Add(wheelCollider);

                    //Cache original friction values for drift control
                    WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
                    normalDriftingExtremum = sidewaysFriction.extremumValue;
                    normalDriftingAsymptote = sidewaysFriction.asymptoteValue;
                }
            }
        }

        lastPivotPoint = pivot_1;
        //Set the center of mass with a transform for better stability 
        rb.centerOfMass = centerOfMass.transform.localPosition;
    }

    private void Update()
    {
        //Clamps Rigidbody's velocity to ensure max speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        MoveVehicle();
        ApplySteering();

        // Check each mother wheel for sighs of slipping to determine if drifing should start 
        foreach (var wheel in motorWheels)
        {
            WheelHit wheelHit;
            if (wheel.GetGroundHit(out wheelHit))
            {
                if (isDrifting == false)
                {
                    //Checks forward slip to detect drift start
                    float forwardsSlip = Mathf.Abs(wheelHit.forwardSlip);

                    if (forwardsSlip > forwardsSlippingThreshold)
                    {
                        isDrifting = true;
                    }

                }

            }
        }

        if (isDrifting)
        {
            ApplyDrifting();
        }
    }

    private void LateUpdate()
    {
        if(occurredThisFrame == true)
        {
            occurredThisFrame = false;
        }
    }

    private void MoveVehicle()
    {
        //Reads movement input (WASD) as a vector2
        move = controls.CommonInputs.Movment.ReadValue<Vector2>();

        //Checks if the breaking input is being pressed 
        bool isBraking = controls.Vehicle.Breaking.IsPressed();

        //Calculates desired torques based on forward/backward input
        float targetMotorTorque = move.y * accelerationSpeed;

        //Loops through eact wheel and apply motor torque based on input 
        foreach (var wheel in motorWheels)
        {
            //stop applying torque if at speed or braking 
            if (isBraking || rb.velocity.magnitude >= maxSpeed)
            {
                wheel.motorTorque = 0;
            }
            else
            {
                wheel.motorTorque = targetMotorTorque;
            }
        }
        ApplyBraking(isBraking);
    }

    private void RotateAroundPivot(Vector3 pivot, float rotationAmount)
    {
        //Mutiplies the rotation speed to be faster
        rotationAmount *= idleTurningMultiplier;

        // Rotate direction vector around pivot
        Vector3 dir = rb.position - pivot;
        dir = Quaternion.Euler(0, rotationAmount, 0) * dir;

        // Move rigidbody to rotated position
        rb.MovePosition(pivot + dir);

        // Apply rotation to rigidbody
        Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    private void ApplyDecayedTurning(Vector3 pivotPoint)
    {

        // If no input is given, gradually reduce the turn speed over time
        if (currentTurnSpeed > 0)
        {
            currentTurnSpeed -= turnDeceleration * Time.deltaTime;
            currentTurnSpeed = Mathf.Max(currentTurnSpeed, 0f); // Prevent going below 0
        }

        // Calculate new rotation amount based on the turn input and speed
        float rotationAmount = lastTurnInput * currentTurnSpeed * Time.deltaTime;
        Debug.Log(rotationAmount);
        //Rotates the rigidbody around the pivot point
        RotateAroundPivot(pivotPoint, rotationAmount);
    }


    private void ApplyBraking(bool isBraking)
    {
        // If any motor wheel is off the ground, skip braking logic
        foreach (var wheel in motorWheels)
        {
            if (!wheel.isGrounded)
            {
                return;
            }
        }

        // Handling for tire-based vehicles only
        if (tireType == TireType.Tire)
        {
            // Choose pivot based on forward or backward input; fallback to last pivot if no input
            mainPivotPoint = (move.y > 0) ? pivot_1 : (move.y < 0 ? pivot_2 : lastPivotPoint);


            if (move.y != 0) // Update the last pivot used only when move.y changes
            {
                lastPivotPoint = mainPivotPoint;
                lastMoveY = Mathf.Round(move.y);
            }

            //Apply burnout if braking and accelerating 
            if (Mathf.Abs(rb.velocity.magnitude) < 0.1f && isBraking && (move.y != 0))
            {

                ApplyBurnout(mainPivotPoint.position);

                return; //Skip rest of the logic
            }
            else 
            {
                Debug.Log("burnout not called");
                ApplyDecayedTurning(mainPivotPoint.position);
            }
        }

        if (isBraking)
        {
            //Gradually increase brake force overtime
            brakeHoldTime += Time.deltaTime * increaseBrakeForce;

            //Calculate and Clmap brake force, so it doesn't get to strong
            float brakeForce = baseBrakeForce * Mathf.Pow(brakeMultiplier, brakeHoldTime);
            brakeForce = Mathf.Clamp(brakeForce, 0f, maxBrakeForce);

            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, brakeForce * Time.deltaTime);

            //Start drifting when braking with some velocity 
            if (rb.velocity.magnitude > 0.1f || move.y != 0)
            {
                isDrifting = true;
            }
            else if (rb.velocity.magnitude < 0.1f)
            {
                //stop drifting and make vehicle come to complete stop 
                rb.velocity = Vector3.zero;
                isDrifting = false;
            }
        }
        else
        {
            //Reset brake hold timer once braking has stopped
            brakeHoldTime = 0f;

            // No input means natural deceleration, like rolling to a stop
            if (move == Vector2.zero)
            {
                //Gradually decelerate vehicle to 0 with idleDeceleration speed
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, idleDeceleration * Time.deltaTime);

                if (rb.velocity.magnitude < 0.1f)
                {
                    //stop drifting and make vehicle come to complete stop 
                    rb.velocity = Vector3.zero;
                    isDrifting = false;
                }
            }
        }
    }

    //Need to lock befre turning is applied 
    private bool turningLocked = false;
    private float turnSpeedVelocity = 0f;
    private bool wobblePenalty = true;
    private float wobbleTimer = 0f;
    [SerializeField] private AnimationCurve wobbleInterval; // Time between direction swaps
    private int wobbleDirection = 1;
    private float totalWobbleRotation = 0f;
    [SerializeField] private float maxWobbleAngle = 10f; // Max degrees of wobble allowed



    private void ApplyBurnout(Vector3 pivotPoint)
    {
        //Detect if the prevopis direction has been changed
        if(lastMoveY != previousPivotPoint)
        {
            previousPivotPoint = lastMoveY;
            occurredThisFrame = true;
        }

        // Reset input if vehicle is no longer turning
        if (currentTurnSpeed <= 0)
        {
            lastTurnInput = 0;
        }

        //Gradually increases turn speed while tracking peak turn speed
        if (currentTurnSpeed <= turnSpeed)
        {
            if(currentTurnSpeed < turnSpeed)
            {
                currentTurnSpeed += turnAcceleration * Time.deltaTime;
                currentTurnSpeed = Mathf.Min(currentTurnSpeed, turnSpeed);
                peakPenaltySpeed = Mathf.Max(peakPenaltySpeed, currentTurnSpeed); //Maxed reached speed for penality for later
            }
            if (move.x == 0)
            {
                // Count down
                wobbleTimer -= Time.deltaTime;

                // When time’s up, flip direction and reset timer
                if (wobbleTimer <= 0f)
                {
                    wobbleDirection *= -1;
                    wobbleTimer = Random.Range(wobbleInterval.keys[0].value,
                                               wobbleInterval.Evaluate(currentTurnSpeed));
                }

                // Normalize timer to [0,1]
                float t = 1f - (wobbleTimer / wobbleInterval.Evaluate(currentTurnSpeed));
                t = Mathf.Clamp01(t);

                // Sine curve modulation: 0→1→0
                float intensity = Mathf.Sin(Mathf.PI * t);

                // Compute how much to wobble this frame
                float baseWobble = wobbleDirection * currentTurnSpeed * Time.deltaTime;
                float appliedWobble = baseWobble * intensity;

                // Enforce your cap
                float newTotal = totalWobbleRotation + appliedWobble;
                if (Mathf.Abs(newTotal) > maxWobbleAngle)
                {
                    appliedWobble = maxWobbleAngle * Mathf.Sign(appliedWobble)
                                   - totalWobbleRotation;
                    // Bounce back if you still want that flip at the edge
                    wobbleDirection *= -1;
                }

                RotateAroundPivot(mainPivotPoint.position, appliedWobble);
                totalWobbleRotation += appliedWobble;

                if (wobblePenalty)
                {
                    Debug.Log("yes sir");
                    wobblePenalty = false;
                }
            }

            else
            {
                if (wobblePenalty == false)
                {
                    Debug.Log("Test2");

                    wobblePenalty = true;
                }
            }
        }

        //Applies burnout penality for swapping turning direction 
        if (burnoutPenalty == true)
        {
            if (targetTurnSpeedSet == false && currentTurnSpeed < peakPenaltySpeed)
            {
                // Apply penalty based on current speed
                penaltySpeed = currentTurnSpeed * burnoutTurningPenalty;
                targetTurnSpeedSet = true;
                turningLocked = true;
            }
            else
            {
                //Gradually decrease turn speed toward the new penatlised turn speed 
                //currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, penaltySpeed, turnDeceleration * Time.deltaTime);
                currentTurnSpeed = Mathf.SmoothDamp(currentTurnSpeed, penaltySpeed, ref turnSpeedVelocity, turnDeceleration);


                //Once penalty is applied, reset all relate values
                if (currentTurnSpeed <= penaltySpeed)
                {
                    lastTurnInput = 0;
                    peakPenaltySpeed = 0;
                    penaltySpeed = 0;
                    targetTurnSpeedSet = false;
                    burnoutPenalty = false;
                    turningLocked = false;
                }
            }
        }

        // Hand active turning input
        if (move.x != 0f)
        {
            // Detect direction switch and activate penalty
            if (Mathf.Sign(lastTurnInput) != Mathf.Sign(move.x) && lastTurnInput != 0)
            {
                burnoutPenalty = true;
            }
            else
            { 
                lastTurnInput = move.x; //Store direction
            }

            // Reset and Acivate flag for penalty trigger
            if (hasReleasedTurnInput != false)
            {
                hasReleasedTurnInput = false;
                occurredThisFrame = true;
            }

            if(turningLocked == false)
            {
                // Rotate around the pivot point
                float rotationAmount = move.x * currentTurnSpeed * Time.deltaTime;
                RotateAroundPivot(pivotPoint, rotationAmount);
            }
            else
            {
                ApplyDecayedTurning(pivotPoint);
            }

        }
        // Handle continued turning momentum after key is released
        else if (move.x == 0f && !hasReleasedTurnInput)
        {
            if (currentTurnSpeed > 0)
            {
                ApplyDecayedTurning(pivotPoint);
                currentTurnSpeed = Mathf.SmoothDamp(currentTurnSpeed, 0f, ref turnSpeedVelocity, turnDeceleration);

                // Once decayed fully, register input release
                if (currentTurnSpeed == 0)
                {
                    hasReleasedTurnInput = true; 
                }
            }
        }

        // Handle burnout penalty if movement occurred this frame
        if (occurredThisFrame == true)
        {
             idleBurnoutPenalty = true;
        }
        //Decrease penalty speed to 0 when penalty is active
        else if (idleBurnoutPenalty == true)
        {
            Debug.Log(penaltySpeed);
        }
    }

    private void ApplySteering()
    {
        float targetSteeringAngle = 0f;

        //Check if sterring input (A or D) is being pressed
        if (Mathf.Abs(move.x) > 0)
        {
            //Determine direction from input: -1 (left) or 1 (right)
            float direction = Mathf.Sign(move.x);

            // Determine if the vehicle is moving forward or backward
            float velocityDirection = Mathf.Sign(Vector3.Dot(transform.forward, rb.velocity));

            // Invert steering if reversing
            if (velocityDirection < 0)
            {
                direction = -direction;
            }

            // Get steering angle from velocity-based curve
            targetSteeringAngle = direction * steeringCurve.Evaluate(rb.velocity.magnitude);

            //Apply Counter-steering during drift
            if (isDrifting == true)
            {
                float counterSteerAngle = 0f;

                // Measure counter steer angle with movement and facing direction
                if (velocityDirection >= 0) // Moving forward
                {
                    counterSteerAngle = Vector3.SignedAngle(transform.forward, rb.velocity.normalized, Vector3.up);
                }
                else // Moving backward
                {
                    counterSteerAngle = Vector3.SignedAngle(-transform.forward, rb.velocity.normalized, Vector3.up);
                }
                // Add counter-steer influence to help stabilize drifting
                targetSteeringAngle += counterSteerAngle * counterSteerWeight;
            }

            // Clamp the final steering angle
            targetSteeringAngle = Mathf.Clamp(targetSteeringAngle, -90f, 90f);
        }
        //Gradually apply the caluclated steering angle to each steering wheel
        foreach (var wheel in steeringWheels)
        {
            float currentSteeringAngle = wheel.steerAngle;
            wheel.steerAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle, Time.deltaTime * turnSpeed);
        }
    }

    private void ApplyDrifting()
    {

        // Apply lower friction values to allow drifting on all motor wheels
        foreach (var wheel in motorWheels)
        {
            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.extremumValue = driftingParameter;
            sidewaysFriction.asymptoteValue = driftingParameter;
            wheel.sidewaysFriction = sidewaysFriction;
        }

        if (move.x != 0f)
        {

            // If turning input is held, reset the drift timer and mark as drifting
            driftInputTimer = timeSinceLastDriftInput;
            isDrifting = true;
        }
        else
        {
            // If no turn input, reduce the timer
            driftInputTimer -= Time.deltaTime;


            if (driftInputTimer <= 0f)
            {
                // If theres no input for a while, stop drifting and restore original friction
                isDrifting = false;
                foreach (var wheel in motorWheels)
                {
                    WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
                    sidewaysFriction.extremumValue = normalDriftingExtremum;
                    sidewaysFriction.asymptoteValue = normalDriftingAsymptote;
                    wheel.sidewaysFriction = sidewaysFriction;
                }
            }
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
