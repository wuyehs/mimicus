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
        if (winText != null) winText.gameObject.SetActive(false);
        SpawnCharacters();
    }

    void SpawnCharacters()
    {
        List<CharacterLogic> allChars = new List<CharacterLogic>();
        float minDistance = 1.5f; 
        int maxAttempts = 10;     

        for (int i = 0; i < totalCharacters; i++)
        {
            Vector2 spawnPos = Vector2.zero;
            bool isPosValid = false;
            int attempts = 0;

            // 根据你的需求缩进的边界范围
            float xRange = 6.0f; 
            float yMax = 4.0f;   
            float yMin = -4.0f;  

            while (!isPosValid && attempts < maxAttempts)
            {
                attempts++;
                spawnPos = new Vector2(Random.Range(-xRange, xRange), Random.Range(yMin, yMax));

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
    } // 这里才是 SpawnCharacters 的正确结尾

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