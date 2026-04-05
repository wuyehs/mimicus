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
    //public TMP_Dropdown resolutionDropdown;
    public AudioSource bgmSource;

    public TMP_Text descriptionTitle;
    public TMP_Text descriptionText;
    
    // 添加第四个地图的按钮引用（可选）
    public Button map4Button; // 可以在Inspector中分配

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

       /* if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        } */

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
        
        SceneManager.LoadScene("scene2");
    }
    
    // 添加第四个地图加载方法
    public void LoadMap4()
    {
        Time.timeScale = 1f;
        Debug.Log("Loading Map 4...");
        SceneManager.LoadScene("Scene3"); // 修改为您的实际场景名称
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

    // 地图选择信息显示处理
    public void ShowMap1Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "监狱";

        if (descriptionText != null)
            descriptionText.text = "你知道探照灯会暴露伪装在机器中的人类吗?";
    }

    public void ShowMap2Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "火山岛";

        if (descriptionText != null)
           descriptionText.text = "你知道熔岩最终会吞没整座岛屿吗?";
    }

    public void ShowMap3Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "星舰";

        if (descriptionText != null)
            descriptionText.text = "你知道外星人可能会为了研究而捕获机器人吗?";
    }
    
    // 添加第四个地图信息显示方法
    public void ShowMap4Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "医院"; // 请在这里填写地图4的名称

        if (descriptionText != null)
            descriptionText.text = "你知道一种致命病毒刚刚席卷了这家医院吗?"; // 请在这里填写地图4的描述
    }
    
    public void ShowDefaultMapInfo()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "地图选择";

        if (descriptionText != null)
            descriptionText.text = "请选择一张地图";
    }
    // 文本显示结束

   /* public void SetResolution(int index)
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
    }   */

    public void QuitGame()
    {
        Application.Quit();
    }
}