Shader "nadeshader/suyasuya" {
    Properties {
        _Color("オーバーレイの色", Color) = (0.12, 0.01, 0.02, 1) // #1e0204
        _OverlayAlpha("オーバーレイの透過度", Range(0,1)) = 0.5
        _RangeStart("開始範囲", Range(0,10)) = 1
        _RangeEnd("終了範囲", Range(0,10)) = 5
        _XScale("X軸の大きさ", Range(0,1)) = 0.6
        _YScale("Y軸の大きさ", Range(0,1)) = 0.5
        _ZScale("Z軸の大きさ", Range(0,1)) = 0.8
        _Exponent("透過の適応指数", Range(1, 5)) = 2.0
        _XSize("スイートスポットX", Range(0, 1)) = 0.1
        _YSize("スイートスポットY", Range(0, 1)) = 0.1
        _ZSize("スイートスポットZ", Range(0, 1)) = 0.1
    }
    SubShader {
        Tags { "Queue" = "Overlay+1" "ForceNoShadowCasting" = "False" "IgnoreProjector" = "True"  "VRCFallback" = "Hidden"}
        ZWrite Off
        ZTest Always
        Cull Off

        Pass {
            Lighting Off
            SeparateSpecular Off
            Fog { Mode Off }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : WORLD_POS;
            };

            float _RangeStart;
            float _RangeEnd;
            float4 _Color;
            float _OverlayAlpha;
            float _XScale;
            float _YScale;
            float _ZScale;
            float _Exponent;
            float _XSize;
            float _YSize;
            float _ZSize;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // オブジェクトのワールド座標を計算
                o.worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                // カメラからオブジェクトへのベクトルを計算
                o.viewDir = _WorldSpaceCameraPos - o.worldPos;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                // オブジェクトの中心からX軸、Y軸、Z軸方向の距離を計算
                float dist = length(float3(abs(i.viewDir.x) * _XScale, abs(i.viewDir.y) * _YScale, abs(i.viewDir.z) * _ZScale)); // 各軸のスケールを適用

                // 各軸のサイズを計算
                float innerRadiusX = _XSize * 0.2; // X軸方向のサイズの20%
                float innerRadiusY = _YSize * 0.2; // Y軸方向のサイズの20%
                float innerRadiusZ = _ZSize * 0.2; // Z軸方向のサイズの20%
                float innerRadius = length(float3(innerRadiusX, innerRadiusY, innerRadiusZ)); // 各軸方向のサイズを合成

                // 透明度の計算
                float alpha;
                if (dist < innerRadius) {
                    alpha = _OverlayAlpha; // 完全に不透明
                } else {
                    float normalizedDist = saturate((dist - innerRadius) / (_RangeEnd - innerRadius));
                    alpha = pow(1.0 - normalizedDist, _Exponent) * _OverlayAlpha; // 指数で急激に変化
                }

                // 指定された色とアルファ値を使用
                float4 color = _Color;
                color.a *= alpha;
                return color;
            }
            ENDCG
        }
    }
    FallBack "Hidden"
}
