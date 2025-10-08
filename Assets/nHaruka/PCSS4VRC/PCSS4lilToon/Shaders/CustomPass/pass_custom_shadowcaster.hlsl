#ifndef LIL_PASS_SHADOWCASTER_INCLUDED
#define LIL_PASS_SHADOWCASTER_INCLUDED

#include "Packages/jp.lilxyzw.liltoon/Shader/Includes/lil_common.hlsl"
#include "Packages/jp.lilxyzw.liltoon/Shader/Includes/lil_common_appdata.hlsl"

#define LIL_V2F_FORCE_TEXCOORD0
#define LIL_V2F_FORCE_POSITION_WS
#define LIL_CUSTOM_VERTEX_OS \
	unity_LightShadowBias.z = _ShadowNormalBias;
#define LIL_CUSTOM_V2F_MEMBER(id0,id1,id2,id3,id4,id5,id6,id7)\
    float3 normalWS : TEXCOORD ## id0;
#define LIL_CUSTOM_VERT_COPY  \
    output.normalWS = vertexNormalInput.normalWS;

//------------------------------------------------------------------------------------------------------------------------------
// Structure
#if !defined(LIL_CUSTOM_V2F_MEMBER)
#define LIL_CUSTOM_V2F_MEMBER(id0,id1,id2,id3,id4,id5,id6,id7)
#endif

#if defined(LIL_V2F_FORCE_TEXCOORD0) || (LIL_RENDER > 0)
#if defined(LIL_FUR)
#define LIL_V2F_TEXCOORD0
#else
#define LIL_V2F_PACKED_TEXCOORD01
#define LIL_V2F_PACKED_TEXCOORD23
#endif
#endif
#if defined(LIL_V2F_FORCE_POSITION_OS) || ((LIL_RENDER > 0) && !defined(LIL_LITE) && defined(LIL_FEATURE_DISSOLVE))
#define LIL_V2F_POSITION_OS
#endif
#if defined(LIL_V2F_FORCE_POSITION_WS) || (LIL_RENDER > 0) && defined(LIL_FEATURE_DISTANCE_FADE)
#define LIL_V2F_POSITION_WS
#endif
#define LIL_V2F_SHADOW_CASTER

struct v2f
{
    LIL_V2F_SHADOW_CASTER_OUTPUT
#if defined(LIL_V2F_TEXCOORD0)
        float2 uv0 : TEXCOORD1;
#endif
#if defined(LIL_V2F_PACKED_TEXCOORD01)
    float4 uv01 : TEXCOORD1;
#endif
#if defined(LIL_V2F_PACKED_TEXCOORD23)
    float4 uv23 : TEXCOORD2;
#endif
#if defined(LIL_V2F_POSITION_OS)
    float4 positionOSdissolve : TEXCOORD3;
#endif
#if defined(LIL_V2F_POSITION_WS)
    float3 positionWS : TEXCOORD4;
#endif
    LIL_CUSTOM_V2F_MEMBER(5, 6, 7, 8, 9, 10, 11, 12)
        LIL_VERTEX_INPUT_INSTANCE_ID
        LIL_VERTEX_OUTPUT_STEREO
};

uniform float _ShadowCasterBias;
uniform float _ShadowCasterBiasOffset;


inline float3 curvatureInterpolation(float3 worldPos, float3 worldNormal)
{
    float3 normalNormalized = normalize(worldNormal);
    float curvature = length(fwidth(normalNormalized)) / length(fwidth(worldPos));

    float3 interpolatedWorldPos = curvature < 0.01 ? worldPos : lerp(worldPos, worldPos + normalNormalized / (curvature) * (length(normalNormalized - worldNormal)), _InterpolationStrength);

    return interpolatedWorldPos;
}

//------------------------------------------------------------------------------------------------------------------------------
// Shader
#include "Packages/jp.lilxyzw.liltoon/Shader/Includes/lil_common_vert.hlsl"
#include "Packages/jp.lilxyzw.liltoon/Shader/Includes/lil_common_frag.hlsl"

float4 frag(v2f input LIL_VFACE(facing), inout float depth: SV_Depth) : SV_Target
{
    LIL_SETUP_INSTANCE_ID(input);
    LIL_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    lilFragData fd = lilInitFragData();

    LIL_UNPACK_TEXCOORD0(input, fd);
    
    LIL_UNPACK_TEXCOORD1(input, fd); 
    
    float2 biasStrength = lerp(1, tex2D(_ShadowBiasMaskTexture, input.uv01.xy * _ShadowBiasMaskTexture_ST.xy + _ShadowBiasMaskTexture_ST.z).rg, _ShadowBiasMaskStrength);

    float3 lightDir = normalize(_WorldSpaceLightPos0.xyz - input.positionWS);

    float NdotL = max(dot(normalize(input.normalWS), lightDir), 0.0);

    float offsetAmount = -saturate(NdotL - _ShadowCasterBiasOffset) * _ShadowCasterBias * biasStrength.r;

    float3 interpolatedWpos = _InterpolationStrength > 0 ? curvatureInterpolation(input.positionWS, input.normalWS) : input.positionWS;

    float3 offsetPos = interpolatedWpos + normalize(input.normalWS) * offsetAmount;
    
    /*
    if (_ShadowNormalBias != 0.0)
    {
        float shadowCos = dot(input.normalWS, lightDir);
        float shadowSine = sqrt(1 - shadowCos * shadowCos);
        float normalBias = _ShadowNormalBias * shadowSine;

        offsetPos -= input.normalWS * normalBias * biasStrength.g;
    }
    */
    float4 clipPos = UnityWorldToClipPos(float4(offsetPos, 1.0));

    clipPos = UnityApplyLinearShadowBias(clipPos);

    depth = clipPos.z / clipPos.w;
    
    float castMask = lerp(1, tex2D(_CastMaskTex, input.uv01.xy * _CastMaskTex_ST.xy + _CastMaskTex_ST.zw).r, _CastMaskStrength) - 0.5f;

    clip(castMask);

    depth = castMask < 0 ? 1 : depth;

    BEFORE_UNPACK_V2F
    OVERRIDE_UNPACK_V2F
    LIL_COPY_VFACE(fd.facing);

    #include "Packages/jp.lilxyzw.liltoon/Shader/Includes/lil_common_frag_alpha.hlsl"

    LIL_SHADOW_CASTER_FRAGMENT(input);
}

#endif