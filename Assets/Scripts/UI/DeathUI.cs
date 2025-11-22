using UnityEngine;

public class DeathUI : MonoBehaviour
{
    //Death UI Canvas

    //Retry Button 
    public void RetryButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    //Main Menu Button//Return 
    public void MainMenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
