using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    [Header("References/General")]

    PlayerBaseState currentState;

    public Idle idle = new Idle();
    public Walk walk = new Walk();
    public Sprint sprint = new Sprint();
    public Jump jump = new Jump();
    public Crouch crouch = new Crouch();
    public Sliding sliding = new Sliding();


    public InputMaster controls;
    public CooldownSystem cooldownSystem = null;


    public CharacterController controller;
    [SerializeField] private Transform fpsCam;

    private Vector2 move;
    private Vector3 movement;
    private Vector3 oldMovement;
    private Vector3 slideMovement;


    public Vector3 velocity;

    [HideInInspector]
    public float currentHeight;
    [HideInInspector]
    public float targetHeight;

    public float targetSpeed;

    public float currentSpeed;
    public float test;

    [Header("Base Parameters")]

    public float baseSpeed = 12f;
    public float baseHeight = 1f;

    [SerializeField, Range(0, 1)] private float airSpeed = 0.5f;
    [SerializeField] private float heightSpeedChange = 1f;

    [Header("Crouch Variables Parameters")]

    public float crouchSpeed = 6f;
    public float crouchHeight = 0.5f;

    [HideInInspector]
    public bool isCrouching;

    [Header("Sprint Parameters")]

    public float sprintSpeed = 24f;

    //[HideInInspector]
    public bool isSprinting;

    [Header("Jump Parameters")]

    public int jumpID;
    public float JumpcooldownDuration = 5;

    public float maxJumpForce = 3f;
    public float minJumpForce = 0.5f;

    [Range(0, 1)]
    public float jumpForceReduction = 0.1f;

    [HideInInspector]
    public float currentJumpForce;

    [Header("Sliding Parameters")]
   
    public Transform slopeCheck;

    public int slideID = 2;
    public float slideCooldownDuration = 5;

    public float slidingDistance = 5f;
    public float maxAdditionalSlidingSpeed = 6f;
    public float minAdditionalSlidingSpeed = 1f;

    [Range(0, 1)]
    public float slideForceReduction = 0.1f;

    [SerializeField] private float slopeSlideSpeed = 1f;
    public float slopeMultiplier = 0.5f;

    private RaycastHit slopeHit;

    [HideInInspector]
    public float currentAdditionalSlidingSpeed;

    [HideInInspector]
    public float slopeAngle;

    [HideInInspector]
    public bool isSliding;
    public bool isSlopeSliding = false;


    [Header("Gravity")]

    public Transform groundCheck;

    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;

    [SerializeField] private float groundRadius = 0.5f;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [HideInInspector]
    public bool isGrounded;

    [Header("Ceiling Checker")]

    public Transform ceilingCheck;

    [SerializeField] private float ceilingRadius = 0.5f;
    [SerializeField] private float ceilingDistance = 0.4f;

    [HideInInspector]
    public bool isCeilingAbove;

    private void Awake()
    {
        controls = new InputMaster();
        currentSpeed = 0;
        currentHeight = baseHeight;
        targetHeight = currentHeight;
    }

    // Start is called before the first frame update
    void Start()
    {
        SwitchState(idle);
    }

    // Update is called once per frame
    void Update()
    {

        currentState.UpdateState(this);


        CrouchSpeed();
        IsCeilingAbove();
        IsGrounded();
        slopeSlider();
        Movement();

        //Moves character controller and FpsCam up and down for crouching
        if (currentHeight != targetHeight)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, Time.deltaTime * heightSpeedChange); //Move slowly toward target height
            float center = currentHeight / 2; //finds center of object
            controller.height = currentHeight; //sets controler height
            controller.center = new Vector3(0, center, 0); //creates new center point, so players feet stay on ground.


            // moves the camera with the character controller 
            var camPos = fpsCam.transform.position;
            camPos.y = transform.TransformPoint(new Vector3(0f, controller.height - 0.2f, 0f)).y;
            fpsCam.transform.position = camPos;
        }
        currentSpeed = targetSpeed;

        Debug.DrawRay(slopeCheck.position, Vector3.down, Color.red);
    }

    /// <summary>
    /// Switches and enters new states
    /// </summary>
    public void SwitchState(PlayerBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    private void Movement()
    {
        move = controls.Player.Movment.ReadValue<Vector2>(); //Read input values

        movement = ((move.y * transform.forward) + (move.x * transform.right)) * currentSpeed; //seperate the input values into different directions

        if (isGrounded == true) oldMovement = movement; //Applies movement like normally 
        else movement = oldMovement + ((oldMovement - movement) * -airSpeed); //Applies an initial force from the last moved direction & gives little air movement in other directions.

        if (isSlopeSliding == true)
        {
            movement = Vector3.ClampMagnitude(movement, slopeSlideSpeed); //Clamps Speed when on a slope to reduce player movement
        }
        else
        {
            movement = Vector3.ProjectOnPlane(movement, slopeHit.normal).normalized * currentSpeed; //Keep player fully grounded
        }

        controller.Move(movement * Time.deltaTime); //Applies the movement to character controller
    }

    private void IsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position + Vector3.down * groundDistance, groundRadius, groundMask); //Check to see if the player is touching the groundMask

        if (isGrounded && velocity.y < 0) //checks if player is not grounded and velocity on y axis is less than 0
        {
            velocity.y = gravity;
        }

        velocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime; //Continouslly ramps up the gravity as the player falls. 

        controller.Move(velocity * Time.deltaTime); //Applies the gravity to the player controller
    }

    /// <summary>
    /// Like graivty it just check if something if about your head.
    /// </summary>
    private void IsCeilingAbove()
    {
        var ceilingCheckPos = ceilingCheck.position;
        ceilingCheckPos.y = transform.TransformPoint(0, controller.height, 0).y;
        ceilingCheck.position = ceilingCheckPos;

        isCeilingAbove = Physics.CheckSphere(ceilingCheck.position + Vector3.up * ceilingDistance, ceilingRadius, groundMask); //Check if head gets hit 

        if (isCrouching == false) //Updates players height when they let go of crouch and hit their head
        {
            if (isCeilingAbove)
            {
                targetHeight = currentHeight;
            }
            else targetHeight = baseHeight;
        }
    }

    private void CrouchSpeed()
    {
        if (isGrounded == true && isSliding == false && isSprinting == false)
        {
            //Following 4 lines is a map range, this controls people speed if they are half crouched without pressing crouch
            //Can also be used for damage drop for nades and bullets.
            float aValue = controller.height;
            float normal = Mathf.InverseLerp(crouchHeight, baseHeight, aValue);
            float bvalue = Mathf.Lerp(crouchSpeed, baseSpeed, normal);
            currentSpeed = bvalue;
        }
    }


    /// <summary>
    /// Checks if player is on a angle, then returns what that angle is
    /// </summary>
    /// //Bug: If player look towards slope while sliding that can get infinite speed overtime.
    public bool SlopeChecker()
    {
        
        if (Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, groundMask)) 
        {
            slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal); //Get the angle of the slope
            return slopeAngle < controller.slopeLimit && slopeAngle != 0;
        }
        return false;
    }

    /// <summary>
    /// Makes player slide to bottom of slope if its too steep or if they try to slide on a slope
    /// 1 Bug/feature: When player slide then jumps the vector wont get reset untill in air so player gets slight jump increase from vector
    /// </summary>
    private void slopeSlider()
    {
        if (isGrounded == false)
        {
            slideMovement = Vector3.zero;
            return;
        }

        if (SlopeChecker())
        {
            if (isSliding)
            {
                isSlopeSliding = true;
                Vector3 slopeDirection = Vector3.up - slopeHit.normal * Vector3.Dot(Vector3.up, slopeHit.normal); //Gets direction of the slope
                slideMovement = -slopeDirection; //Makes it so -slopedirection goes down instead up
            }
            else
            {
                isSlopeSliding = false;
                slideMovement = Vector3.zero;
            }
        }
        else if (slopeAngle > controller.slopeLimit)
        {
            isSlopeSliding = true;
            Vector3 slopeDirection = Vector3.up - slopeHit.normal * Vector3.Dot(Vector3.up, slopeHit.normal); //Gets direction of the slope
            targetSpeed += slopeAngle * slopeMultiplier * Time.deltaTime; //Makes the spend go gradually up from sprintSpeed
            slideMovement = -slopeDirection * targetSpeed; //Applies new direction to Slidemovement
        }
        else if (slopeAngle == 0)
        {
            slideMovement = Vector3.zero; //resets slideMovement that way player dont slide forever
            isSlopeSliding = false;
        }

        if (slopeAngle > controller.slopeLimit)
        {
            slideMovement = (Vector3.ProjectOnPlane(slideMovement, slopeHit.normal));
        }
        else
        {
            slideMovement = (Vector3.ProjectOnPlane(slideMovement, slopeHit.normal)) * currentSpeed;
        }

        controller.Move(slideMovement * Time.deltaTime); //Move the character along slopeDirection
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position + Vector3.down * groundDistance, groundRadius);
        Gizmos.DrawWireSphere(ceilingCheck.position + Vector3.up * ceilingDistance, ceilingRadius);
        Gizmos.DrawRay(groundCheck.position, Vector3.down * 0.3f);
    }
}
