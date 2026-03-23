Shader "Unlit/RainV2"
{
    Properties
    {
        _MainTex ("Rain Texture", 2D) = "white" {}
        _Speed ("Fall Speed", Float) = 10
        _Height ("Loop Height", Float) = 20
        _Opacity ("Opacity", Range(0,1)) = 0.5
        _WindDir ("Wind Direction", Vector) = (0,0,0,0)
        _WindStrength ("Wind Strength", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Speed;
            float _Height;
            float _Opacity;
            float2 _WindDir;
            float _WindStrength;

            //----------------------------------
            // Instancing buffer
            //----------------------------------
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _DropOffset)
                UNITY_DEFINE_INSTANCED_PROP(float, _SpeedMul)
            UNITY_INSTANCING_BUFFER_END(Props)

            //----------------------------------
            // Input / Output structs
            //----------------------------------
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //----------------------------------
            // Vertex shader
            //----------------------------------
            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 forward = normalize(float3(_WindDir.x*_WindStrength, -1, _WindDir.y*_WindStrength));

                // Pick a stable up reference (world up)
                float3 worldUp = abs(forward.y) > 0.99 ? float3(1,0,0) : float3(0,1,0);
                
                // Create right vector
                float3 right = normalize(cross(worldUp, forward));
                
                // Recompute up so it's perpendicular
                float3 up = cross(forward, right);
                
                float3 localPos = v.vertex.xyz;

                // Build rotation matrix from basis vectors
                float3x3 rotMatrix = float3x3(
                    right,
                    forward,
                    up
                );
                
                // Rotate the mesh
                float3 rotated = mul(localPos, rotMatrix);
                
                // Then place it in world
                float3 worldPos = mul(unity_ObjectToWorld, float4(rotated, 1.0)).xyz;

                // Get per-instance data
                float offset   = UNITY_ACCESS_INSTANCED_PROP(Props, _DropOffset);
                float speedMul = UNITY_ACCESS_INSTANCED_PROP(Props, _SpeedMul);
                
                // Animate falling + looping
                float fall = fmod(_Time.y * _Speed * speedMul + offset, _Height);

                worldPos.y -= fall;
                float2 windOffset = _WindDir.xy * _WindStrength * fall;
                worldPos.xz += windOffset;
                // Convert to clip space (correct way)
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

                o.uv = v.uv;

                return o;
            }

            //----------------------------------
            // Fragment shader
            //----------------------------------
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                col.a *= _Opacity;

                return col;
            }

            ENDHLSL
        }
    }
}
