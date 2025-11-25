using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompleteMission : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FadeScreenUI fadeScreenUI;

    private bool isComplete = false;

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out ObjectiveTracker objectiveTracker) && !isComplete)
        {
            if (objectiveTracker.GetFlag("MissionObjectiveCollected") == true)
            {
                isComplete = true;
                fadeScreenUI.SetUIEnabled(true);
                fadeScreenUI.Fade(1f, 3f);
                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene("WinScene");
            }
        }
    }
}