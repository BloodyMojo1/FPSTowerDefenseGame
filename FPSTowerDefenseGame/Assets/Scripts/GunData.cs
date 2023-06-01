using TMPro;
using UnityEngine;

public class GunData : MonoBehaviour
{
    private InputMaster controls;

    public GameObject bullet;

    [SerializeField] private float timeBetweenShooting;
    [SerializeField] private float spread;
    [SerializeField] private float reloadTime;

    [SerializeField] private int magazineSize;
    [SerializeField] private int oldMagazineSize;
    [SerializeField] private float bulletsPerTap;

    [SerializeField] private bool allowButtonHold;

    private int bulletsLeft;
    private int bulletsShot;
    public int bulletsShotInARow;

    public bool shooting;
    private bool readyToShoot;
    private bool reloading;

    public Camera fpsCam;
    public Transform attactPoint;

    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    private MouseLook recoilScript;

    public AnimationCurve yRecoilPattern;
    public AnimationCurve xRecoilPattern;
    public AnimationCurve yADSRecoilPattern;
    public AnimationCurve xADSRecoilPattern;

    public float yRandomRecoil;
    public float xRandomRecoil;
    public float yADSRandomRecoil;
    public float xADSRandomRecoil;

    public float returnSpeed;
    public float minRecoilThreshold;

    public bool isAiming;

    [SerializeField] private Vector3 normalLocalPosition;
    [SerializeField] private Vector3 aimingLocalPosition;

    [SerializeField] private float aimSmoothing = 10;

    public bool allowInvoke = true;


    private void Awake()
    {
        controls = new InputMaster();
        bulletsLeft = magazineSize; //Make sure mag is full 
        readyToShoot = true;

    }

    private void Update()
    {
        recoilScript = gameObject.GetComponentInParent<MouseLook>();
        Input();

        //Set ammo display
        if (ammunitionDisplay != null) ammunitionDisplay.SetText(bulletsLeft + " / " + magazineSize);

        if (allowInvoke) bulletsShotInARow = 0;

        DetermineAim();

    }

    private void Input()
    {
        //Check if allowed to hold down the button
        if (allowButtonHold) shooting = controls.Player.Shoot.IsPressed();
        else shooting = controls.Player.Shoot.WasPerformedThisFrame();

        //Aiming
        if (controls.Player.Aim.IsPressed()) isAiming = true;
        else isAiming = false;

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

        //Invoke resetShot funtion (if not already invoked)
        if (allowInvoke)
        {
            Invoke("ResetShoot", timeBetweenShooting);
            allowInvoke = false;
            bulletsShotInARow++;

            //Calculate recoil after bullet shot
            if (!isAiming) recoilScript.RecoilFire(xRandomRecoil, yRandomRecoil);
            else recoilScript.RecoilFire(xADSRandomRecoil, yADSRandomRecoil);
        }
        
        //if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) Invoke("Shoot", timeBetweenShooting);
    }

    private void DetermineAim()
    {
        //Calculates and moves gun postion to aim or hip fire
        Vector3 target = normalLocalPosition;
        if (isAiming == true) target = aimingLocalPosition;

        Vector3 desiredPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * aimSmoothing);

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

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
