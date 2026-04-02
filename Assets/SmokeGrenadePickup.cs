using UnityEngine;

public class SmokeGrenadePickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterLogic character = other.GetComponent<CharacterLogic>();
        if (character == null) return;

        // 只允许玩家拾取
        if (character.currentRole == CharacterLogic.Role.Player1 ||
            character.currentRole == CharacterLogic.Role.Player2)
        {
            character.PickUpSmokeGrenade();
            Destroy(gameObject);
        }
    }
}