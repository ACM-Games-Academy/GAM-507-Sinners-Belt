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
        if (other.TryGetComponent(out IImpactable component))
        {
            component.OnImpact(new ImpactInfo
            {
                Damage = 10f
            });
        }

        // Optional: spawn impact FX here
        Destroy(gameObject);
    }
}