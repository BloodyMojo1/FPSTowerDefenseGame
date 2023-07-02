using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public ShopManager shopManager;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopManager.InstantiateLoot();
            shopManager.ShopArea.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopManager.ShopArea.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            shopManager.DestroyButtons();
        }
    }
}
