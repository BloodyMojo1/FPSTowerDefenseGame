using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponComplete : MonoBehaviour
{
    public class AttachWeaponPart
    {
        public WeaponBody.PartTypeAttachPoint partTypeAttachPoint;
        public WeaponParts weaponParts;
        public Transform spawnedTransform;
    }

    [SerializeField] private List<WeaponParts> defaultWeaponPartList;


    private WeaponBody weaponBody;
    [SerializeField] private WeaponPartList weaponPartList;
    private Dictionary<WeaponParts.PartType, AttachWeaponPart> attachedWeaponPartDic;

    private void Awake()
    {
        weaponBody = GetComponent<WeaponBody>();

        attachedWeaponPartDic = new Dictionary<WeaponParts.PartType, AttachWeaponPart>();

        foreach(WeaponBody.PartTypeAttachPoint partTypeAttachPoint in weaponBody.GetPartTypeAttachPointList())
        {
            attachedWeaponPartDic[partTypeAttachPoint.partType] = new AttachWeaponPart
            {
                partTypeAttachPoint = partTypeAttachPoint,
            };
        }


    }

    public void SetPart(WeaponParts weaponParts)
    {
        if (attachedWeaponPartDic[weaponParts.partType].spawnedTransform != null)
        {
            Destroy(attachedWeaponPartDic[weaponParts.partType].spawnedTransform.gameObject);
        }

        Transform spawnedPartTransform = Instantiate(weaponParts.prefab);
        AttachWeaponPart attachedWeaponPart = attachedWeaponPartDic[weaponParts.partType];
        attachedWeaponPart.spawnedTransform = spawnedPartTransform;

        Transform attachPointTransform = attachedWeaponPart.partTypeAttachPoint.attachPointTransform;
        spawnedPartTransform.parent = attachPointTransform;
        spawnedPartTransform.localEulerAngles = Vector3.zero;
        spawnedPartTransform.localPosition = Vector3.zero;

        attachedWeaponPart.weaponParts = weaponParts;

        attachedWeaponPartDic[weaponParts.partType] = attachedWeaponPart;
    }

    public WeaponParts GetWeaponParts(WeaponParts.PartType partType)
    {
        AttachWeaponPart attachWeaponPart = attachedWeaponPartDic[partType];
        return attachWeaponPart.weaponParts;
    }

    public WeaponBodyType GetWeaponBody()
    {
        return weaponBody.GetWeaponBody();
    }

    public void changePart(WeaponParts.PartType partType, WeaponParts weaponPart)
    {
        AttachWeaponPart attachWeaponPart = attachedWeaponPartDic[partType]; //if part
        for(int i = 0; i < weaponPartList.GetWeaponPartList(partType).Count; i++)
        {
            if(weaponPart == weaponPartList.GetWeaponPartList(partType)[i])
            {
                if (attachWeaponPart.weaponParts == null)
                {
                    SetPart(weaponPartList.GetWeaponPartList(partType)[i]);
                }
                else
                {
                    SetPart(weaponPartList.GetWeaponPartList(partType)[i]);
                }
            }
        }
    }
}
