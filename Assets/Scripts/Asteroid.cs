using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public Team team; // Only used in symbiotic environment to determine which type of resource to spawn on destruction
    public static event System.Action<Asteroid> OnAsteroidDestroyed;

    public void OnHit()
    {
        OnAsteroidDestroyed?.Invoke(this);
        Destroy(gameObject);
    }
}
