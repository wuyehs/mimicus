using UnityEngine;
using System.Collections;

public class CharacterLogic : MonoBehaviour
{
    public enum Role { Bot, Player1, Player2 }
    public Role currentRole = Role.Bot;

    public bool debugMode = false;

    public enum WeaponType { Melee, Gun }
    public WeaponType currentWeapon = WeaponType.Melee;   // 当前武器
    public Vector2 shootOffset = new Vector2(0.4f, 0f);  // 枪口偏移
    public float shootRange = 10f;       
    
    [Header("移动设置")]
    public float moveSpeed = 2f;
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
    
    // 新增：默认颜色（非调试模式使用）
    private static readonly Color defaultColor = Color.white;

    // ===== 动画控制 =====
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;
    private float attackAnimLength = 0.5f;

    // 新增：碰撞盒引用
    private Collider2D characterCollider;
    
    private Camera mainCamera;
    private float leftBound, rightBound, topBound, bottomBound;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        smoothMoveDir = Vector2.zero;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        characterCollider = GetComponent<Collider2D>();
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(0.3125f, 0.46875f);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null) CalculateBounds();

        debugMode = GameManager.instance.debugMode;
        
        if (currentRole == Role.Bot) StartCoroutine(BotRoutine());

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
        if (isDead) return;
        

        if (animator != null)
        {
            float currentSpeed = rb.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
            
            Vector2 animDir = GetAnimationDirection(lastMoveDir);
            animator.SetFloat("MoveX", animDir.x);
            animator.SetFloat("MoveY", animDir.y);
        }

        if (!isAttacking)
        {
            if (currentRole == Role.Player1) HandleP1();
            else if (currentRole == Role.Player2) HandleP2();
        }
    }
    
    public void PickUpGun()
    {
        currentWeapon = WeaponType.Gun;
    }
    // 修改：获取动画方向

    private Vector2 GetAnimationDirection(Vector2 inputDir)
    {
        if (inputDir.magnitude < 0.1f) return Vector2.right;
        
        // 判断是否为纯下方向移动
        float horizontalThreshold = 0.2f;
        
        if (inputDir.y < 0)
        {
            if (Mathf.Abs(inputDir.x) < horizontalThreshold)
            {
                return new Vector2(0, -1);
            }
            else
            {
                return new Vector2(Mathf.Sign(inputDir.x), 0);
            }
        }
        
        return inputDir.normalized;
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
    float horizontalOffset = 1f; // 左右边界缩回的距离（原为 0.5f）
    float topOffset = 2.5f;        // 上边界缩回的距离（缩得多一些）
    float bottomOffset =-0.3f;     // 下边界保持原样或微调

    rightBound = size * aspect - horizontalOffset;
    leftBound = -rightBound;
    
    topBound = size - topOffset;
    bottomBound = -(size - bottomOffset); 
    // --- 修改结束 ---
    }

   private void LimitToScreen()
{
    if (mainCamera == null) return;
    Vector2 pos = rb.position;
    Vector2 vel = rb.velocity; // 获取速度
    bool changed = false;

    if (pos.x < leftBound) { pos.x = leftBound; vel.x = 0; changed = true; }
    else if (pos.x > rightBound) { pos.x = rightBound; vel.x = 0; changed = true; }
    
    if (pos.y < bottomBound) { pos.y = bottomBound; vel.y = 0; changed = true; }
    else if (pos.y > topBound) { pos.y = topBound; vel.y = 0; changed = true; }

    if (changed)
    {
        rb.position = pos; // 修正坐标
        rb.velocity = vel; // 关键：撞墙瞬间必须杀掉速度，防止抖动
        if (currentRole == Role.Bot) { isStopped = true; moveDir = Vector2.zero; }
    }
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
    if (isDead || isAttacking) return;

    Vector2 attackDir = lastMoveDir.normalized;
    if (attackDir == Vector2.zero) attackDir = Vector2.right;
    Vector2 hitBoxCenter = (Vector2)transform.position + attackDir * 0.65f;   // 往回拉一点，更容易覆盖正前方
    hitBoxCenter.y += 1f;

    if (currentWeapon == WeaponType.Gun)
    {
        // ==================== 枪械射击 ====================
        Vector2 origin = hitBoxCenter + shootOffset * attackDir;
        
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, 0.25f, attackDir, shootRange);
        
        Debug.DrawRay(origin, attackDir * shootRange, Color.red, 0.8f);

        Collider2D[] hitColliders = System.Array.ConvertAll(hits, h => h.collider);

        ProcessHitTargets(hitColliders);
        
        currentWeapon = WeaponType.Melee;
    }
    else
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

        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        // 长度（朝攻击方向）加大，宽度适当
        Vector2 boxSize = new Vector2(1.5f, 1.35f);   // 第一个值是攻击方向长度，第二个是左右宽度

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitBoxCenter, boxSize, angle);
        
        // 可选：画出盒子的四个角（更直观）
        Vector2 right = new Vector2(-attackDir.y, attackDir.x); // 垂直向量
        Vector2 halfSize = boxSize * 0.5f;
        
        Vector2 p1 = hitBoxCenter + attackDir * halfSize.x + right * halfSize.y;
        Vector2 p2 = hitBoxCenter + attackDir * halfSize.x - right * halfSize.y;
        Vector2 p3 = hitBoxCenter - attackDir * halfSize.x - right * halfSize.y;
        Vector2 p4 = hitBoxCenter - attackDir * halfSize.x + right * halfSize.y;

        ProcessHitTargets(hitColliders);
    }
}

// ========== 公共目标处理 ==========
private void ProcessHitTargets(Collider2D[] hits)
{
    CharacterLogic bestTarget = null;
    float nearestDistance = float.MaxValue;
    Vector2 attackerPos = transform.position;

    foreach (var hit in hits)
    {
        if (hit == null) continue;
        if (hit.gameObject == gameObject) continue;
        if (hit.CompareTag("Boundary")) continue;

        CharacterLogic target = hit.GetComponent<CharacterLogic>();
        if (target != null && !target.isDead)
        {
            float distance = Vector2.Distance(attackerPos, target.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                bestTarget = target;
            }
        }
    }

    if (bestTarget != null)
    {
        bestTarget.Die();

        if (bestTarget.currentRole == Role.Player1)
            GameManager.instance.EndGame("玩家二（红）获胜！");
        else if (bestTarget.currentRole == Role.Player2)
            GameManager.instance.EndGame("玩家一（蓝）获胜！");
    }
}

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

        StartCoroutine(DieAndDestroyAfterAnimation());
    }

    IEnumerator DieAndDestroyAfterAnimation()
    {
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
            // 在非调试模式下，闪光效果可能不太明显
            Color flashColor = debugMode ? Color.yellow : new Color(1f, 1f, 0.8f, 1f);
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = debugMode ? 
                (currentRole == Role.Player1 ? Color.blue : 
                 currentRole == Role.Player2 ? Color.red : 
                 currentRole == Role.Bot ? Color.gray : defaultColor) : defaultColor;
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