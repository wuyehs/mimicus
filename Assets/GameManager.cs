using UnityEngine;
using UnityEngine.UI; // 必须有这一行，否则无法识别 Text 组件
using UnityEngine.SceneManagement; // 用于重启游戏
using System.Collections.Generic;
using TMPro;
public class GameManager : MonoBehaviour
{
    // 单例模式：让 CharacterLogic 能够通过 GameManager.instance 找到这里
    public static GameManager instance;
    public Camera mainCamera;   
    [Header("摄像机设置")]
    [SerializeField] private float cameraSize = 5f;  // 正交摄像机大小
    [SerializeField] private Color backgroundColor = Color.black;

    [Header("游戏设置")]
    public GameObject charPrefab;
    public int totalCharacters = 20;
    
    [Header("UI设置")]
   public TextMeshProUGUI winText;
    
    private bool isGameOver = false;

    void Awake()
    {
        // 初始化单例
        instance = this;
    
        mainCamera = Camera.main;
        mainCamera.orthographic = true;  // 2D 游戏使用正交投影
        mainCamera.orthographicSize = cameraSize;
        mainCamera.backgroundColor = backgroundColor;
        mainCamera.transform.position = new Vector3(0, 0, -10f);
    }

    void Start()
    {
        Time.timeScale = 1f; // 确保游戏开始时时间是流动的
        
        // 游戏开始时隐藏胜利文字
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }

        SpawnCharacters();
    }

    void SpawnCharacters()
    {
        List<CharacterLogic> allChars = new List<CharacterLogic>();
        for (int i = 0; i < totalCharacters; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-8f, 8f), Random.Range(-4.5f, 4.5f));
            GameObject go = Instantiate(charPrefab, pos, Quaternion.identity);
            allChars.Add(go.GetComponent<CharacterLogic>());
        }

        // 随机分配玩家角色
        int p1 = Random.Range(0, totalCharacters);
        int p2 = Random.Range(0, totalCharacters);
        while (p1 == p2) p2 = Random.Range(0, totalCharacters);

        allChars[p1].currentRole = CharacterLogic.Role.Player1;
        allChars[p1].GetComponent<SpriteRenderer>().color = Color.blue; // 调试色

        allChars[p2].currentRole = CharacterLogic.Role.Player2;
        allChars[p2].GetComponent<SpriteRenderer>().color = Color.red; // 调试色
    }

    void Update()
    {
        // 游戏结束后，按 R 键重启场景
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // --- 核心：winText 相关的结束逻辑 ---
    public void EndGame(string winnerMessage)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (winText != null)
        {
            // 1. 显示 UI 物体
            winText.gameObject.SetActive(true);
            // 2. 修改文字内容
            winText.text = winnerMessage + "\npress R key to restart";
        }

        // 3. 冻结游戏时间
        Time.timeScale = 0f;
    }
}