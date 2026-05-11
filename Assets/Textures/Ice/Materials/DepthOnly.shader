Shader "Custom/DepthOnly"
{
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="Opaque" }

        Pass
        {
            ZWrite On
            ColorMask 0
            Cull Back
        }
    }
}