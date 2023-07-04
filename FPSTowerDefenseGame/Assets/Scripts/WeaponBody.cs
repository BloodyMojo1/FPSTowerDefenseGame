using System;
using System.Collections;
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

    [SerializeField] private WeaponBodyType weaponBody;

    [SerializeField] private List<PartTypeAttachPoint> partTypeAttachPointList;

    public WeaponBodyType GetWeaponBody()
    {
        return weaponBody;
    }

    public List<PartTypeAttachPoint> GetPartTypeAttachPointList()
    {
        return partTypeAttachPointList;
    }
}