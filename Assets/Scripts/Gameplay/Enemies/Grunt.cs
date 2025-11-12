using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class GruntController : MonoBehaviour
{
    private EnemyController enemyController;
    private HealthComponent health;
    private Transform player;

    [Header("Grunt Settings")]
    public float shortCoverHPThreshold = 0.5f; // below 50% = tall cover
    public string shortCoverTag = "ShortCover";
    public string tallCoverTag = "TallCover";

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        health = GetComponent<HealthComponent>();
    }

    private void Start()
    {
        player = enemyController.GetPlayer();
        enemyController.MoveToNearestCover(shortCoverTag);
    }

    private void Update()
    {
        if (!health.IsAlive()) return;

        if (health.CurrentHealth <= health.MaxHealth * shortCoverHPThreshold)
        {
            enemyController.MoveToNearestCover(tallCoverTag);
        }
    }
}
