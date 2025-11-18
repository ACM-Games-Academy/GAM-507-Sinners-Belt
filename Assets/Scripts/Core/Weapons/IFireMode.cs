using System.Collections;

public enum FireResponse { Fired, NoAmmo, NoFireMode, FireRateDelay, Reloading }

public interface IFireMode
{
    float Range { get; }
    int MaxAmmo { get; }
    int CurrentAmmo { get; }
    bool IsReloading { get; }

    void Initialize(WeaponBase weapon);
    FireResponse Fire();
    bool TryUseAmmo(int count);
    IEnumerator Reload();
}