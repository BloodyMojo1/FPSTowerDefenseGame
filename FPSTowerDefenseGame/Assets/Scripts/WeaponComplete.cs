using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        foreach(WeaponParts weaponParts in defaultWeaponPartList)
        {
            SetPart(weaponParts);
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

}
