using UnityEngine;

public class HealthImpactReceive : MonoBehaviour, IImpactable
{

    private IHealth healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<IHealth>();
    }
    
    public void OnImpact(ImpactInfo data)
    {
        if (data.Damage <= 0f) return;

        healthComponent?.TakeDamage(data.Damage);
            
    }
}
