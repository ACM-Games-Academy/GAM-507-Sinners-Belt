using UnityEngine;

public interface IAttacker
{
    void TryAttack(Transform target);

    bool CanAttack { get; }
}
