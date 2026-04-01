using UnityEngine;

public class VolcanoLavaController : MonoBehaviour
{
    [Header("缩圈设置")]
    public float shrinkSpeed = 0.05f;    // 缩圈速度
    public float minScale = 0.1f;       // 最小缩到多小
    
    [Tooltip("基础半径缩放系数。在 Scene 窗口观察蓝色椭圆并调整此值。")]
    public float baseRadius = 0.2f;     //

    [Header("偏差修正")]
    [Tooltip("如果蓝线对准了但死得不对，调整这个。向上移 2 个单位就填 (0, 2)")]
    public Vector2 centerOffset = Vector2.zero; // 新增：用于对齐视觉与逻辑的偏差

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
        // 寻找 CharacterLogic2 组件
        CharacterLogic2[] allCharacters = Object.FindObjectsByType<CharacterLogic2>(FindObjectsSortMode.None);
        
        // 计算椭圆的长轴 (a) 和短轴 (b)
        float axisX = transform.localScale.x * baseRadius;
        float axisY = transform.localScale.y * baseRadius;

        // 应用中心点偏移
        Vector2 visualCenter = (Vector2)transform.position + centerOffset;

        foreach (CharacterLogic2 character in allCharacters)
        {
            if (character == null) continue;

            // 使用偏移后的中心计算角色相对位置
            Vector2 relativePos = (Vector2)character.transform.position - visualCenter;

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

    void HandleInstantDeath(CharacterLogic2 victim)
    {
        // 调用 CharacterLogic2 的死亡逻辑
        victim.Die(); 

        // 判定胜负逻辑并通知 GameManager2
        if (victim.currentRole == CharacterLogic2.Role.Player1)
        {
            if (GameManager2.instance != null)
                GameManager2.instance.EndGame("玩家一掉入岩浆！玩家二（红）获胜！"); 
        }
        else if (victim.currentRole == CharacterLogic2.Role.Player2)
        {
            if (GameManager2.instance != null)
                GameManager2.instance.EndGame("玩家二掉入岩浆！玩家一（蓝）获胜！"); 
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