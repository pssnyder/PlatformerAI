using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlatformerAgent : Agent
{
    public Transform goal;  // Reference to the Goal (Victory) GameObject
    public float speed = 10f;  // Speed at which the agent moves
    public float jumpForce = 5f;  // Force applied for jumping
    private Rigidbody2D rb;  // Reference to the Rigidbody2D component for physics
    private Vector3 lastPosition;  // To track the agent's last position
    private int idleSteps;  // To track the number of idle steps

    public PlatformerAgent()
    {
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component attached to the agent
        lastPosition = transform.localPosition;
        idleSteps = 0;
    }

    // Called when a new episode begins
    public override void OnEpisodeBegin()
    {
        // Reset the agent's position and velocity
        transform.localPosition = new Vector3(0, 1, 0);
        rb.velocity = Vector2.zero;

        // Randomize the goal's position within a specified range
        goal.localPosition = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));

        // Reset tracking variables
        lastPosition = transform.localPosition;
        idleSteps = 0;
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
        bool jump = actionBuffers.DiscreteActions[0] == 1;

        // Apply horizontal movement
        rb.AddForce(new Vector2(moveX, 0) * speed);

        // Apply jump force if jump action is received
        if (jump && Mathf.Abs(rb.velocity.y) < 0.001f)  // Ensure the agent can only jump when on the ground
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }

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

        // Penalize the agent for moving backwards
        if (transform.localPosition.x < lastPosition.x)
        {
            SetReward(-0.1f);
            Debug.Log("Moved backwards. Penalty: -0.1");
        }

        // Reward the agent for making forward progress
        if (transform.localPosition.x > lastPosition.x)
        {
            SetReward(0.1f);
            Debug.Log("Made forward progress. Reward: 0.1");
        }

        // Penalize the agent for not moving forward in 3 moves
        if (transform.localPosition.x == lastPosition.x)
        {
            idleSteps++;
            if (idleSteps >= 3)
            {
                SetReward(-0.2f);
                Debug.Log("Idle for 3 moves. Penalty: -0.2");
                idleSteps = 0;  // Reset idle steps
            }
        }
        else
        {
            idleSteps = 0;  // Reset idle steps if the agent moves
        }

        // Update the last position
        lastPosition = transform.localPosition;
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

        // Get the discrete actions array
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Map the space bar input to the jump action
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    // Handle collision with enemies and tokens
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Penalize for dying by running into an enemy from the side
            SetReward(-1.0f);
            Debug.Log("Died by enemy. Penalty: -1.0");
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Token"))
        {
            // Reward for collecting a token
            SetReward(0.5f);
            Debug.Log("Collected a token. Reward: 0.5");
            Destroy(collision.gameObject);  // Remove the token
        }
    }

    // Handle killing an enemy
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Reward for killing an enemy
            SetReward(0.5f);
            Debug.Log("Killed an enemy. Reward: 0.5");
            Destroy(other.gameObject);  // Remove the enemy
        }
    }
}