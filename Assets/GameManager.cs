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
    public bool debugMode = true;  // 新增：调试模式开关，与CharacterLogic保持一致
    
    [Header("摄像机设置")]
    [SerializeField] private float cameraSize = 5f;
    [SerializeField] private Color backgroundColor = Color.black;

    [Header("游戏设置")]
    public GameObject charPrefab;
    public int totalCharacters = 20;
    
    [Header("UI设置")]
    public TextMeshProUGUI winText;
    
    private bool isGameOver = false;

    void Awake()
    {
        instance = this;
    
        mainCamera = Camera.main;
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = cameraSize;
        mainCamera.backgroundColor = backgroundColor;
        mainCamera.transform.position = new Vector3(0, 0, -10f);
    }

    void Start()
    {
        Time.timeScale = 1f;
        
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
            CharacterLogic logic = go.GetComponent<CharacterLogic>();
            
            // 设置调试模式
            if (logic != null)
            {
                logic.debugMode = debugMode;  // 传递调试模式设置
            }
            
            allChars.Add(logic);
        }

        // 随机分配玩家角色
        int p1 = Random.Range(0, totalCharacters);
        int p2 = Random.Range(0, totalCharacters);
        while (p1 == p2) p2 = Random.Range(0, totalCharacters);

        allChars[p1].currentRole = CharacterLogic.Role.Player1;
        allChars[p2].currentRole = CharacterLogic.Role.Player2;
        
        // 在调试模式下才染色
        if (debugMode)
        {
            SpriteRenderer sr1 = allChars[p1].GetComponent<SpriteRenderer>();
            SpriteRenderer sr2 = allChars[p2].GetComponent<SpriteRenderer>();
            
            if (sr1 != null) sr1.color = Color.blue;
            if (sr2 != null) sr2.color = Color.red;
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

        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = winnerMessage + "\npress R key to restart";
        }

        Time.timeScale = 0f;
    }
}