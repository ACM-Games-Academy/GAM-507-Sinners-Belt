using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class VisionComponent : MonoBehaviour
{
    [Header("Vision")]
    public float viewRadius = 15f;
    [Range(1f, 360f)] public float viewAngle = 120f;

    [Header("Masks")]
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    public string playerTag = "Player";

    public event Action<Transform> PlayerDetected;
    public event Action PlayerLost;

    private Transform currentPlayer;
    private float lastSeenTime = 0f;
    public float memoryDuration = 1f;

    private void Update()
    {
        ScanForPlayer();
    }

    private void ScanForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, playerMask);
        Transform detected = null;

        foreach (var c in hits)
        {
            if (c == null || !c.gameObject.CompareTag(playerTag)) continue;

            Vector3 dir = (c.transform.position - transform.position).normalized;
            float dotAngle = Vector3.Angle(transform.forward, dir);
            if (dotAngle > viewAngle * 0.5f) continue;

            if (HasLineOfSight(c.transform))
            {
                detected = c.transform;
                break;
            }
        }

        if (detected != null)
        {
            if (currentPlayer != detected)
            {
                currentPlayer = detected;
                PlayerDetected?.Invoke(currentPlayer);
            }
            lastSeenTime = Time.time;
        }
        else
        {
            if (currentPlayer != null && Time.time - lastSeenTime > memoryDuration)
            {
                currentPlayer = null;
                PlayerLost?.Invoke();
            }
        }
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3[] origins =
        {
            transform.position + Vector3.up * 1.6f,
            transform.position + Vector3.up * 1.0f,
            transform.position + Vector3.up * 0.4f
        };

        Vector3[] targets =
        {
            target.position + Vector3.up * 1.6f,
            target.position + Vector3.up * 1.0f,
            target.position + Vector3.up * 0.2f
        };

        for (int i = 0; i < origins.Length; i++)
        {
            Vector3 dir = (targets[i] - origins[i]).normalized;
            float dist = Vector3.Distance(origins[i], targets[i]);

            if (Physics.Raycast(origins[i], dir, out RaycastHit hit, dist, obstacleMask | playerMask))
            {
                if (((1 << hit.collider.gameObject.layer) & playerMask.value) != 0)
                    return true;
            }
        }

        return false;
    }

    public bool CanSee(Transform target)
    {
        if (target == null) return false;

        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        return HasLineOfSight(target);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);
    }
}
