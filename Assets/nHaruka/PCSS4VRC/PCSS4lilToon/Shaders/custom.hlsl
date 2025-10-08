//----------------------------------------------------------------------------------------------------------------------
// Macro

#if defined(SPOT) && !defined(SHADOWS_SOFT)
#define SHADOWS_SOFT
#endif

uniform float _IsOn = 1;
uniform float NoShadowMode = 0;

#include "UnityCG.cginc"

uniform float _EnvLightStrength = 0.3;
uniform float _ShadowDistance = 10.0;
uniform int _BlendOpFA;
uniform float3 _DropShadowColor = float3(0, 0, 0);
float shadowArea;

float3 _normal;
float3 _wpos;

uniform float _EnableSurfaceSmoothing = 1;
uniform float _ShadowcoordzOffset = 0.01;
uniform float _MinusNormalOffset = 0.08;
uniform float _PlusNormalOffset = 0.001;
uniform float _ShadingThreshold = 0.2;
uniform float _ShadingCutOffThreshold = 0.7;
uniform float _ShadingCutOffBlurRadius = 2;
uniform float _ShadingBlurRadius = 2;
uniform float _ShadowClamp = 0;
uniform float _ReceiveMaskStrength = 1;
uniform float _CastMaskStrength = 1;
uniform float _ShadowNormalBias = 0.003;
uniform float _ShadowBias = 0.0001;
uniform float _ShadowDensity = 0;
uniform float _InterpolationStrength = 1;

uniform float _ShadowColorOverrideStrength;
uniform sampler2D _ShadowColorOverrideTexture;
uniform float4 _ShadowColorOverrideTexture_ST;

uniform float _ShadowBiasMaskStrength;
uniform sampler2D _ShadowBiasMaskTexture;
uniform float4 _ShadowBiasMaskTexture_ST;

uniform sampler2D _CastMaskTex;
uniform float4 _CastMaskTex_ST;


#if defined(SPOT) && defined(SHADOWS_DEPTH)
/*
#undef DIRECTIONAL
#undef DIRECTIONAL_COOKIE
#undef POINT_COOKIE
#undef POINT
#undef UNITY_NO_SCREENSPACE_SHADOWS
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef SHADOWS_SCREEN
#undef SHADOWS_CUBE
#undef LIGHTMAP_ON
#undef VERTEXLIGHT_ON
#undef DIRLIGHTMAP_COMBINED
#undef DYNAMICLIGHTMAP_ON
#undef SHADOWS_SHADOWMASK
#undef LIGHTMAP_SHADOW_MIXING
#undef LIGHTPROBE_SH

*/

#define LIL_V2F_FORCE_POSITION_OS

uniform sampler2D _IgnoreCookieTexture;
uniform sampler2D _ReceiveMaskTex;
uniform float4 _ReceiveMaskTex_ST;

#include "Assets/nHaruka/PCSS4VRC/PCSSLogic/AutoLight_mod.cginc"

inline float InvLerp(float from, float to, float value)
{
	return saturate(value - from) / saturate(to - from);
}

#define LIL_V2F_FORCE_NORMAL

#define LIL_CUSTOM_V2F_MEMBER(id0,id1,id2,id3,id4,id5,id6,id7)\
	float4 positionOS : TEXCOORD ## id0;

// Add vertex copy
#define LIL_CUSTOM_VERT_COPY \
	 output.positionOS = input.positionOS;

inline float3 curvatureInterpolation(float3 worldPos, float3 worldNormal)
{
	float3 normalNormalized = normalize(worldNormal);
	float curvature = length(fwidth(normalNormalized)) / length(fwidth(worldPos));
	
	float3 interpolatedWorldPos = curvature < 0.01 ? worldPos : lerp(worldPos, worldPos + normalNormalized / (curvature) * (length(normalNormalized - worldNormal)), _InterpolationStrength);

	return interpolatedWorldPos;
}

uniform sampler2D _EnvLightLevelTexture;
uniform float _EnvLightAdjustLevel;

/*
inline float3 GetWorldPosFromDepth(float4 ScreenUV, float3 worldPos)
{
	float4 screenPos = float4(ScreenUV.xyz, ScreenUV.w + 0.00000000001);

	float4 screenPosNorm = screenPos / screenPos.w;
	screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0) ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;

	float eyeDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPosNorm.xy));
	float3 cameraViewDir = -UNITY_MATRIX_V._m20_m21_m22;
	float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
	float3 wpos = ((eyeDepth * worldViewDir * (1.0 / dot(cameraViewDir, worldViewDir))) + _WorldSpaceCameraPos);

	return wpos;
}
*/

