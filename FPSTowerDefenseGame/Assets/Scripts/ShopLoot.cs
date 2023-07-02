using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class ShopLoot : ScriptableObject
{
    public Sprite lootSprite;
    public Object lootModel;
    public string lootName;
    public int dropChance;
    public int itemCost;

    public ShopLoot(string lootName, int dropChance)
    {
        this.lootName = lootName;
        this.dropChance = dropChance;
    }
}
