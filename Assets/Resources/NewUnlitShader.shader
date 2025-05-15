Shader "Custom/StaticFogVolume"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
        _FogStart ("Fog Start Distance", Float) = 0.0
        _FogEnd   ("Fog End Distance", Float) = 10.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _FogColor;
            float _FogStart;
            float _FogEnd;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = world.xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Center is just the object’s world position
                float3 fogCenter = unity_ObjectToWorld._m03_m13_m23;

                float dist = distance(i.worldPos, fogCenter);
                float fogFactor = saturate((dist - _FogStart) / (_FogEnd - _FogStart));

                return float4(_FogColor.rgb, fogFactor * _FogColor.a);
            }
            ENDCG
        }
    }
}
