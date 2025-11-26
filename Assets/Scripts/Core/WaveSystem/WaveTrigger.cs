using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DoorController[] doorsToLock;

    [Header("Wave Setup")]
    [SerializeField] private bool isEnabled = true;
    [SerializeField] private Wave[] waves;

    private List<IHealth> aliveEnemies;

    private void Awake()
    {
        aliveEnemies = new List<IHealth>();
    }

    public void EnableTrigger()
    {
        isEnabled = true;
    }

    IEnumerator SpawnWave()
    {
        foreach (Wave wave in waves)
        {
            if (wave.enemyPrefab != null)
            {
                GameObject enemy = Instantiate(wave.enemyPrefab, wave.spawnPosition, Quaternion.identity);

                if (enemy.TryGetComponent(out IHealth health))
                {
                    aliveEnemies.Add(health);
                }

                yield return null;
            }
        }

        // Wait for enemies to die
        yield return new WaitUntil(() => aliveEnemies.All(enemy => !enemy.IsAlive()));

        // Re-enable doors
        foreach (DoorController door in doorsToLock)
        {
            door.ToggleLock(false);
            door.ThawAnimators();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnabled && other.CompareTag("Player"))
        {
            isEnabled = false;

            foreach (DoorController door in doorsToLock)
            {
                door.ToggleLock(true);
                door.ForceClose();
            }

            StartCoroutine(SpawnWave());
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Wave wave in waves)
        {
            if (isEnabled)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.yellow;

            Gizmos.DrawWireCube(wave.spawnPosition, new Vector3(1, 2, 1));
        }
    }
}

[System.Serializable]
public struct Wave
{
    public GameObject enemyPrefab;
    public Vector3 spawnPosition;
}