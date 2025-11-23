using UnityEngine;

public class MissionObjective : MonoBehaviour
{
    [SerializeField] private WaveTrigger[] triggersToEnable;

    private bool isEnabled = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isEnabled)
        {
            isEnabled = false;

            foreach (WaveTrigger trigger in triggersToEnable)
            {
                trigger.EnableTrigger();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!isEnabled) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
