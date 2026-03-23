Shader "Hidden/NewImageEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Offset", Float) = 0
        _UpDownSpeed ("Up Down Speed", Float) = 0
        _RotateSpeed ("Rotate Speed", Float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
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
            
            float _Offset;
            float _UpDownSpeed;
            float _RotateSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.y += _Offset * sin(_Time.y * _UpDownSpeed);
                
                float s = sin(_Time.y * _RotateSpeed);
                float c = cos(_Time.y * _RotateSpeed);
                
                float3x3 rotMat = float3x3(
                    c, 0, s, 
                    0, 1, 0, 
                    -s, 0, c);
                
                float3 rotated = mul(rotMat, v.vertex.xyz);
                
                o.vertex = UnityObjectToClipPos(rotated);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
