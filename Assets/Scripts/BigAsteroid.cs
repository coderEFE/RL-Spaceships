using UnityEngine;

public class BigAsteroid : MonoBehaviour
{
    public static event System.Action<BigAsteroid> OnBigAsteroidDestroyed;
    public int numberBlueToucing = 0;
    public int numberOrangeToucing = 0;

    public void OnTouch(Team team)
    {
        if (team == Team.Blue)
        {
            numberBlueToucing++;
        }
        else if (team == Team.Orange)
        {
            numberOrangeToucing++;
        }

        if (numberBlueToucing > 0 && numberOrangeToucing > 0)
        {
            OnBigAsteroidDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public void OnStopTouching(Team team)
    {
        if (team == Team.Blue)
        {
            numberBlueToucing--;
        }
        else if (team == Team.Orange)
        {
            numberOrangeToucing--;
        }
    }
}
