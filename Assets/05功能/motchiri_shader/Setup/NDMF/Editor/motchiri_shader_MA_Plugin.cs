#region

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using UnityEngine.Animations;
using Unity.Mathematics;
using wataameya.motchiri_shader.ndmf.editor;
using System.Data;
using UnityEngine.UIElements;
using System;
using nadena.dev.modular_avatar.core;
using UnityEditor.Animations;

#if USE_NDMF
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
#endif

#endregion

#if USE_NDMF
[assembly: ExportsPlugin(typeof(motchiri_shader_MA_Plugin))]

namespace wataameya.motchiri_shader.ndmf.editor
{
    public class motchiri_shader_MA_Plugin : Plugin<motchiri_shader_MA_Plugin>
    {
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "wataameya.motchiri_shader.ndmf";

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName => "motchiri_shader_MA";
        
        private static readonly string _Prefab_GUID = "1bd39319890918a4698b6e5f09bdde01";
        private static readonly string _Prefab_test_GUID = "bef942693aa5c6f4eaf03629f8419af8";
        private static readonly string _FX_Default_GUID = "655a2842a0c46eb4f8eb7a28a2fd4c8d";
        private static readonly string _FX_Normal_GUID = "a1c97f9c6640cba448f0a73a188042c3";
        private static readonly string _FX_EX_GUID = "6e13ecd37035e014481f8042e4e13a84";
        private static readonly string _FX_Test_GUID = "a7ac400ca4935a94fb432fd85ecdd093";
        private static readonly string _FX_Scale_GUID = "b963ef1bd1869ff44904d0f048db29fe";

        protected override void Configure()
        {
            GameObject _motchiri_instance = null;
            motchiri_shader_MA m;

            _ = InPhase(BuildPhase.Generating).Run("genetate motchirishader", ctx =>
            {
                // もっちりシェーダーを取得、なければ終了
                m = ctx.AvatarRootObject.GetComponentInChildren<motchiri_shader_MA>();
                if (m == null) return;
                
                // テキストの配列の生成
                m._texts = new List<List<string>>();
                for (int i = 0; i < 4; i++) m._texts.Add(new List<string>());
                m._texts[0] = m._texts0;
                m._texts[1] = m._texts1;
                m._texts[2] = m._texts2;
                m._texts[3] = m._texts3;

                // アバターを設定
                m._avatar = ctx.AvatarRootObject;
                
                // もっちりオブジェクトがあれば、エラー
                if(m._avatar.transform.Find("motchiri")) ErrorMessage(m._texts[m._lang][60]);
                
                // プレハブを再生時、アップロード時で変更
                GameObject _motchiri_prefab = null;
                if (EditorApplication.isPlaying) _motchiri_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_Prefab_test_GUID));
                else _motchiri_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_Prefab_GUID));

                // プレハブが無ければ、エラー
                if(!_motchiri_prefab) ErrorMessage(m._texts[m._lang][61]);

