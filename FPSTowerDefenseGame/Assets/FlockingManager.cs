using FlockingSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlockingSystem
{
    [System.Serializable]
    public class Flock
    {
        public List<FlockingAgent> agents = new List<FlockingAgent>();
        public List<Transform> waypoints = new List<Transform>();

        public bool reachedSplitWaypoint = false;
        public bool reachedLastWaypoint = false;
        public bool hasBeenOnPath = false;

        public int SplitWaypointIndex { get; set; }
        public FlockPath CurrentFlockPath { get; set; }

        public int waypointIndex = 0; // Make it private to control access

        public Transform GetCurrentWaypoint()
        {
            if (waypoints != null && waypoints.Count > 0)
            {
                // Ensure waypointIndex is within the bounds of the list
                return waypoints[waypointIndex];
            }

            // Handle the case where the waypoints list is null or empty
            return null; // or another default value depending on your needs
        }

        // Add this method to increment the waypoint index
        public void MoveToNextWaypoint()
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Count;
        }
    }
}


[System.Serializable]
public class FlockPath
{

    public Transform splitWaypoint;
    //public List<Transform> pathWaypoints = new List<Transform>();
    public List<WaypointList> pathWaypoints = new List<WaypointList>();

    [System.Serializable]
    public class WaypointList
    {
        public List<Transform> waypoints = new List<Transform>();
    }
}

public class FlockingManager : MonoBehaviour
{
    public float speedFactor = 1.0f; // Add this variable
    public float minFlockingDistance = 5.0f; // Adjust this as needed
    public float maxFlockingDistance = 15.0f; // Adjust this as needed
    public float speedLerpFactor = 2.0f; // Adjust this as needed

    public float neighborRadius = 2.0f;
    public float flockingRadius = 2.0f;

    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float separationWeight = 1.0f;

    public float flockingWeight = 1.0f;

    public float flockMergeDistance = 2.0f;

    public List<FlockingAgent> agents = new List<FlockingAgent>();
    public List<Flock> flocks = new List<Flock>();

    public List<Transform> globalWaypoints = new List<Transform>();
    public List<FlockPath> flockPaths = new List<FlockPath>();

    void Start()
    {
        if (flocks.Count > 0)
        {
            InitializeFlocks();
        }
        else
        {
            Debug.LogWarning("Flocks list is empty.");
        }
    }


    void Update()
    {
        List<FlockingAgent> agentsToRemove = new List<FlockingAgent>();

        foreach (FlockingAgent agent in agents)
        {
            Flock nearestFlock = FindNearestFlock(agent.transform.position);
            float distanceToCenter = 0f;

            if (nearestFlock != null)
            {
                // Check the distance to the nearest flock's center
                distanceToCenter = Vector3.Distance(agent.transform.position, CalculateFlockCenter(nearestFlock));

                if (distanceToCenter > maxFlockingDistance)
                {
                    // Create a new flock and remove the agent from the previous flock
                    CreateNewFlock(agent);
                    nearestFlock.agents.Remove(agent);
                    continue; // Skip the rest of the logic for this agent if a new flock is created
                }

                // Add the agent to the nearest flock if not already a member
                if (!nearestFlock.agents.Contains(agent))
                {
                    nearestFlock.agents.Add(agent);
                }
            }
            else
            {
                // Handle the case when there is no nearest flock
                CreateNewFlock(agent);
                continue; // Skip the rest of the logic for this agent if a new flock is created
            }

            // Now, you can use distanceToCenter in the rest of your logic.

            if (distanceToCenter > flockingRadius)
            {
                // Create a new flock and remove the agent from the previous flock
                CreateNewFlock(agent);
                nearestFlock.agents.Remove(agent);
            }
        }

        // Remove agents after the iteration
        foreach (FlockingAgent agentToRemove in agentsToRemove)
        {
            agents.Remove(agentToRemove);
        }

        // First, update the positions of global waypoints if needed
        if (globalWaypoints.Count == 0)
            return;

        // Handle flock separation and merging
        HandleFlockSeparationAndMerging();

        flocks.RemoveAll(flock => flock.agents.Count == 0);


        foreach (Flock flock in flocks)
        {
            UpdateFlockingForFlock(flock);

            // Move to the next waypoint for all agents in the flock
            foreach (FlockingAgent agent in flock.agents)
            {
                Transform currentWaypoint = flock.GetCurrentWaypoint();
                Collider waypointCollider = currentWaypoint.GetComponent<Collider>();

                // Check if the agent has entered the collider of the current waypoint
                if (waypointCollider != null && waypointCollider.bounds.Contains(agent.transform.position))
                {
                    // Check if the current waypoint is the last one on the path
                    if (flock.waypointIndex == flock.waypoints.Count - 1 && !flock.reachedSplitWaypoint)
                    {
                        // The flock has reached the last waypoint on the path
                        flock.reachedSplitWaypoint = !flock.reachedSplitWaypoint;
                        UpdateWaypointsForFlock(flock);
                    }
                    else if (flock.waypointIndex == flock.waypoints.Count - 1 && !flock.reachedLastWaypoint)
                    {
                        flock.reachedLastWaypoint = true;
                        UpdateWaypointsForFlock(flock);
                    }
                    else
                    {
                        // Move to the next waypoint
                        flock.MoveToNextWaypoint();

                    }
                }
                else
                {
                    UpdateFlockingForFlock(flock);
                }
            }
        }

        foreach (Flock flock1 in flocks)
        {
            foreach (Flock flock2 in flocks)
            {
                if (flock1 == flock2)
                    continue;

                // Calculate the center of both flocks
                Vector3 center1 = CalculateFlockCenter(flock1);
                Vector3 center2 = CalculateFlockCenter(flock2);

                // Calculate the distance between the centers
                float distance = Vector3.Distance(center1, center2);

                // Define a merging threshold (you can adjust this)
                if (distance < flockingRadius)
                {
                    // Combine the agents of flock2 into flock1
                    flock1.agents.AddRange(flock2.agents);
                    flock2.agents.Clear();
                }
            }
        }

        // Remove any empty flocks
        flocks.RemoveAll(flock => flock.agents.Count == 0);
    }