#define BEFORE_UNPACK_V2F \
	NoShadowMode =  max(max(_LightColor0.r, _LightColor0.g), _LightColor0.b) <= 0.02 && max(max(_LightColor0.r, _LightColor0.g), _LightColor0.b) > 0.01 ? 1 : 0;\
	NoShadowMode = saturate(NoShadowMode + step(_ShadowDistance, distance(_WorldSpaceLightPos0.xyz,input.positionWS)));\
	_LightColor0.rgb = max(max(_LightColor0.r, _LightColor0.g), _LightColor0.b) <= 0.01 ?  _LightColor0.rgb * 100 : NoShadowMode == 1 ? (_LightColor0.rgb - 0.01) * 100 : 0 ;\
	_LightColor0.rgb = _LightColor0.rgb * _IsOn;\
	LIL_UNPACK_TEXCOORD0(input,fd); \
	LIL_UNPACK_TEXCOORD1(input,fd); \
	_normal = input.normalWS;\
	input.positionWS =  _InterpolationStrength > 0 ? curvatureInterpolation(input.positionWS, input.normalWS) : input.positionWS;\
	_wpos = input.positionWS;\
	_LightColor0.rgb = _LightColor0.rgb * lerp((tex2D(_EnvLightLevelTexture, half2(0.5, 0.5))).rgb, 1, 1 - _EnvLightAdjustLevel);\
	float biasStrength = lerp(1, tex2D(_ShadowBiasMaskTexture, fd.uv0 * _ShadowBiasMaskTexture_ST.xy + _ShadowBiasMaskTexture_ST.zw).b, _ShadowBiasMaskStrength);\
	_MinusNormalOffset *= biasStrength;\
	_PlusNormalOffset *= biasStrength;\
	_ShadowcoordzOffset *= biasStrength;

#define BEFORE_SHADOW \
	float rcvMask = lerp(1, tex2D(_ReceiveMaskTex, fd.uv0 * _ReceiveMaskTex_ST.xy + _ReceiveMaskTex_ST.zw).r, _ReceiveMaskStrength);\
	fd.attenuation = lerp(fd.attenuation, 1, 1 - rcvMask);\
	fd.attenuation = (_ShadowClamp > 0) ? step(_ShadowClamp ,fd.attenuation) : fd.attenuation ;\
	fd.attenuation = lerp(_ShadowDensity, 1, fd.attenuation);\
	fd.lightColor = min(LIL_MAINLIGHT_COLOR * fd.attenuation, _LightMaxLimit); \
	fd.lightColor = lerp(fd.lightColor, lilGray(fd.lightColor), _MonochromeLighting); \
	fd.lightColor = lerp(fd.lightColor, 0.0, _AsUnlit);\
	fd.lightColor = lerp(lerp(fd.lightColor, fd.lightColor * _DropShadowColor, (1 - shadowArea) ),fd.lightColor ,lerp(_ShadowDensity, 1, (1 - rcvMask))) ;\
    fd.lightColor = fd.lightColor * lerp(1, tex2D(_ShadowColorOverrideTexture, fd.uv0 * _ShadowColorOverrideTexture_ST.xy + _ShadowColorOverrideTexture_ST.zw).rgb, _ShadowColorOverrideStrength * (1 - shadowArea));




inline float3 OffsetWorldPos(float3 worldPos)
{
	float3 normalized = normalize(_normal);

	float3 lightDir = normalize(_WorldSpaceLightPos0.xyz - worldPos * _WorldSpaceLightPos0.w);
	float NdotL = dot(normalized, lightDir) * 0.5 + 0.5;

	float4 shadowZRow = unity_WorldToShadow[0][2];
	float offsetScale = _ShadowcoordzOffset / dot(shadowZRow, float4(worldPos, 1));
	float3 zOffset = offsetScale * shadowZRow.xyz;

	float plusOffsetArea = (1 - step(NdotL, _ShadingThreshold)) * InvLerp(_ShadingThreshold, _ShadingCutOffBlurRadius, NdotL) * step(NdotL, _ShadingCutOffThreshold) * InvLerp(1 - _ShadingCutOffThreshold, _ShadingCutOffBlurRadius, 1 - NdotL);
	float minusOffsetArea = step(NdotL, _ShadingThreshold) * InvLerp(1 - _ShadingThreshold, _ShadingBlurRadius, 1 - NdotL);
	float zOffsetArea = step(NdotL, _ShadingCutOffThreshold) * InvLerp(1 - _ShadingCutOffThreshold, 1, 1 - NdotL);

	float3 worldPosOffset = -normalized * _MinusNormalOffset * minusOffsetArea + zOffset * zOffsetArea + normalized * _PlusNormalOffset * plusOffsetArea;

	return worldPos + worldPosOffset;
}

