using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuUI : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject mapSelectPanel;
    public GameObject settingsPanel;
    public GameObject pausePanel;

    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;

    public TMP_Text descriptionTitle;
    public TMP_Text descriptionText;

    void Start()
    {
        if (volumeSlider != null)
            volumeSlider.value = AudioListener.volume;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;

        ShowDefaultMapInfo();
    }

    public void OpenMapSelect()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void BackToMainFromMap()
    {
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void BackToMainFromSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void OpenPause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePause()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void BackToMenuScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UITest");
    }

    public void LoadMap1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scene1");
    }

    public void LoadMap2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("map2valcano");
    }

    public void LoadMap3()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scene2");
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    //��ͼѡ���ı���ش���
    public void ShowMap1Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "Iron Cage";

        if (descriptionText != null)
            descriptionText.text = "Judgment Spotlight: Turns Red for humans, white for AI. Hide or be exposed.";
    }

    public void ShowMap2Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "Volcanic Island";

        if (descriptionText != null)
            descriptionText.text = "Lava Erosion: The safe zone shrinks. Stay inside the boundary or perish.";
    }

    public void ShowMap3Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "Starship";

        if (descriptionText != null)
            descriptionText.text = "Collision Annihilation: AI bots destroy each other on contact, stripping your cover fast.";
    }
    public void ShowDefaultMapInfo()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "MAP SELECT";

        if (descriptionText != null)
            descriptionText.text = "PLEASE SELECT A MAP";
    }
    //�ı��������

    public void SetResolution(int index)
    {
        switch (index)
        {
            case 0:
                Screen.SetResolution(1920, 1080, Screen.fullScreen);
                break;
            case 1:
                Screen.SetResolution(1600, 900, Screen.fullScreen);
                break;
            case 2:
                Screen.SetResolution(1280, 720, Screen.fullScreen);
                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}