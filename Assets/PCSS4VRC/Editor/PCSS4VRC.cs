using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using lilToon;
using System.Reflection;

public class PCSS4VRC : EditorWindow
{
    private VRCAvatarDescriptor avatarDescriptor;
    private bool debugMode = false;
    private bool toggle = false;
    private bool WriteDefault = true;
    private bool UseNGSS = false;
    // Start is called before the first frame update
    [MenuItem("nHaruka/PCSS For VRC")]
    // Start is called before the first frame update
    private static void Init()
    {
        var window = GetWindowWithRect<PCSS4VRC>(new Rect(0, 0, 700, 540));
        window.Show();
    }

    // Update is called once per frame
    private void OnGUI()
    {
        GUILayout.Space(10);
        avatarDescriptor =
            (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", avatarDescriptor, typeof(VRCAvatarDescriptor), true);
        GUILayout.Space(10);

        UseNGSS = GUILayout.Toggle(UseNGSS, "NGSS(Next-Gen Soft-Shadows)を持っている");

        GUILayout.Space(10);

        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        style.fontSize = 12;
        style.wordWrap = true;
        style.fontStyle = FontStyle.Normal;

        EditorGUILayout.LabelField("【PCSS For VRC 導入手順】\n" +
            "① [NGSSを使用する場合（推奨）] NGSS(Next-Gen Soft-Shadows)をPackageManagerでImportする。\n" +
            "② lilToon(Ver1.4.0, unitypackage版)を導入する。\n" +
            "③ アバターのシェーダーをlilToonで統一する。\n" +
            "④ 本ツールを実行してアバターをセットアップする。\n" +
            "※アバターや画面がマテリアルエラーになる場合は本ツールの[ShaderのReimport＆キャッシュのクリア]ボタンを押してみてください。\nそれでも治らない場合は、NGSSとlilToonとPCSS4VRC（本ツール）をプロジェクトから削除し、もう一度インポートしなおしてから最初から導入しなおしてみてください。", style);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("マテリアルを選択するとインスペクターから設定値を調整することができます。\n" +
            "[カスタムプロパティ]の[PCSS/NGSS Shadow Settings]という項目です。\n" +
            "設定した値は一番下の[ApplyProperty All PCSS/NGSS Material]ボタンを押すことで、\n全てのPCSS/NGSS Shaderのマテリアルに反映させることができます。", style);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("※SimplePCSS(NGSSなし)でセットアップした場合、設定できるのは影の柔らかさとフェードのみです。\n" +
            "影の強さ、柔らかさ、距離によるフェード等々細かく調整したい場合はぜひNGSSの導入をご検討ください。", style);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("※注意！！アバターのマテリアルを上書きするので、マテリアルまで含めてバックアップを取ることを強く推奨します。", style);
        GUILayout.Space(10);

        WriteDefault = GUILayout.Toggle(WriteDefault, "WriteDefaults");
        EditorGUILayout.LabelField("※導入アバターのFXレイヤーがどちらで統一されているかによって選択してください。\n統一されていないと表情がおかしくなったり正しく機能しなかったりします。", style);
        GUILayout.Space(10);

        if (!toggle)
        {
            toggle = GUILayout.Toggle(toggle, "導入手順を読みました", GUI.skin.button);
        }

        if (toggle)
        {

            if (GUILayout.Button("Setup"))
            {
                if (debugMode)
                {
                    setup();
                }
                else
                {
                    setup();
                    EditorUtility.DisplayDialog("Finished", "Finished!", "OK");
                }

            }

            if (GUILayout.Button("Remove"))
            {
                if (debugMode)
                {
                    remove();
                }
                else
                {
                    try
                    {
                        remove();
                        EditorUtility.DisplayDialog("Finished", "Finished!", "OK");
                    }
                    catch
                    {
                        EditorUtility.DisplayDialog("Error", "An error occurred", "OK");
                    }
                }
            }

            if (GUILayout.Button("ShaderのReimport＆キャッシュのクリア"))
            {
                if (debugMode)
                {
                    Refresh();
                }
                else
                {
                    try
                    {
                        Refresh();
                        EditorUtility.DisplayDialog("Finished", "Finished!", "OK");
                    }
                    catch
                    {
                        EditorUtility.DisplayDialog("Error", "An error occurred", "OK");
                    }
                }
            }
        }
        GUILayout.Space(10);


        debugMode = GUILayout.Toggle(debugMode, "DebugMode");
        GUILayout.Space(10);
        
    }

    void Refresh()
    {
        var shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache");

        try
        {
            if (Directory.Exists(shaderCachePath))
            {
                Directory.Delete(shaderCachePath, true);
            }
        }
        catch
        { }

        AssetDatabase.Refresh();

        foreach (Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)))
        {
            if (mat.shader.name.Contains("NGSS")|| mat.shader.name.Contains("PCSS"))
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat.shader), ImportAssetOptions.Default);
            }
        }


    }


    void SetupLayers()
    { 
        string[] requiredLayers = 
        {
            "Player",
            "PlayerLocal",
            "MirrorReflection",
        };
        int[] requiredLayerIds =
        {
            9,
            10,
            18,
        };
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layersProp = tagManager.FindProperty("layers");
        var index = 0;
        foreach (var layerId in requiredLayerIds)
        {
            if (layersProp.arraySize > layerId)
            {
                var sp = layersProp.GetArrayElementAtIndex(layerId);
                if (sp != null && sp.stringValue != requiredLayers[index])
                {
                    sp.stringValue = requiredLayers[index];
                    Debug.Log("Adding layer " + requiredLayers[index]);
                }
            }

            index++;
        }
        tagManager.ApplyModifiedProperties();
    }

    private void setup()
    {
        
        try
        {
       

            remove();

        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }



        SetupLayers();

        SetAvatarMaterials();

        lilToonSetting settings = new lilToonSetting();
        if (File.Exists("Assets/lilToonSetting/ShaderSetting.asset"))
        {
            settings = AssetDatabase.LoadAssetAtPath<lilToonSetting>("Assets/lilToonSetting/ShaderSetting.asset");
        }
        settings.LIL_OPTIMIZE_USE_FORWARDADD = true;
        settings.LIL_OPTIMIZE_USE_FORWARDADD_SHADOW = true;
        settings.LIL_OPTIMIZE_APPLY_SHADOW_FA = true;
        lilToonSetting.BuildShaderSettingString(settings, true);
        lilToonSetting.SaveShaderSetting(settings);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/nHaruka/PCSS4VRC/SelfLight.prefab");
        var Prefab_Unpack = GameObject.Instantiate<GameObject>(prefab);
        Prefab_Unpack.name = "SelfLight";
        Prefab_Unpack.transform.parent = avatarDescriptor.transform;

        var RootConstraint = Prefab_Unpack.GetOrAddComponent<ParentConstraint>();
        RootConstraint.locked = true;
        RootConstraint.constraintActive = true;
        RootConstraint.SetSource(0, new ConstraintSource { weight = 1, sourceTransform = avatarDescriptor.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head) });
        RootConstraint.SetTranslationOffset(0, Vector3.zero);
        RootConstraint.rotationAtRest = Vector3.zero;
        RootConstraint.enabled = true;

        if(!Directory.Exists("Assets/nHaruka/PCSS4VRC/"+avatarDescriptor.name))
        {
            Directory.CreateDirectory("Assets/nHaruka/PCSS4VRC/" + avatarDescriptor.name);
        }

        AssetDatabase.CopyAsset("Assets/nHaruka/PCSS4VRC/LightControl.controller", "Assets/nHaruka/PCSS4VRC/" + avatarDescriptor.name +"/ LightControl_copy.controller");

        var AddAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/nHaruka/PCSS4VRC/" + avatarDescriptor.name + "/ LightControl_copy.controller");

        EditorUtility.SetDirty(AddAnimatorController);

        if (WriteDefault == false)
        {
            foreach (var layer in AddAnimatorController.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    state.state.writeDefaultValues = false;
                }
            }
        }

        var FxAnimatorLayer =
                avatarDescriptor.baseAnimationLayers.First(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null);
        var FxAnimator = (AnimatorController)FxAnimatorLayer.animatorController;

        EditorUtility.SetDirty(FxAnimator);

        FxAnimator.parameters = FxAnimator.parameters.Union(AddAnimatorController.parameters).ToArray();
        foreach (var layer in AddAnimatorController.layers)
        {
            FxAnimator.AddLayer(layer);
        }
        var AddExpParam = AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>("Assets/nHaruka/PCSS4VRC/LightControl_params.asset");

        avatarDescriptor.expressionParameters.parameters = avatarDescriptor.expressionParameters.parameters.Union(AddExpParam.parameters).ToArray();

        EditorUtility.SetDirty(avatarDescriptor.expressionParameters);

        if (avatarDescriptor.expressionsMenu.controls.Count != 8)
        {
            var AddSubMenu = AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>("Assets/nHaruka/PCSS4VRC/LightControl.asset");

            var newMenu = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control();
            newMenu.name = "LightControl";
            newMenu.type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu;
            newMenu.subMenu = AddSubMenu;

            avatarDescriptor.expressionsMenu.controls.Add(newMenu);
        }

        EditorUtility.SetDirty(avatarDescriptor.expressionsMenu);

        AssetDatabase.SaveAssets();

        Refresh();
    }

    void SetAvatarMaterials()
    {
        var renderers = avatarDescriptor.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            foreach (var m in r.sharedMaterials)
            {
                if (m != null && m.shader.name.Contains("lilToon"))
                {
                    if (UseNGSS)
                    {
                        liltoon4NGSSInspector inspector = new liltoon4NGSSInspector();
                        inspector.ConvertMaterialProxy(m);
                    }
                    else
                    {
                        liltoon4SimplePCSSInspector inspector = new liltoon4SimplePCSSInspector();
                        inspector.ConvertMaterialProxy(m);
                    }
                }
            }
        }
    }

    void remove()
    {
        if (avatarDescriptor.transform.Find("SelfLight") != null)
        {
            DestroyImmediate(avatarDescriptor.transform.Find("SelfLight").gameObject);
        }

        try
        {
            avatarDescriptor.expressionsMenu.controls.RemoveAll(item => item.name == "LightControl");
            avatarDescriptor.expressionParameters.parameters =avatarDescriptor.expressionParameters.parameters.Where(item => !item.name.Contains("LightIntensity") && !item.name.Contains("LightStrength")).ToArray();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        try
        {
            var FxAnimatorLayer =
                avatarDescriptor.baseAnimationLayers.First(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null);
            var FxAnimator = (AnimatorController)FxAnimatorLayer.animatorController;

            

            FxAnimator.layers = FxAnimator.layers.Where(item => !item.name.Contains("LightIntensity") && !item.name.Contains("LightStrength") && !item.name.Contains("LightHandleOn")).ToArray();
            FxAnimator.parameters = FxAnimator.parameters.Where(item => !item.name.Contains("LightIntensity") && !item.name.Contains("LightStrength") && !item.name.Contains("LightHandleOn")).ToArray();
            EditorUtility.SetDirty(FxAnimator);
        }
        catch(Exception e)
        {
            Debug.LogWarning(e);
        }


        AssetDatabase.SaveAssets();
    }

}
