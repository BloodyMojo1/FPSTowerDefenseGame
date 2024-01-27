using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCam;
   
    private WeaponComplete weaponComplete;
    private GameObject currentWeapon;
    private GunData gunDataScript;
    
    public static CurrencyManager main;
    
    public int currency;

    [HideInInspector]
    public bool attachmentAvailable;


    private void Awake()
    {
        currentWeapon = playerCam.transform.GetChild(0).GetChild(1).gameObject;
        main = this;
    }

    private void Start()
    {
        currency = 100;
    }

    private void Update()
    {
        //Gets new Weapon components if spawned
        if (gunDataScript == null)
        {
            gunDataScript = gameObject.GetComponentInChildren<GunData>();
            weaponComplete = gameObject.GetComponentInChildren<WeaponComplete>();
        }
    }

    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }


    /// <summary>
    /// Gets ShopLoot Values from each ShopManager Button
    /// Spawns lootPrefab into world if it meets requirements
    /// </summary>
    
    //need to check if 
    public bool SpendCurrency(int amount, GameObject lootPrefab, ShopLoot.PrefabType prefabType, WeaponParts weaponParts)
    {
        if (amount <= currency)
        {
            if (currentWeapon.name == lootPrefab.name)
            {
                Debug.Log("Already have this Weapon");
                return false;
            } 

            if (weaponComplete.weaponPartName == lootPrefab.name)
            {
                Debug.Log("Already have this Attachment");
                return false;
            }

            if (prefabType == ShopLoot.PrefabType.GunPrefab)
            {
                //weaponComplete.changeBulletStats();
                Destroy(currentWeapon.gameObject);
                currentWeapon = Instantiate(lootPrefab, currentWeapon.transform.position, currentWeapon.transform.rotation, playerCam.transform.GetChild(0));
                currentWeapon.name = currentWeapon.name.Replace("(Clone)", "");
            }
            else
            {
                //Spawns weapon part onto currentWeapon body
                weaponComplete.changePart(weaponParts.partType, weaponParts);

                if (attachmentAvailable == false) 
                {
                    Debug.Log("Attachment Not Available");
                    return false;
                }
                attachmentAvailable = false;

            }

            currency -= amount;
            return true;
        }
        else
        {
            Debug.Log("You do not have enough to purchase this item");
            return false;
        }
    }
}
