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
        "Item Overview",
        "FIREARM",
        "SMOKE GRENADE",
        "HIGH-STAKES BOMB",
        "CROWD BEACON"
    };

    private string[] contents =
    {
        "玩家需要模仿 AI 行为隐藏身份，并伺机击杀对手。攻击一旦失手，会陷入 5 秒僵直。你需要灵活运用道具，同时应对地图机制。在这场伪装与观察的较量中，精准是生存的关键。",
        "Player 1：移动 (WASD) | 攻击 (F) | 使用道具 (G) \n Player 2：移动 (Arrow Keys) | 攻击 (0) | 使用道具 (-) \n按Esc暂停",
        "道具被触碰即被拾取，有概率刷新在不可触碰区域。道具刷新后6秒内不被拾取将消失。场景中没有道具时将在3-8秒内随机刷新一个。拾取新的道具刷新手中原有道具，至多同时拥有1个道具。",
        "高精度远程武器，射程无限，朝角色面向方向发射子弹，可在安全距离击杀目标，使用不会暴露你的位置。但弹道判定较细且无法穿过障碍物，请瞄准后使用",
        "快速释放大片烟雾，遮挡视野，烟雾在5秒后消散。适合刺杀后脱身或暴露后重新隐藏身份。",
        "大范围高杀伤武器。使用后炸弹将在5秒内跟随使用者并在倒计时结束爆炸。若爆炸时没有炸死敌方真人玩家，则判定使用者死亡。反之使用者胜利。请谨慎使用。",
        "在敌人附近0.5-1.5米内召唤一个机器人AI。机器人被召唤时将伴随召唤特效。基础操作可以用特效确定敌人的大概位置。进阶操作还请各位真人探索。"
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