using UnityEngine;

public class VolcanoLavaController : MonoBehaviour
{
    [Header("缩圈设置")]
    public float shrinkSpeed = 0.05f;    // 缩圈速度
    public float minScale = 0.1f;       // 最小缩到多小
    
    [Tooltip("圆图在 Scale 为 1 时的物理半径。如果你的圆刚好铺满 5x5 的单位，这里填 2.5")]
    public float baseRadius = 3.5f;     

    void Update()
    {
        // 1. 实现自动缩圈（控制 Transform 的缩放）
        if (transform.localScale.x > minScale)
        {
            float newScale = transform.localScale.x - (shrinkSpeed * Time.deltaTime);
            transform.localScale = new Vector3(newScale, newScale, 1f);
        }

        // 2. 每一帧检测场景中所有角色的安全性
        CheckAllCharacters();
    }

    void CheckAllCharacters()
    {
        // 引用修改点：寻找 CharacterLogic2 组件
        CharacterLogic2[] allCharacters = Object.FindObjectsByType<CharacterLogic2>(FindObjectsSortMode.None);
        
        // 计算当前安全区的实际物理半径（基于缩放）
        float currentSafeRadius = transform.localScale.x * baseRadius;

        foreach (CharacterLogic2 character in allCharacters)
        {
            // 如果角色已经死亡，跳过检测防止重复触发
            // 注意：CharacterLogic2 中没有 public 的 isDead，但 Die() 内部有判断
            
            // 计算角色到中心点的距离
            float dist = Vector2.Distance(character.transform.position, transform.position);

            // 如果距离大于当前安全半径，说明角色已经踏入岩浆
            if (dist > currentSafeRadius)
            {
                HandleInstantDeath(character);
            }
        }
    }

    void HandleInstantDeath(CharacterLogic2 victim)
    {
        // 调用 CharacterLogic2 中已有的死亡方法
        // Die() 内部会自动处理：标记死亡、停止移动、播放动画、销毁物体
        victim.Die();

        // 判定胜负逻辑并调用 GameManager2
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
        // 注：如果是 Bot 掉入岩浆，只会执行 victim.Die()，不会触发 EndGame
    }
}