    public void InitializeFlocks()
    {
        Debug.Log("InitializeFlocks called.");

        // Iterate through all flocks and assign waypoints
        foreach (Flock flock in flocks)
        {
            UpdateWaypointsForFlock(flock);
        }
    }


    public void RegisterAgent(FlockingAgent agent)
    {
        agents.Add(agent);
        // Find the nearest flock for the agent
    }


    Flock FindNearestFlock(Vector3 agentPosition)
    {
        Flock nearestFlock = null;
        float minDistance = float.MaxValue;

        foreach (Flock flock in flocks)
        {
            foreach (FlockingAgent flockAgent in flock.agents)
            {
                float distance = Vector3.Distance(agentPosition, flockAgent.transform.position);

                if (distance < flockingRadius && distance < minDistance)
                {
                    nearestFlock = flock;
                    minDistance = distance;
                }
            }
        }

        return nearestFlock;
    }

    void HandleFlockSeparationAndMerging()
    {
        for (int i = 0; i < flocks.Count; i++)
        {
            Flock flockA = flocks[i];

            // Check for separation and merging with other flocks
            for (int j = i + 1; j < flocks.Count; j++)
            {
                Flock flockB = flocks[j];
                float distance = Vector3.Distance(CalculateFlockCenter(flockA), CalculateFlockCenter(flockB));

                if (distance < flockMergeDistance)
                {
                    // Combine the agents of flockB into flockA
                    flockA.agents.AddRange(flockB.agents);

                    // Inherit waypoints from flockB to flockA
                    flockA.waypoints.Clear();
                    flockA.waypoints.AddRange(flockB.waypoints);

                    // Clear agents and waypoints from flockB
                    flockB.agents.Clear();
                    flockB.waypoints.Clear();
                }
            }

            // Remove agents that are too far from their flock's center
            List<FlockingAgent> separatedAgents = new List<FlockingAgent>();
            foreach (FlockingAgent agentA in flockA.agents)
            {
                if (Vector3.Distance(agentA.transform.position, CalculateFlockCenter(flockA)) > flockingRadius)
                {
                    separatedAgents.Add(agentA);
                }
            }

            // Create new flocks for separated agents using the CreateNewFlock method
            foreach (FlockingAgent separatedAgent in separatedAgents)
            {
                CreateNewFlock(separatedAgent);
                flockA.agents.Remove(separatedAgent);
            }
        }

        // Remove empty flocks
        flocks.RemoveAll(flock => flock.agents.Count == 0);
    }

