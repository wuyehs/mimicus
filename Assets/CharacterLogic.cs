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
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // ===== 动画控制 =====
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false; // 新增：死亡标记
    private float attackAnimLength = 0.5f;

    private Camera mainCamera;
    private float leftBound, rightBound, topBound, bottomBound;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        smoothMoveDir = Vector2.zero;
        mainCamera = Camera.main;
        if (mainCamera != null) CalculateBounds();

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (currentRole == Role.Bot) StartCoroutine(BotRoutine());
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // 获取攻击动画时长
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name.Contains("Attack"))
                {
                    attackAnimLength = clip.length;
                    break;
                }
            }
        }
    }

    void Update()
    {
        // 死亡后不再更新任何动画与输入
        if (isDead) return;

        // 动画更新
        if (animator != null)
        {
            float currentSpeed = rb.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
            animator.SetFloat("MoveX", lastMoveDir.x);
            animator.SetFloat("MoveY", lastMoveDir.y);
        }

        // 玩家输入
        if (!isAttacking)
        {
            if (currentRole == Role.Player1) HandleP1();
            else if (currentRole == Role.Player2) HandleP2();
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        smoothMoveDir = Vector2.MoveTowards(smoothMoveDir, moveDir, smoothSpeed * Time.fixedDeltaTime);
        LimitToScreen();

        if (isStopped && smoothMoveDir == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.velocity = smoothMoveDir * moveSpeed;
        }
    }

    private void CalculateBounds()
    {
        float size = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        rightBound = size * aspect - 0.5f;
        leftBound = -rightBound;
        topBound = size - 0.5f;
        bottomBound = -topBound;
    }

    private void LimitToScreen()
    {
        if (mainCamera == null) return;
        Vector2 pos = rb.position;

        if (pos.x < leftBound) { pos.x = leftBound; if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; } }
        else if (pos.x > rightBound) { pos.x = rightBound; if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; } }
        if (pos.y < bottomBound) { pos.y = bottomBound; if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; } }
        else if (pos.y > topBound) { pos.y = topBound; if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; } }

        if (pos != rb.position) rb.MovePosition(pos);
    }

    void HandleP1()
    {
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.W)) y += 1;
        if (Input.GetKey(KeyCode.S)) y -= 1;
        if (Input.GetKey(KeyCode.A)) x -= 1;
        if (Input.GetKey(KeyCode.D)) x += 1;
        Vector2 rawInput = new Vector2(x, y).normalized;
        ProcessInput(rawInput.x, rawInput.y);

        if (Input.GetKeyDown(KeyCode.F) && CanAttack()) Attack();
    }

    void HandleP2()
    {
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.UpArrow)) y += 1;
        if (Input.GetKey(KeyCode.DownArrow)) y -= 1;
        if (Input.GetKey(KeyCode.LeftArrow)) x -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) x += 1;
        Vector2 rawInput = new Vector2(x, y).normalized;
        ProcessInput(rawInput.x, rawInput.y);

        if ((Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) && CanAttack()) Attack();
    }

    void ProcessInput(float x, float y)
    {
        Vector2 input = new Vector2(x, y);
        if (input.magnitude > 0.1f)
        {
            input.Normalize();
            moveDir = input;
            lastMoveDir = input;
            isStopped = false;
        }
        else
        {
            moveDir = Vector2.zero;
            isStopped = true;
        }
    }

    bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown && !isAttacking && !isDead;
    }

void Attack()
{
    lastAttackTime = Time.time;

    if (animator != null)
        animator.SetTrigger("Attack");

    isAttacking = true;
    moveDir = Vector2.zero;
    isStopped = true;
    StartCoroutine(WaitForAttackEnd());

    if (spriteRenderer != null)
        StartCoroutine(AttackFlash());

    Vector2 hitBoxCenter = (Vector2)transform.position + lastMoveDir * 0.8f;
    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitBoxCenter, new Vector2(1.2f, 1.2f), 0f);

    foreach (var hit in hitColliders)
    {
        if (hit.gameObject != gameObject && !hit.CompareTag("Boundary"))
        {
            CharacterLogic target = hit.GetComponent<CharacterLogic>();
            if (target != null)
            {
                target.Die(); // 触发死亡动画

                if (target.currentRole == Role.Player1)
                    GameManager.instance.EndGame("玩家二（红）获胜！");
                else if (target.currentRole == Role.Player2)
                    GameManager.instance.EndGame("玩家一（蓝）获胜！");
            }

            // 这里先不销毁！
            return;
        }
    }
}

// 死亡：播放动画 → 等动画结束 → 销毁
public void Die()
{
    if (isDead) return;
    isDead = true;

    if (animator != null)
    {
        animator.SetTrigger("Die");
        animator.SetFloat("Speed", 0);
    }

    rb.velocity = Vector2.zero;
    moveDir = Vector2.zero;
    isStopped = true;
    isAttacking = false;
    StopAllCoroutines();

    // 等待死亡动画播完再销毁
    StartCoroutine(DieAndDestroyAfterAnimation());
}

IEnumerator DieAndDestroyAfterAnimation()
{
    // 等待1秒（和你的死亡动画时长一致）
    yield return new WaitForSeconds(1f);
    Destroy(gameObject);
}

    IEnumerator WaitForAttackEnd()
    {
        float waitTime = attackAnimLength > 0 ? attackAnimLength : 0.5f;
        yield return new WaitForSeconds(waitTime);
        isAttacking = false;
    }

    IEnumerator AttackFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    IEnumerator BotRoutine()
    {
        while (true)
        {
            if (isDead) yield break;

            if (!isAttacking)
            {
                int behavior = Random.Range(0, 10);
                if (behavior != 0)
                {
                    float angle = Random.Range(0f, 360f);
                    moveDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                    lastMoveDir = moveDir;
                    isStopped = false;
                    float moveTime = Random.Range(0.5f, 2f);
                    yield return new WaitForSeconds(moveTime);
                }
                else
                {
                    isStopped = true;
                    moveDir = Vector2.zero;
                    float stopTime = Random.Range(0.3f, 1.5f);
                    yield return new WaitForSeconds(stopTime);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentRole == Role.Bot && !isAttacking && !isDead)
        {
            isStopped = true;
            moveDir = Vector2.zero;
        }
    }
}