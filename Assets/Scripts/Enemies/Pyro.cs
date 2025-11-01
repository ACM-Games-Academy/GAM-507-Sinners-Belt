using UnityEngine;

/// <summary>
/// Example of a melee/charger variant that uses the same components.
/// This script simply configures components — custom movement/charging behavior can be added later.
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class Pyro : MonoBehaviour
{
    public float meleeRange = 3f;
    public float meleeCooldown = 2f;
    public float meleeDamage = 30f;
    public float health = 120f;

    private void Reset()
    {
        if (GetComponent<MovementComponent>() == null) gameObject.AddComponent<MovementComponent>();
        if (GetComponent<AttackComponent>() == null) gameObject.AddComponent<AttackComponent>();
        if (GetComponent<VisionComponent>() == null) gameObject.AddComponent<VisionComponent>();
        if (GetComponent<HealthComponent>() == null) gameObject.AddComponent<HealthComponent>();
    }

    private void Start()
    {
        var atk = GetComponent<AttackComponent>();
        atk.cooldown = meleeCooldown;
        atk.range = meleeRange;
        atk.meleeDamage = meleeDamage;
        var hp = GetComponent<HealthComponent>();
        hp.maxHealth = health;
    }
}
