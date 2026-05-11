Shader "Custom/IceWithEdge"
{
    Properties
    {
        _MainTex ("Ice Texture", 2D) = "white" {}
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        _EdgeColor ("Edge Color", Color) = (0.8, 0.9, 1, 1)
        _IceColor ("Ice Color", Color) = (0.5, 0.7, 0.9, 1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float _EdgeWidth;
            float4 _EdgeColor;
            float4 _IceColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel для краёв
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = 1 - abs(dot(i.normal, viewDir));
                
                // Расширяем эффект на краях UV (если текстура имеет границы)
                float uvEdge = min(i.uv.x, i.uv.y);
                uvEdge = min(uvEdge, 1 - i.uv.x);
                uvEdge = min(uvEdge, 1 - i.uv.y);
                uvEdge = smoothstep(0, _EdgeWidth, uvEdge);
                uvEdge = 1 - uvEdge;
                
                // Комбинируем эффекты
                float edge = max(fresnel * 2, uvEdge);
                edge = saturate(edge * 2);
                
                // Основной цвет льда
                fixed4 iceColor = tex2D(_MainTex, i.uv) * _IceColor;
                
                // Смешиваем с цветом края
                fixed4 finalColor = lerp(iceColor, _EdgeColor, edge);
                
                // Добавляем небольшое свечение на краях
                finalColor.rgb += edge * 0.3;
                
                return finalColor;
            }
            ENDCG
        }
    }
}