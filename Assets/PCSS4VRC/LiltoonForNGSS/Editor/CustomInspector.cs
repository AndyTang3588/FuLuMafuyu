#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class liltoon4NGSSInspector : lilToonInspector
    {
        // Custom properties
        MaterialProperty NGSS_TEST_SAMPLERS;
        MaterialProperty NGSS_FILTER_SAMPLERS;
        MaterialProperty NGSS_LOCAL_SAMPLING_DISTANCE;
        MaterialProperty NGSS_GLOBAL_OPACITY;
        MaterialProperty NGSS_PCSS_FILTER_LOCAL_MIN;                    //Close to blocker (If 0.0 == Hard Shadows). This value cannot be higher than NGSS_PCSS_FILTER_LOCAL_MAX
        MaterialProperty NGSS_PCSS_FILTER_LOCAL_MAX;
        MaterialProperty NGSS_FORCE_HARD_SHADOWS;
        MaterialProperty NGSS_PCSS_LOCAL_BLOCKER_BIAS;
        MaterialProperty _ShadowNormalBias;
        MaterialProperty _EnvLightStrength;
        MaterialProperty _MaskTex;
        MaterialProperty _UseMaskTex;

        private static bool isShowCustomProperties;
        private const string shaderName = "liltoon4NGSS";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {
            isCustomShader = true;

            // If you want to change rendering modes in the editor, specify the shader here
            ReplaceToCustomShaders();
            //isShowRenderMode = !material.shader.name.Contains("Optional");

            // If not, set isShowRenderMode to false
            isShowRenderMode = true;

            //LoadCustomLanguage("");
            NGSS_TEST_SAMPLERS = FindProperty("NGSS_TEST_SAMPLERS", props);
            NGSS_FILTER_SAMPLERS = FindProperty("NGSS_FILTER_SAMPLERS", props);
            NGSS_LOCAL_SAMPLING_DISTANCE = FindProperty("NGSS_LOCAL_SAMPLING_DISTANCE", props);
            NGSS_GLOBAL_OPACITY = FindProperty("NGSS_GLOBAL_OPACITY", props);
            NGSS_PCSS_FILTER_LOCAL_MIN = FindProperty("NGSS_PCSS_FILTER_LOCAL_MIN", props);
            NGSS_PCSS_FILTER_LOCAL_MAX = FindProperty("NGSS_PCSS_FILTER_LOCAL_MAX", props);
            NGSS_FORCE_HARD_SHADOWS = FindProperty("NGSS_FORCE_HARD_SHADOWS", props);
            NGSS_PCSS_LOCAL_BLOCKER_BIAS = FindProperty("NGSS_PCSS_LOCAL_BLOCKER_BIAS", props);
            _MaskTex = FindProperty("_MaskTex", props);
            _UseMaskTex = FindProperty("_UseMaskTex", props);
            _ShadowNormalBias = FindProperty("_ShadowNormalBias", props);
            _EnvLightStrength = FindProperty("_EnvLightStrength", props);

            if(_MaskTex.textureValue == null)
            {
                _UseMaskTex.floatValue = 0;
            }
            else
            {
                _UseMaskTex.floatValue = 1;
            }
        }

        protected override void DrawCustomProperties(Material material)
        {
            // GUIStyles Name   Description
            // ---------------- ------------------------------------
            // boxOuter         outer box
            // boxInnerHalf     inner box
            // boxInner         inner box without label
            // customBox        box (similar to unity default box)
            // customToggleFont label for box

            isShowCustomProperties = Foldout("NGSS Shadow Settings", "Shadow Settings", isShowCustomProperties);
            if(isShowCustomProperties)
            {
                EditorGUILayout.BeginVertical(boxOuter);
                EditorGUILayout.LabelField(GetLoc("Shadow Settings"), customToggleFont);
                EditorGUILayout.BeginVertical(boxInnerHalf);

                
                //m_MaterialEditor.ShaderProperty(customVariable, "Custom Variable");
                m_MaterialEditor.ShaderProperty(NGSS_TEST_SAMPLERS, "NGSS_TEST_SAMPLERS");
                m_MaterialEditor.ShaderProperty(NGSS_FILTER_SAMPLERS, "NGSS_FILTER_SAMPLERS");
                m_MaterialEditor.ShaderProperty(NGSS_LOCAL_SAMPLING_DISTANCE, "NGSS_LOCAL_SAMPLING_DISTANCE");
                m_MaterialEditor.ShaderProperty(NGSS_GLOBAL_OPACITY, "NGSS_GLOBAL_OPACITY");
                m_MaterialEditor.ShaderProperty(NGSS_PCSS_FILTER_LOCAL_MIN, "NGSS_PCSS_FILTER_LOCAL_MIN");
                m_MaterialEditor.ShaderProperty(NGSS_PCSS_FILTER_LOCAL_MAX, "NGSS_PCSS_FILTER_LOCAL_MAX");
                m_MaterialEditor.ShaderProperty(NGSS_FORCE_HARD_SHADOWS, "NGSS_FORCE_HARD_SHADOWS");
                m_MaterialEditor.ShaderProperty(NGSS_PCSS_LOCAL_BLOCKER_BIAS, "NGSS_PCSS_LOCAL_BLOCKER_BIAS");
                m_MaterialEditor.ShaderProperty(_ShadowNormalBias, "_ShadowNormalBias");
                m_MaterialEditor.ShaderProperty(_EnvLightStrength, "_EnvLightStrength");
                m_MaterialEditor.ShaderProperty(_MaskTex, "_MaskTex");
                m_MaterialEditor.ShaderProperty(_UseMaskTex, "_UseMaskTex");


                if (GUILayout.Button("ApplyProperty All NGSS Material"))
                {
                    SetPropertyGlobal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }
        void SetPropertyGlobal()
        {
            foreach(Material mat in  Resources.FindObjectsOfTypeAll(typeof(Material)))
            {
               if(mat.shader.name.Contains("NGSS"))
                {
                    mat.SetInt("NGSS_TEST_SAMPLERS", (int)NGSS_TEST_SAMPLERS.floatValue);
                    mat.SetInt("NGSS_FILTER_SAMPLERS", (int)NGSS_FILTER_SAMPLERS.floatValue);
                    mat.SetFloat("NGSS_LOCAL_SAMPLING_DISTANCE", NGSS_LOCAL_SAMPLING_DISTANCE.floatValue);
                    mat.SetFloat("NGSS_GLOBAL_OPACITY", NGSS_GLOBAL_OPACITY.floatValue);
                    mat.SetFloat("NGSS_PCSS_FILTER_LOCAL_MIN", NGSS_PCSS_FILTER_LOCAL_MIN.floatValue);
                    mat.SetFloat("NGSS_PCSS_FILTER_LOCAL_MAX", NGSS_PCSS_FILTER_LOCAL_MAX.floatValue);
                    mat.SetFloat("NGSS_FORCE_HARD_SHADOWS", NGSS_FORCE_HARD_SHADOWS.floatValue);
                    mat.SetFloat("NGSS_PCSS_LOCAL_BLOCKER_BIAS", NGSS_PCSS_LOCAL_BLOCKER_BIAS.floatValue);
                    mat.SetFloat("_ShadowNormalBias", _ShadowNormalBias.floatValue);
                    mat.SetFloat("_EnvLightStrength", _EnvLightStrength.floatValue);
                    mat.SetTexture("_MaskTex", _MaskTex.textureValue);
                    mat.SetFloat("_UseMaskTex", _UseMaskTex.floatValue);
                }
            }
        }

        protected override void ReplaceToCustomShaders()
        {
            lts         = Shader.Find(shaderName + "/lilToon");
            ltsc        = Shader.Find("Hidden/" + shaderName + "/Cutout");
            ltst        = Shader.Find("Hidden/" + shaderName + "/Transparent");
            ltsot       = Shader.Find("Hidden/" + shaderName + "/OnePassTransparent");
            ltstt       = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparent");

            ltso        = Shader.Find("Hidden/" + shaderName + "/OpaqueOutline");
            ltsco       = Shader.Find("Hidden/" + shaderName + "/CutoutOutline");
            ltsto       = Shader.Find("Hidden/" + shaderName + "/TransparentOutline");
            ltsoto      = Shader.Find("Hidden/" + shaderName + "/OnePassTransparentOutline");
            ltstto      = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparentOutline");

            ltsoo       = Shader.Find(shaderName + "/[Optional] OutlineOnly/Opaque");
            ltscoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Cutout");
            ltstoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Transparent");

            ltstess     = Shader.Find("Hidden/" + shaderName + "/Tessellation/Opaque");
            ltstessc    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Cutout");
            ltstesst    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Transparent");
            ltstessot   = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparent");
            ltstesstt   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparent");

            ltstesso    = Shader.Find("Hidden/" + shaderName + "/Tessellation/OpaqueOutline");
            ltstessco   = Shader.Find("Hidden/" + shaderName + "/Tessellation/CutoutOutline");
            ltstessto   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TransparentOutline");
            ltstessoto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparentOutline");
            ltstesstto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparentOutline");

            ltsl        = Shader.Find(shaderName + "/lilToonLite");
            ltslc       = Shader.Find("Hidden/" + shaderName + "/Lite/Cutout");
            ltslt       = Shader.Find("Hidden/" + shaderName + "/Lite/Transparent");
            ltslot      = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparent");
            ltsltt      = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparent");

            ltslo       = Shader.Find("Hidden/" + shaderName + "/Lite/OpaqueOutline");
            ltslco      = Shader.Find("Hidden/" + shaderName + "/Lite/CutoutOutline");
            ltslto      = Shader.Find("Hidden/" + shaderName + "/Lite/TransparentOutline");
            ltsloto     = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparentOutline");
            ltsltto     = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparentOutline");

            ltsref      = Shader.Find("Hidden/" + shaderName + "/Refraction");
            ltsrefb     = Shader.Find("Hidden/" + shaderName + "/RefractionBlur");
            ltsfur      = Shader.Find("Hidden/" + shaderName + "/Fur");
            ltsfurc     = Shader.Find("Hidden/" + shaderName + "/FurCutout");
            ltsfurtwo   = Shader.Find("Hidden/" + shaderName + "/FurTwoPass");
            ltsfuro     = Shader.Find(shaderName + "/[Optional] FurOnly/Transparent");
            ltsfuroc    = Shader.Find(shaderName + "/[Optional] FurOnly/Cutout");
            ltsfurotwo  = Shader.Find(shaderName + "/[Optional] FurOnly/TwoPass");
            ltsgem      = Shader.Find("Hidden/" + shaderName + "/Gem");
            ltsfs       = Shader.Find(shaderName + "/[Optional] FakeShadow");

            ltsover     = Shader.Find(shaderName + "/[Optional] Overlay");
            ltsoover    = Shader.Find(shaderName + "/[Optional] OverlayOnePass");
            ltslover    = Shader.Find(shaderName + "/[Optional] LiteOverlay");
            ltsloover   = Shader.Find(shaderName + "/[Optional] LiteOverlayOnePass");

            ltsm        = Shader.Find(shaderName + "/lilToonMulti");
            ltsmo       = Shader.Find("Hidden/" + shaderName + "/MultiOutline");
            ltsmref     = Shader.Find("Hidden/" + shaderName + "/MultiRefraction");
            ltsmfur     = Shader.Find("Hidden/" + shaderName + "/MultiFur");
            ltsmgem     = Shader.Find("Hidden/" + shaderName + "/MultiGem");
        }

        // You can create a menu like this
        
        [MenuItem("Assets/liltoon4NGSS/Convert material to liltoon4NGSS", false, 1100)]
        private static void ConvertMaterialToCustomShaderMenu()
        {
            if(Selection.objects.Length == 0) return;
            liltoon4NGSSInspector inspector = new liltoon4NGSSInspector();
            for(int i = 0; i < Selection.objects.Length; i++)
            {
                if(Selection.objects[i] is Material)
                {
                    inspector.ConvertMaterialToCustomShader((Material)Selection.objects[i]);
                }
            }
        }
        public void ConvertMaterialProxy(Material material)
        {
            this.ConvertMaterialToCustomShader(material);

        }
        
    }
}
#endif