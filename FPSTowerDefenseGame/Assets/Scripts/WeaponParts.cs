using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class WeaponParts : ScriptableObject
{
    //All of these can be [seralizeable]
    [SerializeField] private float scopeMagnification = 1;
    [SerializeField] private float nonZoomedFOV = 60;
    public float bulletSpeed, bulletDropOffRange;
    public int weaponDamage;

    [Range(-100f, 100f)]
    public float xSpread, ySpread;
    [Range(-100f, 100f)]
    public float horizontalRecoil, verticalRecoil;
    public float reloadTime;
    public float aimDownSightSpeed, drawTimeSpeed, muzzleFlashSize;
    
    public int magCapicity;

    public Vector3 scopeGunRotation;

    public enum PartType
    {
        Muzzel,
        SideRail,
        Grip,
        MainScope,
        SecondaryScope,
        CantedScope,
        Mag,
    }

    public PartType partType; 
    public Transform prefab;

    public void EquipItem()
    {
        //Get Bullet and Gundata 
        GameObject weaponCam = GameObject.Find("WeaponCamera").gameObject;
        GunData gunDataScript = weaponCam.transform.GetChild(1).GetComponent<GunData>();
        
        gunDataScript.magazineSize += magCapicity;
        gunDataScript.reloadTime += reloadTime;
        gunDataScript.aimDownSightSpeed += aimDownSightSpeed;
        gunDataScript.weaponDrawTime += drawTimeSpeed;
        gunDataScript.horizontalRecoilReduction += horizontalRecoil;
        gunDataScript.verticalRecoilReduction += verticalRecoil;
        gunDataScript.xSpreaadReduction += xSpread;
        gunDataScript.ySpreadReduction += ySpread;

        var factor = 2.0 * Mathf.Tan((float)(0.5 * nonZoomedFOV * Mathf.Deg2Rad));
        var zoomFov = 2.0 * Mathf.Atan((float)(factor / (2.0 * scopeMagnification))) * Mathf.Rad2Deg;
        gunDataScript.scopeCam.fieldOfView = (float)zoomFov;
        
    }

    public void UnEquipItem()
    {
        GameObject weaponCam = GameObject.Find("WeaponCamera").gameObject;
        GunData gunDataScript = weaponCam.transform.GetChild(1).GetComponent<GunData>();

        gunDataScript.magazineSize -= magCapicity;
        gunDataScript.reloadTime -= reloadTime;
        gunDataScript.aimDownSightSpeed -= aimDownSightSpeed;
        gunDataScript.weaponDrawTime -= drawTimeSpeed;
        gunDataScript.horizontalRecoilReduction -= horizontalRecoil;
        gunDataScript.verticalRecoilReduction -= verticalRecoil;
        gunDataScript.xSpreaadReduction += xSpread;
        gunDataScript.ySpreadReduction += ySpread;

        gunDataScript.scopeCam.fieldOfView = nonZoomedFOV;
    }

    public void EquipBulletStats(Bullet currentBulletScipt)
    {
        currentBulletScipt.maxDamage += weaponDamage;
        currentBulletScipt.maxBulletDropOffRange += bulletDropOffRange;
        currentBulletScipt.shootForce += bulletSpeed;
    }
}
