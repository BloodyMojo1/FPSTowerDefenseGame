using TMPro;
using UnityEngine;

public class GunData : MonoBehaviour
{
    public InputMaster controls;
    private MouseLook mouse;
    private PlayerStateManager movementManager;
    private Camera playerCam;
    private Camera weaponCam;
    private WeaponComplete weaponComplete;
    public Camera scopeCam;

    public GameObject muzzleFlash;

    private GameObject attackPoint;
    private GameObject endOfBarrelPoint;
    public GameObject currentBullet;

    private Vector3 previousAttackPos;
    [Header("Shooting Stats")]

    public GameObject bullet;

    [SerializeField] private bool allowButtonHold;

    [SerializeField] private float bulletsPerTap;
    [SerializeField] private float timeBetweenShooting;

    [SerializeField] private float spreadX;
    [SerializeField] private float spreadY;

    [HideInInspector, Range(0f, 100f)]
    public float xSpreaadReduction = 0, ySpreadReduction = 0;

    [SerializeField] private float weaponCheckDistance;

    private float maxRecoilTimer = 0.1f;
    private float lerpPos;
    RaycastHit hit;

    [HideInInspector]
    public int bulletsShot;
    [HideInInspector]
    public bool shooting;
    private bool readyToShoot;

    private Vector3 weaponHitRotation;
    private Vector3 weaponHitPosition;

    [Header("Reload Stats")]

    private TextMeshProUGUI ammunitionDisplay;

    public int magazineSize;

    public float reloadTime;

    [HideInInspector]
    public int bulletsLeft;
    [HideInInspector]
    public bool reloading;

    [Header("Aiming System")]
     
    [Range(0f, 100f)]
    public float recoilAimReduction;

    public float aimingOffset;
    public float aimDownSightSpeed = 10;
    public float weaponDrawTime;

    private GameObject sightTargetPos;
    private Vector3 currentScopeGunRot;
    private Vector3 desiredPosition;

    [HideInInspector]
    public Vector3 sightPos;
    [HideInInspector]
    public Vector3 desiredGunRotation;

    [Space(20)]
    public Vector3 normalLocalPosition;
    public Vector3 aimingLocalPosition;
    [SerializeField] private Vector3 walkLocalPositon;
    



    [Space(20)]
    [SerializeField] private Vector3 sprintLocalPosition;
    [SerializeField] private Vector3 sprintLocalRotation;

    [Space(20)]
    [SerializeField] private Vector3 wallHitLocalPosition;
    [SerializeField] private Vector3 wallHitLocalRotation;

    [HideInInspector]
    public bool isAiming;

    [Header("Recoil System")]
    [HideInInspector, Range(0f, 100f)]
    public float horizontalRecoilReduction = 0, verticalRecoilReduction = 0;

    [Space(10)]
    public AnimationCurve yRecoilPattern;
    public AnimationCurve xRecoilPattern;

    public float xRandomRecoil;
    public float yRandomRecoil;
    public float zRandomRecoil;

    public float snapiness;
    public float maxPatternRecoilSpread;

    private float bulletTimer;

    [HideInInspector]
    public int bulletsShotInARow;
    [HideInInspector]
    public bool allowInvoke = true;

    private Vector3 zGunRecoil;
    [SerializeField] private float xRecoilRotation;
    [SerializeField] private float zRecoilSmoothing;
    private Vector3 recoilRotation;

    [Header("Weapon Sway")]
    [Range(0f, 100f)]
    [SerializeField] private float swayAimReduction;

    [SerializeField] private float swayStep = 0.01f;
    [SerializeField] private float maxStepDistance = 0.06f;

    [SerializeField] private float roatationStep = 4f;
    [SerializeField] private float maxRoatationStep = 5f;

    private Vector3 swayPos;
    private Vector3 swayEulerRot;

    private float smooth = 10f;

    [Header("Bobbing")]
    [Range(0f, 100f)]
    [SerializeField] private float bobAimReduction;

    [SerializeField] private float bobSpeed = 2;
    [SerializeField] private float aimBobSpeed = 4;

    [SerializeField] private Vector3 travelLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 bobLimit = Vector3.one * 0.01f;
    [SerializeField] private Vector3 bobMultiplier;

    private float currentBobSpeed;
    private float speedCurve;
    private float curveSin { get => Mathf.Sin(speedCurve); }
    private float curveCos { get => Mathf.Cos(speedCurve); }

    private Vector3 bobPosition;
    private Vector3 bobEulerRotation;

