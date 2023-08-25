using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBody : MonoBehaviour
{

    [Serializable]
    public class PartTypeAttachPoint
    {

        public WeaponParts.PartType partType;
        public Transform attachPointTransform;

    }

    public WeaponBodyType weaponBody;
    public WeaponPartList weaponPartList;

    [SerializeField] private List<PartTypeAttachPoint> partTypeAttachPointList;


    public List<PartTypeAttachPoint> GetPartTypeAttachPointList()
    {
        return partTypeAttachPointList;
    }
}