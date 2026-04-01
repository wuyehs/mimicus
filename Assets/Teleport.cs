using UnityEngine;

public class DeathEffectPlayer : MonoBehaviour
{
    private Animator animator;
    
    [Header("缩放设置")]
    public float scaleFactor = 0.05f; // 默认缩小到原来的一半

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 设置动画缩放
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        
        // 获取动画长度，自动销毁
        AnimationClip clip = animator.runtimeAnimatorController.animationClips[0];
        Destroy(gameObject, clip.length);
    }
}