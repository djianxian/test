Shader "Custom/StencilMaskShader"
{
    Properties
    {
        _MaskTex ("Mask Texture", 2D) = "black" {}  // 用作遮罩的纹理
    }

    SubShader
    {
        // Pass 1: 写入模板缓冲，标记遮罩区域
        Pass
        {
            Name "StencilWrite"
            Tags { "LightMode" = "Always" }

            // 写入模板缓冲数据
            Stencil
            {
                Ref 1         // 设置模板参考值
                Comp always   // 总是通过模板测试
                Pass replace  // 替换模板缓冲的值
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MaskTex;

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
                fixed4 mainColor = tex2D(_MaskTex, i.uv);
                if (mainColor.a < 0.5)
                {
                    discard;
                }
                return mainColor;
            }
            ENDCG
        }

    }

    Fallback "Diffuse"
}
