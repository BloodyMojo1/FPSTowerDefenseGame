using UnityEngine;

public class TargetBehaviour : MonoBehaviour, IDamageable
{
    
    private GameObject targetSpawner;
    private TargetMiniGame targetSpawner_Script;
    
    [SerializeField] private int maxHealth;
    [SerializeField] private int health;
    [SerializeField] private int currencyWorth = 50;

    public UnitHealth targetHealth;
    //public UnitHealth targetHealth = new UnitHealth(100, 100); //Original code which cant set in inspector (just too show)

    private void Awake()
    {
        targetSpawner = GameObject.FindGameObjectWithTag("Spawner");
        targetSpawner_Script = targetSpawner.GetComponent<TargetMiniGame>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        targetHealth = new UnitHealth(health, maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (targetHealth.Health == 0)
        {
            
            targetSpawner_Script.test();
            Destroy(transform.parent.gameObject);
            CurrencyManager.main.IncreaseCurrency(currencyWorth);
        }
    }

    public void PlayerTakeDmg(int dmg)
    {
        targetHealth.DmgUnit(dmg);
        Debug.Log(targetHealth.Health);

    }

    //For anything that heals
    private void playerHeal(int healing)
    {
        targetHealth.HealUnit(healing);
    }

    public void Damage(int damageAmount)
    {
        PlayerTakeDmg(damageAmount);
    }
}
