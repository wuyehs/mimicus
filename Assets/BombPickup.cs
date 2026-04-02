using UnityEngine;

public class BombPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 获取触碰对象的 CharacterLogic 组件
        CharacterLogic character = other.GetComponent<CharacterLogic>();
        if (character == null) return;

        // 只允许玩家拾取
        if (character.currentRole == CharacterLogic.Role.Player1 ||
            character.currentRole == CharacterLogic.Role.Player2)
        {
            character.PickUpBomb();   // 调用角色的炸弹拾取方法
            Destroy(gameObject);      // 拾取后炸弹消失
        }
        // 如果是机器人或其他角色，则不做任何事
    }
}