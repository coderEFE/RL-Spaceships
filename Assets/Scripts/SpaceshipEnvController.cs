using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class SpaceshipEnvController : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    private List<GameObject> asteroids = new List<GameObject>();
    private int numAsteroids = 3;
    
    [SerializeField] private GameObject resourcePrefab;
    private List<GameObject> resources = new List<GameObject>();

    public List<SpaceshipAgent> agentsList = new List<SpaceshipAgent>();
    private SimpleMultiAgentGroup blueAgentGroup;
    private SimpleMultiAgentGroup orangeAgentGroup;
    
    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps;
    private int resetTimer;
    
    void Start()
    {
        blueAgentGroup = new SimpleMultiAgentGroup();
        orangeAgentGroup = new SimpleMultiAgentGroup();
        foreach (var agent in agentsList)
        {
            if (agent.team == Team.Blue) {
                blueAgentGroup.RegisterAgent(agent);
            }
            else
            {
                orangeAgentGroup.RegisterAgent(agent);
            }
        }
        ResetScene();
    }

    private void ResetScene()
    {
        resetTimer = 0;

        // Reset agents
        foreach (var agent in agentsList)
        {
            agent.transform.localPosition = new Vector3(Random.Range(-19f, 19f), Random.Range(-19f, 19f), 0f);
            agent.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            agent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

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
    }

    void OnDisable()
    {
        Asteroid.OnAsteroidDestroyed -= HandleAsteroidDestroyed;
        Resource.OnResourceCollected -= HandleResourceCollected;
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
            ResetScene();
        }
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
        //Debug.Log("Expected penalty: " + ((-2f / MaxEnvironmentSteps) * resetTimer));
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            blueAgentGroup.GroupEpisodeInterrupted();
            orangeAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }
}
