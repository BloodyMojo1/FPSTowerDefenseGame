using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPenetration : MonoBehaviour
{
    public float maxPenetrationAmount;
    public int wallPenetrationDamage;
    [Range(0f, 1f)] public float survivalChance;
}
