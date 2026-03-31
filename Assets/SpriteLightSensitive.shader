Shader "Custom/SpriteSurfaceLightSensitive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 感光控制
        _LightSensitivity ("Light Sensitivity", Range(0, 5)) = 2.0
        _BaseBrightness ("Base Brightness", Range(0, 2)) = 1.0
        _EmissionStrength ("Emission", Range(0, 2)) = 0.5
        
        [Toggle(RECEIVE_SHADOWS)] _ReceiveShadows ("Receive Shadows", Float) = 1
    }
    
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "DisableBatching"="False"
        }
        
        LOD 200
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert
        #pragma shader_feature RECEIVE_SHADOWS
        #pragma target 3.0
        
        sampler2D _MainTex;
        fixed4 _Color;
        float _LightSensitivity;
        float _BaseBrightness;
        float _EmissionStrength;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
            INTERNAL_DATA
        };
        
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            // 传递纹理坐标
            o.uv_MainTex = v.texcoord;
            
            // 计算世界空间位置和法线
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
        }
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            // 采样纹理
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 c = tex * _Color;
            
            // 计算光照强度
            float3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
            float3 worldNormal = normalize(IN.worldNormal);
            float ndotl = saturate(dot(worldNormal, worldLightDir));
            
            // 应用感光效果
            float lightResponse = ndotl * _LightSensitivity;
            float finalBrightness = _BaseBrightness + lightResponse;
            
            // 输出
            o.Albedo = c.rgb;
            o.Emission = c.rgb * _EmissionStrength * lightResponse;
            o.Alpha = c.a;
        }
        ENDCG
    }
    
    FallBack "Transparent/Diffuse"
}