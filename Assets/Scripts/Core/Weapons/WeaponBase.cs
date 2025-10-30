using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Setup")]
    public Transform muzzlePoint;
    public LayerMask hitMask;
    public GameObject bulletTrailPrefab;

    [Header("Weapon Stats")]
    public float maxAmmo;
    [SerializeField] protected float ammo;

    protected IFireMode fireMode;
    protected Camera cam;

    protected virtual void Awake()
    {
        cam = Camera.main;
        ammo = maxAmmo;
    }

    public virtual void Initialize(IFireMode mode)
    {
        fireMode = mode;
        fireMode.Initialize(this);
    }

    public virtual void Fire()
    {
        fireMode?.Fire();
    }

    public virtual bool TryUseAmmo(float count)
    {
        if (ammo - count < 0)
            return false;

        ammo -= count;
        return true;
    }

    public Vector3 GetCameraTargetPoint()
    {
        float range = fireMode?.Range ?? 1000f;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
            return hit.point;

        return ray.origin + ray.direction * range;
    }

    public void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (!bulletTrailPrefab) return;
        
        Transform trail = Instantiate(bulletTrailPrefab).transform;
        StartCoroutine(AnimateTracer(trail, start, end));
    }

    public IEnumerator AnimateTracer(Transform trail, Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float speed = 200f;
        float traveled = 0f;

        while (traveled < distance)
        {
            traveled += speed * Time.deltaTime;
            float progress = traveled / distance;
            trail.position = Vector3.Lerp(start, end, progress);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);
        Destroy(trail.gameObject);
    }
}