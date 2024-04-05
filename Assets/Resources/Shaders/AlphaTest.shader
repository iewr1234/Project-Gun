Shader "Draw/AlphaTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1) // ������ ������ ���� �Ӽ�
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5 // ���� �׽�Ʈ �Ӱ谪
    }

    SubShader
    {
        Tags { "Queue" = "AlphaTest" }

        Pass
        {
            Name "AlphaTest"
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha // ���� ���� ���� (������ �ؽ�ó ������ ���� �ʿ�)

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
                fixed4 col = tex2D(_MainTex, i.uv) * _Color; // �ؽ�ó�� ������ ������ ���Ͽ� ���� ���

                if (col.a < _Cutoff)
                    discard;

                return col;
            }
            ENDCG
        }
    }
}