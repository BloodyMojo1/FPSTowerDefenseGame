using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public ShopManager shopManager;
    [SerializeField] private GunData gunData;
    [SerializeField] private GameObject WeaponCam;
    [SerializeField] private MouseLook mouse;

    private bool playerInShop;

    private void Awake()
    {
    }

    private void Update()
    {
        if(gunData == null)
        {
            gunData = WeaponCam.GetComponentInChildren<GunData>();

            if(playerInShop == true)
            {
                gunData.controls.Disable();

            }

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            mouse.controls.Disable();
            playerInShop = true;
            gunData.controls.Disable();
            shopManager.InstantiateLoot();
            shopManager.ShopArea.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInShop= false;
            gunData.controls.Enable();
            mouse.controls.Enable();
            shopManager.ShopArea.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            shopManager.DestroyButtons();
        }
    }
}
