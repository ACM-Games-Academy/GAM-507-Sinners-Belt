using UnityEngine;

public interface IImpactable
{
    void OnImpact(ImpactInfo info);
}

public struct ImpactInfo
{
    public Vector3 Point;
    public Vector3 Normal;
    public float Force;
    public float Damage;
    public GameObject Source;
    public GameObject Instigator;
    public DamageType DamageType;
}

public enum DamageType
{
    Physical,
    Explosion
}