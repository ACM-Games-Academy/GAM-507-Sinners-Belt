using UnityEngine;

public interface IAggro
{
    void OnPlayerDetected(Transform player);
    void OnPlayerLost();
}
