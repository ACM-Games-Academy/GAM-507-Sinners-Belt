public interface IFireMode
{
    float Range { get; }

    void Initialize(WeaponBase weapon);
    void Fire();
}