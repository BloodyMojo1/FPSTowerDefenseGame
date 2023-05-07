using System.Collections;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 200f;

    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;

    Vector2 mouseLook;

    private InputMaster controls;

    private void Awake()
    {
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
        mouseLook = controls.Player.MouseLook.ReadValue<Vector2>(); //Get Mouses Values


        //Adds sensitivity to be able to look around
        float mouseX = mouseLook.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseLook.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY; 
        xRotation = Mathf.Clamp(xRotation, -90, 90); //Clamps the Y axis value that way player dont look past head/feet

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f).normalized; //Rotates the Camera 

        playerBody.Rotate(Vector3.up * mouseX); //Rotates the Player on the X axis
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
