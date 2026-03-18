Shader "Unlit/TestShader" //used tutorial: https://www.youtube.com/watch?v=OrWBSN0yasQ
{
    Properties
    {
        _Color("Test Color", color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert //runs on every single vert
            #pragma fragment frag //runs on every single pixel
            
            fixed4 _Color;
            
            #include "UnityCG.cginc"

            struct appdata //object data / mesh data
            {
                float4 vertex : POSITION; //local position
            };

            struct v2f // vert to fragment
            {
                float4 vertex : SV_POSITION;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                return col;
            }
            ENDCG
        }
    }
}
