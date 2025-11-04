using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public float lifeTime = 5f;
    public float damage = 10f;

    public GameObject instigator;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Vector3 velocity, GameObject instigator = null)
    {
        rb.linearVelocity = velocity;
        this.instigator = instigator;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.TryGetComponent(out IImpactable impactable))
        {
            impactable.OnImpact(new ImpactInfo
            {
                Damage = damage,
                Instigator = instigator,
                Point = collision.contacts[0].point,
                Normal = collision.contacts[0].normal
            });
        }
        

        Destroy(gameObject);
    }


















    
    
}