using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamageable
{
    public NavMeshAgent agent;

    public Transform player;
    public Transform defencePoint;

    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    [SerializeField] private int maxHealth;
    [SerializeField] private int health;
    [SerializeField] private int currencyWorth = 50;

    public UnitHealth targetHealth;

    //Attacking
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    private bool foundTarget;

    //States
    public float sightRange;
    public float attackRange;
    public bool playerInSightRange;
    public bool playerInAttackRange;




    private void Awake()
    {
        targetHealth = new UnitHealth(health, maxHealth);

        player = GameObject.Find("Player").transform;
        defencePoint = GameObject.Find("DefencePoint").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (targetHealth.Health == 0)
        {
            WaveSpawner.onEnemyDestory.Invoke();
            Destroy(gameObject);
            CurrencyManager.main.IncreaseCurrency(currencyWorth);
        }

        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        //if(playerInSightRange && !playerInAttackRange) ChasePlayer();
        //if (playerInAttackRange && playerInSightRange) AttackPlayer(); 

        //After few seconds reset to see if AI should target player or not

    }

    private void Patrolling()
    {
        agent.SetDestination(defencePoint.position);
        //set chase bool to false 
        //Patrolling will just be walking towards the objective
    }

    private void ChasePlayer()
    {
        /*if()
        {
            if (Random.value < 0.5f)
            {
                agent.SetDestination(player.position);

                //Set bool to true
            }
            else
            {
                agent.SetDestination(defencePoint.position);
            }
        }*/
        //When Ai is about to chase a player make sure they are not going the wrong direction to a point


    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //Attack code here

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void PlayerTakeDmg(int dmg)
    {
        targetHealth.DmgUnit(dmg);
        Debug.Log(targetHealth.Health);

    }

    public void Damage(int damageAmount)
    {
        PlayerTakeDmg(damageAmount);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
