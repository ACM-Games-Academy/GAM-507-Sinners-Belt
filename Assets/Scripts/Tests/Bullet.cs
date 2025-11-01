using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public float lifeTime = 5f;
    private Vector3 velocity;
    private bool hasVelocity;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (hasVelocity)
        {
            transform.position += velocity * Time.deltaTime;
        }
    }

    // Called by AttackComponent if no Rigidbody exists
    public void Initialize(Vector3 vel)
    {
        velocity = vel;
        hasVelocity = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore triggers (like sensors or vision)
        if (other.isTrigger) return;

        // Optional: deal damage
        var health = other.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.TakeDamage(10f); // or pass damage through parameters if needed
        }

        // Optional: spawn impact FX here
        Destroy(gameObject);
    }
}
