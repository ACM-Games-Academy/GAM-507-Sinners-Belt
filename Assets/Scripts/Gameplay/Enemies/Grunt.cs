using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class GruntController : MonoBehaviour
{
    private EnemyController enemyController;
    private HealthComponent health;
    private Transform player;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        health = GetComponent<HealthComponent>();
    }

    private void Start()
    {
        player = enemyController.GetPlayer();
    }

    private void Update()
    {
        if (!health.IsAlive()) return;
    }
}
