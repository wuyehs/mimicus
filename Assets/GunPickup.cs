using UnityEngine;

public class GunPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 获取触碰对象的 CharacterLogic 组件
        CharacterLogic character = other.GetComponent<CharacterLogic>();
        if (character == null) return;  // 没有该组件，忽略

        // 判断是否为玩家（Player1 或 Player2）
        if (character.currentRole == CharacterLogic.Role.Player1 ||
            character.currentRole == CharacterLogic.Role.Player2)
        {
            character.PickUpGun();  // 调用拾取方法
            Destroy(gameObject);              // 拾取后枪消失
        }
        // 如果是机器人（Bot）或其他角色，则不做任何事
    }
}