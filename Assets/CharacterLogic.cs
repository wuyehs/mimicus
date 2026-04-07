using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class CharacterLogic : MonoBehaviour
{
    public enum Role { Bot, Player1, Player2 }
    public Role currentRole = Role.Bot;

    public bool debugMode = false;

    // 统一的武器/工具系统
    [Header("工具系统")]
    public ToolType currentTool = ToolType.None;
    public enum ToolType { Smoke, Gun, Bomb, PhantomStaff, None }
    
    public Vector2 shootOffset = new Vector2(0.4f, 0f);
    public float shootRange = 10f;       
    public float bombRadius = 3f;     
    public float bombCountdown = 3f;  // 新增：炸弹倒计时时长
    [Header("UI 相关")]
    public TextMeshProUGUI player1CDText;  // 分配给 Player1
    public TextMeshProUGUI player2CDText;  // 分配给 Player2
    private float currentAttackTimer = 0f;
        
    [Header("炸弹倒计时UI")]
    public GameObject bombCountdownUI;  // 倒计时UI预制体
    
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
    
    private static readonly Color defaultColor = Color.white;
    // ===== 病毒参数 =====
    private float InfectedTime = 0f;
    private bool isInfected = false;

    // ===== 动画控制 =====
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;
    private float attackAnimLength = 0.5f;
    
    // 新增：炸弹相关变量
    private float bombTimer = 0f;
    private bool bombActive = false;
    private GameObject bombUIInstance = null;
    private TextMeshProUGUI bombText = null;

    private Collider2D characterCollider;
    
    [Header("特效预制体")]
    public GameObject teleportEffectPrefab;
    public GameObject explosionEffectPrefab; 
    public GameObject smokeEffectPrefab;
    
    private Camera mainCamera;
    private bool bombAnimation = false;

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

        debugMode = GameManager.instance.debugMode;
        
        if (currentRole == Role.Bot) StartCoroutine(BotRoutine());

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
        if(GameManager.instance.Map_id == 4 && isInfected)
        {
           UpdateInfection(); 
        } 
        UpdateAttackCooldownDisplay();
        // 新增：炸弹倒计时更新
        if (bombActive)
        {
            bombTimer -= Time.deltaTime;
            UpdateBombUI();
            if(bombTimer <= 0.6f && bombAnimation)
            {
                bombAnimation = false;
                PlayExplosionAtPosition(transform.position + Vector3.up * 1f);
            }
            if (bombTimer <= 0f)
            {
                ExplodeBomb();
            }
        }

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

    private void UpdateAttackCooldownDisplay()
    {
        float timeSinceAttack = Time.time - lastAttackTime;
        float remainingTime = Mathf.Max(0, attackCooldown - timeSinceAttack);
        
        // 格式化显示（如 "5.0" 秒）
        string displayText = remainingTime > 0 ? remainingTime.ToString("F1") + "s" : "Ready";
        
        // 根据角色更新对应的文本
        if (currentRole == Role.Player1 && player1CDText != null)
        {
            player1CDText.text = "Player1: " + displayText;
            
            // 颜色变化：红色表示冷却中，绿色表示可用
            if (remainingTime > 0)
            {
                player1CDText.color = Color.red;
            }
            else
            {
                player1CDText.color = Color.white;
            }
        }
        else if (currentRole == Role.Player2 && player2CDText != null)
        {
            player2CDText.text = "Player2: " + displayText;
            
            if (remainingTime > 0)
            {
                player2CDText.color = Color.red;
            }
            else
            {
                player2CDText.color = Color.white;
            }
        }
    }


    public void UpdateInfection()
    {
        float infectionDuration = Time.time - InfectedTime;
        if(infectionDuration >= 30f)
        {
            animator.SetTrigger("Die");
            Die();
            if (GameManager.instance != null)
            { 
                if (currentRole == Role.Player1)
                {
                    GameManager.instance.EndGame("PLAYER 2 WINS!");
                }
                else if (currentRole == Role.Player2)
                {
                    GameManager.instance.EndGame("PLAYER 1 WINS!");
                }
            }
        }
        float infectionProgress = infectionDuration / 30f;
        Color infectedColor = Color.Lerp(defaultColor, Color.green, infectionProgress);
        spriteRenderer.color = infectedColor;
    }
    public void SetInfected()
    {
        if(isInfected)return;
        InfectedTime = Time.time;
        isInfected = true;
    }


    public void PickUpGun()
    {
        currentTool = ToolType.Gun;
    }
    
    public void PickUpBomb()
    {
        currentTool = ToolType.Bomb;
    }

    public void PickUpSmokeGrenade()
    {
        currentTool = ToolType.Smoke;
    }

    public void PickUpPhantomStaff()
    {
        currentTool = ToolType.PhantomStaff;
    }

    private Vector2 GetAnimationDirection(Vector2 inputDir)
    {
        if (inputDir.magnitude < 0.1f) return Vector2.right;
        
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

    private void LimitToScreen()
    {
        if (mainCamera == null) return;
        Vector2 pos = rb.position;
        Vector2 vel = rb.velocity;
        bool changed = false;

        if (changed)
        {
            rb.position = pos;
            rb.velocity = vel;
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
        if (Input.GetKeyDown(KeyCode.G) && currentTool != ToolType.None) UseTool();
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
        if (Input.GetKeyDown(KeyCode.Minus) && currentTool != ToolType.None) UseTool();
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
    
    void UseTool()
    {
        if (currentTool == ToolType.Gun)
        {
            Vector2 shootDir = lastMoveDir.normalized;
            if (shootDir == Vector2.zero) shootDir = Vector2.right;
            
            Vector2 origin = (Vector2)transform.position + shootOffset + shootDir * 0.65f;
            origin.y += 1f;
            
            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, 0.25f, shootDir, shootRange);
            
            Debug.DrawRay(origin, shootDir * shootRange, Color.red, 0.8f);

            Collider2D[] hitColliders = System.Array.ConvertAll(hits, h => h.collider);
            ProcessHitTargets(hitColliders);
        }
        else if (currentTool == ToolType.Bomb)
        {
            StartBombCountdown();
            bombAnimation = true;
        }
        else if (currentTool == ToolType.Smoke)
        {
            Instantiate(smokeEffectPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        }
        else if (currentTool == ToolType.PhantomStaff)
        {
            SummonPhantomAtEnemy();
        }
        
        currentTool = ToolType.None;
    }
    
    // 新增：启动炸弹倒计时
    private void StartBombCountdown()
    {
        bombTimer = bombCountdown;
        bombActive = true;
        
        // 创建UI
        if (bombCountdownUI != null && bombUIInstance == null)
        {
            bombUIInstance = Instantiate(bombCountdownUI, transform);
            
            // 获取TextMeshPro组件
            bombText = bombUIInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (bombText != null)
            {
                bombText.text = Mathf.CeilToInt(bombTimer).ToString();
            }
        }
    }
    
    // 新增：更新炸弹UI
    private void UpdateBombUI()
    {
        if (bombUIInstance != null)
        {
            // 跟随角色
            bombUIInstance.transform.position = transform.position  + Vector3.right * 0.9f + Vector3.up * 2f;
            
            // 更新倒计时显示
            if (bombText != null)
            {
                int displayTime = Mathf.CeilToInt(bombTimer);
                bombText.text = displayTime.ToString();
                
                // 颜色闪烁效果
                if (bombTimer <= 3f)
                {
                    bombText.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 5f, 1f));
                }
                else
                {
                    bombText.color = Color.yellow;
                }
            }
        }
    }
    
    // 新增：炸弹爆炸
    private void ExplodeBomb()
    {
        bombActive = false;
        
        // 检测爆炸范围内的所有角色
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position + Vector3.up * 1f, bombRadius);
        
        bool killedEnemyPlayer = false;
        bool killedSelf = false;
        
        foreach (Collider2D hit in hitColliders)
        {
            if (hit == null || hit.gameObject == gameObject) continue;
            
            CharacterLogic target = hit.GetComponent<CharacterLogic>();
            if (target != null && !target.isDead)
            {
                // 杀死范围内的所有人物
                target.Die();
                
                // 检查是否杀死了敌方玩家
                if (target.currentRole != currentRole)
                {
                    if ( target.currentRole != Role.Bot)
                    {
                        killedEnemyPlayer = true;
                    }
                }
            }
        }
        
        // 清理UI
        if (bombUIInstance != null)
        {
            Destroy(bombUIInstance);
            bombUIInstance = null;
            bombText = null;
        }
        
        // 重置工具
        currentTool = ToolType.None;
        
        // 判定胜负
        if (GameManager.instance != null)
        {
            if (killedEnemyPlayer)
            {
                // 杀死了敌方玩家，当前玩家获胜
                if (currentRole == Role.Player1)
                {
                    GameManager.instance.EndGame("PLAYER 1 WINS!");
                }
                else if (currentRole == Role.Player2)
                {
                    GameManager.instance.EndGame("PLAYER 2 WINS!");
                }
            }
            else
            {
                // 只炸死了自己，对方获胜
                if (currentRole == Role.Player1)
                {
                    GameManager.instance.EndGame("PLAYER 2 WINS!");
                }
                else if (currentRole == Role.Player2)
                {
                    GameManager.instance.EndGame("PLAYER 1 WINS!");
                }
            }
        }
    }
    
    // 修改：死亡时清理炸弹
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // 清理炸弹UI
        if (bombUIInstance != null)
        {
            Destroy(bombUIInstance);
            bombUIInstance = null;
            bombText = null;
        }
        
        // 停止炸弹倒计时
        bombActive = false;

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

    void Attack()
    {
        if (isDead || isAttacking) return;        
        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack");
        UpdateAttackCooldownDisplay();
        isAttacking = true;
        moveDir = Vector2.zero;
        isStopped = true;

        StartCoroutine(WaitForAttackEnd());
        if (spriteRenderer != null)
            StartCoroutine(AttackFlash());

        Vector2 attackDir = lastMoveDir.normalized;
        if (attackDir == Vector2.zero) attackDir = Vector2.right;
        
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        Vector2 boxSize = new Vector2(0.5f, 0.45f);
        Vector2 hitBoxCenter = (Vector2)transform.position + attackDir * 0.5f;
        hitBoxCenter.y += 1f;

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitBoxCenter, boxSize, angle);
        ProcessHitTargets(hitColliders);
    }
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
                GameManager.instance.EndGame("PLAYER 2 WINS!");
            else if (bestTarget.currentRole == Role.Player2)
                GameManager.instance.EndGame("PLAYER 1 WINS!");
        }
    }

    // ==================== 幻影法杖召唤逻辑 ====================
    private void SummonPhantomAtEnemy()
    {
        // 1. 确定敌方玩家
        Role enemyRole = (this.currentRole == Role.Player1) ? Role.Player2 : Role.Player1;

        // 2. 查找敌方玩家对象
        CharacterLogic enemy = null;
        CharacterLogic[] allChars = FindObjectsOfType<CharacterLogic>();
        foreach (var c in allChars)
        {
            if (c.currentRole == enemyRole && !c.isDead)
            {
                enemy = c;
                break;
            }
        }
        if (enemy == null) return;

        // 3. 随机生成点（敌方周围 2~4 单位）
        Vector2 spawnPos = GetRandomPositionNear(enemy.transform.position, 0.2f, 1f);

        // 4. 实例化新的 AI（使用 GameManager 中的角色预制体）
        if (GameManager.instance == null || GameManager.instance.charPrefab == null)
        {
            Debug.LogError("GameManager 或 charPrefab 未设置！");
            return;
        }

        GameObject newBotObj = Instantiate(GameManager.instance.charPrefab, spawnPos, Quaternion.identity);
        CharacterLogic newBot = newBotObj.GetComponent<CharacterLogic>();
        if (newBot != null)
        {
            // 设置为 Bot 角色
            newBot.currentRole = Role.Bot;
            newBot.debugMode = GameManager.instance.debugMode;
            // 可选：同步颜色（调试模式）
            if (newBot.debugMode)
            {
                SpriteRenderer sr = newBot.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.gray;
            }
            // 可选：添加短暂无敌或标记为“幻影”（此处无特殊行为）
        }

        // 5. 播放召唤特效（复用传送特效）
        PlayTeleportAtPosition(spawnPos, 0f);
    }

    private Vector2 GetRandomPositionNear(Vector2 center, float minRadius, float maxRadius)
    {
        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2);
            float radius = Random.Range(minRadius, maxRadius);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector2 candidate = center + offset;

            // 简单边界检测（使用 Camera 边界，避免生成到墙外）
            if (mainCamera != null)
            {
                Vector3 viewPos = mainCamera.WorldToViewportPoint(candidate);
                if (viewPos.x > 0.1f && viewPos.x < 0.9f && viewPos.y > 0.1f && viewPos.y < 0.9f)
                    return candidate;
            }
            else
            {
                return candidate;
            }
        }
        // 保底位置：敌方位置稍微偏移
        return center + new Vector2(1f, 0);
    }
    // ==================== 召唤逻辑结束 ====================

    

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
                int behavior = Random.Range(0, 3);
                if (behavior != 0)
                {
                    float angle = Random.Range(0f, 360f);
                    moveDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                    lastMoveDir = moveDir;
                    isStopped = false;
                    float moveTime = Random.Range(0.3f, 1f);
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
            
            CheckBotCollision(collision.gameObject);
        }
    }
    
    private void CheckBotCollision(GameObject otherObject)
    {
        if (GameManager.instance == null || (GameManager.instance.Map_id != 2 && GameManager.instance.Map_id != 4))
            return;
            
        CharacterLogic otherCharacter = otherObject.GetComponent<CharacterLogic>();
        if (otherCharacter == null) return;
        if(GameManager.instance.Map_id == 4 && isInfected)
        { 
            if(Random.Range(0f, 1f) <= 0.5f)
            {
                otherCharacter.SetInfected();
            }
        }
        if (currentRole == Role.Bot && otherCharacter.currentRole == Role.Bot && GameManager.instance.Map_id == 2)
        {
            if (Random.Range(0f, 1f) <= 0.05f)
            {
                if (!isDead && !otherCharacter.isDead)
                {
                    PlayTeleportAtPosition(transform.position, 1.5f);
                    otherCharacter.PlayTeleportAtPosition(otherCharacter.transform.position, 1.5f);
                    
                    StartCoroutine(DelayedDestroyBoth(otherCharacter));
                }
            }
        }
    }
    
    private IEnumerator DelayedDestroyBoth(CharacterLogic otherBot)
    {
        yield return new WaitForSeconds(0.05f);
        
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
        
        yield return new WaitForSeconds(0.5f);
        
        Destroy(gameObject);
        Destroy(otherBot.gameObject);
    }
    
    private void PlayExplosionAtPosition(Vector3 position)
    {
        if (explosionEffectPrefab == null) return;
        
        GameObject explosion = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
        
        SpriteRenderer explosionRenderer = explosion.GetComponent<SpriteRenderer>();
        if (explosionRenderer != null)
        {
            explosionRenderer.sortingOrder = 99;
        }
        explosion.transform.localScale = new Vector3(bombRadius * 0.7f, bombRadius * 0.7f, 1f);
    }
    
    public void PlayTeleportAtPosition(Vector3 position, float yOffset = 1.5f)
    {   
        if (teleportEffectPrefab == null) return;
        
        Vector3 effectPosition = new Vector3(position.x, position.y + yOffset+1.5f, position.z);
        GameObject effect = Instantiate(teleportEffectPrefab, effectPosition, Quaternion.identity);
        
        SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
        if (effectRenderer != null)
        {
            effectRenderer.sortingOrder = 100;
        }
    }
}