    public void UpdateWaypointsForFlock(Flock flock)
    {
        // Clear the existing waypoints
        flock.waypoints.Clear();

        if (flock.reachedLastWaypoint)
        {
            // Find the index of the current split waypoint
            int currentSplitIndex = globalWaypoints.FindIndex(wp => wp.name.ToLower().Contains("split"));

            if (currentSplitIndex != -1)
            {
                // Get the waypoints between the current and next split, including the last split
                List<Transform> nextSetOfWaypoints = globalWaypoints.Skip(currentSplitIndex + 1)
                    .TakeWhile(wp => !wp.name.ToLower().Contains("split"))
                    .Concat(new[] { globalWaypoints[currentSplitIndex] }) // Include the last split
                    .ToList();

                // Add the next set of waypoints to the flock
                flock.waypoints.AddRange(nextSetOfWaypoints);

                // Reset necessary flags and indices
                flock.waypointIndex = 0;
                flock.reachedSplitWaypoint = false;
                flock.hasBeenOnPath = false;
                flock.reachedLastWaypoint = false;
            }
            else
            {
                // Handle the case where the current split waypoint is not found
                Debug.LogError("Split waypoint not found in globalWaypoints.");
            }
        }
        else if (flock.reachedSplitWaypoint)
        {
            // Choose a specific FlockPath instance from the list (replace 0 with the desired index)
            FlockPath selectedPath = flockPaths[0];
            int selectedPathIndex = 1;

            // Assign the chosen FlockPath to the CurrentFlockPath
            flock.CurrentFlockPath = selectedPath;

            // Add waypoints to the new flock without modifying the original path
            if(selectedPathIndex < selectedPath.pathWaypoints.Count)
            {
                var selectedWaypointList = selectedPath.pathWaypoints[selectedPathIndex];
                flock.waypoints.AddRange(selectedWaypointList.waypoints);
            }


            // Reset waypointIndex to 0 only if the flock has not been on the path before
            if (!flock.hasBeenOnPath)
            {
                flock.waypointIndex = 0;
                flock.hasBeenOnPath = true; // Set the flag to true once the flock has been on the path
            }
        }
        else if (flock.CurrentFlockPath != null)
        {
            // Use splitWaypointIndex to avoid always starting from the beginning
            int splitWaypointIndex = flock.CurrentFlockPath.pathWaypoints.FindIndex(wp => wp.waypoints.Any(w => w.name.ToLower().Contains("split")));

            if (splitWaypointIndex != -1)
            {
                flock.SplitWaypointIndex = splitWaypointIndex;

                // Add waypoints only if the flock's waypoints list is empty
                if (flock.waypoints.Count == 0)
                {
                    flock.waypoints.AddRange(flock.CurrentFlockPath.pathWaypoints[splitWaypointIndex].waypoints);
                    flock.waypoints.AddRange(flock.CurrentFlockPath.pathWaypoints[(splitWaypointIndex + 1) % flock.CurrentFlockPath.pathWaypoints.Count].waypoints);
                }
            }
        }
        else
        {
            // CurrentFlockPath is null, use global waypoints only if the flock's waypoints list is empty
            if (flock.waypoints.Count == 0)
            {
                flock.waypoints.AddRange(globalWaypoints.TakeWhile(wp => !wp.name.ToLower().Contains("split")).Concat(new[] { globalWaypoints.FirstOrDefault(wp => wp.name.ToLower().Contains("split")) }));
            }
        }
    }

    public void CreateNewFlock(FlockingAgent agent)
    {
        Flock newFlock = new Flock();
        newFlock.agents = new List<FlockingAgent>();
        newFlock.agents.Add(agent);

        // Find the original flock of the agent
        Flock originalFlock = FindOriginalFlock(agent);

        // If the original flock is found, inherit its waypoints and current waypoint index
        if (originalFlock != null)
        {
            newFlock.waypoints.AddRange(originalFlock.waypoints);
            newFlock.waypointIndex = originalFlock.waypointIndex;
            newFlock.reachedSplitWaypoint = originalFlock.reachedSplitWaypoint;
            newFlock.reachedLastWaypoint = originalFlock.reachedLastWaypoint;
            newFlock.hasBeenOnPath = originalFlock.hasBeenOnPath;
            newFlock.CurrentFlockPath = originalFlock.CurrentFlockPath;
            newFlock.SplitWaypointIndex = originalFlock.SplitWaypointIndex;
        }

        flocks.Add(newFlock);

        // Update waypoints for the new flock
        UpdateWaypointsForFlock(newFlock);
    }
    Flock FindOriginalFlock(FlockingAgent agent)
    {
        // Iterate through all flocks to find the original flock of the agent
        foreach (Flock flock in flocks)
        {
            if (flock.agents.Contains(agent))
            {
                return flock;
            }
        }
        return null; // Return null if the original flock is not found
    }


    void UpdateFlockingForFlock(Flock flock)
    {
        if (flock.agents.Count > 0)
        {
            Transform targetWaypoint = flock.GetCurrentWaypoint();
            UpdateFlocking(flock.agents, targetWaypoint, flock);

            // Call the method to adjust speed for the furthest agents
            AdjustSpeedForFurthestAgents(flock);
        }
    }

