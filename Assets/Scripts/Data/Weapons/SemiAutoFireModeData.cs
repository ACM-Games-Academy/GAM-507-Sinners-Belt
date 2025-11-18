using UnityEngine;

[CreateAssetMenu(fileName = "FireModeData", menuName = "FireModeData/SemiAuto")]
public class SemiAutoFireModeData : ScriptableObject
{
    [Header("Weapon Stats")]
    [Tooltip("Shots per second")]
    public float fireRate = 8f;
    public float damage = 20f;
    public float range = 1000f;
    [Tooltip("Ammo used per shot")]
    public int ammoConsumption = 1;
    [Tooltip("Kick (jolt) power on firing (recommended = 0.01f)")]
    public float kickPower = 0.01f;
    [Tooltip("Maximum ammo of the gun")]
    public int maxAmmo = 10;
    [Tooltip("Time in seconds to reload the gun")]
    public float reloadTime = 1f;

    [Header("Extra (optional)")]
    public AudioClip fireSound;
    public GameObject muzzleFlashPrefab;
}