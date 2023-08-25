using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu]
public class ShopLoot : ScriptableObject
{
    public Sprite lootSprite;
    public WeaponBodyType weaponBodyType;
    public WeaponParts weaponParts;

    public string lootName;
    public int dropChance;
    public int itemCost;


    public ShopLoot(string lootName, int dropChance)
    {
        this.lootName = lootName;
        this.dropChance = dropChance;
    }


    public enum PrefabType
    {
        GunPrefab,
        WeaponPart,
    }

    public PrefabType prefabType;
    //Get game object from weaponBodyType or WeaponParts

    //if prefabType = GunPrefab get weaponBodyType.prefab GameObject
    //if prefabType = WeaponParts get weaponParts.prefab gameObject

}