#undef UNITY_LIGHT_ATTENUATION
#define UNITY_LIGHT_ATTENUATION(destName, input, worldPos) \
	float3 OffsettedWorldPos = OffsetWorldPos(worldPos); \
	DECLARE_LIGHT_COORD(input, NoShadowMode < 1 ? OffsettedWorldPos : worldPos); \
	shadowArea = 1; \
	if (NoShadowMode < 1) { shadowArea = UNITY_SHADOW_ATTENUATION(input, OffsettedWorldPos); }\
	fixed destName = (lightCoord.z > 0) *  tex2D(_IgnoreCookieTexture, lightCoord.xy / lightCoord.w + 0.5).w * UnitySpotAttenuate(lightCoord.xyz) * shadowArea;
#endif


/*
#if defined (UNITY_PASS_SHADOWCASTER) 
#define LIL_V2F_FORCE_NORMAL
#define LIL_V2F_FORCE_TEXCOORD0
uniform sampler2D _CastMaskTex;
uniform float4 _CastMaskTex_ST;
#define BEFORE_UNPACK_V2F \
    LIL_UNPACK_TEXCOORD0(input,fd); \
    LIL_UNPACK_TEXCOORD1(input,fd); \
	clip(lerp(1, tex2D(_CastMaskTex,fd.uv0 * _CastMaskTex_ST.xy + _CastMaskTex_ST.zw).r, _CastMaskStrength) - 0.5f);
#endif
*/


// Custom variables
//#define LIL_CUSTOM_PROPERTIES \
//    float _CustomVariable;

#define LIL_CUSTOM_PROPERTIES 

// Custom textures
#define LIL_CUSTOM_TEXTURES

// Add vertex shader input
//#define LIL_REQUIRE_APP_POSITION
//#define LIL_REQUIRE_APP_TEXCOORD0
//#define LIL_REQUIRE_APP_TEXCOORD1
//#define LIL_REQUIRE_APP_TEXCOORD2
//#define LIL_REQUIRE_APP_TEXCOORD3
//#define LIL_REQUIRE_APP_TEXCOORD4
//#define LIL_REQUIRE_APP_TEXCOORD5
//#define LIL_REQUIRE_APP_TEXCOORD6
//#define LIL_REQUIRE_APP_TEXCOORD7
//#define LIL_REQUIRE_APP_COLOR
//#define LIL_REQUIRE_APP_NORMAL
//#define LIL_REQUIRE_APP_TANGENT
//#define LIL_REQUIRE_APP_VERTEXID

// Add vertex shader output
//#define LIL_V2F_FORCE_TEXCOORD0
//#define LIL_V2F_FORCE_TEXCOORD1
//#define LIL_V2F_FORCE_POSITION_OS
//#define LIL_V2F_FORCE_POSITION_WS
//#define LIL_V2F_FORCE_POSITION_SS
//#define LIL_V2F_FORCE_NORMAL
//#define LIL_V2F_FORCE_TANGENT
//#define LIL_V2F_FORCE_BITANGENT
//#define LIL_CUSTOM_V2F_MEMBER(id0,id1,id2,id3,id4,id5,id6,id7)\
	float4 positionOS : TEXCOORD ## id0;

// Add vertex copy
//#define LIL_CUSTOM_VERT_COPY \
	 output.positionOS = input.positionOS;

//#define LIL_CUSTOM_VERTEX_OS \
	unity_LightShadowBias.z = _ShadowNormalBias;

// Inserting a process into pixel shader
//#define BEFORE_xx
//#define OVERRIDE_xx


#if !defined(SPOT) || (defined(SPOT) && !defined(SHADOWS_DEPTH))

#undef DIRECTIONAL
#undef DIRECTIONAL_COOKIE
#undef POINT_COOKIE
#undef POINT
#undef UNITY_NO_SCREENSPACE_SHADOWS
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef SHADOWS_SOFT
#undef SHADOWS_DEPTH
#undef SHADOWS_SCREEN
#undef SHADOWS_CUBE
#undef LIGHTMAP_ON
#undef VERTEXLIGHT_ON
#undef DIRLIGHTMAP_COMBINED
#undef DYNAMICLIGHTMAP_ON
#undef SHADOWS_SHADOWMASK
#undef LIGHTMAP_SHADOW_MIXING
#undef LIGHTPROBE_SH

#define LIL_V2F_POSITION_WS
#define BEFORE_MAIN\
		float3 overrideCol = lerp(1, tex2D(_ShadowColorOverrideTexture, fd.uv0 * _ShadowColorOverrideTexture_ST.xy + _ShadowColorOverrideTexture_ST.zw).rgb, _ShadowColorOverrideStrength);\
		fd.lightColor = fd.lightColor.rgb * (1 - _IsOn) + _IsOn * (fd.lightColor.rgb  * _EnvLightStrength) * overrideCol;\
		fd.indLightColor = fd.indLightColor.rgb * (1 - _IsOn) + _IsOn * (fd.indLightColor.rgb * _EnvLightStrength) * overrideCol;\
		fd.addLightColor = fd.addLightColor.rgb * (1 - _IsOn) + _IsOn * (fd.addLightColor.rgb  * _EnvLightStrength) * overrideCol;
