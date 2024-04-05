Shader "Draw/AlphaTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1) // 반투명 색상을 위한 속성
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5 // 알파 테스트 임계값
    }

    SubShader
    {
        Tags { "Queue" = "AlphaTest" }

        Pass
        {
            Name "AlphaTest"
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha // 알파 블렌딩 설정 (반투명 텍스처 적용을 위해 필요)

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _Cutoff;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color; // 텍스처와 반투명 색상을 곱하여 색상 계산

                if (col.a < _Cutoff)
                    discard;

                return col;
            }
            ENDCG
        }
    }
}