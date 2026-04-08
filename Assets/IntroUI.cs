using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    public GameObject introPanel;
    public GameObject mainPanel;

    public TMP_Text introTitle;
    public TMP_Text introText;
    public TMP_Text pageText;

    public Button prevButton;
    public TMP_Text nextButtonText;

    private int currentPage = 0;

    private string[] titles =
    {
        "GAME OVERVIEW",
        "KEY CONTORLS",
        "FIREARM",
        "SMOKE GRENADE",
        "HIGH-STAKES BOMB",
        "CROWD BEACON"
    };

    private string[] contents =
    {
        "玩家需要模仿 AI 行为隐藏身份，并伺机击杀对手。攻击一旦失手，会陷入 5 秒僵直。你需要灵活运用道具，同时应对地图机制。在这场伪装与观察的较量中，精准是生存的关键。",
        "Player 1：移动 (WASD) | 攻击 (F) | 使用道具 (G) \n Player 2：移动 (Arrow Keys) | 攻击 (0) | 使用道具 (-) \n按Esc暂停",
        "高精度远程武器，可在安全距离击杀目标，但枪口火光会暴露你的位置。",
        "快速释放大片烟雾，遮挡视野，适合刺杀后脱身或暴露后重新隐藏身份。",
        "大范围高杀伤武器，但若未能炸死对手，5 秒后会反噬使用者。",
        "在敌人附近召唤一群 AI，制造混乱，干扰判断，甚至将对方困入 AI 人潮中。"
    };

    void Start()
    {
        bool skipIntro = PlayerPrefs.GetInt("SkipIntroOnce", 0) == 1;

        if (skipIntro)
        {
            if (introPanel != null) introPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);

            PlayerPrefs.SetInt("SkipIntroOnce", 0);
            PlayerPrefs.Save();
            return;
        }

        if (introPanel != null) introPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);

        ShowPage(0);
    }

    private void ShowIntroOnly()
    {
        if (introPanel != null) introPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    private void ShowMainMenuOnly()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void ShowPage(int index)
    {
        currentPage = index;

        if (introTitle != null)
            introTitle.text = titles[currentPage];

        if (introText != null)
            introText.text = contents[currentPage];

        if (pageText != null)
            pageText.text = (currentPage + 1).ToString() + " / " + titles.Length.ToString();

        if (prevButton != null)
            prevButton.gameObject.SetActive(currentPage > 0);

        if (nextButtonText != null)
        {
            if (currentPage == titles.Length - 1)
                nextButtonText.text = "START";
            else
                nextButtonText.text = "NEXT";
        }
    }

    public void NextPage()
    {
        if (currentPage < titles.Length - 1)
        {
            ShowPage(currentPage + 1);
        }
        else
        {
            EnterMainMenu();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
        }
    }

    public void SkipIntro()
    {
        EnterMainMenu();
    }

    private void EnterMainMenu()
    {
        PlayerPrefs.SetInt("HasSeenIntro", 1);
        PlayerPrefs.Save();

        ShowMainMenuOnly();
    }
}