//#define BEFORE_BACKLIGHT\
		return fd.col;
#endif



//----------------------------------------------------------------------------------------------------------------------
// Information about variables
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
// Vertex shader inputs (appdata structure)
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   input.positionOS        POSITION
// float2   input.uv0               TEXCOORD0
// float2   input.uv1               TEXCOORD1
// float2   input.uv2               TEXCOORD2
// float2   input.uv3               TEXCOORD3
// float2   input.uv4               TEXCOORD4
// float2   input.uv5               TEXCOORD5
// float2   input.uv6               TEXCOORD6
// float2   input.uv7               TEXCOORD7
// float4   input.color             COLOR
// float3   input.normalOS          NORMAL
// float4   input.tangentOS         TANGENT
// uint     vertexID                SV_VertexID

//----------------------------------------------------------------------------------------------------------------------
// Vertex shader outputs or pixel shader inputs (v2f structure)
//
// The structure depends on the pass.
// Please check lil_pass_xx.hlsl for details.
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   output.positionCS       SV_POSITION
// float2   output.uv01             TEXCOORD0 TEXCOORD1
// float2   output.uv23             TEXCOORD2 TEXCOORD3
// float3   output.positionOS       object space position
// float3   output.positionWS       world space position
// float3   output.normalWS         world space normal
// float4   output.tangentWS        world space tangent




//----------------------------------------------------------------------------------------------------------------------
// Variables commonly used in the forward pass
//
// These are members of `lilFragData fd`
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   col                     lit color
// float3   albedo                  unlit color
// float3   emissionColor           color of emission
// -------- ----------------------- --------------------------------------------------------------------
// float3   lightColor              color of light
// float3   indLightColor           color of indirectional light
// float3   addLightColor           color of additional light
// float    attenuation             attenuation of light
// float3   invLighting             saturate((1.0 - lightColor) * sqrt(lightColor));
// -------- ----------------------- --------------------------------------------------------------------
// float2   uv0                     TEXCOORD0
// float2   uv1                     TEXCOORD1
// float2   uv2                     TEXCOORD2
// float2   uv3                     TEXCOORD3
// float2   uvMain                  Main UV
// float2   uvMat                   MatCap UV
// float2   uvRim                   Rim Light UV
// float2   uvPanorama              Panorama UV
// float2   uvScn                   Screen UV
// bool     isRightHand             input.tangentWS.w > 0.0;
// -------- ----------------------- --------------------------------------------------------------------
// float3   positionOS              object space position
// float3   positionWS              world space position
// float4   positionCS              clip space position
// float4   positionSS              screen space position
// float    depth                   distance from camera
// -------- ----------------------- --------------------------------------------------------------------
// float3x3 TBN                     tangent / bitangent / normal matrix
// float3   T                       tangent direction
// float3   B                       bitangent direction
// float3   N                       normal direction
// float3   V                       view direction
// float3   L                       light direction
// float3   origN                   normal direction without normal map
// float3   origL                   light direction without sh light
// float3   headV                   middle view direction of 2 cameras
// float3   reflectionN             normal direction for reflection
// float3   matcapN                 normal direction for reflection for MatCap
// float3   matcap2ndN              normal direction for reflection for MatCap 2nd
// float    facing                  VFACE
// -------- ----------------------- --------------------------------------------------------------------
// float    vl                      dot(viewDirection, lightDirection);
// float    hl                      dot(headDirection, lightDirection);
// float    ln                      dot(lightDirection, normalDirection);
// float    nv                      saturate(dot(normalDirection, viewDirection));
// float    nvabs                   abs(dot(normalDirection, viewDirection));
// -------- ----------------------- --------------------------------------------------------------------
// float4   triMask                 TriMask (for lite version)
// float3   parallaxViewDirection   mul(tbnWS, viewDirection);
// float2   parallaxOffset          parallaxViewDirection.xy / (parallaxViewDirection.z+0.5);
// float    anisotropy              strength of anisotropy
// float    smoothness              smoothness
// float    roughness               roughness
// float    perceptualRoughness     perceptual roughness
// float    shadowmix               this variable is 0 in the shadow area
// float    audioLinkValue          volume acquired by AudioLink
// -------- ----------------------- --------------------------------------------------------------------
// uint     renderingLayers         light layer of object (for URP / HDRP)
// uint     featureFlags            feature flags (for HDRP)
// uint2    tileIndex               tile index (for HDRP)