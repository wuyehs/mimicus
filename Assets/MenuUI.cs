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
    public AudioSource bgmSource;

    public TMP_Text descriptionTitle;
    public TMP_Text descriptionText;

    void Start()
    {
        if (volumeSlider != null && bgmSource != null)
        {
            volumeSlider.SetValueWithoutNotify(bgmSource.volume);
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

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
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadMap3()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scene1");
    }

    public void SetVolume(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    //地图选择文本相关代码
    public void ShowMap1Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "MAP　ONE";

        if (descriptionText != null)
            descriptionText.text = "abc";
    }

    public void ShowMap2Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "MAP TWO";

        if (descriptionText != null)
            descriptionText.text = "def";
    }

    public void ShowMap3Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "MAP THRIE";

        if (descriptionText != null)
            descriptionText.text = "hij";
    }
    public void ShowDefaultMapInfo()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "MAP SELECT";

        if (descriptionText != null)
            descriptionText.text = "PLEASE SELECT A MAP";
    }
    //文本代码结束

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