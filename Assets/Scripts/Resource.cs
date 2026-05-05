using UnityEngine;

public class Resource : MonoBehaviour
{
    public static event System.Action<Resource, Team> OnResourceCollected;
    public float value = 1.0f;

    public void OnCollected(Team team)
    {
        OnResourceCollected?.Invoke(this, team);
        Destroy(gameObject);
    }
}
