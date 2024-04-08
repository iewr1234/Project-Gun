Shader "Draw/AlwaysVisible(solid)"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        [PerRendererData] _MainTex ("Base (RGB)", 2D) = "white" {}
        _PhantomColor ("AlwaysVisible Color", Color) = (1,1,1,1)
        _PhantomPower ("AlwaysVisible Power", Float) = 1
    }

    SubShader
    {
        // Pass: character
        Stencil
        {
            Ref 20
            Comp Always
            Pass Replace
        }

        Tags { "Queue" = "Geometry+1" "RenderType" = "Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Emission = c.rgb; // 여기에 Emission을 추가
        }
        ENDCG

        // Pass: Phantom  
        Stencil
        {
            Ref 20
            Comp NotEqual
            Pass Replace
            ZFail IncrSat
        }

        ZWrite On
        ZTest Always  
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #include "UnityCG.cginc"
        #pragma surface surf BlinnPhong

        uniform float4 _Color;
        uniform fixed _PhantomPower;
        uniform fixed4 _PhantomColor;
        uniform sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), IN.worldNormal));
            fixed3 rim_final = _PhantomColor.rgb * pow(rim, _PhantomPower);
            o.Emission = rim_final.rgb;
            o.Alpha = rim * _PhantomColor.a;
        }
        ENDCG
    }

    Fallback "Diffuse", 0
}