using UnityEngine;
using System.Collections;

public class AutoHideCanvas : MonoBehaviour
{
    [Header("显示时间设置")]
    [SerializeField] private float displayTime = 5f;  // 显示时长，默认5秒
    
    private Canvas canvasComponent;
    
    void Start()
    {
        // 获取Canvas组件
        canvasComponent = GetComponent<Canvas>();
        
        if (canvasComponent != null)
        {
            // 启动计时器协程
            StartCoroutine(HideAfterDelay());
        }
        else
        {
            Debug.LogError("当前GameObject没有Canvas组件！");
        }
    }
    
    IEnumerator HideAfterDelay()
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(displayTime);
        
        // 禁用Canvas
        canvasComponent.enabled = false;
        
        Debug.Log($"Canvas 已在 {displayTime} 秒后隐藏");
    }
}