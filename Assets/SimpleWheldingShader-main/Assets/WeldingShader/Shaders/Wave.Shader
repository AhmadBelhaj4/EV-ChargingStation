Shader "Weston/Wave"{   

    Properties{
        // _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
        _PainterPosition ("Painter Position", Vector) = (0,0,0)
        _Radius ("Radius",Float) = 0.5
        _Hardness ("Hardness", Float) = 0.5
        _Strength ("Strength",Float) = 1
        _Speed ("Speed",Float) = 5
    }

    SubShader{
        Cull Off ZWrite Off ZTest Off
        // Tags { "RenderType"="Opaque" }
        // LOD 100

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float3 _PainterPosition;
            float _Radius;
            float _Hardness;
            float _Strength;
            // float4 _PainterColor;
            float _Speed;
            float _PrepareUV;

            struct appdata{
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 time : MYDICK;
            };

            float mask(float3 position, float3 center, float radius, float hardness){
                float m = distance(center, position);
                return 1 - smoothstep(radius * hardness, radius, m);    
            }

            float wave(float3 position, float3 center, float radius, float amplitude,float time){
                float d = distance(center, position);
                if(d<radius){
                    return amplitude * sin(d / radius * 3.14 * 2+time*_Speed);
                }
                else{
                    return 0;
                }
                
            }

            v2f vert (appdata v){
                v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                
				float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2( 2, 2) - float2(1, 1));
				o.vertex = uv;
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // o.vertex = UnityObjectToClipPos(v.vertex);
                o.time = _Time;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
                if(_PrepareUV > 0 ){
                    return float4(0, 0, 1, 1);
                }
                float4 col = tex2D(_MainTex, i.uv);

                float d = distance(i.worldPos, _PainterPosition);
                if(d<_Radius){
                    float f = mask(i.worldPos, _PainterPosition, _Radius, _Hardness);
                    float w = wave(i.worldPos, _PainterPosition, _Radius, _Strength,i.time.z);


                    
                    float3 horizontal = i.worldPos.xyz - _PainterPosition.xyz;
                    float3 vertical = float3(horizontal.xy,w);
                    float3 normal = horizontal+vertical;
                    return float4(normalize((normal+1)*0.5),1);
                    
                }
                else{
                    return col;
                }
                
                
            }
            ENDCG
        }
    }
}