using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Setting Up GameManager for future purposes
    //With health system in mind to end game when players are all downed, or main defend point is destroyed

    public static GameManager gameManager { get; private set; }

    public UnitHealth playerHealth = new UnitHealth(100, 100); //Temporary system (will be used for main defend point L8)

    private void Awake()
    {
        //Deletes any other Game Manager if there is multiple created
        if(gameManager != null && gameManager != this)
        {
            Destroy(this);
        }
        else
        {
            gameManager = this;
        }
    }
}