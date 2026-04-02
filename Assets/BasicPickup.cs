using UnityEngine;
using System.Collections;

public abstract class BasePickup : MonoBehaviour
{
    [Header("特效")]
    public ParticleSystem spawnEffect;   // 登场特效
    public ParticleSystem despawnEffect; // 消失特效

    private bool isPickedUp = false;

    protected virtual void Start()
    {
        // 播放登场特效
        if (spawnEffect != null)
            spawnEffect.Play();

        // 可选：缩放动画
        StartCoroutine(SpawnScale());
    }

    IEnumerator SpawnScale()
    {
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

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;

        CharacterLogic character = other.GetComponent<CharacterLogic>();
        if (character == null) return;

        // 只允许玩家拾取
        if (character.currentRole == CharacterLogic.Role.Player1 ||
            character.currentRole == CharacterLogic.Role.Player2)
        {
            // 调用具体的拾取逻辑（由子类实现）
            OnPickup(character);

            isPickedUp = true;

            // 播放消失特效
            if (despawnEffect != null)
            {
                Debug.Log("播放消失特效");
                despawnEffect.Play();
                // 禁用渲染器和碰撞体             
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                float effectDuration = despawnEffect.main.duration + 1.3f;
                StartCoroutine(DestroyAfterEffect(effectDuration));
            }
            else
            {
                Debug.LogError("despawnEffect 为空，无法播放消失特效");
                Destroy(gameObject);
            }
        }
    }

    IEnumerator DestroyAfterEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // 子类需要实现此方法，根据武器类型调用角色的相应方法
    protected abstract void OnPickup(CharacterLogic character);
}