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
    public GameObject introPanel;

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
        //ShowMainMenu();//使用强制生成主菜单函数

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

    //强制回主菜单的函数
    public void ShowMainMenu()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void ShowIntro()
    {
        if (introPanel != null) introPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void BackToMainFromIntro()
    {
        ShowMainMenu();
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
            descriptionText.text = "你知道探照灯会暴露伪装在人群中的人类吗\r\n当有真人玩家暴露在探照灯下，灯光将变为红色?";
    }

    public void ShowMap2Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "火山岛";

        if (descriptionText != null)
           descriptionText.text = "末日已经来临\r\n角色碰到岩浆将会直接死亡。随时间流逝，岩浆将逐步侵蚀小岛。";
    }

    public void ShowMap3Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "星舰";

        if (descriptionText != null)
            descriptionText.text = "外星人为了研究会捕获机器人。\r\n当两个机器人相互碰撞时，两个机器人有概率直接消失";
    }
    
    // 添加第四个地图信息显示方法
    public void ShowMap4Info()
    {
        if (descriptionTitle != null)
            descriptionTitle.text = "疗养院"; // 请在这里填写地图4的名称

        if (descriptionText != null)
            descriptionText.text = "一种致命病毒席卷了这里。无人生还。\r\n携带病毒的角色将在一段时间后自动死亡。被病毒携带者触碰的角色也将感染。场上没有病毒携带者时，随机感染一位角色。"; // 请在这里填写地图4的描述
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