using UnityEngine;

public class Resource : MonoBehaviour
{
    public static event System.Action<Resource, Team> OnResourceCollected;

    public void OnCollected(Team team)
    {
        OnResourceCollected?.Invoke(this, team);
        Destroy(gameObject);
    }
}
