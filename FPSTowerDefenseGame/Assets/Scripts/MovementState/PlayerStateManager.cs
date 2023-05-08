using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    PlayerBaseState currentState;

    public Idle idle = new Idle();
    public Walk walk = new Walk();
    public Sprint sprint = new Sprint();
    public Jump jump = new Jump();
    public Crouch crouch = new Crouch();

    [Header("Gneral/Base Character Variables")]

    public CooldownSystem cooldownSystem = null;

    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform fpsCam;

    public InputMaster controls;

    public float baseSpeed = 12f;
    public float baseHeight = 1f;


    [SerializeField, Range(0, 1)] private float airSpeed = 0.5f;

    [HideInInspector]
    public float currentHeight;

    [HideInInspector]
    public float currentSpeed;

    private Vector2 move;
    private Vector3 movement;
    private Vector3 oldmovement;
    [HideInInspector]
    public Vector3 velocity;

    [Header("Crouch Variables")]

    [SerializeField] private float heightSpeed = 1f;

    public float crouchSpeed = 6f;
    public float crouchHeight = 0.5f;

    [HideInInspector]
    public float targetHeight;
    [HideInInspector]
    public bool isCrouching;

    [Header("Sprint Variables")]

    public float sprintSpeed = 24f;

    [HideInInspector]
    public bool isSprinting;

    [Header("Jump Variables")]

    public int jumpId;
    public float JumpcooldownDuration = 5;

    public float maxJumpForce = 3f;
    public float minJumpForce = 0.5f;
    public float jumpForceReduction = 0.1f;

    [HideInInspector]
    public float currentJumpForce;

    [Header("Gravity")]

    public Transform groundCheck;

    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    private float groundRadius = 0.5f;

    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [HideInInspector]
    public bool isGrounded;

    [Header("Ceiling Checker")]

    public Transform ceilingCheck;

    private float ceilingRadius = 0.5f;

    [SerializeField] private float ceilingDistance = 0.4f;

    [HideInInspector]
    public bool isCeilingAbove;

    private void Awake()
    {
        controls = new InputMaster();
        currentSpeed = baseSpeed;
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
        IsGrounded();
        IsCeilingAbove();
        Movement();

        Debug.Log(currentState);
        Debug.Log(currentSpeed);

        //Moves character controller and FpsCam up and down for crouching
        if(currentHeight != targetHeight)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, Time.deltaTime * heightSpeed); //Move slowly toward target height
            float center = currentHeight / 2; //finds center of object
            controller.height = currentHeight; //sets controler height
            controller.center = new Vector3(0, center, 0); //creates new center point, so players feet stay on ground.


            // moves the camera with the character controller 
            var camPos = fpsCam.transform.position;
            camPos.y = transform.TransformPoint(new Vector3(0f, controller.height - 0.2f, 0f)).y;
            fpsCam.transform.position = camPos;
        }
    }

    public void SwitchState(PlayerBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);

    }

    private void Movement()
    {
        move = controls.Player.Movment.ReadValue<Vector2>(); //Read input values

        movement = ((move.y * transform.forward) + (move.x * transform.right)) * currentSpeed; //seperate the input values into different directions

        if (isGrounded == true)  oldmovement = movement; //Applies movement like normally 
        else  movement = oldmovement + ((oldmovement - movement) * -airSpeed); //Applies an initial force from the last moved direction & gives little air movement in other directions.
        
        controller.Move(movement * Time.deltaTime); //Applies the movement to character controller
    }

    private void IsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position + Vector3.down * groundDistance, groundRadius, groundMask); //Check to see if the player is touching the groundMask

        if(isGrounded && velocity.y < 0) //checks if player is not grounded and velocity on y axis is less than 0
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
        isCeilingAbove = Physics.CheckSphere(ceilingCheck.position + Vector3.up * ceilingDistance, ceilingRadius, groundMask);

        if(isCrouching == false)
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
        if (isSprinting == true) return;

        if(isGrounded == true)
        { 
            //Following 4 lines is a map range, this controls people speed if they are half crouched without pressing crouch
            //Can also be used for damage drop for nades and bullets.
            float aValue = controller.height;
            float normal = Mathf.InverseLerp(crouchHeight, baseHeight, aValue);
            float bvalue = Mathf.Lerp(crouchSpeed, baseSpeed, normal);
            currentSpeed = bvalue;
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position + Vector3.up * groundDistance, groundRadius);
        Gizmos.DrawWireSphere(ceilingCheck.position + Vector3.up * ceilingDistance, groundRadius);
    }
}
