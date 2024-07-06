using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum AgentState
{
    Idle,
    FollowingWaypoints,
    Fighting
}

public class FlockingAgentV2 : MonoBehaviour
{
    // Public variables for agent parameters
    public int waypointIndex = 0;
    public int splitWaypointIndex = 0;
    public float stuckTimerDuration = 5f;
    public float minFlockingDistance = 10f;
    public float baseSpeed = 3.5f;
    public float maxSpeed = 7f;

    // State machine variables
    private AgentState currentState;
    private NavMeshAgent navMeshAgent;
    private WaypointsManager waypointsManager;
    private FlockingManagerV2 flockingManager;

    // Timer variables
    private Coroutine stuckTimerCoroutine;
    private bool isInsideCollider;

    // Should not be seen
    public List<Transform> waypointList = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        // Get references to required components
        navMeshAgent = GetComponent<NavMeshAgent>();
        waypointsManager = FindObjectOfType<WaypointsManager>();
        flockingManager = FindObjectOfType<FlockingManagerV2>();

        List<FlockingAgentV2> agentsList = new List<FlockingAgentV2> { this };
        flockingManager.CreateNewFlock(agentsList);
        currentState = AgentState.FollowingWaypoints;
        waypointList = waypointsManager.InitializeWaypoints();
    }

    // Update is called once per frame
    void Update()
    {
        // State machine logic
        switch (currentState)
        {
            case AgentState.Idle:
                // Handle idle state
                break;
            case AgentState.FollowingWaypoints:
                FollowWaypoints();
                break;
            case AgentState.Fighting:
                // Handle fighting state
                break;
        }

        CheckWaypointIndices();
    }

    // Method to change the state
    public void ChangeState(AgentState newState)
    {
        currentState = newState;
    }

    private void FollowWaypoints()
    {
        // Check if there are waypoints left to follow
        if (waypointIndex < waypointList.Count)
        {
            FlockingManagerV2.FlockV2 agentsFlock = flockingManager.GetFlockFromAgent(this);

            // Calculate flocking behaviors
            Vector3 combinedDirection = flockingManager.CalculateFlockingBehaviors(this, agentsFlock);

            // Calculate distance to center of flock
            Vector3 centerOfFlock = agentsFlock.flockCenter;
            float distanceToCenter = Vector3.Distance(transform.position, centerOfFlock);

            // Determine if agent is ahead or behind the center
            float directionFactor = Vector3.Dot(transform.forward, (centerOfFlock - transform.position).normalized);

            // Adjust speed based on position relative to the center
            float targetSpeed;
            if (distanceToCenter > minFlockingDistance)
            {
                if (directionFactor > 0) // Agent is ahead of the center
                {
                    targetSpeed = baseSpeed;
                }
                else // Agent is behind the center
                {
                    AdjustSpeedForEntireFlock(agentsFlock, baseSpeed);
                    targetSpeed = maxSpeed;
                }
            }
            else
            {
                targetSpeed = baseSpeed; // Agents within the circle maintain consistent speed
            }

            // Apply target speed to NavMeshAgent
            navMeshAgent.speed = targetSpeed;

            // Set the destination to the combined direction
            navMeshAgent.SetDestination(transform.position + combinedDirection);
        }
    }

    private void AdjustSpeedForEntireFlock(FlockingManagerV2.FlockV2 flock, float newSpeed)
    {
        foreach (FlockingAgentV2 agent in flock.agents)
        {
            agent.navMeshAgent.speed = newSpeed;
        }
    }

    void CheckWaypointIndices()
    {
        if (waypointIndex >= waypointList.Count)
        {
            waypointIndex = waypointList.Count - 1;
        }

        if (splitWaypointIndex >= waypointsManager.flockPaths.Count)
        {
            splitWaypointIndex = waypointsManager.flockPaths.Count - 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Start the timer when the agent enters the collider
        isInsideCollider = true;

        // Check if the triggered object is a waypoint
        if (other.CompareTag("Waypoint"))
        {
            waypointIndex++;
            // Split waypoint logic
            Debug.Log("Waypoint Triggered!");
            // Perform waypoint splitting or any other related actions
        }
        else if (other.CompareTag("SplitWaypoint"))
        {
            waypointIndex = 0;

            if (splitWaypointIndex >= waypointsManager.flockPaths.Count)
            {
                splitWaypointIndex = 0; // Future Note: This should be needed when AI is only pushing forward to obj.
            }
            waypointList = waypointsManager.RetrievePathWaypoints(splitWaypointIndex, flockingManager.GetFlockFromAgent(this));
            splitWaypointIndex++;
        }
        else if (other.CompareTag("LastWaypoint"))
        {
            // Handle last waypoint logic;
            waypointIndex = 0;
            waypointList = waypointsManager.RetrieveGlobalWaypoints();
        }

        if (isInsideCollider && stuckTimerCoroutine == null)
        {
            stuckTimerCoroutine = StartCoroutine(StuckTimer(other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Stop the timer and reset it when the agent exits the collider
        isInsideCollider = false;

        if (stuckTimerCoroutine != null)
        {
            StopCoroutine(stuckTimerCoroutine);
            stuckTimerCoroutine = null;
        }
    }

    IEnumerator StuckTimer(Collider other)
    {
        yield return new WaitForSecondsRealtime(stuckTimerDuration);

        // Decrease waypointIndex if the timer expires
        if (other.CompareTag("Waypoint"))
        {
            waypointIndex++;
            // Split waypoint logic
            Debug.Log("Waypoint Triggered!");
            // Perform waypoint splitting or any other related actions
        }
        else if (other.CompareTag("SplitWaypoint"))
        {
            waypointIndex = 0;

            if (splitWaypointIndex >= waypointsManager.flockPaths.Count)
            {
                splitWaypointIndex = 0; // Future Note: This should be needed when AI is only pushing forward to obj.
            }

            waypointList = waypointsManager.RetrievePathWaypoints(splitWaypointIndex, flockingManager.GetFlockFromAgent(this));
            splitWaypointIndex++;
        }
        else if (other.CompareTag("LastWaypoint"))
        {
            // Handle last waypoint logic;
            waypointIndex = 0;
            waypointList = waypointsManager.RetrieveGlobalWaypoints();
        }
    }
}
