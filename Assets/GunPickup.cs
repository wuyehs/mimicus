using UnityEngine;
using System.Collections;

public class GunPickup : MonoBehaviour
{
    public ParticleSystem spawnEffect;   // 登场特效（在 Inspector 中拖入）
    public ParticleSystem despawnEffect; // 消失特效（在 Inspector 中拖入）
    private bool isPickedUp = false;

    void Start()
    {
        // 播放登场特效
        if (spawnEffect != null)
        {
            spawnEffect.Play();
        }

        // 可选：让枪自身有轻微的缩放淡入效果（可与特效同步）
        StartCoroutine(SpawnScale());
    }

    IEnumerator SpawnScale()
    {
        // 从极小缩放逐渐恢复
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        float duration = 0.2f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;

        CharacterLogic character = other.GetComponent<CharacterLogic>();
        if (character == null) return;

        // 只允许玩家拾取
        if (character.currentRole == CharacterLogic.Role.Player1 ||
            character.currentRole == CharacterLogic.Role.Player2)
        {
            character.PickUpGun();
            isPickedUp = true;

            // 播放消失特效
            if (despawnEffect != null)
            {
                despawnEffect.Play();
                // 禁用渲染器和碰撞体，避免二次触发
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                // 等待特效播放完毕再销毁
                float effectDuration = despawnEffect.main.duration+0.3f;
                StartCoroutine(DestroyAfterEffect(effectDuration));
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    IEnumerator DestroyAfterEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}