    void UpdateFlocking(List<FlockingAgent> agents, Transform targetWaypoint, Flock flock)
    {
        if (agents == null || targetWaypoint == null)
        {
            // Handle null references or log an error
            Debug.LogError("Null reference in UpdateFlocking");
            return;
        }

        // Calculate average position of agents
        Vector3 avgPosition = Vector3.zero;
        foreach (FlockingAgent agent in agents)
        {
            avgPosition += agent.transform.position;
        }
        avgPosition /= agents.Count;

        foreach (FlockingAgent agent in agents)
        {
            if (agent == null)
            {
                // Handle null agent reference or log an error
                Debug.LogError("Null agent reference in UpdateFlocking");
                continue;
            }

            Vector3 alignment = CalculateAlignment(agent, agents);
            Vector3 cohesion = CalculateCohesion(agent, agents);
            Vector3 separation = CalculateSeparation(agent, agents);

            // Combine flocking behaviors
            Vector3 flockingDirection = alignment * alignmentWeight +
                                       cohesion * cohesionWeight +
                                       separation * separationWeight;

            // Separate waypoint influence
            Vector3 waypointDirection = CalculateWaypointDirection(agent.transform.position, targetWaypoint.position);

            // Calculate a target position that is closer to the average position of the flock
            Vector3 targetPosition = Vector3.Lerp(agent.transform.position, avgPosition, 0.5f);

            // Adjust speed based on distance to target position
            float targetSpeed = Mathf.Lerp(agent.currentSpeed, agent.maxSpeed * speedFactor, Time.deltaTime * speedLerpFactor);

            // Apply separate weights to flocking and waypoint directions
            Vector3 combinedDirection = flockingDirection * flockingWeight + waypointDirection;

            // Only adjust speed if the agent's speed is less than the target speed
            if (agent.GetCurrentSpeed() < targetSpeed)
            {
                agent.SetSpeed(targetSpeed);
            }

            agent.AdjustHeading(combinedDirection);
        }
    }

    Vector3 CalculateWaypointDirection(Vector3 agentPosition, Vector3 targetWaypointPosition)
    {
        // Calculate the direction from the agent to the waypoint
        return (targetWaypointPosition - agentPosition).normalized;
    }

    void AdjustSpeedForFurthestAgents(Flock flock)
    {
        // Adjust the speed of agents that are within the minFlockingDistance
        foreach (FlockingAgent agent in flock.agents)
        {
            float distanceToCenter = Vector3.Distance(agent.transform.position, CalculateFlockCenter(flock));

            if (distanceToCenter < minFlockingDistance)
            {
                float adjustedSpeed = agent.maxSpeed * speedFactor; // Use maxSpeed here
                agent.SetSpeed(adjustedSpeed);
            }
        }
    }


    Vector3 CalculateFlockCenter(Flock flock)
    {
        Vector3 center = Vector3.zero;

        if (flock != null && flock.agents.Count > 0)
        {
            foreach (FlockingAgent agent in flock.agents)
            {
                center += agent.transform.position;
            }
            center /= flock.agents.Count;
        }

        return center;
    }

    Vector3 CalculateAlignment(FlockingAgent agent, List<FlockingAgent> agents)
    {
        Vector3 avgHeading = Vector3.zero;
        int count = 0;

        foreach (FlockingAgent otherAgent in agents)
        {
            if (otherAgent == agent)
                continue;

            float distance = Vector3.Distance(agent.transform.position, otherAgent.transform.position);

            if (distance < neighborRadius)
            {
                avgHeading += otherAgent.transform.forward;
                count++;
            }
        }

        if (count > 0)
        {
            avgHeading /= count;
            avgHeading.Normalize();
        }

        return avgHeading;
    }

    Vector3 CalculateCohesion(FlockingAgent agent, List<FlockingAgent> agents)
    {
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;

        foreach (FlockingAgent otherAgent in agents)
        {
            float distance = Vector3.Distance(agent.transform.position, otherAgent.transform.position);

            if (distance < neighborRadius)
            {
                centerOfMass += otherAgent.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            centerOfMass /= count;
            return centerOfMass - agent.transform.position;
        }

        return Vector3.zero;
    }

    Vector3 CalculateSeparation(FlockingAgent agent, List<FlockingAgent> agents)
    {
        Vector3 separation = Vector3.zero;

        foreach (FlockingAgent otherAgent in agents)
        {
            if (otherAgent == agent)
                continue;

            float distance = Vector3.Distance(agent.transform.position, otherAgent.transform.position);

            if (distance < neighborRadius)
            {
                Vector3 toOther = agent.transform.position - otherAgent.transform.position;
                separation += toOther.normalized / distance;
            }
        }

        return separation;
    }


    void OnDrawGizmos()
    {
        foreach (Flock flock in flocks)
        {
            Vector3 center = CalculateFlockCenter(flock);

            // Visualize min flocking distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(center, minFlockingDistance);

            // Visualize max flocking distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, flockingRadius);

            // Visualize separation radius around agents
            foreach (FlockingAgent agent in flock.agents)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(agent.transform.position, separationWeight);
            }
        }
    }
}