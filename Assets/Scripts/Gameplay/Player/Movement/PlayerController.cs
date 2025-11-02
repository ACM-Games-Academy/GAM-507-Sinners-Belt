using UnityEngine;

public class PlayerController : MonoBehaviour, IImpactable
{
    public void OnImpact(ImpactInfo data)
    {
        if (TryGetComponent(out IHealth component))
        {
            component.TakeDamage(data.Damage);
        }   
    }
}
