using TMPro;
using UnityEngine;

public class GunData : MonoBehaviour
{
    private InputMaster controls;
    private MouseLook mouse;
    [SerializeField]
    private PlayerStateManager movementManager;

    public Camera fpsCam;
    
    public GameObject muzzleFlash;

    public Transform attactPoint;


    [Header("Shooting Stats")]
    
    public GameObject bullet;


    [SerializeField] private bool allowButtonHold;

    [SerializeField] private float bulletsPerTap;
    [SerializeField] private float timeBetweenShooting;
    [SerializeField] private float spread;

    [HideInInspector]
    public int bulletsShot;

    [HideInInspector]
    public bool shooting;
    private bool readyToShoot;

    [Header("Reload Stats")]
    
    public TextMeshProUGUI ammunitionDisplay;

    [SerializeField] private int magazineSize;

    [SerializeField] private float reloadTime;
    
    private int bulletsLeft;

    private bool reloading;

    [Header("Aiming System")]
    [SerializeField] private Vector3 normalLocalPosition;
    [SerializeField] private Vector3 aimingLocalPosition;
    [SerializeField] private Vector3 desiredPosition;

    [SerializeField] private float aimSmoothing = 10;

    [HideInInspector]
    public bool isAiming;


    [Header("Recoil System")]
    
    public AnimationCurve yRecoilPattern;
    public AnimationCurve xRecoilPattern;
    public AnimationCurve yADSRecoilPattern;
    public AnimationCurve xADSRecoilPattern;

    public float xRandomRecoil;
    public float yRandomRecoil;
    public float zRandomRecoil;

    public float xADSRandomRecoil;
    public float yADSRandomRecoil;
    public float zADSRandomRecoil;

    public float snapiness;
    public float maxPatternRecoilSpread;

    [HideInInspector]
    private float bulletTimer;
    [HideInInspector]
    public int bulletsShotInARow;
    [HideInInspector]
    public bool allowInvoke = true;

    private Vector3 zGunRecoil;
    [SerializeField] private float xRecoilRotation;
    [SerializeField] private float xADSRecoilRotation;
    [SerializeField] private float zRecoilSmoothing;
    private Vector3 recoilRotation;

    [Header("Weapon Sway")]
    [SerializeField] private float swayStep = 0.01f;
    [SerializeField] private float maxStepDistance = 0.06f;

    [SerializeField] private float roatationStep = 4f;
    [SerializeField] private float maxRoatationStep = 5f;

    [Space(10)]
    [SerializeField] private float aimSwayStep = 0.01f;
    [SerializeField] private float aimMaxStepDistance = 0.06f;

    [SerializeField] private float aimRoatationStep = 2f;
    [SerializeField] private float aimMaxRoatationStep = 3f;

    private Vector3 swayPos;
    Vector3 swayEulerRot;

    float smooth = 10f;
    float smoothRot = 12f;

    [Header("Bobbing")]
    [SerializeField] private float bobSpeed = 2;

    [SerializeField] private Vector3 travelLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 bobLimit = Vector3.one * 0.01f;
    [SerializeField] private Vector3 bobMultiplier;


    [Space(10)]
    [SerializeField] private float aimBobSpeed = 4;

    [SerializeField] private Vector3 aimTravelLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 aimBobLimit = Vector3.one * 0.01f;
    [SerializeField] private Vector3 aimBobMultiplier;


    private float currentBobSpeed;
    private float speedCurve;
    private float curveSin { get => Mathf.Sin(speedCurve); }
    private float curveCos { get => Mathf.Cos(speedCurve); }

    private Vector3 bobPosition;
    private Vector3 bobEulerRotation;



    private void Awake()
    {
        mouse = gameObject.GetComponentInParent<MouseLook>();
        controls = new InputMaster();
        bulletsLeft = magazineSize; //Make sure mag is full 
        readyToShoot = true;
        bulletsShotInARow = 0;

    }

