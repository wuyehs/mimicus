using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleCDCanvas : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI player1CDText;
    [SerializeField] private TextMeshProUGUI player2CDText;
    
    [SerializeField] private float startDelay = 5f;  // 5秒后才开始显示冷却时间
    
    private CharacterLogic player1;
    private CharacterLogic player2;
    
    void Start()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.enabled = true;
        
        // 延迟后才设置Text引用
        StartCoroutine(DelayedSetup());
    }
    
    IEnumerator DelayedSetup()
    {
        // 等待5秒
        yield return new WaitForSeconds(startDelay);
        
        // 5秒后才查找玩家并设置Text引用
        FindPlayers();
        
        if (player1 != null)
        {
            player1.player1CDText = player1CDText;
        }
        
        if (player2 != null)
        {
            player2.player2CDText = player2CDText;
        }
        
        Debug.Log($"5秒后开始显示冷却时间");
    }
    
    void FindPlayers()
    {
        CharacterLogic[] characters = FindObjectsOfType<CharacterLogic>();
        foreach (CharacterLogic character in characters)
        {
            if (character.currentRole == CharacterLogic.Role.Player1)
            {
                player1 = character;
            }
            else if (character.currentRole == CharacterLogic.Role.Player2)
            {
                player2 = character;
            }
        }
    }
}