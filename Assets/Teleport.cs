using UnityEngine;

public class DeathEffectPlayer : MonoBehaviour
{
    private Animator animator;
    
    [Header("缩放设置")]
    [Tooltip("特效的缩放比例，0.05表示缩小到原来的5%")]
    public float scaleFactor = 0.05f;
    
    [Header("持续时间设置")]
    [Tooltip("如果大于0，将使用此值作为持续时间；如果为0或负数，将使用动画长度")]
    public float customDuration = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 设置动画缩放
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        
        // 计算持续时间
        float duration = GetEffectDuration();
        
        // 在指定时间后销毁对象
        Destroy(gameObject, duration);
    }
    private float GetEffectDuration()
    {
        // 如果设置了自定义持续时间且大于0，则使用该值
        if (customDuration > 0)
        {
            return customDuration;
        }
        
        // 否则使用动画的长度
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips.Length > 0)
            {
                return clips[0].length;
            }
        }
        
        // 默认返回1秒
        Debug.LogWarning("无法获取动画长度，使用默认持续时间1秒");
        return 1f;
    }
    
    public void SetCustomDuration(float duration)
    {
        customDuration = duration;
    }
}