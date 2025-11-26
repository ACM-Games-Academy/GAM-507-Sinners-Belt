using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnRetry()
    {
        SceneManager.LoadScene("Playable Demo");
    }

    public void OnMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}