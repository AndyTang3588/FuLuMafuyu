#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class motchiriInspector : lilToonInspector
    {
        // Custom properties
        //MaterialProperty customVariable;
		MaterialProperty _Mask;
		MaterialProperty _strength;
		MaterialProperty _effect;
		MaterialProperty _ao;
		MaterialProperty _blur;
		MaterialProperty _color;
		MaterialProperty _cancel;
		MaterialProperty _oldver;
		MaterialProperty _multi;
		MaterialProperty _depth;
		MaterialProperty _func1;
		MaterialProperty _func2;
		MaterialProperty _func3;
		MaterialProperty _normal;
		MaterialProperty _nfunc;
		MaterialProperty _excollider;
		MaterialProperty _exfactor;
		MaterialProperty _radius;
		MaterialProperty _unit;
		MaterialProperty _SR;
		MaterialProperty _SL;
		MaterialProperty _OR;
		MaterialProperty _OL;
		MaterialProperty _SE1;
		MaterialProperty _SE2;
		MaterialProperty _SE3;
		MaterialProperty _OE1;
		MaterialProperty _OE2;
		MaterialProperty _OE3;
		MaterialProperty _T;

        private static bool isShowCustomProperties;
        private const string shaderName = "motchiri";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {
            isCustomShader = true;

            // If you want to change rendering modes in the editor, specify the shader here
            ReplaceToCustomShaders();
            isShowRenderMode = !material.shader.name.Contains("Optional");

            // If not, set isShowRenderMode to false
            //isShowRenderMode = false;

            //LoadCustomLanguage("");
            //customVariable = FindProperty("_CustomVariable", props);
			_Mask = FindProperty("_Mask", props);
			_strength = FindProperty("_strength", props);
			_effect = FindProperty("_effect", props);
			_ao = FindProperty("_ao", props);
			_blur = FindProperty("_blur", props);
			_color = FindProperty("_color", props);
			_cancel = FindProperty("_cancel", props);
			_oldver = FindProperty("_oldver", props);
			_multi = FindProperty("_multi", props);
			_depth = FindProperty("_depth", props);
			_func1 = FindProperty("_func1", props);
			_func2 = FindProperty("_func2", props);
			_func3 = FindProperty("_func3", props);
			_normal = FindProperty("_normal", props);
			_nfunc = FindProperty("_nfunc", props);
			_excollider = FindProperty("_excollider", props);
			_exfactor = FindProperty("_exfactor", props);
			_radius = FindProperty("_radius", props);
			_unit = FindProperty("_unit", props);
			_SR = FindProperty("_SR", props);
			_SL = FindProperty("_SL", props);
			_OR = FindProperty("_OR", props);
			_OL = FindProperty("_OL", props);
			_OE1 = FindProperty("_OE1", props);
			_OE2 = FindProperty("_OE2", props);
			_OE3 = FindProperty("_OE3", props);
			_SE1 = FindProperty("_SE1", props);
			_SE2 = FindProperty("_SE2", props);
			_SE3 = FindProperty("_SE3", props);
			_T = FindProperty("_T", props);
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

            isShowCustomProperties = Foldout("Custom Properties", "Custom Properties", isShowCustomProperties);
            if(isShowCustomProperties)
            {
                EditorGUILayout.BeginVertical(boxOuter);
                EditorGUILayout.LabelField(GetLoc("Custom Properties"), customToggleFont);
                EditorGUILayout.BeginVertical(boxInnerHalf);

                //m_MaterialEditor.ShaderProperty(customVariable, "Custom Variable");
				m_MaterialEditor.ShaderProperty(_Mask, "Mask(範囲指定マスク)");
				m_MaterialEditor.ShaderProperty(_strength, "strength(強さ)");
				m_MaterialEditor.ShaderProperty(_effect, "effect(効果範囲)");
				m_MaterialEditor.ShaderProperty(_ao, "ao(影生成)");
				m_MaterialEditor.ShaderProperty(_blur, "blur(にじみ)");
				m_MaterialEditor.ShaderProperty(_color, "color(色)");
				m_MaterialEditor.ShaderProperty(_cancel, "cancel(無効化)");
				m_MaterialEditor.ShaderProperty(_oldver, "oldver(旧仕様)");
				m_MaterialEditor.ShaderProperty(_multi, "strength×5");
				m_MaterialEditor.ShaderProperty(_depth, "depth");
				m_MaterialEditor.ShaderProperty(_func1, "func1");
				m_MaterialEditor.ShaderProperty(_func2, "func2");
				m_MaterialEditor.ShaderProperty(_func3, "func3");
				m_MaterialEditor.ShaderProperty(_normal, "normal(法線修正)");
				m_MaterialEditor.ShaderProperty(_nfunc, "normal function");
				m_MaterialEditor.ShaderProperty(_excollider, "EX collider");
				m_MaterialEditor.ShaderProperty(_exfactor, "EX scaling factor");
				m_MaterialEditor.ShaderProperty(_radius, "radius");
				m_MaterialEditor.ShaderProperty(_unit, "unit");
				m_MaterialEditor.ShaderProperty(_SR, "SelfR");
				m_MaterialEditor.ShaderProperty(_SL, "SelfL");
				m_MaterialEditor.ShaderProperty(_OR, "OthersR");
				m_MaterialEditor.ShaderProperty(_OL, "OthersL");
				m_MaterialEditor.ShaderProperty(_SE1, "SelfEX1");
				m_MaterialEditor.ShaderProperty(_SE2, "SelfEX2");
				m_MaterialEditor.ShaderProperty(_SE3, "SelfEX3");
				m_MaterialEditor.ShaderProperty(_OE1, "OtherEX1");
				m_MaterialEditor.ShaderProperty(_OE2, "OtherEX2");
				m_MaterialEditor.ShaderProperty(_OE3, "OtherEX3");
				m_MaterialEditor.ShaderProperty(_T, "Test");

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
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
        /*
        [MenuItem("Assets/TemplateFull/Convert material to custom shader", false, 1100)]
        private static void ConvertMaterialToCustomShaderMenu()
        {
            if(Selection.objects.Length == 0) return;
            TemplateFullInspector inspector = new TemplateFullInspector();
            for(int i = 0; i < Selection.objects.Length; i++)
            {
                if(Selection.objects[i] is Material)
                {
                    inspector.ConvertMaterialToCustomShader((Material)Selection.objects[i]);
                }
            }
        }
        */
    }
}
#endif