using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    PlayerBaseState currentState;

    public Idle idle = new Idle();
    public Walk walk = new Walk();

    [Header("Gneral/Base Character Variables")]

    [SerializeField] private CharacterController controller;

    public InputMaster controls;

    public float baseSpeed = 12f;

    [SerializeField, Range(0, 1)] private float airSpeed = 0.5f;

    [HideInInspector]
    public float currentSpeed;

    private Vector2 move;
    private Vector3 movement;
    private Vector3 oldmovement;
    [HideInInspector]
    public Vector3 velocity;

    [Header("Gravity")]

    public Transform groundCheck;

    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    private float groundRadius = 0.6f;

    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [HideInInspector]
    public bool isGrounded;

    private void Awake()
    {
        controls = new InputMaster();
        currentSpeed = baseSpeed;
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

        IsGrounded();
        Movement();
        Debug.Log(currentState);
    }

    public void SwitchState(PlayerBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);

        Debug.Log(currentSpeed);
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

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
