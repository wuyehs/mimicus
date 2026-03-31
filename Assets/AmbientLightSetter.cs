using UnityEngine;

public class AmbientLightSetter : MonoBehaviour
{
    [Header("环境光设置")]
    public Color ambientColor = new Color(0.2f, 0.2f, 0.2f); // 默认深灰色
    public float intensity = 1.0f;

    void Start()
    {
        // 强制设置环境光模式为“纯色”
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        // 设置环境光颜色和强度
        RenderSettings.ambientLight = ambientColor * intensity;
        // 设置环境光强度系数
        RenderSettings.ambientIntensity = intensity;
        
        Debug.Log("[环境光] 已设置为: " + ambientColor + " | 强度: " + intensity);
    }
}