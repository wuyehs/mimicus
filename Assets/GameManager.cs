using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Camera mainCamera;   
    
    [Header("调试设置")]
    public bool debugMode = true;
    
    [Header("摄像机设置")]
    [SerializeField] private float cameraSize = 5f;  // 图一二的相机大小
    
    [Header("图三专用相机设置")]
    [Tooltip("Map_id=3时的相机大小（原始10f，缩小一半为5f）")]
    [SerializeField] private float cameraSizeForMap3 = 5f;  // 原始10f → 5f（缩小一半）
    
    [SerializeField] private Color backgroundColor = Color.black;
    
    [Header("游戏设置")]
    public int Map_id = 1;
    public GameObject charPrefab;
    public int totalCharacters = 20;
    
    [Header("图三专用生成范围设置")]
    [Tooltip("Map_id=3时的X轴范围（原始3.0f，缩小一半为1.5f）")]
    [SerializeField] private float xRangeForMap3 = 1.5f;  // 原始3.0f → 1.5f
    
    [Tooltip("Map_id=3时的Y轴最大值（原始4.0f，缩小一半为2.0f）")]
    [SerializeField] private float yMaxForMap3 = 2.0f;    // 原始4.0f → 2.0f
    
    [Tooltip("Map_id=3时的Y轴最小值（原始-4.0f，缩小一半为-2.0f）")]
    [SerializeField] private float yMinForMap3 = -2.0f;   // 原始-4.0f → -2.0f
    
    [Header("图一二生成范围设置")]
    [Tooltip("Map_id=1,2时的X轴范围")]
    [SerializeField] private float xRangeForMap12 = 6.0f;
    
    [Header("Y轴范围（图一二）")]
    [SerializeField] private float yMax = 4.0f;
    [SerializeField] private float yMin = -4.0f;
    
    [Header("UI设置")]
    public TextMeshProUGUI winText;
    
    private bool isGameOver = false;
    //新增胜利判定
    public static string finalWinnerMessage = "";
    public static string lastLevelSceneName = "";

    void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        mainCamera.orthographic = true;
        
        // 根据Map_id设置相机大小
        if (Map_id == 3)
        {
            mainCamera.orthographicSize = cameraSizeForMap3;  // 缩小一半：5f
        }
        else
        {
            mainCamera.orthographicSize = cameraSize;  // 保持原来的5f
        }
        
        mainCamera.backgroundColor = backgroundColor;
        mainCamera.transform.position = new Vector3(0, 0, -10f);
    }

    void Start()
    {
        Time.timeScale = 1f;
        //新增
        lastLevelSceneName = SceneManager.GetActiveScene().name;

        if (winText != null) winText.gameObject.SetActive(false);
        SpawnCharacters();
    }

    void SpawnCharacters()
    {
        // 使用统一的CharacterLogic类
        List<CharacterLogic> allChars = new List<CharacterLogic>();
        float minDistance = 1.5f; 
        int maxAttempts = 10;     

        for (int i = 0; i < totalCharacters; i++)
        {
            Vector2 spawnPos = Vector2.zero;
            bool isPosValid = false;
            int attempts = 0;

            // 根据Map_id选择不同的生成范围
            float currentXRange, currentYMax, currentYMin;
            
            if (Map_id == 3)
            {
                // 图三的生成范围（已缩小一半）
                currentXRange = xRangeForMap3;  // 1.5f
                currentYMax = yMaxForMap3;      // 2.0f
                currentYMin = yMinForMap3;      // -2.0f
            }
            else
            {
                // 图一二的生成范围（保持不变）
                currentXRange = xRangeForMap12; // 6.0f
                currentYMax = yMax;             // 4.0f
                currentYMin = yMin;             // -4.0f
            }

            while (!isPosValid && attempts < maxAttempts)
            {
                attempts++;
                // 使用当前地图对应的生成范围
                spawnPos = new Vector2(Random.Range(-currentXRange, currentXRange), Random.Range(currentYMin, currentYMax));
                Debug.Log($"目前生成的地图ID: {Map_id}, 使用的X范围: {-currentXRange} to {currentXRange}, Y范围: {currentYMin} to {currentYMax}");

                isPosValid = true;
                foreach (var existingChar in allChars)
                {
                    if (existingChar != null && Vector2.Distance(spawnPos, (Vector2)existingChar.transform.position) < minDistance)
                    {
                        isPosValid = false;
                        break;
                    }
                }
            }

            GameObject go = Instantiate(charPrefab, spawnPos, Quaternion.identity);
            
            // 使用统一的CharacterLogic类
            CharacterLogic logic = go.GetComponent<CharacterLogic>();
            if (logic != null)
            {
                logic.debugMode = debugMode; 
                allChars.Add(logic);
            }
        }

        // 分配玩家角色
        if (allChars.Count >= 2)
        {
            int p1 = Random.Range(0, allChars.Count);
            int p2 = Random.Range(0, allChars.Count);
            while (p1 == p2) p2 = Random.Range(0, allChars.Count);

            allChars[p1].currentRole = CharacterLogic.Role.Player1;
            allChars[p2].currentRole = CharacterLogic.Role.Player2;
            
            if (debugMode)
            {
                if (allChars[p1].GetComponent<SpriteRenderer>() != null) 
                    allChars[p1].GetComponent<SpriteRenderer>().color = Color.blue;
                if (allChars[p2].GetComponent<SpriteRenderer>() != null) 
                    allChars[p2].GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void EndGame(string winnerMessage)
    {
        if (isGameOver) return;
        isGameOver = true;
        finalWinnerMessage = winnerMessage;

        Time.timeScale = 1f;
        SceneManager.LoadScene("VictoryScene");
    }
}