using UnityEngine;

public class TargetMiniGame : MonoBehaviour
{
    [SerializeField] private Vector3 size;

    [SerializeField] private GameObject[] target;
    [SerializeField] private float spacingGap;
    public int targetCapacity;

    public int targetCount;

    private Vector3 pos;

    private GameObject clone;


    // Start is called before the first frame update
    void Start()
    {
        //targetCount = 0;
    }

    private void FixedUpdate()
    {
        while (targetCount < targetCapacity)
        {
            targetCount++;
            int randomIndex = Random.Range(0, target.Length);

            pos = transform.position + new Vector3(Random.Range(size.x, -size.x) / spacingGap,
                Random.Range(size.y, -size.y) / spacingGap,
                Random.Range(size.z, -size.z) / spacingGap);
            clone = Instantiate(target[randomIndex], pos, Quaternion.identity);
        }
    }

    public void test()
    {
        targetCount--;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawCube(transform.position, size);
    }
}
