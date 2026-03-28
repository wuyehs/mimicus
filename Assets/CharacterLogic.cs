using UnityEngine;
using System.Collections;

public class CharacterLogic : MonoBehaviour
{
    public enum Role { Bot, Player1, Player2 }
    public Role currentRole = Role.Bot;

    [Header("移动设置")]
    public float moveSpeed = 4f;
    public float smoothSpeed = 10f;
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Vector2 smoothMoveDir;
    private Vector2 lastMoveDir = Vector2.right; 
    private bool isStopped = false;

    [Header("攻击设置")]
    public float attackRange = 1.5f;
    public float attackCooldown = 5f;
    private float lastAttackTime = 0f;
    
    private Camera mainCamera;
    private float leftBound, rightBound, topBound, bottomBound;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        smoothMoveDir = Vector2.zero;
        mainCamera = Camera.main;
        if (mainCamera != null) {
            CalculateBounds();
        }
    }

    void Start() {
        if (currentRole == Role.Bot) StartCoroutine(BotRoutine());
    }

    void Update() {
        if (currentRole == Role.Player1) HandleP1();
        else if (currentRole == Role.Player2) HandleP2();
    }

    void FixedUpdate() {
        smoothMoveDir = Vector2.MoveTowards(smoothMoveDir, moveDir, smoothSpeed * Time.fixedDeltaTime);
        
        LimitToScreen();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (isStopped && smoothMoveDir == Vector2.zero) {
            rb.velocity = Vector2.zero;
        } else {
            rb.velocity = smoothMoveDir * moveSpeed;
        }
    }
    
    private void CalculateBounds() {
        float size = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        
        rightBound = size * aspect - 0.5f;
        leftBound = -rightBound;
        topBound = size - 0.5f;
        bottomBound = -topBound;
    }
    
    private void LimitToScreen() {
        if (mainCamera == null) return;
        
        Vector2 pos = rb.position;
        
        if (pos.x < leftBound) {
            pos.x = leftBound;
            if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; }
        } else if (pos.x > rightBound) {
            pos.x = rightBound;
            if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; }
        }
        
        if (pos.y < bottomBound) {
            pos.y = bottomBound;
            if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; }
        } else if (pos.y > topBound) {
            pos.y = topBound;
            if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; }
        }
        
        if (pos != rb.position) rb.MovePosition(pos);
    }

    void HandleP1() {
        // 修改：使用独立的按键检测，允许同时按下
        float x = 0;
        float y = 0;
        
        // 垂直方向
        if (Input.GetKey(KeyCode.W)) y += 1;
        if (Input.GetKey(KeyCode.S)) y -= 1;  // 移除else，允许同时检测
        
        // 水平方向
        if (Input.GetKey(KeyCode.A)) x -= 1;
        if (Input.GetKey(KeyCode.D)) x += 1;  // 移除else，允许同时检测
        
        // 标准化输入向量
        Vector2 rawInput = new Vector2(x, y);
        
        // 如果有输入，进行标准化
        if (rawInput.magnitude > 0) {
            rawInput.Normalize();
        }
        
        // 调用ProcessInput
        ProcessInput(rawInput.x, rawInput.y);
        
        if (Input.GetKeyDown(KeyCode.F) && CanAttack()) Attack();
    }

    void HandleP2() {
        float x = 0;
        float y = 0;
        
        // 垂直方向
        if (Input.GetKey(KeyCode.UpArrow)) y += 1;
        if (Input.GetKey(KeyCode.DownArrow)) y -= 1;
        
        // 水平方向
        if (Input.GetKey(KeyCode.LeftArrow)) x -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) x += 1;
        
        Vector2 rawInput = new Vector2(x, y);
        if (rawInput.magnitude > 0) {
            rawInput.Normalize();
        }
        
        ProcessInput(rawInput.x, rawInput.y);
        
        if ((Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) && CanAttack()) Attack();
    }

    void ProcessInput(float x, float y) {
        Vector2 input = new Vector2(x, y);
        if (input.magnitude > 0.1f) {  // 使用小阈值避免抖动
            input.Normalize();
            moveDir = input;
            lastMoveDir = input;
            isStopped = false; 
        } else {
            moveDir = Vector2.zero;
            isStopped = true;
        }
    }
    
    bool CanAttack() {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    void Attack() {
        lastAttackTime = Time.time;
        
        Vector2 hitBoxCenter = (Vector2)transform.position + lastMoveDir * 0.8f;
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitBoxCenter, new Vector2(1.2f, 1.2f), 0f);

        foreach (var hit in hitColliders) {
            if (hit.gameObject != this.gameObject && !hit.CompareTag("Boundary")) {
                CharacterLogic target = hit.GetComponent<CharacterLogic>();
                if (target != null) {
                    if (target.currentRole == Role.Player1) {
                        GameManager.instance.EndGame("玩家二 (红) 获胜！");
                    } else if (target.currentRole == Role.Player2) {
                        GameManager.instance.EndGame("玩家一 (蓝) 获胜！");
                    }
                }
                Destroy(hit.gameObject);
                return; 
            }
        }
    }

    IEnumerator BotRoutine() {
        while (true) {
            int behavior = Random.Range(0, 10);
            
            if (behavior != 0) {
                float angle = Random.Range(0f, 360f);
                moveDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                lastMoveDir = moveDir;
                isStopped = false;
                
                float moveTime = Random.Range(0.5f, 2f);
                yield return new WaitForSeconds(moveTime);
            }
            else {
                isStopped = true;
                moveDir = Vector2.zero;
                
                float stopTime = Random.Range(0.3f, 1.5f);
                yield return new WaitForSeconds(stopTime);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (currentRole == Role.Bot) {
            isStopped = true;
            moveDir = Vector2.zero;
        }
    }
}