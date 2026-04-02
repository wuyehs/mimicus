using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    public string menuSceneName = "UITest";
    public string replaySceneName = "Scene1";

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("SkipIntroOnce", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(menuSceneName);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(replaySceneName);
    }
}