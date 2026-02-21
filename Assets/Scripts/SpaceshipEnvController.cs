using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class SpaceshipEnvController : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    private List<GameObject> asteroids = new List<GameObject>();
    private int numAsteroids = 5;
    
    [SerializeField] private GameObject resourcePrefab;
    private List<GameObject> resources = new List<GameObject>();

    public List<SpaceshipAgent> agentsList = new List<SpaceshipAgent>();
    private SimpleMultiAgentGroup blueAgentGroup;
    private SimpleMultiAgentGroup orangeAgentGroup;
    int numberBlueAgentsRemaining;
    int numberOrangeAgentsRemaining;
    float sumOfAgentDeathTimes = 0f;
    //float mockBlueGroupReward = 0f;
    
    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps;
    private int resetTimer;
    
    void Start()
    {
        blueAgentGroup = new SimpleMultiAgentGroup();
        orangeAgentGroup = new SimpleMultiAgentGroup();
        ResetScene(true);
    }

    private void ResetScene(bool firstTime)
    {
        resetTimer = 0;
        //mockBlueGroupReward = 0f;

        if (!firstTime) {
            // Record custom stats
            Academy.Instance.StatsRecorder.Add("Custom/NumAsteroidsLeft", asteroids.Count);
            Academy.Instance.StatsRecorder.Add("Custom/NumBlueAlive", numberBlueAgentsRemaining);
            Academy.Instance.StatsRecorder.Add("Custom/NumOrangeAlive", numberOrangeAgentsRemaining);
            Academy.Instance.StatsRecorder.Add("Custom/NumTotalAlive", (numberBlueAgentsRemaining + numberOrangeAgentsRemaining));
            Academy.Instance.StatsRecorder.Add("Custom/AverageAgentDeathTime", sumOfAgentDeathTimes / (numberBlueAgentsRemaining + numberOrangeAgentsRemaining));
        }

        // Clustered randomization of teams
        Vector2 blueTeamCenter = new Vector2(Random.Range(-16f, 16f), Random.Range(-16f, 16f));
        Vector2 orangeTeamCenter;
        do {
            orangeTeamCenter = new Vector2(Random.Range(-16f, 16f), Random.Range(-16f, 16f));
        } while (Vector2.Distance(blueTeamCenter, orangeTeamCenter) < 10f);

        // Reset agents
        foreach (var agent in agentsList)
        {
            //agent.transform.localPosition = new Vector3(Random.Range(-19f, 19f), Random.Range(-19f, 19f), 0f);
            agent.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            agent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            agent.gameObject.SetActive(true);

            if (agent.team == Team.Blue) {
                agent.transform.localPosition = (Vector3)(blueTeamCenter + (3f * Random.insideUnitCircle));
                blueAgentGroup.RegisterAgent(agent);
            }
            else
            {
                agent.transform.localPosition = (Vector3)(orangeTeamCenter + (3f * Random.insideUnitCircle));
                orangeAgentGroup.RegisterAgent(agent);
            }
        }
        numberBlueAgentsRemaining = blueAgentGroup.GetRegisteredAgents().Count;
        numberOrangeAgentsRemaining = orangeAgentGroup.GetRegisteredAgents().Count;
        sumOfAgentDeathTimes = 0f;

        // Destroy existing asteroids
        foreach (var asteroid in asteroids)
        {
            Destroy(asteroid);
        }
        asteroids.Clear();
        
        // Destroy existing resources
        foreach (var resource in resources)
        {
            Destroy(resource);
        }
        resources.Clear();
        
        // Spawn new asteroids
        for (int i = 0; i < numAsteroids; i++)
        {
            Vector3 asteroidPosition = transform.position + new Vector3(Random.Range(-18f, 18f), Random.Range(-18f, 18f), 0f);
            GameObject asteroid = Instantiate(asteroidPrefab, asteroidPosition, Quaternion.identity, transform);
            asteroids.Add(asteroid);
        }
    }

    void OnEnable()
    {
        Asteroid.OnAsteroidDestroyed += HandleAsteroidDestroyed;
        Resource.OnResourceCollected += HandleResourceCollected;
        SpaceshipAgent.OnSpaceshipDestroyed += HandleSpaceshipDestroyed;
    }

    void OnDisable()
    {
        Asteroid.OnAsteroidDestroyed -= HandleAsteroidDestroyed;
        Resource.OnResourceCollected -= HandleResourceCollected;
        SpaceshipAgent.OnSpaceshipDestroyed -= HandleSpaceshipDestroyed;
    }

    private void HandleAsteroidDestroyed(Asteroid asteroid)
    {
        GameObject resource = Instantiate(resourcePrefab, asteroid.transform.position, Quaternion.identity, transform);
        resources.Add(resource);
        asteroids.Remove(asteroid.gameObject);
    }

    private void HandleResourceCollected(Resource resource, Team team)
    {
        resources.Remove(resource.gameObject);
        if (team == Team.Blue)
        {
            Debug.Log("Blue team collected a resource!");
            blueAgentGroup.AddGroupReward(1.0f);
            //mockBlueGroupReward += 1.0f;
        }
        else
        {
            Debug.Log("Orange team collected a resource!");
            orangeAgentGroup.AddGroupReward(1.0f);
        }

        // End episode if all asteroids and resources are gone
        if (AreObjectsGone())
        {
            Debug.Log("All objects gone, ending episode");
            blueAgentGroup.EndGroupEpisode();
            orangeAgentGroup.EndGroupEpisode();
            ResetScene(false);
        }
    }

    private void HandleSpaceshipDestroyed(SpaceshipAgent agent)
    {
        agent.gameObject.SetActive(false);
        if (agent.team == Team.Blue)
        {
            numberBlueAgentsRemaining--;
            foreach (var otherAgent in agentsList)
            {
                if (otherAgent.team == Team.Blue && otherAgent != agent)
                {
                    otherAgent.isTeammateAlive = false;
                }
            }
            /*if (numberBlueAgentsRemaining == 0)
            {
                Debug.Log("All blue agents destroyed");
                blueAgentGroup.AddGroupReward(-3.0f);
            }*/
            //blueAgentGroup.AddGroupReward(-2.0f);
            blueAgentGroup.AddGroupReward(-8.0f);
            //mockBlueGroupReward -= 2.0f;
        }
        else
        {
            numberOrangeAgentsRemaining--;
            foreach (var otherAgent in agentsList)
            {
                if (otherAgent.team == Team.Orange && otherAgent != agent)
                {
                    otherAgent.isTeammateAlive = false;
                }
            }
            /*if (numberOrangeAgentsRemaining == 0)
            {
                Debug.Log("All orange agents destroyed");
                orangeAgentGroup.AddGroupReward(-3.0f);
            }*/
            //orangeAgentGroup.AddGroupReward(-2.0f);
            orangeAgentGroup.AddGroupReward(-8.0f);
        }
        sumOfAgentDeathTimes += resetTimer;
        // Drop a single resource where ship is destroyed
        GameObject resource = Instantiate(resourcePrefab, agent.transform.position, Quaternion.identity, transform);
        resources.Add(resource);
    }

    bool AreObjectsGone()
    {
        return asteroids.Count == 0 && resources.Count == 0;
    }

    void FixedUpdate()
    {
        resetTimer += 1;
        blueAgentGroup.AddGroupReward(-2f / MaxEnvironmentSteps);
        orangeAgentGroup.AddGroupReward(-2f / MaxEnvironmentSteps);
        //mockBlueGroupReward -= (2f / MaxEnvironmentSteps);
        //Debug.Log("Blue group reward: " + mockBlueGroupReward);
        //Debug.Log("Expected penalty: " + ((-2f / MaxEnvironmentSteps) * resetTimer));
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            blueAgentGroup.GroupEpisodeInterrupted();
            orangeAgentGroup.GroupEpisodeInterrupted();
            ResetScene(false);
        }
    }
}
