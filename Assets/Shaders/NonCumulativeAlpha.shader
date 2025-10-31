Shader "Unlit/NonCumulativeAlpha"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0.5)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            // 기본 알파 블렌딩을 사용합니다.
            Blend SrcAlpha OneMinusSrcAlpha
            // 하지만 최종 알파 값을 계산할 때 덧셈(Add) 대신 최댓값(Max)을 사용합니다.
            // 이렇게 하면 겹치는 픽셀의 알파 값이 더 높은 쪽으로 대체될 뿐, 더해지지 않습니다.
            BlendOp Max, Add

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 지정된 색상을 그대로 반환합니다.
                return _Color;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
