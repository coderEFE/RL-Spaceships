using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public enum Team
{
    Blue = 0,
    Orange = 1
}

public class SpaceshipAgent : Agent
{
    //[SerializeField] private SpaceshipEnvController envController;
    [SerializeField] private GameObject laser;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float laserLength = 15f;
    [SerializeField] private float shootCooldown = 1f;

    public Team team;
    private Rigidbody2D agentRb;
    private bool isShooting = false; // TODO: might not need this?
    private float lastShootTime = 0f;
    private int currentEpisode = 0;
    private float cumulativeReward = 0f;

    public override void Initialize()
    {
        Debug.Log("Initialize");

        agentRb = GetComponent<Rigidbody2D>();
        currentEpisode = 0;
        cumulativeReward = 0f;
        isShooting = false;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin");

        currentEpisode++;
        cumulativeReward = 0f;
        isShooting = false;
        laser.transform.localScale = new Vector3(0f, 0f, 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position
        float agentPosX_normalized = transform.localPosition.x / 20f;
        float agentPosY_normalized = transform.localPosition.y / 20f;

        // Agent's rotation
        float agentRot_normalized = (transform.localRotation.eulerAngles.z / 360f) * 2f - 1f;

        // Agent's velocity // TODO: Only add this later if I decide to use rigidbody forces instead of directly changing position
        //var agentVelocity_normalized = transform.InverseTransformDirection(agentRb.linearVelocity).normalized;

        sensor.AddObservation(agentPosX_normalized);
        sensor.AddObservation(agentPosY_normalized);
        sensor.AddObservation(agentRot_normalized);
        //sensor.AddObservation(agentVelocity_normalized.x);
        //sensor.AddObservation(agentVelocity_normalized.y);
        //sensor.AddObservation(isShooting); // should this be converted to 1 or 0 instead?
        // time remaining to shoot = 1 if just shot, 0 if can shoot now
        var timeRemainingToShoot = Mathf.Clamp01((lastShootTime + shootCooldown - Time.time) / shootCooldown);
        sensor.AddObservation(timeRemainingToShoot);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }
        // Turn
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 1; // turn left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2; // turn right
        }
        else
        {
            discreteActionsOut[1] = 0; // don't turn
        }
        // Shoot
        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[2] = 1;
        }
        else
        {
            discreteActionsOut[2] = 0;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions);

        //AddReward(-2f / MaxStep);
        //cumulativeReward = GetCumulativeReward();
    }

    public void MoveAgent(ActionBuffers actions)
    {
        // 3 different discrete actions: move forward or not, turn left or turn right or don't turn, shoot or not
        var discreteActions = actions.DiscreteActions;

        // Move forward if first action is 1, otherwise don't move
        if (discreteActions[0] == 1)
        {
            //transform.position += transform.up * moveSpeed * Time.deltaTime;
            //agentRb.AddForce(transform.up * moveSpeed, ForceMode2D.Force);
            agentRb.MovePosition(agentRb.position + (Vector2)(transform.up * moveSpeed * Time.deltaTime));
        }
        // Turn left if second action is 1, turn right if second action is 2, otherwise don't turn
        if (discreteActions[1] == 1)
        {
            transform.Rotate(0f, 0f, turnSpeed * Time.deltaTime);
        } else if (discreteActions[1] == 2)
        {
            transform.Rotate(0f, 0f, -turnSpeed * Time.deltaTime);
        }
        // Shoot if third action is 1, otherwise don't shoot
        if (discreteActions[2] == 1 && Time.time >= lastShootTime + shootCooldown)
        {
            isShooting = true;
            lastShootTime = Time.time;
            laser.transform.localScale = new Vector3(0.2f, laserLength, 0.2f);
            var rayDir = laserLength * transform.up;
            //Debug.DrawRay(transform.position, rayDir, Color.red, 0.5f, true);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, laserLength);
            if (hit.collider != null)
            {
                // Shoot asteroid
                if (hit.collider.CompareTag("asteroid"))
                {
                    hit.collider.gameObject.GetComponent<Asteroid>().OnHit();
                }
            }
        } else
        {
            isShooting = false;
            laser.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("wall"))
        {
            AddReward(-0.05f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("wall"))
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("resource"))
        {
            collider.gameObject.GetComponent<Resource>().OnCollected(team);
        }
    }
}
