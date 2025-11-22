using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlockingManager : MonoBehaviour
{
    [System.Serializable]
    public class Flock
    {
        public bool waypointsAssigned = false;
        public List<FlockingAgent> agents = new List<FlockingAgent>();
        [HideInInspector] public Vector3 flockCenter; // Store the center of the flock

        public int totalAgents = 0;
        public int totalPaths = 0;

        public int agentsPerPath = 0;
        public int remainingAgents = 0;

        public int agentsProcessed = 0;
        public int totalAgentsProcessed = 0;
        public int PathsProcessed = 0;
    }

    [SerializeField] private List<Flock> flocks = new List<Flock>();
    public float mergeRadius = 10f; // Radius within which flocks will be merged
    [SerializeField] private float neighborRadius = 2f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float separationWeight = 1f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float waypointWeight = 1f;



    // Update is called once per frame
    void Update()
    {
        foreach (Flock flock in flocks)
        {
            CalculateFlockCenter(flock);
        }
        MergeFlocks();
        SeparateAgentsFromFlocks();

        DeleteEmptyFlocks();
    }

    public void CreateNewFlock(List<FlockingAgent> agents)
    {
        Flock flock = new Flock();
        flock.agents.AddRange(agents);
        flocks.Add(flock);
    }

    private void MergeFlocks()
    {
        for (int i = 0; i < flocks.Count; i++)
        {
            for (int j = i + 1; j < flocks.Count; j++)
            {
                if (Vector3.Distance(flocks[i].flockCenter, flocks[j].flockCenter) < mergeRadius)
                {
                    // Transfer all agents from flock j to flock i
                    flocks[i].agents.AddRange(flocks[j].agents);

                    // Determine the agent with the furthest waypoint progress
                    FlockingAgent furthestAgent = FindFurthestAgent(flocks[i]);

                    // Update all agents in the merged flock to follow the furthest agent's waypoints
                    foreach (var agent in flocks[i].agents)
                    {
                        agent.waypointList = furthestAgent.waypointList;

                        agent.waypointIndex = furthestAgent.waypointIndex;
                        agent.splitWaypointIndex = furthestAgent.splitWaypointIndex;
                    }

                    // Remove flock j
                    flocks.RemoveAt(j);
                    j--; // Adjust index after removal
                }
            }
        }
    }

    private FlockingAgent FindFurthestAgent(Flock flock)
    {
        FlockingAgent furthestAgent = null; // Initialize furthestAgent to null
        foreach (var agent in flock.agents)
        {
            // Check if furthestAgent is null or if the current agent has progressed further
            if (furthestAgent == null ||
                agent.splitWaypointIndex > furthestAgent.splitWaypointIndex ||
                (agent.splitWaypointIndex > furthestAgent.splitWaypointIndex && agent.waypointIndex > furthestAgent.waypointIndex))

            {
                furthestAgent = agent; // Update furthestAgent
            }
        }
        return furthestAgent;
    }

    private void DeleteEmptyFlocks()
    {
        for (int i = flocks.Count - 1; i >= 0; i--)
        {
            if (flocks[i].agents.Count == 0)
            {
                flocks.RemoveAt(i);
            }
        }
    }

    private void CalculateFlockCenter(Flock flock)
    {
        Vector3 center = Vector3.zero;

        foreach (var agent in flock.agents)
        {
            center += agent.transform.position;
        }

        center /= flock.agents.Count;
        flock.flockCenter = center;
    }

    public void SeparateAgentsFromFlocks()
    {
        List<Flock> newFlocks = new List<Flock>(); // List to store new flocks to add

        foreach (var flock in flocks)
        {
            List<FlockingAgent> agentsToRemove = new List<FlockingAgent>(); // List to store agents to remove

            for (int i = flock.agents.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(flock.agents[i].transform.position, flock.flockCenter) > mergeRadius)
                {
                    // Remove the agent from the current flock
                    FlockingAgent agentToSeparate = flock.agents[i];
                    agentsToRemove.Add(agentToSeparate); // Queue for removal

                    // Create a new flock with the separated agent
                    Flock newFlock = new Flock();
                    newFlock.agents.Add(agentToSeparate);
                    newFlocks.Add(newFlock); // Queue new flock for addition
                }
            }

            // Remove agents from current flock
            foreach (var agent in agentsToRemove)
            {
                flock.agents.Remove(agent);
            }
        }

        // Add new flocks to main flock list
        flocks.AddRange(newFlocks);
    }


    public Flock GetFlockFromAgent(FlockingAgent agent)
    {
        foreach (Flock flock in flocks)
        {
            if (flock.agents.Contains(agent))
            {
                return flock;
            }
        }

        return null;
    }

    public Vector3 CalculateFlockingBehaviors(FlockingAgent agent, Flock agentsFlock)
    {

        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 waypointDirection = Vector3.zero;
        int neighborCount = 0;

        foreach (var otherAgent in agentsFlock.agents)
        {
            if (otherAgent != agent && Vector3.Distance(agent.transform.position, otherAgent.transform.position) <= neighborRadius)
            {
                cohesion += otherAgent.transform.position;
                separation += (agent.transform.position - otherAgent.transform.position);
                alignment += otherAgent.GetComponent<NavMeshAgent>().velocity;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            cohesion = (cohesion / neighborCount - agent.transform.position).normalized;
            separation = (separation / neighborCount).normalized;
            alignment = (alignment / neighborCount).normalized;
        }

        if (agent.waypointIndex < agent.waypointList.Count)
        {
            waypointDirection = (agent.waypointList[agent.waypointIndex].position - agent.transform.position).normalized;
        }

        // Combine behaviors
        Vector3 combinedDirection = cohesion * cohesionWeight + separation * separationWeight + alignment * alignmentWeight + waypointDirection * waypointWeight;
        return combinedDirection.normalized;
    }

    private void OnDrawGizmos()
    {
        // Draw a gizmo for each flock to visualize its center
        foreach (var flock in flocks)
        {
            Gizmos.color = Color.red; // You can choose any color
            Gizmos.DrawWireSphere(flock.flockCenter, mergeRadius); // Adjust the size of the sphere as needed
            Gizmos.color = Color.blue;
            if(flocks != null && flock.agents.Count > 0)
            {
                Gizmos.DrawWireSphere(flock.flockCenter, flock.agents[0].minFlockingDistance);

            }
        }
    }


}
