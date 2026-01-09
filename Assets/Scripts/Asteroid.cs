using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public static event System.Action<Asteroid> OnAsteroidDestroyed;

    public void OnHit()
    {
        OnAsteroidDestroyed?.Invoke(this);
        Destroy(gameObject);
    }
}