    private void Awake()
    {
        GameObject canvas = GameObject.FindGameObjectWithTag("UI");
        ammunitionDisplay = canvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        Transform rootParent = gameObject.transform.root;
        movementManager = rootParent.GetComponent<PlayerStateManager>();
        weaponCam = gameObject.GetComponentInParent<Camera>();
        playerCam = weaponCam.transform.parent.GetComponent<Camera>();
        scopeCam = playerCam.transform.GetChild(1).GetComponent<Camera>();
        sightTargetPos = weaponCam.transform.GetChild(0).gameObject;

        attackPoint = gameObject.transform.GetChild(1).gameObject;
        previousAttackPos = attackPoint.transform.localPosition;
        endOfBarrelPoint = gameObject.transform.GetChild(2).gameObject;


        weaponComplete = gameObject.GetComponent<WeaponComplete>();
        weaponComplete.scopeAimPositionList.Insert(0, aimingLocalPosition + new Vector3(0,0, aimingLocalPosition.z - aimingOffset));
        weaponComplete.scopeGunRotationList.Insert(0, Vector3.zero);
        sightPos = weaponComplete.scopeAimPositionList[(weaponComplete.scopeAimPositionList.IndexOf(sightPos) + 0) % weaponComplete.scopeAimPositionList.Count];
        desiredGunRotation = weaponComplete.scopeGunRotationList[(weaponComplete.scopeGunRotationList.IndexOf(desiredGunRotation) + 0) % weaponComplete.scopeGunRotationList.Count];


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
        if (bulletsLeft > magazineSize) bulletsLeft = magazineSize;

        //Checks if player is consectuvely shooting before reseting pattern
        if (bulletTimer >= timeBetweenShooting + 0.2)
        {
            bulletsShotInARow = 0;
        }

        if (bulletTimer >= maxRecoilTimer) mouse.ResetRecoil();



        if (Physics.Raycast(attackPoint.transform.position, attackPoint.transform.forward, out hit, weaponCheckDistance))
        {
            lerpPos = 1 - (hit.distance / weaponCheckDistance);
        }
        else lerpPos = 0;

        Mathf.Clamp01(lerpPos);

        DetermineAim();
        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRoation();
    }

    private void Input()
    {
        //Cycles through scope pos/rot list 
        if (controls.Player.NextSightPos.WasPerformedThisFrame())
        {
            float scrollDir = controls.Player.NextSightPos.ReadValue<Vector2>().normalized.y;
            if (scrollDir >= 1)
            {
                Vector3 nextScopePos = weaponComplete.scopeAimPositionList[(weaponComplete.scopeAimPositionList.IndexOf(sightPos) + 1) % weaponComplete.scopeAimPositionList.Count];
                Vector3 nextScopeGunRot = weaponComplete.scopeGunRotationList[(weaponComplete.scopeGunRotationList.IndexOf(desiredGunRotation) + 1) % weaponComplete.scopeGunRotationList.Count];
                sightPos = nextScopePos;
                desiredGunRotation = nextScopeGunRot;
            }
            else if (scrollDir <= -1)
            {
                Vector3 previousScopePos = weaponComplete.scopeAimPositionList[(weaponComplete.scopeAimPositionList.IndexOf(sightPos) + 1) % weaponComplete.scopeAimPositionList.Count];
                Vector3 previousScopeGunRot = weaponComplete.scopeGunRotationList[(weaponComplete.scopeGunRotationList.IndexOf(desiredGunRotation) + 1) % weaponComplete.scopeGunRotationList.Count];
                sightPos = previousScopePos;
                desiredGunRotation = previousScopeGunRot;
            }
            currentScopeGunRot = desiredGunRotation;
        }

        //Check if allowed to hold down the button
        if (allowButtonHold) shooting = controls.Player.Shoot.IsPressed();
        else shooting = controls.Player.Shoot.WasPerformedThisFrame();

        //Aiming
        if (controls.Player.Aim.IsPressed())
        {
            isAiming = true;
            if (currentBobSpeed != aimBobSpeed) currentBobSpeed = aimBobSpeed;
            desiredGunRotation = currentScopeGunRot;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 45, Time.deltaTime * aimDownSightSpeed);
            weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, 45, Time.deltaTime * aimDownSightSpeed);

