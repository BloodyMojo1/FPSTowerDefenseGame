using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class WeaponParts : ScriptableObject
{
    public enum PartType
    {
        Barrel,
        Muzzel,
        Underbarrel,
        Stock,
        Grip,
        Scope,
        mag,
    }

    public PartType partType ; 
    public Transform prefab;
}

