using UnityEngine;
using System.Collections;

public class CharacterLogic : MonoBehaviour
{
    public enum Role { Bot, Player1, Player2 }
    public Role currentRole = Role.Bot;

    [Header("移动设置")]
    public float moveSpeed = 4f;
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Vector2 lastMoveDir = Vector2.right; 
    private bool isStopped = false;

    [Header("攻击设置")]
    public float attackRange = 1.5f; // 稍微调长一点点

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        if (currentRole == Role.Bot) StartCoroutine(BotRoutine());
    }

    void Update() {
        // 核心修改：使用具体的键位，实现真正的双人隔离
        if (currentRole == Role.Player1) HandleP1();
        else if (currentRole == Role.Player2) HandleP2();
    }

    void FixedUpdate() {
    // 只有当既没有移动输入，且处于停止状态时，才锁死物理
    if (isStopped && moveDir == Vector2.zero) {
        rb.velocity = Vector2.zero;
        // 锁死位移和旋转
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
    } else {
        // 只要有任何移动意图，立刻解锁位移限制，只保留旋转锁定
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = moveDir * moveSpeed;
    }
}

    // --- P1: 严格锁定 WASD ---
    void HandleP1() {
        float x = 0; float y = 0;
        if (Input.GetKey(KeyCode.W)) y = 1;
        else if (Input.GetKey(KeyCode.S)) y = -1;
        if (Input.GetKey(KeyCode.A)) x = -1;
        else if (Input.GetKey(KeyCode.D)) x = 1;

        ProcessInput(x, y);

        if (Input.GetKeyDown(KeyCode.F)) Attack();
    }

    // --- P2: 严格锁定 方向键 + 小键盘0 ---
    void HandleP2() {
        float x = 0; float y = 0;
        if (Input.GetKey(KeyCode.UpArrow)) y = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) y = -1;
        if (Input.GetKey(KeyCode.LeftArrow)) x = -1;
        else if (Input.GetKey(KeyCode.RightArrow)) x = 1;

        ProcessInput(x, y);

        // 同时兼容大键盘0和小键盘0
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) Attack();
    }

   void ProcessInput(float x, float y) {
    Vector2 input = new Vector2(x, y).normalized;
    
    if (input != Vector2.zero) {
        // 【关键】只要有输入，就强制打破停止状态
        moveDir = input;
        lastMoveDir = input;
        isStopped = false; 
    } else {
        moveDir = Vector2.zero;
        isStopped = true;
    }
}

    void Attack() {
    Debug.Log(currentRole + " 挥出一记重拳！");

    // 1. 定义杀伤区的大小 (稍微比方块大一点，增加容错)
    Vector2 hitBoxSize = new Vector2(1.2f, 1.2f);
    
    // 2. 计算杀伤区的中心点
    // 我们把中心点往“上一次移动方向”偏移 0.8 个单位，确保它覆盖前方但不包含自己中心
    Vector2 hitBoxCenter = (Vector2)transform.position + lastMoveDir * 0.8f;

    // 3. 【核心】区域扫描：抓取这个矩形范围内的所有碰撞体
    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitBoxCenter, hitBoxSize, 0f);

    // 4. 视觉辅助（仅在Scene窗口可见，帮你调试杀伤区位置）
    // 你会看到一个红色的方框在出拳时闪现
    Debug.Log("攻击中心点: " + hitBoxCenter);

    foreach (var hit in hitColliders) {
        // 排除掉自己，且不能是边界
        if (hit.gameObject != this.gameObject && !hit.CompareTag("Boundary")) {
            
            Debug.Log(currentRole + " 成功击杀: " + hit.name);

            // 检查被击中的是不是玩家
            CharacterLogic target = hit.GetComponent<CharacterLogic>();
            if (target != null) {
                if (target.currentRole == Role.Player1) {
                    GameManager.instance.EndGame("玩家二 (红) 获胜！");
                } else if (target.currentRole == Role.Player2) {
                    GameManager.instance.EndGame("玩家一 (蓝) 获胜！");
                }
            }

            // 摧毁目标
            Destroy(hit.gameObject);
            
            // 如果你只想一拳打死一个人，就加个 return；想一拳打死一片，就不加
            return; 
        }
    }
}

// 建议加上这个，方便你在编辑器里直接看到攻击范围
void OnDrawGizmos() {
    Gizmos.color = Color.red;
    Vector2 hitBoxCenter = (Vector2)transform.position + lastMoveDir * 0.8f;
    Gizmos.DrawWireCube(hitBoxCenter, new Vector2(1.2f, 1.2f));
}

    IEnumerator BotRoutine() {
        while (true) {
            float angle = Random.Range(0f, 360f);
            moveDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            lastMoveDir = moveDir;
            isStopped = false;

            yield return new WaitForSeconds(Random.Range(1f, 3f));
            isStopped = true;
            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
    // 只有在没按键乱跑（比如Bot）或者刚撞上时才停
    // 这样不会干扰玩家后续的按键操作
    if (currentRole == Role.Bot) {
        isStopped = true;
        moveDir = Vector2.zero;
    }
}
}