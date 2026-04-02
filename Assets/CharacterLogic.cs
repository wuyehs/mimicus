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
    public enum ToolType { Smoke, Gun, Bomb, None }
    
    public Vector2 shootOffset = new Vector2(0.4f, 0f);
    public float shootRange = 10f;       
    public float bombRadius = 3f;     
    public float bombCountdown = 3f;  // 新增：炸弹倒计时时长
    
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
    private float leftBound, rightBound, topBound, bottomBound;
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
        if (mainCamera != null) CalculateBounds();

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

    private void CalculateBounds()
    {
        float size = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        
        float leftOffset = 1.5f;
        float rightOffset = 1f;
        float topOffset = 2.5f;
        float bottomOffset = -0.3f;
        
        if (GameManager.instance != null && GameManager.instance.Map_id == 2)
        {
            leftOffset = 1.4f;
            rightOffset = 2.8f;
            topOffset = 3f;
            bottomOffset = 1.4f;
        }

        rightBound = size * aspect - rightOffset;
        leftBound = -(size * aspect - leftOffset);
        
        topBound = size - topOffset;
        bottomBound = -(size - bottomOffset);
    }

    private void LimitToScreen()
    {
        if (mainCamera == null) return;
        Vector2 pos = rb.position;
        Vector2 vel = rb.velocity;
        bool changed = false;

        if (pos.x < leftBound) { pos.x = leftBound; vel.x = 0; changed = true; }
        else if (pos.x > rightBound) { pos.x = rightBound; vel.x = 0; changed = true; }
        
        if (pos.y < bottomBound) { pos.y = bottomBound; vel.y = 0; changed = true; }
        else if (pos.y > topBound) { pos.y = topBound; vel.y = 0; changed = true; }

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
                    GameManager.instance.EndGame("玩家一炸死玩家二！玩家一（蓝）获胜！");
                }
                else if (currentRole == Role.Player2)
                {
                    GameManager.instance.EndGame("玩家二炸死玩家一！玩家二（红）获胜！");
                }
            }
            else
            {
                // 只炸死了自己，对方获胜
                if (currentRole == Role.Player1)
                {
                    GameManager.instance.EndGame("玩家一炸死自己！玩家二（红）获胜！");
                }
                else if (currentRole == Role.Player2)
                {
                    GameManager.instance.EndGame("玩家二炸死自己！玩家一（蓝）获胜！");
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

        isAttacking = true;
        moveDir = Vector2.zero;
        isStopped = true;

        StartCoroutine(WaitForAttackEnd());
        if (spriteRenderer != null)
            StartCoroutine(AttackFlash());

        Vector2 attackDir = lastMoveDir.normalized;
        if (attackDir == Vector2.zero) attackDir = Vector2.right;
        
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        Vector2 boxSize = new Vector2(1.5f, 1.35f);
        Vector2 hitBoxCenter = (Vector2)transform.position + attackDir * 0.65f;
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
                GameManager.instance.EndGame("玩家二（红）获胜！");
            else if (bestTarget.currentRole == Role.Player2)
                GameManager.instance.EndGame("玩家一（蓝）获胜！");
        }
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
        if (GameManager.instance == null || GameManager.instance.Map_id != 2)
            return;
            
        CharacterLogic otherCharacter = otherObject.GetComponent<CharacterLogic>();
        if (otherCharacter == null) return;
        
        if (currentRole == Role.Bot && otherCharacter.currentRole == Role.Bot)
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
        
        Vector3 effectPosition = new Vector3(position.x, position.y + yOffset, position.z);
        GameObject effect = Instantiate(teleportEffectPrefab, effectPosition, Quaternion.identity);
        
        SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
        if (effectRenderer != null)
        {
            effectRenderer.sortingOrder = 100;
        }
    }
}