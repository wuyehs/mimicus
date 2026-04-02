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
        "FIREARM",
        "SMOKE GRENADE",
        "HIGH-STAKES BOMB",
        "CROWD BEACON"
    };


    private string[] contents =
    {
    "Stalker is a multiplayer stealth game where you blend in with AI bots and eliminate your rival. Missed attacks cause a 5-second rigidity, so every move matters.",

    "A precise long-range weapon for safe eliminations. However, every shot reveals your position with a visible muzzle flash.",

    "Creates a dense smoke screen that blocks vision. Use it to escape after a kill or to hide your identity when exposed.",

    "A powerful explosive with a wide blast radius. If it fails to kill your rival, it backfires and kills the user after 5 seconds.",

    "Summons a group of AI bots near your enemy. Use the chaos to confuse, distract, or trap your opponent."
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