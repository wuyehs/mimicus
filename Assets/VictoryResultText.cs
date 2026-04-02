using UnityEngine;
using TMPro;

public class VictoryResultText : MonoBehaviour
{
    public TMP_Text resultText;

    void Start()
    {
        if (resultText != null)
        {
            resultText.text = GameManager.finalWinnerMessage;
        }
    }
}
