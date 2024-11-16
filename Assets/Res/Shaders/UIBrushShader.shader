Shader "UI/ScratchCard/Brush"
{
    Properties
    {
        _MainTex ("Brush Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _Offset ("Offset", Vector) = (0,0,0,0)
        [PerRendererData] _Scale ("Scale", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        BlendOp RevSub
        Blend Zero One, One One

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
        
            struct a2v
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _Offset;
            float4 _Scale;

            v2f vert(a2v v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex + _Offset);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex) * _Scale.xy;
                o.color = v.color * _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.texcoord) * i.color;
                return color;
            }
        ENDCG
        }
    }
}