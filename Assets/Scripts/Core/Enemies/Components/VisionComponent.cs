using UnityEngine;
using System;

/// <summary>
/// Very simple vision component that raises events when it sees the player (by tag).
/// Uses OverlapSphere + line-of-sight raycast for reliability.
/// Adds memory so enemy doesn't instantly lose sight.
/// </summary>
[RequireComponent(typeof(Collider))]
public class VisionComponent : MonoBehaviour
{
    [Header("Vision")]
    public float viewRadius = 15f;
    [Range(1f, 360f)] public float viewAngle = 120f;

    [Header("Masks")]
    public LayerMask obstacleMask;
    public LayerMask playerMask; // set to the player layer(s) in inspector

    public string playerTag = "Player";

    public event Action<Transform> PlayerDetected;
    public event Action PlayerLost;

    private Transform currentPlayer;
    private float lastSeenTime = 0f;
    public float memoryDuration = 1f; // seconds to remember player after losing LOS

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

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, viewRadius, obstacleMask | playerMask))
            {
                if (((1 << hit.collider.gameObject.layer) & playerMask.value) != 0)
                {
                    detected = c.transform;
                    break;
                }
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
            // only lose player after memoryDuration
            if (currentPlayer != null && Time.time - lastSeenTime > memoryDuration)
            {
                currentPlayer = null;
                PlayerLost?.Invoke();
            }
        }
    }

    public bool CanSee(Transform target)
    {
        if (target == null) return false;

        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, viewRadius, obstacleMask | playerMask))
        {
            return hit.transform == target;
        }

        return false;
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
