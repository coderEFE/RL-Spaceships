using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class CollaborativeSpaceshipEnvController : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject bigAsteroidPrefab;
    private List<GameObject> asteroids = new List<GameObject>();
    private List<GameObject> bigAsteroids = new List<GameObject>();
    private int numAsteroids = 0;
    private int numBigAsteroids = 1;
    private bool singleRewardAsteroids = true;
    
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
            Academy.Instance.StatsRecorder.Add("Custom/NumBigAsteroidsLeft", bigAsteroids.Count);
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
        foreach (var bigAsteroid in bigAsteroids)
        {
            Destroy(bigAsteroid);
        }
        bigAsteroids.Clear();
        
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
        for (int i = 0; i < numBigAsteroids; i++)
        {
            Vector3 asteroidPosition = transform.position + new Vector3(Random.Range(-17f, 17f), Random.Range(-17f, 17f), 0f);
            GameObject bigAsteroid = Instantiate(bigAsteroidPrefab, asteroidPosition, Quaternion.identity, transform);
            bigAsteroids.Add(bigAsteroid);
        }
    }

    void OnEnable()
    {
        Asteroid.OnAsteroidDestroyed += HandleAsteroidDestroyed;
        BigAsteroid.OnBigAsteroidDestroyed += HandleBigAsteroidDestroyed;
        Resource.OnResourceCollected += HandleResourceCollected;
        SpaceshipAgent.OnSpaceshipDestroyed += HandleSpaceshipDestroyed;
    }

    void OnDisable()
    {
        Asteroid.OnAsteroidDestroyed -= HandleAsteroidDestroyed;
        BigAsteroid.OnBigAsteroidDestroyed -= HandleBigAsteroidDestroyed;
        Resource.OnResourceCollected -= HandleResourceCollected;
        SpaceshipAgent.OnSpaceshipDestroyed -= HandleSpaceshipDestroyed;
    }

    private void HandleAsteroidDestroyed(Asteroid asteroid)
    {
        if (singleRewardAsteroids)
        {
            GameObject resource = Instantiate(resourcePrefab, asteroid.transform.position + new Vector3(0f, 0f, 0f), Quaternion.identity, transform);
            resources.Add(resource);
        }
        else
        {
            GameObject resource1 = Instantiate(resourcePrefab, asteroid.transform.position + new Vector3(-0.5f, 0f, 0f), Quaternion.identity, transform);
            resources.Add(resource1);
            GameObject resource2 = Instantiate(resourcePrefab, asteroid.transform.position + new Vector3(0.5f, 0f, 0f), Quaternion.identity, transform);
            resources.Add(resource2);
        }
        asteroids.Remove(asteroid.gameObject);
    }

    private void HandleBigAsteroidDestroyed(BigAsteroid bigAsteroid)
    {
        // Gives large reward to both teams to incentivize cooperation, since one team alone can't destroy it
        blueAgentGroup.AddGroupReward(3.0f);
        orangeAgentGroup.AddGroupReward(3.0f);
        bigAsteroids.Remove(bigAsteroid.gameObject);

        // End episode if all asteroids and resources are gone
        if (AreObjectsGone())
        {
            Debug.Log("All objects gone, ending episode");
            blueAgentGroup.EndGroupEpisode();
            orangeAgentGroup.EndGroupEpisode();
            ResetScene(false);
        }
    }

    private void HandleResourceCollected(Resource resource, Team team)
    {
        resources.Remove(resource.gameObject);
        if (team == Team.Blue)
        {
            Debug.Log("Blue team collected a resource!");
            blueAgentGroup.AddGroupReward(resource.value);
            //mockBlueGroupReward += 1.0f;
        }
        else
        {
            Debug.Log("Orange team collected a resource!");
            orangeAgentGroup.AddGroupReward(resource.value);
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
                if (otherAgent.team == Team.Blue)
                {
                    otherAgent.proportionOfThisTeamAgentsRemaining = (float)numberBlueAgentsRemaining / 2f; // This approach would change if we have more than 2 agents per team
                    if (otherAgent != agent)
                    {
                        otherAgent.isTeammateAlive = false;
                    }
                }
                if (otherAgent.team == Team.Orange)
                {
                    otherAgent.proportionOfOppositeTeamAgentsRemaining = (float)numberBlueAgentsRemaining / 2f; // This approach would change if we have more than 2 agents per team
                }
            }
            /*if (numberBlueAgentsRemaining == 0)
            {
                Debug.Log("All blue agents destroyed");
                blueAgentGroup.AddGroupReward(-3.0f);
            }*/
            //blueAgentGroup.AddGroupReward(-4.0f);
            //mockBlueGroupReward -= 2.0f;
        }
        else
        {
            numberOrangeAgentsRemaining--;
            foreach (var otherAgent in agentsList)
            {
                if (otherAgent.team == Team.Orange)
                {
                    otherAgent.proportionOfThisTeamAgentsRemaining = (float)numberOrangeAgentsRemaining / 2f; // This approach would change if we have more than 2 agents per team
                    if (otherAgent != agent)
                    {
                        otherAgent.isTeammateAlive = false;
                    }
                }
                if (otherAgent.team == Team.Blue)
                {
                    otherAgent.proportionOfOppositeTeamAgentsRemaining = (float)numberOrangeAgentsRemaining / 2f; // This approach would change if we have more than 2 agents per team
                }
            }
            /*if (numberOrangeAgentsRemaining == 0)
            {
                Debug.Log("All orange agents destroyed");
                orangeAgentGroup.AddGroupReward(-3.0f);
            }*/
            //orangeAgentGroup.AddGroupReward(-4.0f);
        }
        sumOfAgentDeathTimes += resetTimer;
        // Drop a single resource where ship is destroyed
        GameObject resource = Instantiate(resourcePrefab, agent.transform.position, Quaternion.identity, transform);
        resources.Add(resource);
    }

    bool AreObjectsGone()
    {
        return asteroids.Count == 0 && bigAsteroids.Count == 0 && resources.Count == 0;
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
