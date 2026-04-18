Shader "Custom/Sprite-Lit-WithColor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite On
        ZTest LEqual
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "Sprite Lit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXEL_SNAP
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _TintColor;
                float _Alpha;
                float _PixelSnap;
                float _EnableExternalAlpha;
                float4 _RendererColor;
                float4 _Flip;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float4 vertex = IN.vertex;
                vertex.xy *= _Flip.xy;
                
                OUT.vertex = TransformObjectToHClip(vertex);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _TintColor * _RendererColor;
                
                #ifdef PIXEL_SNAP
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                #ifdef ETC1_EXTERNAL_ALPHA
                float4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, IN.uv);
                color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                #endif
                
                color *= IN.color;
                color.a *= _Alpha;
                color.rgb *= color.a;
                
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Sprite Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXEL_SNAP
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _TintColor;
                float _Alpha;
                float _PixelSnap;
                float _EnableExternalAlpha;
                float4 _RendererColor;
                float4 _Flip;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float4 vertex = IN.vertex;
                vertex.xy *= _Flip.xy;
                
                OUT.vertex = TransformObjectToHClip(vertex);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _TintColor * _RendererColor;
                
                #ifdef PIXEL_SNAP
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                #ifdef ETC1_EXTERNAL_ALPHA
                float4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, IN.uv);
                color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                #endif
                
                color *= IN.color;
                color.a *= _Alpha;
                color.rgb *= color.a;
                
                return color;
            }
            ENDHLSL
        }
    }
    
    Fallback "Sprites/Default"
}