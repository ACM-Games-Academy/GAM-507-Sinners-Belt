using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Setup")]
    public Transform muzzlePoint;
    public LayerMask negativeHitMask;
    public GameObject bulletTrailPrefab;
    public Camera cam;
    protected IFireMode fireMode;

    public virtual void Initialize(IFireMode mode)
    {
        fireMode = mode;
        fireMode.Initialize(this);
    }

    public virtual FireResponse Fire()
    {
        return fireMode != null ? fireMode.Fire() : FireResponse.NoFireMode;
    }

    public virtual int GetCurrentAmmo()
    {
        return fireMode != null ? fireMode.CurrentAmmo : 0;
    }

    public virtual int GetMaxAmmo()
    {
        return fireMode != null ? fireMode.MaxAmmo : 0;
    }

    public virtual bool TryReload()
    {
        if (fireMode != null && !fireMode.IsReloading && fireMode.CurrentAmmo != fireMode.MaxAmmo)
        {
            StartCoroutine(Reload());
            return true;
        }
        else return false;
    }

    private IEnumerator Reload()
    {
        yield return StartCoroutine(fireMode.Reload());
    }

    public Vector3 GetCameraTargetPoint()
    {
        float range = fireMode?.Range ?? 1000f;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, negativeHitMask))
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