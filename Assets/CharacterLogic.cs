using UnityEngine;
using System.Collections;

public class CharacterLogic : MonoBehaviour
{
    public enum Role { Bot, Player1, Player2 }
    public Role currentRole = Role.Bot;

    public bool debugMode = false;

    public enum WeaponType { Melee, Gun, Bomb}
    public WeaponType currentWeapon = WeaponType.Melee;   // 当前武器
    public Vector2 shootOffset = new Vector2(0.4f, 0f);  // 枪口偏移
    public float shootRange = 10f;   
    public float bombRadius = 2f;     
    
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
    
    // 默认颜色（非调试模式使用）
    private static readonly Color defaultColor = Color.white;

    // ===== 动画控制 =====
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;
    private float attackAnimLength = 0.5f;

    // 碰撞盒引用
    private Collider2D characterCollider;
    
    // 传送动画预制体
    [Header("测试特效")]
    public GameObject teleportEffectPrefab;  // 在Inspector中拖入teleport预制体
    public GameObject explosionEffectPrefab; 

    // ===== 烟雾弹系统 =====
    [Header("烟雾弹设置")]
    private bool hasSmokeEffect = false;               // 是否拥有烟雾弹效果
    public GameObject smokeEffectPrefab;               // 烟雾特效预制体（在Inspector中拖入）
    
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
    public void PickUpBomb()
    {
        currentWeapon = WeaponType.Bomb;
    }

    // ===== 烟雾弹拾取方法 =====
    public void PickUpSmokeGrenade()
    {
        hasSmokeEffect = true;
        // 可选：播放拾取音效，显示UI提示
    }

    // 获取动画方向
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
        float leftOffset = 1.5f;
        float rightOffset = 1f;
        float topOffset = 2.5f;        // 上边界缩回的距离（缩得多一些）
        float bottomOffset =-0.3f;  
    if (GameManager.instance.Map_id == 2)
    {
        leftOffset = 1.4f;
        rightOffset = 2.8f; // 左右边界缩回的距离（原为 0.5f）
        topOffset = 3f;        // 上边界缩回的距离（缩得多一些）
        bottomOffset =1.4f; 
    }

    rightBound = size * aspect - rightOffset;
    leftBound = -(size * aspect - leftOffset);
    
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

        if (currentWeapon == WeaponType.Gun)
        {
            // ==================== 枪械射击 ====================
            Vector2 shootDir = lastMoveDir.normalized;
            if (shootDir == Vector2.zero) shootDir = Vector2.right;
            
            Vector2 origin = (Vector2)transform.position + shootOffset + shootDir * 0.65f;
            origin.y += 1f;
            
            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, 0.25f, shootDir, shootRange);
            
            Debug.DrawRay(origin, shootDir * shootRange, Color.red, 0.8f);

            Collider2D[] hitColliders = System.Array.ConvertAll(hits, h => h.collider);

            ProcessHitTargets(hitColliders);
            
