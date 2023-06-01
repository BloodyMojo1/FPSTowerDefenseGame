using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float shootForce;
    [SerializeField] private float maxRange;
    [SerializeField] private float maxDamage = 50;
    [SerializeField] private int predictionStepsPerFrame = 6;

    [Tooltip("Subtracts value from dist to give better penetration")]
    [SerializeField] private float penetrationValue;

    [Tooltip("Subtracts value from survivalChance value on wall which give better pentration. If they = the same value its 100%.")]
    [SerializeField, Range(0, 1)] private float survivalRate;

    [SerializeField] private Vector3 bulletVelocity;
    [SerializeField] private GameObject bulletHole;

    private RaycastHit[] hitInfoA;
    private RaycastHit[] hitInfoB;

    private Vector3 lastPosition;
    private Vector3 entryPoint;
    private Vector3 exitPoint;
    private Vector3 point2;

    private float obstacleDistance;
    private float distanceTraveled;
    private int currentDamage;
    private int currentPenDamage;

    // Start is called before the first frame update
    private void Start()
    {
        lastPosition = transform.position;
        bulletVelocity = this.transform.forward * shootForce;
        currentDamage = Mathf.RoundToInt(maxDamage);
    }

    private void Update()
    {
        DamageFallOff();

    }

    private float FixedUpdate()
    {
        float stepSize = 1.0f / predictionStepsPerFrame;
        Vector3 point1 = this.transform.position; //Calculates new start position

        for (float step = 0; step < 1; step += stepSize)
        {
            bulletVelocity += Physics.gravity * stepSize * Time.deltaTime; //Calculates gravity overtime 
            point2 = point1 + bulletVelocity * stepSize * Time.deltaTime; //Calculates pridicted foward point with gravity

            hitInfoB = Physics.RaycastAll(point2, -bulletVelocity, (point2 - point1).magnitude); //Raycasts a point on all backward facing colliders
            for (int i = 0; i < hitInfoB.Length; i++)
            {
                exitPoint = hitInfoB[0].point; //Adds new exit point at new point

                if (hitInfoB[i].collider.GetComponent<WallPenetration>() != null) //Cehcks if collider has WallPenetration script
                {
                    WallPenetration wallPenetration = hitInfoB[i].collider.GetComponent<WallPenetration>(); //Gets WallPenetration values
                    float survivalChance = wallPenetration.survivalChance - survivalRate; //Calculates bullet survial chance

                    if (Random.value > survivalChance) //Checks if bullet can survive
                    {
                        obstacleDistance = Vector3.Distance(entryPoint, exitPoint); //finds the distance between entry,exit points
                        float maxPen = wallPenetration.maxPenetrationAmount - -penetrationValue; 

                        if (obstacleDistance <= maxPen)
                        {
                            int _ObstacleDistance = Mathf.RoundToInt(obstacleDistance);

                            currentPenDamage = _ObstacleDistance * wallPenetration.wallPenetrationDamage;

                            currentDamage = currentDamage - currentPenDamage;
                            Debug.Log(currentDamage);
                        }
                        else Destroy(gameObject);
                    }
                    else Destroy(gameObject);
                }
                else Destroy(gameObject);
            }

            //Raycasts a point on all forward facing colliders
            hitInfoA = Physics.RaycastAll(point1, bulletVelocity, (point2 - point1).magnitude);
            for (int i = 0; i < hitInfoA.Length; i++)
            {
                entryPoint = hitInfoA[i].point;
                //Instantiate a bullethole at entrypoint
                GameObject bulletHoles = Instantiate(bulletHole, hitInfoA[i].point + hitInfoA[i].normal * 0.001f, Quaternion.LookRotation(-hitInfoA[i].normal));
                //Destroy(bulletHoles.gameObject, 5f);

                //Destroys buttet if collider doesnt have tag
                if (hitInfoA[i].collider.tag != "Penetrable Wall") Destroy(gameObject);
            }
            point1 = point2; //Makes point 1 = point 2 to move bullet
        }

        //Destroy(gameObject, 4);

        this.transform.position = point1;
        transform.GetChild(0).rotation = Quaternion.LookRotation(bulletVelocity); //Make bullet look towards the bullet trajectory

        return (point2 - point1).magnitude;
    }

    /// <summary>
    /// Calculates how far the bullet has traveled and reduces damage.
    /// </summary>
    private void DamageFallOff()
    {
        //Finds how long bullet has traveled
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        //Calculates bullet damage drop off
        float avalue = distanceTraveled;
        float normal = Mathf.InverseLerp(0, maxRange, avalue);
        float bvalue = Mathf.Lerp(maxDamage, 0, normal);
        currentDamage = Mathf.RoundToInt(bvalue);

        if (currentDamage == 0) Destroy(gameObject); //This kinda gets rid of the need for a timer to destroy gameObject unless u have a high maxRange
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 point1 = this.transform.position;
        Vector3 predictedBulletVelocity = bulletVelocity;
        float stepSize = 0.01f;

        for (float step = 0; step < 1; step += stepSize)
        {
            predictedBulletVelocity += Physics.gravity * stepSize;
            Vector3 point2 = point1 + predictedBulletVelocity * stepSize;
            Gizmos.DrawLine(point1, point2);
            point1 = point2;

        }
        Gizmos.DrawSphere(entryPoint, 0.1f);
        Gizmos.DrawSphere(exitPoint, 0.1f);
    }
}
