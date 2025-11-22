using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class WaypointsManager : MonoBehaviour
{
    public List<Transform> globalWaypoints = new List<Transform>();

    public List<FlockPath> flockPaths = new List<FlockPath>();

    [System.Serializable]
    public class FlockPath
    {
        public List<WaypointList> pathWaypoints = new List<WaypointList>();

        [System.Serializable]
        public class WaypointList
        {
            public List<Transform> waypoints = new List<Transform>();
        }
    }

    public List<Transform> RetrieveGlobalWaypoints()
    {
        int splitIndex = globalWaypoints.FindLastIndex(wp => wp.name.ToLower().Contains("split"));
        if (splitIndex != -1)
        {
            // Take waypoints from just after the split to the end of the list
            List<Transform> waypointsAfterSplit = globalWaypoints.Skip(splitIndex + 1).ToList();

            // Take waypoints from the start of the list up to the split
            List<Transform> waypointsBeforeSplit = globalWaypoints.Take(splitIndex + 1).ToList();

            // Concatenate both lists to ensure wrapping around
            List<Transform> nextSetOfWaypoints = waypointsAfterSplit.Concat(waypointsBeforeSplit).ToList();

            return nextSetOfWaypoints;
        }
        else
        {
            Debug.LogWarning("No split waypoints found in the global waypoints list.");
            return globalWaypoints;
        }
    }

    public List<Transform> RetrievePathWaypoints(int pathIndex, FlockingManager.Flock flock)
    {
        List<Transform> waypoints = new List<Transform>();

        // Retrieve the selected path
        FlockPath selectedPath = flockPaths[pathIndex];

        // Only assign once per flock
        if (!flock.waypointsAssigned)
        {
            flock.totalAgents = flock.agents.Count;
            flock.totalPaths = selectedPath.pathWaypoints.Count;
            flock.remainingAgents = flock.totalAgents % flock.totalPaths;

            flock.agentsPerPath = flock.totalAgents / flock.totalPaths; // Calculate the initial number of agents per path

            flock.waypointsAssigned = true; // Mark waypoints as assigned

            // Initialize the total agents processed counter
            flock.totalAgentsProcessed = 0;
        }

        // Assign waypoints to the current agent from the current path
        waypoints = selectedPath.pathWaypoints[flock.PathsProcessed].waypoints;

        // Check if this is the last remaining agent and there are remaining agents
        if (flock.totalAgentsProcessed == flock.totalAgents - 1 && flock.remainingAgents > 0)
        {
            // Randomly select a path for the last agent
            int randomPathIndex = UnityEngine.Random.Range(0, selectedPath.pathWaypoints.Count);
            waypoints = selectedPath.pathWaypoints[randomPathIndex].waypoints;

            // Reset total agents processed counter for the next round of assignments
            flock.totalAgentsProcessed = 0;
        }
        else
        {
            // Increment the total number of processed agents
            flock.totalAgentsProcessed++;
        }

        // Increment the number of processed agents for the current path
        flock.agentsProcessed++;

        // Check if all agents for the current path have been processed
        if (flock.agentsProcessed >= flock.agentsPerPath)
        {
            // Reset the agents processed count for the current path
            flock.agentsProcessed = 0;

            // Move to the next path
            flock.PathsProcessed++;

            // Reset the path index if it exceeds the total number of paths
            if (flock.PathsProcessed >= flock.totalPaths)
            {
                flock.PathsProcessed = 0;
            }
        }

        return waypoints;
    }


    public List<Transform> InitializeWaypoints()
    {
        // Retrieve the initial set of waypoints for agents
        List<Transform> initialWaypoints = globalWaypoints.TakeWhile(wp => !wp.name.ToLower().Contains("split"))
            .Concat(new[] { globalWaypoints.FirstOrDefault(wp => wp.name.ToLower().Contains("split")) })
            .ToList();

        return initialWaypoints;
    }
}
