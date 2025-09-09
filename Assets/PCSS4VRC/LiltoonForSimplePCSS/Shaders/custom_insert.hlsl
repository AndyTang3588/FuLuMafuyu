#undef unity_LightShadowBias

//#define BEFORE_UNPACK_V2F \
	_EnvLightStrength = (_EnvLightStrength * (1 - _UseMaskTex)) + ((1 - LIL_SAMPLE_2D(_MaskTex,sampler_MaskTex, fd.uv0).r)* _UseMaskTex);
