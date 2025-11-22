using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefencePoint : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int health;

    public UnitHealth targetHealth;

    private void Awake()
    {
        targetHealth = new UnitHealth(health, maxHealth);
    }

    private void Update()
    {
        if (targetHealth.Health == 0)
        {

            DestroyAllAgents();
            WaveSpawner.onDefencePointDestory.Invoke();
            playerHeal(maxHealth);
        }
    }

    public void TakeDamage(int dmg)
    {
        targetHealth.DmgUnit(dmg);
        health = targetHealth.Health;
        Debug.Log(targetHealth.Health);
    }

    private void playerHeal(int healing)
    {
        targetHealth.HealUnit(healing);
        health = targetHealth.Health;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an enemy AI agent
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            WaveSpawner.onEnemyDestory.Invoke();
            Destroy(other.gameObject);

            // Remove the agent from the flock if it has the FlockingAgent component
            FlockingAgent agentScript = other.gameObject.GetComponent<FlockingAgent>();
            if (agentScript != null)
            {
                agentScript.RemoveFromFlock();
            }
        }
    }

    private void DestroyAllAgents()
    {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject agent in agents)
        {
            //WaveSpawner.onEnemyDestory.Invoke();
            Destroy(agent);
            FlockingAgent agentScript = agent.gameObject.GetComponent<FlockingAgent>();
            if (agentScript != null)
            {
                agentScript.RemoveFromFlock();
            }

        }
    }
}
