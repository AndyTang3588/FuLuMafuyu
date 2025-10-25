Shader "ZeroFactory/APS/VRCCameraModeShader"
{
  Properties {
    _MainTex ("Main Texture", 2D) = "white" {}
    _MainColor ("Background Color", color) = (0,0,0,1)
		[Toggle] _IsGrayScale("GrayScale", Float) = 0
		[Toggle] _IsShowCameraAndScreenshot("Show Camera And Screenshot", Float) = 1
    [KeywordEnum(OFF,ON)] _ZWrite("ZWrite",Int) = 0
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Int) = 4
  }

  SubShader {
  // Draw ourselves after all opaque geometry
  Tags { 
    "Queue" = "Transparent"
    "RenderType" = "Transparent"
    "DisableBatching" = "True" 
  }

  Blend SrcAlpha OneMinusSrcAlpha //重なったオブジェクトの画素の色とのブレンド方法の指定

  Pass {
    ZWrite [_ZWrite]
    ZTest [_ZTest]

    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"

    float _VRChatCameraMode;
    float _IsShowCameraAndScreenshot;
    float _IsGrayScale;

    sampler2D _MainTex;
    float4 _MainTex_ST;
    float4 _MainColor;

    struct v2f {
      float4 pos : SV_POSITION;
      float2 uv : TEXCOORD0;
    };

    v2f vert(appdata_base v)
    {
        v2f o;
        if (_IsShowCameraAndScreenshot < 0.5 && _VRChatCameraMode > 0.5) return o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        if (_IsShowCameraAndScreenshot < 0.5 && _VRChatCameraMode > 0.5) discard;
        float4 c = float4(0, 0, 0, 0);
        c = tex2D(_MainTex, i.uv) * _MainColor;
        if(_IsGrayScale > 0.5){
          float g = c.r * 0.299 + c.g * 0.587 + c.b * 0.114;
          c = float4(g, g, g, c.a);
        }
        return c;
    }

    ENDCG
    }
  }
}