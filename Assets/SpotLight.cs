using UnityEngine;

public class SimpleAutoSpotlight : MonoBehaviour
{
    [Header("灯光设置")]
    public float lightRange = 15f;
    public float intensity = 5f;
    public Color lightColor = Color.yellow;
    
    [Header("移动设置")]
    public float moveSpeed = 2.5f;
    [Header("随机方向时间范围")]
    public float minMoveInterval = 2f;     // 最小移动间隔
    public float maxMoveInterval = 8f;     // 最大移动间隔
    
    [Header("Bot检测设置")]
    public float detectionRadius = 1.5f; // 探测半径
    public Color botDetectionColor = Color.red; // 检测到Bot时的灯光颜色
    
    // 屏幕边界相关
    private Camera mainCamera;
    private float leftBound, rightBound, topBound, bottomBound;
    private float boundaryBuffer = 0.5f;
    
    private Light spotLight;
    private Vector3 startPosition;
    
    // 随机移动相关
    private Vector2 currentDirection = Vector2.zero;
    private float directionTimer = 0f;
    private float nextChangeTime = 0f;
    
    void Start()
    {
        // 获取主相机
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CalculateBounds();
        }
        
        // 初始化灯光
        spotLight = GetComponent<Light>();
        if (spotLight == null) spotLight = gameObject.AddComponent<Light>();
        
        spotLight.type = LightType.Spot;
        spotLight.range = lightRange;
        spotLight.intensity = intensity;
        spotLight.color = lightColor;
        
        transform.rotation = Quaternion.Euler(-10, 0, 0);
        startPosition = transform.position;
        
        // 初始化随机方向
        ChangeRandomDirection();
        
        Debug.Log("探照灯初始化完成");
    }
    
    // 计算屏幕边界
    private void CalculateBounds()
    {
        float size = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        
        float horizontalOffset = 1f;
        float topOffset = 2.5f;
        float bottomOffset = -0.3f;
        
        rightBound = size * aspect - horizontalOffset;
        leftBound = -rightBound;
        topBound = size - topOffset;
        bottomBound = -(size - bottomOffset);
    }
    
    // 检测并处理角色
    private void DetectAndHandleCharacters()
    {
        // 检测范围内的所有碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        
        // 存储当前检测到的Bot
        CharacterLogic currentlyDetectedBot = null;
        
        foreach (Collider2D collider in colliders)
        {
            CharacterLogic character = collider.GetComponent<CharacterLogic>();
            if (character != null)
            {
                if (character.currentRole != CharacterLogic.Role.Bot)
                {
                    currentlyDetectedBot = character;
                }
            }
        }
        // 更新灯光效果
        if (currentlyDetectedBot != null)
        {
            // 检测到Bot时灯光变红
            spotLight.color = Color.Lerp(spotLight.color, botDetectionColor, Time.deltaTime * 5f);
        }
        else
        {
            // 没有检测到Bot时恢复原色
            spotLight.color = Color.Lerp(spotLight.color, lightColor, Time.deltaTime * 5f);
        }
    }
    
    // 边界检查
    private bool CheckScreenBounds(out Vector3 correctedPosition)
    {
        correctedPosition = transform.position;
        bool hitBoundary = false;
        
        if (correctedPosition.x < leftBound + boundaryBuffer) 
        { 
            correctedPosition.x = leftBound + boundaryBuffer; 
            hitBoundary = true; 
        }
        else if (correctedPosition.x > rightBound - boundaryBuffer) 
        { 
            correctedPosition.x = rightBound - boundaryBuffer; 
            hitBoundary = true; 
        }
        
        if (correctedPosition.y < bottomBound + boundaryBuffer) 
        { 
            correctedPosition.y = bottomBound + boundaryBuffer; 
            hitBoundary = true; 
        }
        else if (correctedPosition.y > topBound - boundaryBuffer) 
        { 
            correctedPosition.y = topBound - boundaryBuffer; 
            hitBoundary = true; 
        }
        
        return hitBoundary;
    }
    
    void Update()
    {
        // 更新方向计时器
        directionTimer += Time.deltaTime;
        
        // 到达方向变化时间
        if (directionTimer >= nextChangeTime)
        {
            ChangeRandomDirection();
        }
        
        // 根据当前方向移动
        Vector3 moveDelta = new Vector3(currentDirection.x, currentDirection.y, 0) * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + moveDelta;
        
        // 应用移动
        transform.position = newPos;
        
        // 检查是否撞到边界
        if (CheckScreenBounds(out Vector3 correctedPos))
        {
            transform.position = correctedPos;
            
            // 碰撞到边界后立即随机改变方向（修改点1：改为随机方向）
            ChangeRandomDirection();
        }
        
        // 检测并处理角色
        DetectAndHandleCharacters();
    }
    
    // 随机改变方向
    private void ChangeRandomDirection()
    {
        int directionIndex = Random.Range(0, 4);
        currentDirection = directionIndex switch
        {
            0 => Vector2.up,
            1 => Vector2.down,
            2 => Vector2.left,
            3 => Vector2.right,
            _ => Vector2.up
        };
        
        currentDirection = currentDirection.normalized;
        
        // 随机设置下一次改变方向的时间（修改点2：改为随机时间）
        nextChangeTime = Random.Range(minMoveInterval, maxMoveInterval);
        directionTimer = 0f;
    }
    
    // 可视化调试
    void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;
        
        // 屏幕边界
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireCube(
            new Vector3((leftBound + rightBound) * 0.5f, (topBound + bottomBound) * 0.5f, 0),
            new Vector3(rightBound - leftBound, topBound - bottomBound, 0)
        );
        
        // 当前位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        // 当前移动方向
        Gizmos.color = Color.red;
        Vector3 endPoint = transform.position + new Vector3(currentDirection.x, currentDirection.y, 0) * 1f;
        Gizmos.DrawLine(transform.position, endPoint);
        
        // Bot检测范围
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}