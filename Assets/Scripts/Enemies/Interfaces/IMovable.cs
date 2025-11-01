using UnityEngine;

public interface IMovable
{
    void MoveTo(Vector3 worldPosition);
    void StopMovement();
    bool IsMoving { get; }
}
