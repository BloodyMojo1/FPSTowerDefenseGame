using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WeaponPartList : ScriptableObject
{
    public List<WeaponParts> weaponPartList;

    public List<WeaponParts> GetWeaponPartList(WeaponParts.PartType partType)
    {
        List<WeaponParts> returnWeaponPartList = new List<WeaponParts>();

        foreach(WeaponParts weaponParts in weaponPartList)
        {
            if(weaponParts.partType == partType)
            {
                returnWeaponPartList.Add(weaponParts);
            }
        }

        return returnWeaponPartList;
    }
}
