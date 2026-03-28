using UnityEngine;
using System.Collections;

public class CharacterLogic : MonoBehaviour
{
    public enum Role { Bot, Player1, Player2 }
    public Role currentRole = Role.Bot;

    [Header("移动设置")]
    public float moveSpeed = 4f;
    public float smoothSpeed = 10f;  // 新增：平滑过渡速度
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Vector2 smoothMoveDir;  // 新增：平滑后的移动方向
    private Vector2 lastMoveDir = Vector2.right; 
    private bool isStopped = false;

    [Header("攻击设置")]
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;
    
    private Camera mainCamera;
    private float leftBound, rightBound, topBound, bottomBound;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        smoothMoveDir = Vector2.zero;  // 初始化
        mainCamera = Camera.main;
        if (mainCamera != null) {
            CalculateBounds();
        }
    }

    void Start() {
        if (currentRole == Role.Bot) StartCoroutine(UnspottableBotRoutine());
    }

    void Update() {
        if (currentRole == Role.Player1) HandleP1();
        else if (currentRole == Role.Player2) HandleP2();
    }

    void FixedUpdate() {
        // 平滑过渡
        smoothMoveDir = Vector2.MoveTowards(smoothMoveDir, moveDir, smoothSpeed * Time.fixedDeltaTime);
        
        LimitToScreen();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (isStopped && smoothMoveDir == Vector2.zero) {
            rb.velocity = Vector2.zero;
        } else {
            rb.velocity = smoothMoveDir * moveSpeed;  // 使用平滑后的方向
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
        float x = 0; float y = 0;
        if (Input.GetKey(KeyCode.W)) y = 1;
        else if (Input.GetKey(KeyCode.S)) y = -1;
        if (Input.GetKey(KeyCode.A)) x = -1;
        else if (Input.GetKey(KeyCode.D)) x = 1;
        ProcessInput(x, y);
        if (Input.GetKeyDown(KeyCode.F) && CanAttack()) Attack();
    }

    void HandleP2() {
        float x = 0; float y = 0;
        if (Input.GetKey(KeyCode.UpArrow)) y = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) y = -1;
        if (Input.GetKey(KeyCode.LeftArrow)) x = -1;
        else if (Input.GetKey(KeyCode.RightArrow)) x = 1;
        ProcessInput(x, y);
        if ((Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) && CanAttack()) Attack();
    }

    void ProcessInput(float x, float y) {
        Vector2 input = new Vector2(x, y).normalized;
        if (input != Vector2.zero) {
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

    // 在编辑器里画出攻击范围，方便你调试
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + lastMoveDir * attackOffset;
        Gizmos.DrawWireSphere(center, attackRadius);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (currentRole == Role.Bot) {
            isStopped = true;
            moveDir = Vector2.zero;
        }
    }
}