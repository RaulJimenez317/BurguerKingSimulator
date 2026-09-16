using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButton : MonoBehaviour
{
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }
}