using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WeaponBodyType : ScriptableObject
{
    public enum Body
    {
        RifleA,
        RifleB,
        Pistol,
    }

    public Body body;
    public Transform prefab;
    public Transform prefabUI;
    public WeaponPartList weaponPartList;

}