            currentWeapon = WeaponType.Melee;
        }
        else if (currentWeapon == WeaponType.Bomb)
        {
            // ==================== 炸弹攻击 ====================
            lastAttackTime = Time.time;

            if (animator != null)
                animator.SetTrigger("Attack");

            isAttacking = true;
            moveDir = Vector2.zero;
            isStopped = true;

            StartCoroutine(WaitForAttackEnd());
            if (spriteRenderer != null)
                StartCoroutine(AttackFlash());

            // 播放爆炸特效
            PlayExplosionAtPosition(transform.position + Vector3.up * 1f);
            
            // 检测爆炸范围内的所有人物
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position + Vector3.up * 1f, bombRadius);
            
            foreach (Collider2D hit in hitColliders)
            {
                if (hit == null || hit.gameObject == gameObject) continue;
                
                CharacterLogic target = hit.GetComponent<CharacterLogic>();
                if (target != null && !target.isDead)
                {
                    // 杀死范围内的所有人物
                    target.Die();
                    
                    // 检查是否有玩家死亡，触发游戏结束
                    if (target.currentRole == Role.Player1)
                        GameManager.instance.EndGame("玩家二（红）获胜！");
                    else if (target.currentRole == Role.Player2)
                        GameManager.instance.EndGame("玩家一（蓝）获胜！");
                }
            }
            
            // 炸弹使用后变回近战武器
            currentWeapon = WeaponType.Melee;
        }
        else
        {
            // ==================== 近战攻击 ====================
            lastAttackTime = Time.time;

            if (animator != null)
                animator.SetTrigger("Attack");

            isAttacking = true;
            moveDir = Vector2.zero;
            isStopped = true;

            StartCoroutine(WaitForAttackEnd());
            if (spriteRenderer != null)
                StartCoroutine(AttackFlash());

            Vector2 attackDir = lastMoveDir.normalized;
            if (attackDir == Vector2.zero) attackDir = Vector2.right;
            
            float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

            // 长度（朝攻击方向）加大，宽度适当
            Vector2 boxSize = new Vector2(1.5f, 1.35f);   // 第一个值是攻击方向长度，第二个是左右宽度
            Vector2 hitBoxCenter = (Vector2)transform.position + attackDir * 0.65f;
            hitBoxCenter.y += 1f;

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
            // ===== 烟雾弹效果：如果拥有，则在目标位置生成烟雾特效并消耗效果 =====
            if (hasSmokeEffect && smokeEffectPrefab != null)
            {
                GameObject smoke = Instantiate(smokeEffectPrefab, bestTarget.transform.position + Vector3.up * 1f, Quaternion.identity);
                hasSmokeEffect = false;
            }

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
            
            // === 新增代码：当Map_id是2且两个Bot相撞时 ===
            CheckBotCollision(collision.gameObject);
        }
    }
    
    // === 新增方法：检测Bot与Bot的碰撞 ===
    private void CheckBotCollision(GameObject otherObject)
    {
        // 检查GameManager是否存在且Map_id为2
        if (GameManager.instance == null || GameManager.instance.Map_id != 2)
            return;
            
        // 检查两个对象是否都是Bot
        CharacterLogic otherCharacter = otherObject.GetComponent<CharacterLogic>();
        if (otherCharacter == null) return;
        
        // 确保两个都是Bot角色且都不是玩家
        if (currentRole == Role.Bot && otherCharacter.currentRole == Role.Bot)
        {
            // 50%概率触发
            if (Random.Range(0f, 1f) <= 0.05f)
            {
                // 防止重复触发
                if (!isDead && !otherCharacter.isDead)
                {
                    // 在两个机器人上方分别创建teleport动画
                    PlayTeleportAtPosition(transform.position, 1.5f);
                    otherCharacter.PlayTeleportAtPosition(otherCharacter.transform.position, 1.5f);
                    
                    // 让两个Bot都消失
                    StartCoroutine(DelayedDestroyBoth(otherCharacter));
                }
            }
        }
    }
    
    private void PlayExplosionAtPosition(Vector3 position)
    {
        if (explosionEffectPrefab == null)
        {
            Debug.LogWarning("Explosion effect prefab is not assigned!");
            return;
        }
        
        // 创建爆炸特效
        GameObject explosion = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
        
        // 检查并设置SpriteRenderer
        SpriteRenderer explosionRenderer = explosion.GetComponent<SpriteRenderer>();
        if (explosionRenderer != null)
        {
            explosionRenderer.sortingOrder = 99; // 略低于传送特效
        }
        explosion.transform.localScale = new Vector3(bombRadius * 0.7f, bombRadius * 0.7f, 1f);
    }
    private IEnumerator DelayedDestroyBoth(CharacterLogic otherBot)
    {
        // 短暂延迟，确保视觉效果
        yield return new WaitForSeconds(0.05f);
        
        // 销毁当前对象
        if (!isDead)
        {
            isDead = true;
            if (animator != null)
                animator.SetTrigger("Die");
            rb.velocity = Vector2.zero;
            moveDir = Vector2.zero;
            isStopped = true;
            isAttacking = false;
        }
        
        // 销毁另一个Bot
        if (!otherBot.isDead)
        {
            otherBot.isDead = true;
            if (otherBot.animator != null)
                otherBot.animator.SetTrigger("Die");
            otherBot.rb.velocity = Vector2.zero;
            otherBot.moveDir = Vector2.zero;
            otherBot.isStopped = true;
            otherBot.isAttacking = false;
        }
        
        // 等待死亡动画（如果有的话）
        yield return new WaitForSeconds(0.5f);
        
        // 销毁两个游戏对象
        Destroy(gameObject);
        Destroy(otherBot.gameObject);
    }
    
    // === 新增方法：在指定位置上方播放teleport动画 ===
    public void PlayTeleportAtPosition(Vector3 position, float yOffset = 1.5f)
    {   
        // 计算特效位置（机器人正上方）
        Vector3 effectPosition = new Vector3(position.x, position.y + yOffset, position.z);
        
        // 创建特效
        GameObject effect = Instantiate(teleportEffectPrefab, effectPosition, Quaternion.identity);
        
        // 检查并设置SpriteRenderer
        SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
        if (effectRenderer != null)
        {
            effectRenderer.sortingOrder = 100; // 设置高排序层级
        }
        
        // 检查Animator
        Animator effectAnimator = effect.GetComponent<Animator>();
    }
}