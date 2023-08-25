using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WeaponPartList : ScriptableObject
{
    public List<WeaponParts> weaponPartList;

    /// <summary>
    /// Checks if Shop Button Part is in PartList
    /// if True add to list
    /// </summary>
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
