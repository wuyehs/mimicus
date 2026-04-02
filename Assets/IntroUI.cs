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
        "GAME INTRODUCTION",
        "MISSION",
        "HOW TO PLAY"
    };

    private string[] contents =
    {
        "This game is about hiding among robots in a sci-fi environment. The player must stay unnoticed and survive in a world full of mechanical guards.",
        "Your mission is to blend in with the robots, avoid suspicion, and complete your objectives without being detected.",
        "Observe the movement of other robots carefully. Move, hide, and act naturally so that enemies cannot identify you."
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
        if (introPanel != null) introPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }
}