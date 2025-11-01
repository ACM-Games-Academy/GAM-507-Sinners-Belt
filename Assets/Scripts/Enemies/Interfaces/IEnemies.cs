public interface IEnemy
{
    float Health { get; }
    bool IsAlive { get; }
    void TakeDamage(float amount);
    void Kill();
}
