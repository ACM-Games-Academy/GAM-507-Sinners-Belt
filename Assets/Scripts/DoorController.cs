using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Animation")]
    public Animator leftDoorAnimator;
    public Animator rightDoorAnimator;
    public string openTriggerName = "Open";
    public string closeTriggerName = "Close";

    private bool isLocked;
    private Vector3 leftDoorClosedPosition;
    private Vector3 rightDoorClosedPosition;

    private void Awake()
    {
        leftDoorClosedPosition = leftDoorAnimator.transform.localPosition;
        rightDoorClosedPosition = rightDoorAnimator.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            leftDoorAnimator.SetTrigger(openTriggerName);
            rightDoorAnimator.SetTrigger(openTriggerName);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            leftDoorAnimator.SetTrigger(closeTriggerName);
            rightDoorAnimator.SetTrigger(closeTriggerName);
        }
    }

    public void FreezeAnimators()
    {
        leftDoorAnimator.enabled = false;
        rightDoorAnimator.enabled = false;
        leftDoorAnimator.SetTrigger(closeTriggerName);
        rightDoorAnimator.SetTrigger(closeTriggerName);
    }

    public void ThawAnimators()
    {
        leftDoorAnimator.enabled = true;
        rightDoorAnimator.enabled = true;
    }

    public void ForceClose()
    {
        FreezeAnimators();
        leftDoorAnimator.transform.localPosition = leftDoorClosedPosition;
        rightDoorAnimator.transform.localPosition = rightDoorClosedPosition;
    }

    public void ToggleLock(bool toggle)
    {
        isLocked = toggle;
    }
}
