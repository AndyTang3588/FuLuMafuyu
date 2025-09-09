#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class liltoon4SimplePCSSInspector : lilToonInspector
    {
        // Custom properties
        MaterialProperty SimplePCSS_Softness;
        MaterialProperty _EnvLightStrength;
        MaterialProperty _MaskTex;
        MaterialProperty _UseMaskTex;

        private static bool isShowCustomProperties;
        private const string shaderName = "liltoon4SimplePCSS";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {
            isCustomShader = true;

            // If you want to change rendering modes in the editor, specify the shader here
            ReplaceToCustomShaders();
            //isShowRenderMode = !material.shader.name.Contains("Optional");

            // If not, set isShowRenderMode to false
            isShowRenderMode = true;

            //LoadCustomLanguage("");
            SimplePCSS_Softness = FindProperty("SimplePCSS_Softness", props);
            _EnvLightStrength = FindProperty("_EnvLightStrength", props);
            //_MaskTex = FindProperty("_MaskTex", props);
            //_UseMaskTex = FindProperty("_UseMaskTex", props);
            //if (_MaskTex.textureValue == null)
            //{
            //    _UseMaskTex.floatValue = 0;
            //}
            //else
            //{
            //    _UseMaskTex.floatValue = 1;
            //}
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

            isShowCustomProperties = Foldout("PCSS Shadow Settings", "Shadow Settings", isShowCustomProperties);
            if(isShowCustomProperties)
            {
                EditorGUILayout.BeginVertical(boxOuter);
                EditorGUILayout.LabelField(GetLoc("Shadow Settings"), customToggleFont);
                EditorGUILayout.BeginVertical(boxInnerHalf);

                
                //m_MaterialEditor.ShaderProperty(customVariable, "Custom Variable");
                m_MaterialEditor.ShaderProperty(SimplePCSS_Softness, "SimplePCSS_Softness");
                m_MaterialEditor.ShaderProperty(_EnvLightStrength, "EnvLightStrength");
                //m_MaterialEditor.ShaderProperty(_MaskTex, "_MaskTex");
                //m_MaterialEditor.ShaderProperty(_UseMaskTex, "_UseMaskTex");

                if (GUILayout.Button("ApplyProperty All PCSS Material"))
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
               if(mat.shader.name.Contains("PCSS"))
                {
                    mat.SetFloat("SimplePCSS_Softness", SimplePCSS_Softness.floatValue);
                    mat.SetFloat("_EnvLightStrength", _EnvLightStrength.floatValue);
                    //mat.SetTexture("_MaskTex", _MaskTex.textureValue);
                    //mat.SetFloat("_UseMaskTex", _UseMaskTex.floatValue);
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
        
        [MenuItem("Assets/liltoon4SimplePCSS/Convert material to liltoon4SimplePCSS", false, 1100)]
        private static void ConvertMaterialToCustomShaderMenu()
        {
            if(Selection.objects.Length == 0) return;
            liltoon4SimplePCSSInspector inspector = new liltoon4SimplePCSSInspector();
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