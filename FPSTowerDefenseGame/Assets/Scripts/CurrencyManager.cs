using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCam;
    private GameObject currentWeapon;

    public static CurrencyManager main;
    private GunData gunDataScript;
    private WeaponComplete weaponComplete;

    public int currency;

    private void Awake()
    {
        currentWeapon = playerCam.transform.GetChild(0).GetChild(0).gameObject;
        main = this;
    }

    private void Start()
    {
        currency = 100;
    }

    private void Update()
    {
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

    public bool SpendCurrency(int amount, GameObject lootModel, ShopLoot.PrefabType prefabType, WeaponParts weaponParts)
    {
        if (amount <= currency)
        {
            currency -= amount;

            if (prefabType == ShopLoot.PrefabType.GunPrefab)
            {
                Destroy(currentWeapon.gameObject);
                currentWeapon = Instantiate(lootModel, currentWeapon.transform.position, currentWeapon.transform.rotation, playerCam.transform.GetChild(0));
            }
            else
            {
                //Get weapon complete SetPart should therically work by destroying part, then set part 
                //Need to check if part is in part list


                //weaponComplete.SetPart(weaponParts);

                weaponComplete.changePart(weaponParts.partType, weaponParts);
            }

            return true;
        }
        else
        {
            Debug.Log("You do not have enough to purchase this item");
            return false;
        }
    }
}
