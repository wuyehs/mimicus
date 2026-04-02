using UnityEngine;

public class SimpleAutoSpotlight : MonoBehaviour
{
    private bool playerDetected = false;

    [Header("灯光设置")]
    public float lightRange = 15f;
    public float intensity = 5f;
    public Color lightColor = Color.yellow;
    
    [Header("移动设置")]
    public float moveSpeed = 2.5f;
    
    [Header("随机方向时间范围")]
    public float minMoveInterval = 2f;
    public float maxMoveInterval = 8f;
    
    [Header("Bot检测设置")]
    public float detectionRadius = 2.5f;
    public Color botDetectionColor = Color.red;

    [Header("实时光圈设置")]
    public Color circleColor = Color.red;
    public float circleThickness = 0.08f;
    public int circleSegments = 60;
    public bool showCircle = true;

    // 屏幕边界相关
    private Camera mainCamera;
    private float leftBound, rightBound, topBound, bottomBound;
    private float boundaryBuffer = 0.5f;
    
    private Light spotLight;
    private Vector2 currentDirection = Vector2.zero;
    private float directionTimer = 0f;
    private float nextChangeTime = 0f;

    private LineRenderer lineRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null) CalculateBounds();
        
        // 初始化灯光
        spotLight = GetComponent<Light>();
        if (spotLight == null) spotLight = gameObject.AddComponent<Light>();
        
        spotLight.type = LightType.Spot;
        spotLight.range = lightRange;
        spotLight.intensity = intensity;
        spotLight.color = lightColor;
        
        transform.rotation = Quaternion.Euler(-10, 0, 0);

        ChangeRandomDirection();
        
        CreateDetectionCircle();
        
        Debug.Log("探照灯初始化完成");
    }

    private void CreateDetectionCircle()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = circleSegments + 1;
        lineRenderer.startWidth = circleThickness;
        lineRenderer.endWidth = circleThickness;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
    }

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

    private void DetectAndHandleCharacters()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + Vector3.up * 1.8f, detectionRadius);
        
        playerDetected = false;

        foreach (Collider2D col in colliders)
        {
            if (col == null) continue;
            CharacterLogic character = col.GetComponent<CharacterLogic>();
            if (character == null) continue;
            
            if (character.currentRole != CharacterLogic.Role.Bot)
            {
                playerDetected = true;
                break;
            }
        }
        
        // 更新灯光颜色
        if (playerDetected)
            spotLight.color = Color.Lerp(spotLight.color, botDetectionColor, Time.deltaTime * 8f);
        else
            spotLight.color = Color.Lerp(spotLight.color, lightColor, Time.deltaTime * 6f);
    }

    void Update()
    {
        // 移动逻辑
        directionTimer += Time.deltaTime;
        if (directionTimer >= nextChangeTime)
            ChangeRandomDirection();

        Vector3 moveDelta = new Vector3(currentDirection.x, currentDirection.y, 0) * moveSpeed * Time.deltaTime;
        transform.position += moveDelta;

        // 边界处理
        if (CheckScreenBounds(out Vector3 correctedPos))
        {
            transform.position = correctedPos;
            ChangeRandomDirection();
        }

        DetectAndHandleCharacters();
    }

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
        nextChangeTime = Random.Range(minMoveInterval, maxMoveInterval);
        directionTimer = 0f;
    }
}