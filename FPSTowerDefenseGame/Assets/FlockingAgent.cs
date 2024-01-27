using UnityEngine;
using UnityEngine.AI;

public class FlockingAgent : MonoBehaviour
{
    private FlockingManager manager;
    private NavMeshAgent agent;

    public float maxSpeed = 5.0f;
    public float currentSpeed;
    public float headingAdjustmentSpeed = 2.0f;
    private Vector3 desiredVelocity;

    void Start()
    {
        manager = GameObject.FindWithTag("FlockingManager").GetComponent<FlockingManager>();
        manager.RegisterAgent(this);
        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = 0.1f;
        currentSpeed = maxSpeed;

        // Set the NavMeshAgent speed to a high value initially to avoid slowing down due to NavMeshAgent's acceleration
        agent.speed = 1000.0f;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        LimitSpeed(); // Ensure the speed doesn't exceed the maximum allowed speed
    }

    public void AdjustHeading(Vector3 flockingDirection)
    {
        // Normalize the desired flocking direction
        Vector3 desiredDirection = flockingDirection.normalized;

        // Calculate the desired velocity using currentSpeed instead of maxSpeed
        desiredVelocity = desiredDirection * currentSpeed;

        // Directly set the agent's velocity
        agent.velocity = desiredVelocity;

        // Adjust the agent's heading only if it is moving
        if (agent.velocity.magnitude > 0.1f)
        {
            // Gradually adjust the heading
            Vector3 newDirection = Vector3.Slerp(transform.forward, agent.velocity.normalized, headingAdjustmentSpeed * Time.deltaTime);

            // Calculate the new rotation based on the new direction
            Quaternion newRotation = Quaternion.LookRotation(newDirection, Vector3.up);
            transform.rotation = newRotation;
        }
    }

    public void LimitSpeed()
    {
        // Limit the agent's speed
        if (agent.velocity.magnitude > maxSpeed)
        {
            agent.velocity = agent.velocity.normalized * maxSpeed;
        }
    }

    public void MoveTowardsWaypoint(Vector3 targetPosition)
    {
        // Set the destination if the agent is not close enough
        agent.SetDestination(targetPosition);

    }


    void Update()
    {
        // Perform the actual movement towards the destination
        agent.velocity = desiredVelocity;

        // Limit the agent's speed
        LimitSpeed();

        // Update the current speed
        currentSpeed = agent.velocity.magnitude;

        // Adjust the agent's heading only if it is moving
        if (agent.velocity.magnitude > 0.1f)
        {
            // Gradually adjust the heading
            Vector3 newDirection = Vector3.Slerp(transform.forward, agent.velocity.normalized, headingAdjustmentSpeed * Time.deltaTime);

            // Calculate the new rotation based on the new direction
            Quaternion newRotation = Quaternion.LookRotation(newDirection, Vector3.up);
            transform.rotation = newRotation;
        }
    }
}