            attackPoint.transform.localPosition = new Vector3(previousAttackPos.x, sightPos.y, previousAttackPos.z);
        }
        else
        {
            isAiming = false;
            if (currentBobSpeed != bobSpeed) currentBobSpeed = bobSpeed;
            desiredGunRotation = Vector3.zero;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 60, Time.deltaTime * aimDownSightSpeed);
            weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, 60, Time.deltaTime * aimDownSightSpeed);
            //desiredGunRotation = Vector3.zero;
            attackPoint.transform.localPosition = previousAttackPos;
        }

        //Reload
        if (controls.Player.Reload.WasPressedThisFrame() && bulletsLeft < magazineSize && !reloading) Reload();

        //Automatically reloads on when gun is shot with no bullets
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        //Shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            mouse.ResetRecoil();
            mouse.CalculateRecoil();

            Shoot();
        }
    }

    private void Shoot()
    {

        bulletTimer = 0;
        readyToShoot = false;

        //Calculates spread
        float x = Random.Range(-spreadX, spreadX);
        float y = Random.Range(-spreadY, spreadY);
        x = x - (x * xSpreaadReduction) / 100;
        y = y - (y * ySpreadReduction) / 100;


        //Calculates direction with spread.
        //Vector3 directionWithSpread = (fpsCam.transform.forward + fpsCam.transform.up * y + fpsCam.transform.right * x).normalized;
        Vector3 directionWithSpread = (attackPoint.transform.forward + attackPoint.transform.up * y + attackPoint.transform.right * x).normalized;

        //Instantiate bullet in world
        currentBullet = Instantiate(bullet, attackPoint.transform.position, attackPoint.transform.rotation);
        weaponComplete.changeBulletStats(currentBullet.gameObject);

        //Rotates bullet to shoot direction
        currentBullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);

        //Instantiate muzzle flash
        if (muzzleFlash != null)
        {
            var muzzleFlashObject = Instantiate(muzzleFlash, endOfBarrelPoint.transform.position, Quaternion.identity);
            Destroy(muzzleFlashObject, 4f);
        }

        bulletsLeft--;
        bulletsShotInARow++;
        bulletsShot++;


        //Invoke resetShot funtion (if not already invoked)
        if (allowInvoke)
        {
            allowInvoke = false;

            Invoke("ResetShoot", timeBetweenShooting);

            //Calculate recoil after bullet shot
            if (!isAiming)
            {
                mouse.RecoilFire(xRandomRecoil - (xRandomRecoil * horizontalRecoilReduction / 100), yRandomRecoil - (yRandomRecoil * verticalRecoilReduction / 100));
                zGunRecoil = new Vector3(0, 0, Random.Range(0, -zRandomRecoil));
                recoilRotation = new Vector3(Random.Range(0, -xRecoilRotation), 0, 0);
            }
            else
            {
                mouse.RecoilFire(xRandomRecoil - (xRandomRecoil * (horizontalRecoilReduction + recoilAimReduction) / (100 + recoilAimReduction)), yRandomRecoil - (yRandomRecoil * (verticalRecoilReduction + recoilAimReduction) / (100 + recoilAimReduction)));
                var zAimReducedRecoil = zRandomRecoil - (zRandomRecoil * recoilAimReduction / 100);
                var xAimReducedRecoilRot = xRecoilRotation - (xRecoilRotation * recoilAimReduction / 100);
                zGunRecoil = new Vector3(0, 0, Random.Range(0, -zAimReducedRecoil));
                recoilRotation = new Vector3(Random.Range(0, -xAimReducedRecoilRot), 0, 0);
            }
        }


        //if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) Invoke("Shoot", timeBetweenShooting);
    }

    private void DetermineAim()
    {
        //Calculates and moves gun postion to aim or hip fire
        Vector3 target = normalLocalPosition;
        //if (movementManager.isSprinting == true) target = sprinLocaltPositon;
        if (isAiming == true && lerpPos < 0.4 && lerpPos != 0)
        {
            desiredPosition = Vector3.Lerp(transform.localPosition, sightTargetPos.transform.localPosition + -sightPos + new Vector3(0, 0, aimingOffset), Time.deltaTime * aimDownSightSpeed);
        }
        else if (isAiming == true && lerpPos == 0)
        {
            desiredPosition = Vector3.Lerp(transform.localPosition, sightTargetPos.transform.localPosition + -sightPos + new Vector3(0, 0, aimingOffset), Time.deltaTime * aimDownSightSpeed);
        }
        else if (movementManager.isWalking == true && lerpPos == 0) target = walkLocalPositon;
        else if (movementManager.isSprinting == true && lerpPos == 0) target = sprintLocalPosition;

        if (isAiming == false)
        {
            desiredPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * aimDownSightSpeed);
        }
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
        if (isAiming == false)
        {
            Vector3 invertLook = mouse.mouseLook * -swayStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);
            invertLook = transform.localPosition + invertLook;

            swayPos = invertLook;
        }
        else
        {
            var reducedAimMaxStep = maxStepDistance - (maxStepDistance * swayAimReduction / 100);
            var reducedAimSwayStep = swayStep - (swayStep * swayAimReduction / 100);

            Vector3 invertLook = mouse.mouseLook * -reducedAimSwayStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -reducedAimMaxStep, reducedAimMaxStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -reducedAimMaxStep, reducedAimMaxStep);
            invertLook = transform.localPosition + invertLook;

            swayPos = invertLook;
        }

    }

    /// <summary>
    /// Calculates Rotatation for the gun based on mouse movement
    /// </summary>
    private void SwayRotation()
    {
        if (isAiming == false)
        {
            Vector2 invertLook = mouse.mouseLook * -roatationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxRoatationStep, maxRoatationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxRoatationStep, maxRoatationStep);

            swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }
        else
        {
            var reducedAimRotStep = roatationStep - (roatationStep * swayAimReduction / 100);
            var reducedAimMaxRotStep = maxRoatationStep - (maxRoatationStep * swayAimReduction / 100);

            Vector2 invertLook = mouse.mouseLook * -reducedAimRotStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -reducedAimMaxRotStep, reducedAimMaxRotStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -reducedAimMaxRotStep, reducedAimMaxRotStep);

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
        if (speedCurve >= 100) speedCurve = 0;


        if (isAiming == false)
        {
            bobPosition.x = (curveCos * bobLimit.x * (movementManager.isGrounded ? 1 : 0)) - (movementManager.move.x * travelLimit.x);
            bobPosition.y = (curveSin * bobLimit.y) - (movementManager.movement.y * travelLimit.y);
            bobPosition.z = -(movementManager.move.y * travelLimit.y);

        }
        else
        {
            var ruducedAimBobLimit = bobLimit - (bobLimit * bobAimReduction / 100);
            var reducedAimTravelLimit = travelLimit - (travelLimit * bobAimReduction / 100);

            bobPosition.x = (curveCos * ruducedAimBobLimit.x * (movementManager.isGrounded ? 1 : 0)) - (movementManager.move.x * reducedAimTravelLimit.x);
            bobPosition.y = (curveSin * ruducedAimBobLimit.y) - (movementManager.movement.y * reducedAimTravelLimit.y);
            bobPosition.z = -(movementManager.move.y * reducedAimTravelLimit.y);
        }
    }

    /// <summary>
    /// Calculates Rotatation for the gun based on movement
    /// </summary>
    private void BobRotation()
    {
        if (isAiming == false)
        {
            bobEulerRotation.x = (movementManager.move != Vector2.zero ? bobMultiplier.x * (Mathf.Sin(2 * speedCurve)) : bobMultiplier.x * (Mathf.Cos(2 * speedCurve) / 2));
            bobEulerRotation.y = (movementManager.move != Vector2.zero ? bobMultiplier.y * curveCos : 0);
            bobEulerRotation.z = (movementManager.move != Vector2.zero ? bobMultiplier.z * curveCos * movementManager.move.x : 0);
        }
        else
        {
            var reducedAimBobMultiplier = bobMultiplier - (bobMultiplier * bobAimReduction / 100);

            bobEulerRotation.x = (movementManager.move != Vector2.zero ? reducedAimBobMultiplier.x * (Mathf.Sin(2 * speedCurve)) : reducedAimBobMultiplier.x * (Mathf.Cos(2 * speedCurve) / 2));
            bobEulerRotation.y = (movementManager.move != Vector2.zero ? reducedAimBobMultiplier.y * curveCos : 0);
            bobEulerRotation.z = (movementManager.move != Vector2.zero ? reducedAimBobMultiplier.z * curveCos * movementManager.move.x : 0);
        }

    }

    /// <summary>
    /// Makes gun move/rotate based on the sway and bob calculations 
    /// </summary>
    private void CompositePositionRoation()
    {
        Debug.DrawRay(attackPoint.transform.position, attackPoint.transform.forward * weaponCheckDistance, Color.blue);

        weaponHitRotation = Vector3.Lerp(Vector3.zero, wallHitLocalRotation, lerpPos);
        weaponHitPosition = Vector3.Lerp(Vector3.zero, wallHitLocalPosition, lerpPos);


        if (isAiming == true && lerpPos < 0.4)
        {
            Debug.Log("Keep aiming");
            weaponHitPosition = Vector3.zero;
            weaponHitRotation = Vector3.zero;

        }

        zGunRecoil = Vector3.Lerp(zGunRecoil, Vector3.zero, zRecoilSmoothing * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos + bobPosition + zGunRecoil + weaponHitPosition, smooth * Time.deltaTime);

        recoilRotation = Vector3.Lerp(recoilRotation, Vector3.zero, smooth * Time.deltaTime);

        if (movementManager.isSprinting == true && lerpPos == 0 && isAiming == false)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation) * Quaternion.Euler(recoilRotation) * Quaternion.Euler(sprintLocalRotation) * Quaternion.Euler(weaponHitRotation), Time.deltaTime * weaponDrawTime);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation) * Quaternion.Euler(recoilRotation) * Quaternion.Euler(desiredGunRotation) * Quaternion.Euler(weaponHitRotation), Time.deltaTime * weaponDrawTime);
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
