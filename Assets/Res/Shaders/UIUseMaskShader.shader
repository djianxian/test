Shader "Custom/StencilUseMaskShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
    }

    SubShader
    {

        Pass
        {
            Name "StencilApply"
            Tags { "LightMode" = "Always" }

            Stencil
            {
                Ref 1          // 设置参考值为 1
                Comp equal     // 如果模板缓冲的值等于 1，则通过模板测试
                Pass keep      // 保持模板缓冲的值不变
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 渲染目标纹理（基础纹理）
                fixed4 mainColor = tex2D(_MainTex, i.uv);
                return mainColor;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
