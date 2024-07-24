using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlatformerAgent : Agent
{
    public Transform goal;  // Reference to the Goal (Victory) GameObject
    public float speed = 10f;  // Speed at which the agent moves
    private Rigidbody2D rb;  // Reference to the Rigidbody2D component for physics

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component attached to the agent
    }

    // Called when a new episode begins
    public override void OnEpisodeBegin()
    {
        // Reset the agent's position and velocity
        transform.localPosition = new Vector3(0, 1, 0);
        rb.velocity = Vector2.zero;

        // Randomize the goal's position within a specified range
        goal.localPosition = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
    }

    // Collect observations about the environment
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the agent's position to the observations
        sensor.AddObservation(transform.localPosition);
        // Add the goal's position to the observations
        sensor.AddObservation(goal.localPosition);
        // Add the agent's velocity to the observations
        sensor.AddObservation(rb.velocity);
    }

    // Called when the agent receives an action
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Get the continuous actions for movement
        float moveX = actionBuffers.ContinuousActions[0];
        float moveY = actionBuffers.ContinuousActions[1];

        // Apply a force to the Rigidbody2D based on the actions
        rb.AddForce(new Vector2(moveX, moveY) * speed);

        // Calculate the distance to the goal
        float distanceToGoal = Vector3.Distance(transform.localPosition, goal.localPosition);

        // Reward the agent for reaching the goal
        if (distanceToGoal < 1.5f)
        {
            SetReward(1.0f);
            Debug.Log("Reached the goal. Reward: 1.0");
            EndEpisode();
        }

        // Penalize the agent for falling off platforms
        if (transform.localPosition.y < -1)
        {
            SetReward(-1.0f);
            Debug.Log("Fell off platform. Penalty: -1.0");
            EndEpisode();
        }

        // Log the actions and rewards
        Debug.Log($"Actions: ({moveX}, {moveY}), Distance to Goal: {distanceToGoal}");
    }

    // Provide manual control for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get the continuous actions array
        var continuousActionsOut = actionsOut.ContinuousActions;
        // Map the horizontal input to the first action
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        // Map the vertical input to the second action
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}