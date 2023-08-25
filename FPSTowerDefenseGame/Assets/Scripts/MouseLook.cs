using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 200f;

    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;

    public Vector2 mouseLook;

    private GunData gunDataScript;

    private float xRecoil;
    private float yRecoil;

    private float sideRecoil;
    private float upRecoil;

    public InputMaster controls;

    private void Awake()
    {
        gunDataScript = gameObject.GetComponentInChildren<GunData>();
        controls = new InputMaster(); //Sets up controls with Input Manager
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (gunDataScript == null)
        {
            gunDataScript = gameObject.GetComponentInChildren<GunData>();
        }

        mouseLook = controls.Player.MouseLook.ReadValue<Vector2>(); //Get Mouses Values

        //Calculates recoul pattern
        if(gunDataScript.shooting == false || gunDataScript.reloading)
        {
            ResetRecoil();
        }

        //Adds random and pattern recoil here
        mouseLook.x += ((xRecoil + upRecoil) * gunDataScript.maxPatternRecoilSpread * Time.deltaTime) * 10;
        mouseLook.y += ((yRecoil + sideRecoil) * gunDataScript.maxPatternRecoilSpread * Time.deltaTime) * 10;


        //Adds sensitivity to the camera
        float mouseX = mouseLook.x * mouseSensitivity;
        float mouseY = mouseLook.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90); //Clamps the Y axis value that way player dont look past head/feet

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f).normalized; //Rotates the Camera 

        playerBody.Rotate(Vector3.up * mouseX); //Rotates the Player on the X axis
    }

    public void RecoilFire(float up, float side)
    {
        upRecoil += Random.Range(-up, up);
        sideRecoil += Random.Range(-side, side);
    }

    public void CalculateRecoil()
    {

        xRecoil = gunDataScript.xRecoilPattern.Evaluate(gunDataScript.bulletsShotInARow);
        yRecoil = gunDataScript.yRecoilPattern.Evaluate(gunDataScript.bulletsShotInARow);



        if (gunDataScript.isAiming)
        {
            xRecoil = xRecoil - (xRecoil * (gunDataScript.horizontalRecoilReduction + gunDataScript.recoilAimReduction) / (100 + gunDataScript.recoilAimReduction));
            yRecoil = yRecoil - (yRecoil * (gunDataScript.verticalRecoilReduction + gunDataScript.recoilAimReduction) / (100 + gunDataScript.recoilAimReduction));

        }
        else
        {
            xRecoil = xRecoil - (xRecoil * gunDataScript.horizontalRecoilReduction / 100);
            yRecoil = yRecoil - (yRecoil * gunDataScript.verticalRecoilReduction / 100);

        }

        upRecoil += gunDataScript.snapiness * Time.deltaTime * ((upRecoil > 0) ? -1 : 1);
        sideRecoil += gunDataScript.snapiness * Time.deltaTime * ((sideRecoil > 0) ? -1 : 1);
    }

    public void ResetRecoil()
    {
        xRecoil = 0;
        yRecoil = 0;
        upRecoil = 0;
        sideRecoil = 0;
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