                // プレハブをインスタンス化、アバター下へ
                _motchiri_instance = GameObject.Instantiate(_motchiri_prefab);
                _motchiri_instance.name = "motchiri_shader";
                // インスタンスのスケールはワールドスケールで1である必要がある
                _motchiri_instance.transform.localScale = Vector3.one;
                _motchiri_instance.transform.parent = m._avatar.transform;
            });

            InPhase(BuildPhase.Generating).Run("set motchirishader", ctx =>
            {
                // もっちりシェーダーを取得、なければ終了
                m = ctx.AvatarRootObject.GetComponentInChildren<motchiri_shader_MA>();
                if (m == null) return;

                // アニメーター取得
                Animator _animator = m._avatar.GetComponent<Animator>();
                if(!_animator) ErrorMessage(m._texts[m._lang][63]);

                // bool _is_unity_version_2019 = Application.unityVersion.StartsWith("2019");

                // Armature、Hipsを取得
                Transform _Armature_copy_transform = null;
                Transform _Hips_copy_transform = null;

                // アニメーターがヒューマノイドであれば、Armature、Hipsを取得
                if(_animator.isHuman)
                {
                    _Hips_copy_transform = _animator.GetBoneTransform(HumanBodyBones.Hips);
                    _Armature_copy_transform = _Hips_copy_transform.parent.gameObject.transform;
                }

                // インスタンスの位置、回転を初期化
                _motchiri_instance.transform.localPosition = new Vector3(0f,m._radius,0f);
                _motchiri_instance.transform.localRotation = quaternion.identity;

                // Armatureが存在すれば、大きさを変更
                // if(_Armature_copy_transform)
                // {
                //     Vector3 _Armature_scale = _Armature_copy_transform.transform.localScale;
                //     // Armatureのデフォルトスケールが異なる場合
                //     if(_Armature_scale.x>4) _Armature_scale = Vector3.one;
                //     _motchiri_instance.transform.localScale = new Vector3
                //     (
                //         _motchiri_instance.transform.localScale.x*_Armature_scale.x,
                //         _motchiri_instance.transform.localScale.y*_Armature_scale.y,
                //         _motchiri_instance.transform.localScale.z*_Armature_scale.z
                //     );
                // }
                
                // インスタンスをHips以下へ移動
                if(_Hips_copy_transform) _motchiri_instance.transform.parent = _Hips_copy_transform;

                // インスタンス設定
                VRCContactReceiver[] receivers = _motchiri_instance.GetComponentsInChildren<VRCContactReceiver>(true);
                foreach (VRCContactReceiver receiver in receivers)
                {
                    Debug.Log("Found VRCContactReceiver on: " + receiver.position);
                    receiver.position = (receiver.position / 0.05f) * m._unit;
                    receiver.radius = m._radius;
                    
                }

                // EXCollider設定
                if(m._useEXCollider)
                {
                    GameObject _EX_contact = _motchiri_instance.transform.Find("Receiver/EX").gameObject;
                    _EX_contact.tag = "Untagged";
                }
                
                //Contact設定
                // PositionConstraint motchiriPC = _motchiri_instance.GetComponent<PositionConstraint>();

                // ConstraintSource _Hips_copy_constraintsource = new ConstraintSource
                // {
                //     sourceTransform = _Hips_copy_transform,
                //     weight = 1f
                // };

                // motchiriPC.SetSource(0,_Hips_copy_constraintsource);
                
                // Debug.Log(shaderDict.liltoonToMotchiri.Count);

                // FXレイヤーの取得
                var _motchiri_FX_Default = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(_FX_Default_GUID));
                var _motchiri_FX_Normal = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(_FX_Normal_GUID));
                var _motchiri_FX_EX = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(_FX_EX_GUID));
                var _motchiri_FX_Test = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(_FX_Test_GUID));
                var _motchiri_FX_Scale = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(_FX_Scale_GUID));
                
                //シェーダー設定のインスタンス化
                var shaderDict = new motchiri_shader_MA_ShaderDictionary();

                // メッシュが一つも設定されていなければエラー
                if(m._meshRenderer[0]==null && m._meshRenderer[1]==null && m._meshRenderer[2]==null) ErrorMessage(m._texts[m._lang][64]);

                for (int i = 0; i < m._meshRenderer.Length; i++)
                {
                    // メッシュ取得
                    SkinnedMeshRenderer mesh = m._meshRenderer[i];

                    // メッシュが無ければループ続行
                    if(!mesh) continue;

                    // アニメーターのマージ用コンポーネントを設定
                    SetMergeAnimator(mesh,_motchiri_FX_Default,true);
                    SetMergeAnimator(mesh,_motchiri_FX_Normal,false);
                    if(m._useEXCollider) SetMergeAnimator(mesh,_motchiri_FX_EX,false);
                    if (EditorApplication.isPlaying) SetMergeAnimator(mesh,_motchiri_FX_Test,false);
                    SetMergeAnimator(mesh,_motchiri_FX_Scale,true);

                    // メッシュ設定用のコンポーネントを設定
                    UnityEngine.Object.DestroyImmediate(mesh.gameObject.GetComponent<ModularAvatarMeshSettings>());
                    var meshSetting = mesh.gameObject.AddComponent<ModularAvatarMeshSettings>();

                    // メッシュのルートボーンにインスタンスを設定
                    AvatarObjectReference avatarReference = new AvatarObjectReference();
                    avatarReference.Set(_motchiri_instance);
                    meshSetting.InheritBounds = ModularAvatarMeshSettings.InheritMode.Set;
                    meshSetting.RootBone = avatarReference;

                    // マスクにStreaming Mipmapが設定されていなければ、エラー
                    if(m._meshMask[i]&&!m._meshMask[i].streamingMipmaps) ErrorMessage(m._texts[m._lang][65]);

                    // マテリアル設定
                    for (int j = 0; j < mesh.sharedMaterials.Length; j++)
                    {
                        // 対応マテリアルを複製し、それ以外はそのまま
                        Material material = null;
                        Material[] materials_temp = new Material[mesh.sharedMaterials.Length];
                        // material = new Material(mesh.sharedMaterials[j]);
                        material = new Material(mesh.sharedMaterials[j]);
                        materials_temp = mesh.sharedMaterials;
                        materials_temp[j] = material;
                        mesh.SetMaterials(new List<Material>(materials_temp));

                        // if (EditorApplication.isPlaying)
                        // {
                        //     material = mesh.materials[j];
                        //     Debug.Log("isPlaying");
                        // }
                        // else
                        // {
                        //     material = new Material(mesh.sharedMaterial);
                        //     mesh.sharedMaterial = material;
                        //     Debug.Log("notPlaying");
                        // }

                        // マテリアルが存在し、対象のマテリアルの場合、シェーダーを変換
                        if(material&&j==m._meshMaterialSlot[i])
                        {
                            string shaderName = material.shader.name;

                            // シェーダーの対応ディクショナリを取得
                            Dictionary<string,string> tempShaderDict;
                            if(m._meshIsTessellation[i]) tempShaderDict = shaderDict.liltoonToMotchiriTessellation;
                            else tempShaderDict = shaderDict.liltoonToMotchiri;
                            
                            // lilToonの場合、対応するMotchiriShaderに変換
                            if(tempShaderDict.ContainsKey(material.shader.name))
                            {
                                material.shader = Shader.Find(tempShaderDict[shaderName]);
                            }
                            string currentShaderName = material.shader.name;
                            
                            // MotchiriShaderの場合、設定を変更
                            if(currentShaderName.Contains("motchiri"))
                            {
                                // if(j==m._meshMaterialSlot[i])
                                // {
                                    material.SetTexture("_Mask",m._meshMask[i]);
                                    material.SetFloat("_effect",m._effect);
                                    material.SetFloat("_strength",m._strength);
                                    material.SetFloat("_ao",m._ao);
                                    material.SetFloat("_blur",m._blur);
                                    material.SetColor("_color",m._color);
                                    // material.SetInt("_oldver",Convert.ToInt32(m._oldver));
                                    material.SetFloat("_depth",m._depth);
                                    material.SetFloat("_func1",m._func1);
                                    material.SetFloat("_func2",m._func2);
                                    material.SetFloat("_func3",m._func3);
                                    material.SetInt("_normal",Convert.ToInt32(m._normal));
                                    material.SetFloat("_nfunc",m._normalFunction);
                                    material.SetInt("_excollider",Convert.ToInt32(m._useEXCollider));
                                    material.SetFloat("_exfactor",m._exScalingFactor);
                                    material.SetFloat("_radius",m._radius);
                                    material.SetFloat("_unit",m._unit);
                                // }
                                // else
                                // {
                                //     material.SetFloat("_effect",0f);
                                //     material.SetFloat("_strength",0f);
                                //     material.SetInt("_cancel",1);
                                // }
                            }
                            // //blackbodyの場合、何もしない
                            // else if(currentShaderName.Contains("blackbody"))
                            // {
                            //     //何もしない
                            // }
                            // lilToonやMotchiriShaderではない場合、エラー
                            else ErrorMessage(m._texts[m._lang][63]);
                        }
                    }
                }
                // 設定用オブジェクトを削除
                UnityEngine.Object.DestroyImmediate(m.gameObject);
            });
        }

        private string GetFullPath(GameObject obj)
        {
            return GetFullPath(obj.transform);
        }

        private string GetFullPath(Transform t)
        {
            string path = t.name;
            var parent = t.parent;
            while (parent)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            return path;
        }

        private Type CheckType<Type>(Transform trans, string str)
        {
            Transform target = trans.Find(str);
            if(target&&target!=trans) return target.GetComponent<Type>();
            return default;
        }
        
        private void SetMergeAnimator(SkinnedMeshRenderer mesh, AnimatorController FX, bool writedefaults)
        {
            var MAMA =  mesh.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            MAMA.animator = FX;
            MAMA.matchAvatarWriteDefaults = writedefaults;
        }

        private void ErrorMessage(string str)
        {
            EditorUtility.DisplayDialog("Error", str + "\n(Playモードを終了し、設定し直して下さい)", "OK");
        }
    }
}
#endif