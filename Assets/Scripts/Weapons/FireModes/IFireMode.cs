public interface IFireMode
{
    float FireRate { get; }
    float Damage { get; }
    float Range { get; }

    void Initialize(Weapon weapon, FireModeData data);
    void Fire();
}