    private void Update()
    {
        //Add backward recoil, for game feel. ATM it doesnt feel like there any recoil being applied when the gun is shot
        
        Input();

        bulletTimer += Time.deltaTime;
        //Set ammo display
        if (ammunitionDisplay != null) ammunitionDisplay.SetText(bulletsLeft + " / " + magazineSize);


        //Checks if player is consectuvely shooting before reseting pattern
        if (bulletTimer >= timeBetweenShooting + 0.2)
        {
            bulletsShotInARow = 0;
        }

        DetermineAim();
        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRoation();
        
    }

    private void Input()
    {
        //Check if allowed to hold down the button
        if (allowButtonHold) shooting = controls.Player.Shoot.IsPressed();
        else shooting = controls.Player.Shoot.WasPerformedThisFrame();

        //Aiming
        if (controls.Player.Aim.IsPressed())
        {
            isAiming = true;
            if(currentBobSpeed != aimBobSpeed) currentBobSpeed = aimBobSpeed;
        }
        else
        {
            isAiming = false;
            if (currentBobSpeed != bobSpeed) currentBobSpeed = bobSpeed;
        }
   

        //Reload
        if (controls.Player.Reload.WasPressedThisFrame() && bulletsLeft < magazineSize && !reloading) Reload();

        //Automatically reloads on when gun is shot with no bullets
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        //Shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;

            Shoot();
        }
    }

    private void Shoot()
    {
        bulletTimer = 0;
        readyToShoot = false;

        //Calculates spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Calculates direction with spread.
        Vector3 directionWithSpread = (fpsCam.transform.forward + fpsCam.transform.up * y + fpsCam.transform.right * x).normalized;

        //Instantiate bullet in world
        GameObject currentBullet = Instantiate(bullet, attactPoint.position, attactPoint.rotation);
        //Rotates bullet to shoot direction
        currentBullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);

        //Instantiate muzzle flash
        if (muzzleFlash != null)
        {
            var muzzleFlashObject = Instantiate(muzzleFlash, attactPoint.position, Quaternion.identity);
            Destroy(muzzleFlashObject, 4f);
        }

        bulletsLeft--;
        bulletsShot++;
        bulletsShotInARow++;

        //Invoke resetShot funtion (if not already invoked)
        if (allowInvoke)
        {
            allowInvoke = false;
            Invoke("ResetShoot", timeBetweenShooting);
            
            

            //Calculate recoil after bullet shot
            if (!isAiming)
            {
                mouse.RecoilFire(xRandomRecoil, yRandomRecoil);
                zGunRecoil = new Vector3(0, 0, Random.Range(0, -zRandomRecoil));
                recoilRotation = new Vector3(Random.Range(0, -xRecoilRotation), 0, 0);
            }
            else
            {
                mouse.RecoilFire(xADSRandomRecoil, yADSRandomRecoil);
                zGunRecoil = new Vector3(0, 0, Random.Range(0, -zADSRandomRecoil));
                recoilRotation = new Vector3(Random.Range(0, -xADSRecoilRotation), 0, 0);
            }
        }

        //if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) Invoke("Shoot", timeBetweenShooting);
    }

    private void DetermineAim()
    {
        //Calculates and moves gun postion to aim or hip fire
        Vector3 target = normalLocalPosition;
        if (isAiming == true) target = aimingLocalPosition;

        desiredPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * aimSmoothing);

        transform.localPosition = desiredPosition;
    }

    private void ResetShoot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;

    }

    /// <summary>
    /// calculates new gun x,y,z postion based on mouse movement
    /// </summary>
    private void Sway()
    {
        if(isAiming == false)
        {
            Vector3 invertLook = mouse.mouseLook * -swayStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);
            invertLook = desiredPosition + invertLook;

            swayPos = invertLook;
        }
        else
        {
            Vector3 invertLook = mouse.mouseLook * -aimSwayStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -aimMaxStepDistance, aimMaxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -aimMaxStepDistance, aimMaxStepDistance);
            invertLook = desiredPosition + invertLook;

            swayPos = invertLook;
        }

    }

    /// <summary>
    /// Calculates Rotatation for the gun based on mouse movement
    /// </summary>
    private void SwayRotation()
    {
        if(isAiming == false)
        {
            Vector2 invertLook = mouse.mouseLook * -roatationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxRoatationStep, maxRoatationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxRoatationStep, maxRoatationStep);

            swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }
        else
        {
            Vector2 invertLook = mouse.mouseLook * -aimRoatationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -aimMaxRoatationStep, aimMaxRoatationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -aimMaxRoatationStep, aimMaxRoatationStep);

            swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }
    }

    /// <summary>
    /// Calulates position based on movement
    /// </summary>
    private void BobOffset()
    {
        //Used to generate our sin and cos waves
        speedCurve += (Time.deltaTime * (movementManager.isGrounded ? movementManager.movement.magnitude : 1f) + 0.01f) / currentBobSpeed;
        if(speedCurve >= 100) speedCurve = 0;


        if (isAiming == false)
        {
            bobPosition.x = (curveCos * bobLimit.x * (movementManager.isGrounded ? 1 : 0)) - (movementManager.move.x * travelLimit.x);
            bobPosition.y = (curveSin * bobLimit.y) - (movementManager.movement.y * travelLimit.y);
            bobPosition.z = -(movementManager.move.y * travelLimit.y);
            
        }
        else
        {
            bobPosition.x = (curveCos * aimBobLimit.x * (movementManager.isGrounded ? 1 : 0)) - (movementManager.move.x * aimTravelLimit.x);
            bobPosition.y = (curveSin * aimBobLimit.y) - (movementManager.movement.y * aimTravelLimit.y);
            bobPosition.z = -(movementManager.move.y * aimTravelLimit.y);
        }
    }

    /// <summary>
    /// Calculates Rotatation for the gun based on movement
    /// </summary>
    private void BobRotation()
    {
        if(isAiming == false)
        {
            bobEulerRotation.x = (movementManager.move != Vector2.zero ? bobMultiplier.x * (Mathf.Sin(2 * speedCurve)) : bobMultiplier.x * (Mathf.Cos(2 * speedCurve) / 2));
            bobEulerRotation.y = (movementManager.move != Vector2.zero ? bobMultiplier.y * curveCos : 0);
            bobEulerRotation.z = (movementManager.move != Vector2.zero ? bobMultiplier.z * curveCos * movementManager.move.x : 0);
        }
        else
        {
            bobEulerRotation.x = (movementManager.move != Vector2.zero ? aimBobMultiplier.x * (Mathf.Sin(2 * speedCurve)) : aimBobMultiplier.x * (Mathf.Cos(2 * speedCurve) / 2));
            bobEulerRotation.y = (movementManager.move != Vector2.zero ? aimBobMultiplier.y * curveCos : 0);
            bobEulerRotation.z = (movementManager.move != Vector2.zero ? aimBobMultiplier.z * curveCos * movementManager.move.x : 0);
        }

    }

    /// <summary>
    /// Makes gun move/rotate based on the sway and bob calculations 
    /// </summary>
    private void CompositePositionRoation()
    {

        zGunRecoil = Vector3.Lerp(zGunRecoil,  Vector3.zero, zRecoilSmoothing * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos + bobPosition + zGunRecoil, smooth * Time.deltaTime);

        recoilRotation = Vector3.Lerp(recoilRotation, Vector3.zero, smooth * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation) * Quaternion.Euler(recoilRotation), Time.deltaTime * smoothRot);

        Vector3 attackSway = ((swayPos + bobPosition) - desiredPosition) + new Vector3(0, 0, 0.2f);
        attactPoint.transform.localPosition = Vector3.Lerp(attactPoint.transform.localPosition, attackSway, smooth * Time.deltaTime);

        //Slight randomness in Sway and bob will go along way. The movement is too repetivtive where u notice the loop.


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
