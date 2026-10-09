Shader "Weston/Triangle"{   

    Properties{
        _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
    }

    SubShader{
        Cull Off ZWrite Off ZTest Off

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float3 _P1;
            float3 _P2;
            float3 _P3;
            float4 _PainterColor;
            float _PrepareUV;

            struct appdata{
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float mask(float3 position, float3 center, float radius, float hardness){
                float m = distance(center, position);
                return 1 - smoothstep(radius * hardness, radius, m);    
            }
            inline float3 projectOnPlane( float3 vec, float3 normal )
            {
                return vec - normal * ( dot( vec, normal ) / dot( normal, normal ) );
            }

            bool isInsideTriangle(float3 p, float3 p1, float3 p2, float3 p3){
                // 计算三角形的边向量
                float3 v0 = p3 - p1;
                float3 v1 = p2 - p1;
                float3 v2 = p - p1;
                        
                // 计算重心坐标
                float dot00 = dot(v0, v0);
                float dot01 = dot(v0, v1);
                float dot02 = dot(v0, v2);
                float dot11 = dot(v1, v1);
                float dot12 = dot(v1, v2);

                float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
                float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
                        
                // 检查点P是否在三角形内
                return (u >= 0.0f) && (v >= 0.0f) && (u + v <= 1.0f);
            }

            v2f vert (appdata v){
                v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
				float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2( 2, 2) - float2(1, 1));
				o.vertex = uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{   
                if(_PrepareUV > 0 ){
                    return float4(0, 0, 1, 1);
                }         

                float4 col = tex2D(_MainTex, i.uv);
                
                // float d = distance(i.worldPos, _P1);
                // float d2 = distance(i.worldPos, _P2);
                // float d3 = distance(i.worldPos, _P3);

                // if(d<0.05 || d2<0.05 || d3<0.05){
                    
                //     return _PainterColor;
                // }
                // else{
                //     return col;
                // }

                bool pInside = isInsideTriangle(i.worldPos,_P1,_P2,_P3);
                if(pInside){
                    return _PainterColor;
                }
                else{
                    return col;
                }
            }
            ENDCG
        }
    }
}