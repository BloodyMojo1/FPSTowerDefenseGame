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

    private WeaponBody weaponBody;
    private WeaponBody attachmentWeaponBody;
    private WeaponParts currentWeaponPart;

    [HideInInspector]
    public List<Vector3> scopeAimPositionList;
    [HideInInspector]
    public List<Vector3> scopeGunRotationList;
    public List<WeaponPartList> weaponPartList;
    [SerializeField] private List<WeaponParts> defaultWeaponPartList;
    
    public List<WeaponParts> currentWeaponPartList;

    private Dictionary<WeaponParts.PartType, AttachWeaponPart> attachedWeaponPartDic;


    [HideInInspector]
    public string weaponPartName;

    private Vector3 scopeAimPosition;
    private GunData gunData;


    private void Awake()
    {
        weaponBody = GetComponent<WeaponBody>();
        gunData = GetComponent<GunData>();

        attachedWeaponPartDic = new Dictionary<WeaponParts.PartType, AttachWeaponPart>();

        //Finds all attachment points
        foreach (WeaponBody.PartTypeAttachPoint partTypeAttachPoint in weaponBody.GetPartTypeAttachPointList())
        {
            attachedWeaponPartDic[partTypeAttachPoint.partType] = new AttachWeaponPart
            {
                partTypeAttachPoint = partTypeAttachPoint,
            };
        }

        foreach (WeaponParts weaponParts in defaultWeaponPartList)
        {
            SetPart(weaponParts);
        }
    }

    public void SetPart(WeaponParts weaponParts)
    {

        //Destroy current weaponPart if 1 is attached.
        if (attachedWeaponPartDic[weaponParts.partType].spawnedTransform != null)
        {
            currentWeaponPart = attachedWeaponPartDic[weaponParts.partType].weaponParts;

            //Removes the extra added attachment list that are not spawned in
            if (attachmentWeaponBody != null)
            {
                if (weaponPartList.Contains(attachmentWeaponBody.weaponPartList))
                {
                    weaponPartList.Remove(attachmentWeaponBody.weaponPartList);
                }
            }

            //Remove any extra added spawns if attachments are not spawned in
            if (attachedWeaponPartDic.ContainsKey(WeaponParts.PartType.SecondaryScope))
            {
                if (attachedWeaponPartDic[WeaponParts.PartType.SecondaryScope].spawnedTransform != null)
                {
                    currentWeaponPartList.Remove(attachedWeaponPartDic[WeaponParts.PartType.SecondaryScope].weaponParts);
                }
            }

            attachedWeaponPartDic.Remove(WeaponParts.PartType.SecondaryScope);
            currentWeaponPartList.Remove(attachedWeaponPartDic[weaponParts.partType].weaponParts);
            Destroy(attachedWeaponPartDic[weaponParts.partType].spawnedTransform.gameObject);
            currentWeaponPart.UnEquipItem();
        }


        Transform spawnedPartTransform = Instantiate(weaponParts.prefab);

        if (spawnedPartTransform.GetComponent<WeaponBody>())
        {
            attachmentWeaponBody = spawnedPartTransform.GetComponent<WeaponBody>();

            Instantiate(attachmentWeaponBody.weaponBody.prefab, spawnedPartTransform);//Spawns Model
            if (!weaponPartList.Contains(attachmentWeaponBody.weaponPartList)) weaponPartList.Add(attachmentWeaponBody.weaponPartList);

            foreach (WeaponBody.PartTypeAttachPoint partTypeAttachPoint in attachmentWeaponBody.GetPartTypeAttachPointList())
            {
                attachedWeaponPartDic[partTypeAttachPoint.partType] = new AttachWeaponPart
                {
                    partTypeAttachPoint = partTypeAttachPoint,
                };
            }
        }

        AttachWeaponPart attachedWeaponPart = attachedWeaponPartDic[weaponParts.partType]; //Gets Weapon Part Type Location
        attachedWeaponPart.spawnedTransform = spawnedPartTransform; //Moves spawned part to location

        spawnedPartTransform.name = spawnedPartTransform.name.Replace("(Clone)", "");
        weaponPartName = spawnedPartTransform.name;

        //Parents spawned part and resets it Transform point to zero
        Transform attachPointTransform = attachedWeaponPart.partTypeAttachPoint.attachPointTransform;
        spawnedPartTransform.parent = attachPointTransform;
        spawnedPartTransform.localEulerAngles = Vector3.zero;
        spawnedPartTransform.localPosition = Vector3.zero;

        attachedWeaponPart.weaponParts = weaponParts;

        attachedWeaponPartDic[weaponParts.partType] = attachedWeaponPart;

        currentWeaponPartList.Add(attachedWeaponPartDic[weaponParts.partType].weaponParts);
        UpdateScopePostion();

    }


    /// <summary>
    /// Checks if attachment wanted, is availble for current weapon body
    /// </summary>

    public void changePart(WeaponParts.PartType partType, WeaponParts weaponPart)
    {
        //Got to check all list, have to add the attacmentbody weapon part list
        //Loops through alls part in weaponPartList
        for (int i = 0; i < weaponPartList.Count; i++)
        {
            for (int ii = 0; ii < weaponPartList[i].GetWeaponPartList(partType).Count; ii++)
            {

                //Makes part available & Calls SetPart to instantiate part
                if (weaponPart == weaponPartList[i].GetWeaponPartList(partType)[ii])
                {

                    CurrencyManager.main.attachmentAvailable = true;

                    SetPart(weaponPartList[i].GetWeaponPartList(partType)[ii]);
                    weaponPart.EquipItem();


                    break;
                }
                else if (weaponPart != weaponPartList[i].GetWeaponPartList(partType)[ii])
                {
                    CurrencyManager.main.attachmentAvailable = false;
                }
            }
        }
    }

    public void changeBulletStats(GameObject currentBullet)
    {
        Bullet bulletScript = currentBullet.GetComponent<Bullet>();
        for (int i = 0; i < currentWeaponPartList.Count; i++)
        {
            //currentWeaponPartList[i].UnEquipItem();
            currentWeaponPartList[i].EquipBulletStats(bulletScript);
        }
    }

    /// <summary>
    /// Checks if there are any scopes in current attachments
    /// Adds all scope vector pos/rot to a list 
    /// </summary>
    public void UpdateScopePostion()
    {
        scopeAimPositionList.Clear();
        scopeGunRotationList.Clear();

        for (int i = 0; i < currentWeaponPartList.Count; i++)
        {
            //Gets Aim position empty from scopes empty AimPos object
            if (currentWeaponPartList[i].partType == WeaponParts.PartType.MainScope || currentWeaponPartList[i].partType == WeaponParts.PartType.SecondaryScope || currentWeaponPartList[i].partType == WeaponParts.PartType.CantedScope)
            {
                scopeAimPosition = currentWeaponPartList[i].prefab.GetChild(0).position;
                if (currentWeaponPartList[i].prefab.GetComponent<WeaponBody>())
                {
                    scopeAimPosition = attachmentWeaponBody.weaponBody.prefab.GetChild(0).position;
                }

                if (currentWeaponPartList[i].partType == WeaponParts.PartType.SecondaryScope)
                {
                    scopeAimPosition = scopeAimPosition + attachedWeaponPartDic[WeaponParts.PartType.MainScope].partTypeAttachPoint.attachPointTransform.localPosition;
                }
                scopeAimPosition = scopeAimPosition + attachedWeaponPartDic[currentWeaponPartList[i].partType].partTypeAttachPoint.attachPointTransform.localPosition;

                //Adds scopes vector to list
                scopeAimPositionList.Add(scopeAimPosition);
                scopeGunRotationList.Add(currentWeaponPartList[i].scopeGunRotation);
            }
            else if (currentWeaponPartList[i].partType != WeaponParts.PartType.MainScope) //Check if there is no Mainscope
            {
                //Adds iron sight pos to the first slot
                scopeAimPositionList.Insert(0, gunData.aimingLocalPosition + new Vector3(0, 0, gunData.aimingLocalPosition.z - gunData.aimingOffset));
                scopeGunRotationList.Insert(0, Vector3.zero);
            }

            if (currentWeaponPartList[i].partType == WeaponParts.PartType.MainScope)
            {
                //Makes sure cycle is reset for no extra scope pos
                gunData.desiredGunRotation = scopeGunRotationList[0];
                gunData.sightPos = scopeAimPositionList[0];
            }
        }

    }

}

