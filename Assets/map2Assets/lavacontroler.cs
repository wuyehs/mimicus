using UnityEngine;

public class VolcanoLavaController : MonoBehaviour
{
    [Header("缩圈设置")]
    public float shrinkSpeed = 0.05f;    // 缩圈速度
    public float minScale = 0.1f;       // 最小缩到多小
    
    [Tooltip("基础半径缩放系数。在 Scene 窗口观察蓝色椭圆并调整此值。")]
    public float baseRadius = 0.4f;     //

    [Header("偏差修正")]
    [Tooltip("如果蓝线对准了但死得不对，调整这个。向上移 2 个单位就填 (0, 2)")]
    public Vector2 centerOffset = Vector2.zero; // 新增：用于对齐视觉与逻辑的偏差
    
    [Header("遮罩设置")]
    [Tooltip("背景精灵对象，需要在Inspector中拖拽赋值")]
    public SpriteRenderer backgroundSprite;
    
    private SpriteMask spriteMask;

    void Start()
    {
        // 获取SpriteMask组件
        spriteMask = gameObject.GetComponent<SpriteMask>();
        if (spriteMask == null)
        {
            Debug.LogError("没有找到SpriteMask组件！");
            return;
        }

        // 启用自定义层级范围
        spriteMask.isCustomRangeActive = true;
        spriteMask.frontSortingOrder = -9;   // 前向边界
        spriteMask.backSortingOrder = -11;   // 后向边界
        
        // 检查背景精灵
        if (backgroundSprite != null)
        {
            // 设置背景精灵的层级
            backgroundSprite.sortingOrder = -10;  // 在-11到-9之间
            
            // 设置遮罩交互
            backgroundSprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            
            Debug.Log("背景遮罩设置完成");
        }
        else
        {
            Debug.LogWarning("backgroundSprite未赋值，请在Inspector中拖拽背景对象");
        }
    }

    void Update()
    {
        // 1. 实现自动缩圈（按 20:18 比例同步缩放）
        if (transform.localScale.x > minScale)
        {
            float newScaleX = transform.localScale.x - (shrinkSpeed * Time.deltaTime);
            float newScaleY = transform.localScale.y - (shrinkSpeed * (18f / 20f) * Time.deltaTime); 
            transform.localScale = new Vector3(Mathf.Max(newScaleX, minScale), Mathf.Max(newScaleY, minScale), 1f);
        }

        // 2. 每一帧检测场景中所有角色的安全性
        CheckAllCharacters();
    }

    void CheckAllCharacters()
    {
        // 修改点：从 CharacterLogic2 改为 CharacterLogic
        CharacterLogic[] allCharacters = Object.FindObjectsByType<CharacterLogic>(FindObjectsSortMode.None);
        
        // 计算椭圆的长轴 (a) 和短轴 (b)
        float axisX = transform.localScale.x * baseRadius;
        float axisY = transform.localScale.y * baseRadius;

        // 应用中心点偏移
        Vector2 visualCenter = (Vector2)transform.position;

        foreach (CharacterLogic character in allCharacters)
        {
            if (character == null) continue;

            // 使用偏移后的中心计算角色相对位置
            Vector2 relativePos = (Vector2)character.transform.position + Vector2.up * 1f - visualCenter;

            // 确保轴不为 0 防止计算错误
            if (axisX <= 0 || axisY <= 0) continue;

            // 椭圆判定公式: (x^2 / a^2) + (y^2 / b^2)
            float ellipseValue = (relativePos.x * relativePos.x) / (axisX * axisX) + 
                                 (relativePos.y * relativePos.y) / (axisY * axisY);

            // 如果结果 > 1，说明角色在椭圆判定区外
            if (ellipseValue > 1f)
            {
                HandleInstantDeath(character); 
            }
        }
    }

    void HandleInstantDeath(CharacterLogic victim)
    {
        // 修改点：调用 CharacterLogic 的死亡逻辑
        victim.Die(); 

        // 修改点：判定胜负逻辑并通知对应的GameManager
        if (GameManager.instance != null)
        {
            if (victim.currentRole == CharacterLogic.Role.Player1)
            {
                GameManager.instance.EndGame("PLAYER 2 WINS!"); 
            }
            else if (victim.currentRole == CharacterLogic.Role.Player2)
            {
                GameManager.instance.EndGame("PLAYER 1 WINS!"); 
            }
        }
    }

    // 在 Scene 窗口绘制带有偏移量的蓝色椭圆辅助线
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; 
        
        float axisX = transform.localScale.x * baseRadius;
        float axisY = transform.localScale.y * baseRadius;

        // 辅助线也应用偏移量，确保所见即所得
        Vector3 visualCenter = transform.position + (Vector3)centerOffset;

        Vector3 lastPos = Vector3.zero;
        for (int i = 0; i <= 360; i += 10)
        {
            float angle = i * Mathf.Deg2Rad;
            Vector3 nextPos = visualCenter + new Vector3(Mathf.Cos(angle) * axisX, Mathf.Sin(angle) * axisY, 0);
            
            if (i > 0)
            {
                Gizmos.DrawLine(lastPos, nextPos);
            }
            lastPos = nextPos;
        }
    }
}