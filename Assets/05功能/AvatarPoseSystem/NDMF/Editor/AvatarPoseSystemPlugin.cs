#region

using System.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDK3.Dynamics.Constraint.Components;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using ZeroFactory.AvatarPoseSystem.NDMF.Editor;
using ZeroFactory.AvatarPoseSystem.NDMF.Interface;



#if USE_NDMF
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
#endif

#endregion

#if USE_NDMF
[assembly: ExportsPlugin(typeof(AvatarPoseSystemPlugin))]

namespace ZeroFactory.AvatarPoseSystem.NDMF.Editor
{
    public class AvatarPoseSystemPlugin : Plugin<AvatarPoseSystemPlugin>
    {
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "ZeroFactory.AvatarPoseSystem.NDMF";

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName => "AvatarPoseSystem";

        string Version = "4.2.1";
        List<Type> ApsTypes = new List<Type> { typeof(AvatarPoseSystem), typeof(IAvatarPoseSystemExtraBone), typeof(IAvatarPoseSystemAlterBody), typeof(IAvatarPoseSystemPropPlacer) };

        TraceInfoWritter tw = new TraceInfoWritter();

        Dictionary<float, List<HumanBodyBones>> dictSphereSize = new Dictionary<float, List<HumanBodyBones>>(){
            {0.1f, new List<HumanBodyBones>(){
                    HumanBodyBones.Head,
                    }},
            {0.05f, new List<HumanBodyBones>(){
                    HumanBodyBones.Hips,
                    HumanBodyBones.Chest,
                    }},
            {0.025f, new List<HumanBodyBones>(){
                    HumanBodyBones.LeftUpperLeg,
                    HumanBodyBones.RightUpperLeg,
                    HumanBodyBones.LeftLowerLeg,
                    HumanBodyBones.RightLowerLeg,
                    HumanBodyBones.LeftFoot,
                    HumanBodyBones.RightFoot,
                    HumanBodyBones.LeftLowerArm,
                    HumanBodyBones.RightLowerArm,
                    HumanBodyBones.LeftHand,
                    HumanBodyBones.RightHand,
                    HumanBodyBones.RightToes,
                    HumanBodyBones.LeftToes,
                    }},
            {0.005f, new List<HumanBodyBones>(){
                    HumanBodyBones.LeftThumbProximal,
                    HumanBodyBones.LeftThumbIntermediate,
                    HumanBodyBones.LeftThumbDistal,
                    HumanBodyBones.LeftIndexProximal,
                    HumanBodyBones.LeftIndexIntermediate,
                    HumanBodyBones.LeftIndexDistal,
                    HumanBodyBones.LeftMiddleProximal,
                    HumanBodyBones.LeftMiddleIntermediate,
                    HumanBodyBones.LeftMiddleDistal,
                    HumanBodyBones.LeftRingProximal,
                    HumanBodyBones.LeftRingIntermediate,
                    HumanBodyBones.LeftRingDistal,
                    HumanBodyBones.LeftLittleProximal,
                    HumanBodyBones.LeftLittleIntermediate,
                    HumanBodyBones.LeftLittleDistal,
                    HumanBodyBones.RightThumbProximal,
                    HumanBodyBones.RightThumbIntermediate,
                    HumanBodyBones.RightThumbDistal,
                    HumanBodyBones.RightIndexProximal,
                    HumanBodyBones.RightIndexIntermediate,
                    HumanBodyBones.RightIndexDistal,
                    HumanBodyBones.RightMiddleProximal,
                    HumanBodyBones.RightMiddleIntermediate,
                    HumanBodyBones.RightMiddleDistal,
                    HumanBodyBones.RightRingProximal,
                    HumanBodyBones.RightRingIntermediate,
                    HumanBodyBones.RightRingDistal,
                    HumanBodyBones.RightLittleProximal,
                    HumanBodyBones.RightLittleIntermediate,
                    HumanBodyBones.RightLittleDistal,
                    }},
        };

        Dictionary<BuildContext, APSBuildInfo> apsBuildCache = new Dictionary<BuildContext, APSBuildInfo>();

        List<EditorCurveSetter> restorationEditorCurveSetters = new List<EditorCurveSetter>();

        protected override void Configure()
        {

            InPhase(BuildPhase.Resolving)
            .BeforePlugin("nadena.dev.modular-avatar")
            .Run("AvatarPoseSystem", ctx =>
            {
                GameObject root = ctx.AvatarRootObject;
                AvatarPoseSystem[] aps = root.GetComponentsInChildren<AvatarPoseSystem>(true);
                if (aps.Length == 0) return;
                if (aps.Length > 1) throw new InvalidOperationException($"アバター「{root.name}」に複数の AvatarPoseSystem コンポーネントが存在します。1つのみ使用してください。");
                AvatarPoseSystem ap = aps[0];

                APSBuildInfo buildInfo = new APSBuildInfo() { apsTransform = ap.transform, enableTrace = ap.EnableTrace };
                apsBuildCache.Add(ctx, buildInfo);

                IAvatarPoseSystemExtraBone[] extraBones = root.GetComponentsInChildren<IAvatarPoseSystemExtraBone>(true);

                IAvatarPoseSystemAlterBody alterBody = root.GetComponentInChildren<IAvatarPoseSystemAlterBody>(true);

                IAvatarPoseSystemPropPlacer[] propPlacers = root.GetComponentsInChildren<IAvatarPoseSystemPropPlacer>(true);

                try
                {
                    if (buildInfo.enableTrace)
                    {
                        string zeroFactoryLogsPath = Path.Combine(Application.dataPath, "ZeroFactory", "AvatarPoseSystem", "__trace");
                        string twPath = Path.Combine(zeroFactoryLogsPath, $"AvatarPoseSystem_Trace_{ctx.AvatarRootObject.name}.log");
                        tw.Open(twPath, root.transform);
                        tw.Write($"AvatarPoseSystem Trace Info: v{Version} -Resolving");
                        tw.WriteSeparator();
                        tw.Write("+ Avatar Hierarchy\n");
                        tw.WriteHierarchy(root.transform);
                        tw.WriteEmptyLine();
                        tw.WriteSeparator();
                        tw.Write($"+ AvatarPoseSystem\n");
                        tw.WriteProperties(ap);
                        tw.WriteEmptyLine();
                        tw.Write($"+ ExtraBones ({extraBones.Length} exists)\n");
                        extraBones.ToList().ForEach(extraBone =>
                        {
                            tw.WriteProperties((Component)extraBone);
                            tw.WriteNamedValue("AffectedTransforms", extraBone.GetAffectedTransforms());
                            tw.WriteEmptyLine();
                        });
                        tw.Write($"+ PropPlacer ({propPlacers.Length} exists)\n");
                        propPlacers.ToList().ForEach(propPlacer =>
                        {
                            tw.WriteProperties((Component)propPlacer);
                            tw.WriteNamedValue("AffectedTransform", propPlacer.GetAffectedTransform());
                            tw.WriteEmptyLine();
                        });
                        tw.Write($"+ AlterBody {(alterBody != null ? "is" : "is not")} exists)\n");
                        if (alterBody != null)
                        {
                            tw.WriteProperties(alterBody);
                            tw.WriteSeparator();
                            tw.Write("+ AlterBody Avatar Hierarchy\n");
                            tw.WriteHierarchy(alterBody.alterBodyAvatar?.transform);
                        }
                    }

                    VRCAvatarDescriptor descripter = root.GetComponent<VRCAvatarDescriptor>();
                    Vector3 viewPos = descripter.ViewPosition;

                    bool hasAlterBody = alterBody != null && alterBody.alterBodyAvatar != null;
                    if (hasAlterBody)
                    {
                        buildInfo.alterBodyTransform = alterBody.transform;

                        GameObject alterBodyInstance = UnityEngine.Object.Instantiate(alterBody.alterBodyAvatar);

                        Animator animator = alterBodyInstance.GetComponent<Animator>();
                        VRCAvatarDescriptor alterBodyDescripter = alterBodyInstance.GetComponent<VRCAvatarDescriptor>();
                        Vector3 alterBodyViewPos = alterBodyDescripter.ViewPosition;
                        RuntimeAnimatorController fxController = alterBodyDescripter.baseAnimationLayers
                                                .Where(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null)
                                                .Select(item => item.animatorController).FirstOrDefault();
                        if (fxController != null)
                        {
                            ModularAvatarMergeAnimator maMA = alterBodyInstance.AddComponent<ModularAvatarMergeAnimator>();
                            maMA.animator = fxController;
                        }

                        Dictionary<ModularAvatarBoneProxy, Transform> boneProxyTargetDict = new Dictionary<ModularAvatarBoneProxy, Transform>();
                        alterBodyInstance.transform.GetComponentsInChildren<ModularAvatarBoneProxy>(true)
                            .ToList()
                            .ForEach(bp => boneProxyTargetDict.Add(bp, bp.target));

                        for (HumanBodyBones bodyBone = HumanBodyBones.Hips; bodyBone < HumanBodyBones.LastBone; bodyBone++)
                        {
                            buildInfo.alterBodyBoneDict.Add(bodyBone, animator.GetBoneTransform(bodyBone));
                        }
                        Transform alterBodyArmature = animator.GetBoneTransform(HumanBodyBones.Hips).parent;
                        alterBodyArmature.name += ".1";

                        // AnimatorとTransform、ModularAvatarMergeAnimator以外のすべてのコンポーネントを削除
                        alterBodyInstance.GetComponents<Component>()
                            .Where(comp => !(comp is Transform) && !(comp is ModularAvatarMergeAnimator))
                            .ToList()
                            .ForEach(comp => UnityEngine.Object.DestroyImmediate(comp));

                        //除外対象を削除する
                        alterBody.ignoreObjects.Where(obj => obj != null).ToList().ForEach(obj =>
                        {
                            string path = AnimationUtility.CalculateTransformPath(obj.transform, alterBody.alterBodyAvatar.transform);
                            Transform objectToDestroy = alterBodyInstance.transform.Find(path);
                            if (objectToDestroy != null)
                            {
                                UnityEngine.Object.DestroyImmediate(objectToDestroy.gameObject);
                            }
                        });

                        //分身体で競合するコンポーネントを削除する
                        alterBodyInstance.GetComponentsInChildren<ModularAvatarMergeAnimator>()
                            .Where(comp => comp.layerType != VRCAvatarDescriptor.AnimLayerType.FX)
                            .ToList()
                            .ForEach(comp => UnityEngine.Object.DestroyImmediate(comp));

                        alterBodyInstance.GetComponentsInChildren<ModularAvatarMeshSettings>()
                            .Where(comp =>
                            {
                                return ((comp.InheritBounds != ModularAvatarMeshSettings.InheritMode.Set || comp.InheritBounds != ModularAvatarMeshSettings.InheritMode.SetOrInherit)
                                         && comp.RootBone.Get(comp) == null)
                                    || ((comp.InheritProbeAnchor != ModularAvatarMeshSettings.InheritMode.Set || comp.InheritProbeAnchor != ModularAvatarMeshSettings.InheritMode.SetOrInherit)
                                         && comp.ProbeAnchor.Get(comp) == null);
                            })
                            .ToList()
                            .ForEach(comp => UnityEngine.Object.DestroyImmediate(comp));

                        alterBodyInstance.GetComponentsInChildren<Component>(true)
                            .Where(c => ApsTypes.Any(t => t.IsAssignableFrom(c.GetType())))
                            .ToList().ForEach(c =>
                            {
                                //ExtraBoneはコンポーネントだけ削除、それ以外はGameObjectごと削除
                                if (c is IAvatarPoseSystemExtraBone || c is IAvatarPoseSystemPropPlacer)
                                {
                                    UnityEngine.Object.DestroyImmediate(c);
                                }
                                else
                                {
                                    UnityEngine.Object.DestroyImmediate(c.gameObject);
                                }
                            });

                        buildInfo.alterBodyAvatarContainer = buildInfo.alterBodyTransform.Find("WorldFix/AvatarContainer");
                        buildInfo.alterBodyAvatarContainer.gameObject.SetActive(true);
                        alterBodyInstance.SetActive(true);
                        alterBodyInstance.transform.parent = buildInfo.alterBodyAvatarContainer;
                        //オリジナルとViewPointを合わせる
                        buildInfo.alterBodyAvatarContainer.position = root.transform.position - (alterBodyViewPos - viewPos);

                        buildInfo.alterBodyArmatureContainer = buildInfo.alterBodyTransform.Find("WorldFix/ArmatureContainer");
                        ModularAvatarBoneProxy maBP = alterBodyArmature.gameObject.AddComponent<ModularAvatarBoneProxy>();
                        maBP.attachmentMode = BoneProxyAttachmentMode.AsChildKeepWorldPose;
                        maBP.target = buildInfo.alterBodyArmatureContainer;

                        //親の設定後にModularAvatarBoneProxyのターゲットを置き換える
                        foreach (var kvp in boneProxyTargetDict)
                        {
                            if (kvp.Key != null && kvp.Value != null)
                            {
                                kvp.Key.target = kvp.Value;
                            }
                        }

                    }

                    tw.WriteSeparator();
                    tw.Write("AvatarPoseSystem Trace Info End -Resolving");
                    tw.Close();
                }
                catch (Exception ex)
                {
                    tw.WriteSeparator();
                    tw.Write(ex.ToString());
                    tw.WriteSeparator();
                    tw.Write("AvatarPoseSystem Trace Info Abend -Resolving");
                    tw.Close();
                    throw new InvalidOperationException("ResolvingフェーズでAvatarPoseSystemの処理に失敗しました。詳細はトレースログを確認してください。", ex);
                }
            });

            InPhase(BuildPhase.Generating)
            .BeforePlugin("nadena.dev.modular-avatar")
            .AfterPlugin("com.hhotatea.avatar_pose_library.editor.DataConvertPlugin") //APLで生成したパラメータ名を参照したいため
            .Run("AvatarPoseSystem", ctx =>
            {
                GameObject root = ctx.AvatarRootObject;
                AvatarPoseSystem[] aps = root.GetComponentsInChildren<AvatarPoseSystem>(true);
                if (aps.Length == 0) return;
                if (aps.Length > 1) throw new InvalidOperationException($"アバター「{root.name}」に複数の AvatarPoseSystem コンポーネントが存在します。1つのみ使用してください。");
                AvatarPoseSystem ap = aps[0];

                APSBuildInfo buildInfo = apsBuildCache[ctx];

                VRCAvatarDescriptor descripter = root.GetComponent<VRCAvatarDescriptor>();
                Animator animator = root.GetComponent<Animator>();

                IAvatarPoseSystemExtraBone[] extraBones = root.GetComponentsInChildren<IAvatarPoseSystemExtraBone>(true);
                bool hasExBone = extraBones.Length > 0;

                IAvatarPoseSystemAlterBody alterBody = root.GetComponentInChildren<IAvatarPoseSystemAlterBody>(true);
                bool hasAlterBody = alterBody != null && buildInfo.alterBodyBoneDict.Count > 0;

                IAvatarPoseSystemPropPlacer[] propPlacers = root.GetComponentsInChildren<IAvatarPoseSystemPropPlacer>(true);
                bool hasPropPlacer = propPlacers.Length > 0;

                try
                {

                    if (buildInfo.enableTrace)
                    {
                        string zeroFactoryLogsPath = Path.Combine(Application.dataPath, "ZeroFactory", "AvatarPoseSystem", "__trace");
                        string twPath = Path.Combine(zeroFactoryLogsPath, $"AvatarPoseSystem_Trace_{ctx.AvatarRootObject.name}.log");
                        tw.Open(twPath, root.transform, true);
                        tw.Write($"AvatarPoseSystem Trace Info: v{Version} -Generating");
                        tw.WriteSeparator();
                        tw.Write("+ Avatar Hierarchy\n");
                        tw.WriteHierarchy(root.transform);
                        tw.WriteEmptyLine();
                        tw.WriteSeparator();
                    }

                    Transform apsMenu = ap.transform.Find("__Menu/AvatarPoseSystem");

                    List<GameObject> unfixObjectsFromPaths = new List<GameObject>();
                    foreach (string path in ap.UnfixObjectPaths)
                    {
                        GameObject objUnfixObject = root.transform.Find(path)?.gameObject;
                        //unfixObjectsFromPaths.Add(objUnfixObject);
                        if (objUnfixObject) ap.UnfixObjects.Add(objUnfixObject);
                    }

                    IndexedTransformManager bodyMan = new IndexedTransformManager()
                    {
                        rootTransform = ap.transform.Find("Ghost_Proxies/APS_Ghost_Hips_Proxy/APS_Body"),
                        bonePathFormat = "Armature/BodyBone.{0:000}",
                        meshPath = "BodyModel",
                    };
                    IndexedTransformManager bodyHandleMan = new IndexedTransformManager()
                    {
                        rootTransform = ap.transform.Find("Body_Proxies/APS_Body_Hips_Proxy/APS_Handle"),
                        bonePathFormat = "Armature/Bone_Root.{0:000}",
                        meshPath = "HandleMesh",
                    };
                    IndexedTransformManager exHandleMan = new IndexedTransformManager()
                    {
                        rootTransform = ap.transform.Find("Body_Proxies/APS_Body_Hips_Proxy/APS_HandleEx"),
                        bonePathFormat = "Armature/Bone_Root.{0:000}",
                        meshPath = "HandleMesh",
                    };
                    if (!hasExBone && !hasPropPlacer)
                    {
                        UnityEngine.Object.DestroyImmediate(exHandleMan.rootTransform.gameObject);
                    }

                    GameObject objPropHandleOrg = ap.transform.Find("Body_Proxies/APS_Body_Hips_Proxy/APS_PropHandle").gameObject;

                    IndexedTransformManager alterBodyHandleMan = new IndexedTransformManager();
                    GameObject objAlterHipsHandle = null;
                    if (hasAlterBody)
                    {
                        Transform handleRoot = alterBody.transform.Find("Body_Proxies/APSAB_Body_Hips_Proxy/APS_Handle");
                        objAlterHipsHandle = alterBody.transform.Find("Body_Proxies/APSAB_Body_Hips_Proxy/APS_HipsHandle").gameObject;
                        if (alterBody.createHandle)
                        {
                            alterBodyHandleMan.rootTransform = handleRoot;
                            alterBodyHandleMan.bonePathFormat = "Armature/Bone_Root.{0:000}";
                            alterBodyHandleMan.meshPath = "HandleMesh";
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(handleRoot.gameObject);
                            UnityEngine.Object.DestroyImmediate(objAlterHipsHandle);
                        }
                    }

                    GameObject objHipsHandle = ap.transform.Find("Body_Proxies/APS_Body_Hips_Proxy/APS_HipsHandle").gameObject;
                    GameObject objHandHandleL = ap.transform.Find("Body_Proxies/APS_Body_LowerArmL_Proxy/APS_HandHandle_L").gameObject;
                    GameObject objHandHandleR = ap.transform.Find("Body_Proxies/APS_Body_LowerArmR_Proxy/APS_HandHandle_R").gameObject;

                    GameObject objFixRoot = ap.transform.Find("WorldFix/FixRoot").gameObject;

                    GameObject objHeadOrg = animator.GetBoneTransform(HumanBodyBones.Head).gameObject;
                    GameObject objHipsOrg = animator.GetBoneTransform(HumanBodyBones.Hips).gameObject;
                    GameObject objArmature = objHipsOrg.transform.parent.gameObject;

                    //目が頭の外にある場合は頭に入れる（キメラとか）
                    new List<Transform> { descripter.customEyeLookSettings.leftEye, descripter.customEyeLookSettings.rightEye }
                    .ForEach(eyeTransform =>
                    {
                        //if (eyeTransform == null || Util.IsSelfOrAncestor(eyeTransform, objHeadOrg.transform)) return;
                        if (eyeTransform == null || eyeTransform.IsChildOf(objHeadOrg.transform)) return;

                        Transform orgEyeTransform = objHeadOrg.transform.Find(eyeTransform.name);
                        if (orgEyeTransform != null)
                        {
                            orgEyeTransform.name = "__" + orgEyeTransform.name;
                        }
                        eyeTransform.parent = objHeadOrg.transform;
                    });

                    //なぜかArmatureより上にあるMABoneProxyが動かなくなるため、Armatureを一番上に持っていく
                    objArmature.transform.SetSiblingIndex(0);

                    GameObject objHipsClone = CloneTransform(objHipsOrg, objHipsOrg.name + ".Clone");
                    objHipsClone.transform.parent = objArmature.transform;

                    GameObject objHipsFix = CloneTransform(objHipsOrg, objHipsOrg.name);
                    objHipsFix.transform.parent = objFixRoot.transform;

                    RootBoneSetGroup mainBodyRootBoneGroup = new RootBoneSetGroup();
                    mainBodyRootBoneGroup.armatureRootBoneSet = new BoneSet() { original = objHipsOrg.transform, clone = objHipsClone.transform, fix = objHipsFix.transform };

                    RootBoneSetGroup alterBodyRootBoneGroup = new RootBoneSetGroup();
                    if (hasAlterBody && alterBody.createHandle)
                    {

                        GameObject objAlterHipsOrg = buildInfo.alterBodyBoneDict[HumanBodyBones.Hips].gameObject;
                        GameObject objAlterArmature = objAlterHipsOrg.transform.parent.gameObject;

                        //AlterBodyにはcloneは要らない
                        //GameObject objAlterHipsClone = CloneTransform(objAlterHipsOrg, objHipsOrg.name + ".Clone");
                        //objAlterHipsClone.transform.parent = objAlterArmature.transform;

                        GameObject objAlterHipsFix = CloneTransform(objAlterHipsOrg, objHipsOrg.name);
                        objAlterHipsFix.transform.parent = objFixRoot.transform;
                        alterBodyRootBoneGroup.armatureRootBoneSet = new BoneSet() { original = objAlterHipsOrg.transform, clone = null, fix = objAlterHipsFix.transform };

                    }

                    if (hasExBone || hasPropPlacer)
                    {
                        tw.WriteSeparator();
                        tw.Write("+ Setup BoneProxyRootBoneSets\n");

                        foreach (ModularAvatarBoneProxy maBP in root.GetComponentsInChildren<ModularAvatarBoneProxy>(true))
                        {
                            tw.WriteProperties(maBP);

                            if (buildInfo.isAPSParts(maBP.transform))
                            {
                                tw.Write($"    ->ignored (is APS Parts)\n");
                                continue;
                            }

                            if (IsUnfixObject(maBP.gameObject, ap))
                            {
                                tw.Write($"    ->ignored (is Unfix Object)\n");
                                continue;
                            }

                            if (maBP.target == null)
                            {
                                tw.Write($"    ->ignored (target is null)\n");
                                continue;
                            }
                            GameObject objOrg = maBP.gameObject;

                            BoneSet boneSet = mainBodyRootBoneGroup.GetBoneSet(maBP.target);
                            if (boneSet == null)
                            {
                                tw.Write($"    ->ignored (can't clone BoneProxy object)\n");
                                continue;
                            }

                            GameObject objCloneParent = new GameObject();
                            objCloneParent.name = objOrg.name + ".Clone_Parent";
                            objCloneParent.transform.parent = objOrg.transform.parent;

                            GameObject objClone = CloneTransform(objOrg);
                            objClone.transform.parent = objCloneParent.transform;
                            ModularAvatarBoneProxy maBPClone = CopyComponent(objClone, maBP);
                            maBPClone.target = boneSet.clone;

                            GameObject objFixParent = new GameObject();
                            objFixParent.name = objOrg.name + ".Fix_Parent";
                            objFixParent.transform.parent = objOrg.transform.parent;

                            GameObject objFix = CloneTransform(objOrg);
                            objFix.transform.parent = objFixParent.transform;
                            ModularAvatarBoneProxy maBPFix = CopyComponent(objFix, maBP);
                            maBPFix.target = boneSet.fix;

                            mainBodyRootBoneGroup.boneProxyRootBoneSets.Add(objOrg.transform,
                                new BoneSet() { original = objOrg.transform, clone = objClone.transform, fix = objFix.transform });

                            tw.Write($"    ->done\n");
                        }

                        tw.WriteSeparator();
                        tw.Write("+ Setup MergeArmatureRootBoneSets\n");

                        foreach (ModularAvatarMergeArmature maMA in root.GetComponentsInChildren<ModularAvatarMergeArmature>(true))
                        {
                            tw.WriteProperties(maMA);
                            tw.WriteEmptyLine();

                            if (maMA.mergeTarget == null) continue;

                            Transform tfMergeHipsOrg = maMA.transform.Find(maMA.prefix + objHipsOrg.name + maMA.suffix);
                            tw.WriteNamedValue($" tfMergeHipsOrg", tfMergeHipsOrg);
                            if (tfMergeHipsOrg == null)
                            {
                                tw.Write($"    ->ignored (target is not armature)\n");
                                continue;
                            }

                            foreach (Transform tr in FindMergeArmatureDifferenceTransforms(maMA.transform, maMA.mergeTargetObject.transform, maMA.prefix, maMA.suffix))
                            {

                                tw.WriteNamedValue($" DifferenceTransform", tr);

                                Transform tfOrgParent = tr.parent;
                                tw.WriteNamedValue($" tfOrgParent", tfOrgParent);
                                //Hipsの子孫にないもの(Armature直下など)は無視する
                                if (!tfOrgParent.IsChildOf(tfMergeHipsOrg))
                                {
                                    tw.Write($"    ->ignored (is not self or ancestor hips)\n");
                                    continue;
                                }

                                string relPath = AnimationUtility.CalculateTransformPath(tfOrgParent, tfMergeHipsOrg);
                                relPath = RemovePrefixAndSuffix(relPath, maMA.prefix, maMA.suffix);
                                tw.WriteNamedValue($" relPath", relPath);
                                Transform tfCloneParent = objHipsClone.transform.Find(relPath);
                                Transform tfFixParent = objHipsFix.transform.Find(relPath);

                                GameObject objOrg = tr.gameObject;

                                GameObject objClone = CloneTransform(objOrg);
                                objClone.transform.parent = tfCloneParent;

                                GameObject objFix = CloneTransform(objOrg);
                                objFix.transform.parent = tfFixParent;

                                mainBodyRootBoneGroup.mergeArmatureRootBoneSets.Add(objOrg.transform,
                                    new BoneSet() { original = objOrg.transform, clone = objClone.transform, fix = objFix.transform });

                                tw.Write($"    ->done\n");
                            }
                        }
                    }

                    // VVVVVV ルルネAFK対応 VVVVVV
                    // HipsにScaleConstraintが使われている。APSで構造が変わった後にScaleできるようにScale対象を変える
                    ScaleConstraint sc = objHipsOrg.GetComponent<ScaleConstraint>();
                    if (sc != null)
                    {
                        CopyComponent(objHipsClone, sc);
                        UnityEngine.Object.DestroyImmediate(sc);
                    }
                    // AAAAAA ルルネAFK対応 AAAAAA

                    //持つときのソース
                    List<Transform> ghostProxies = new List<Transform>();
                    ghostProxies.Add(ap.transform.Find("Ghost_Proxies"));
                    if (hasAlterBody)
                    {
                        ghostProxies.Add(alterBody.transform.Find("Ghost_Proxies"));
                    }
                    foreach (Transform ghostProxy in ghostProxies)
                    {
                        foreach (ModularAvatarBoneProxy c in ghostProxy.GetComponentsInChildren<ModularAvatarBoneProxy>(true))
                        {
                            c.target = mainBodyRootBoneGroup.GetBoneSet(c.target).clone;
                        }
                    }

                    GameObject objAnimAbs = ap.transform.Find("__Anim").gameObject;
                    ModularAvatarMergeAnimator maMergeAnimator = objAnimAbs.GetComponent<ModularAvatarMergeAnimator>();
                    AnimatorController fx = new AnimatorController() { name = "APS_FX_A" };
                    fx.AddParameter("APS_FixBody", AnimatorControllerParameterType.Bool);
                    fx.AddParameter("APS_FixPB", AnimatorControllerParameterType.Bool);
                    fx.AddParameter("APS_HandHandle_L_Stretch", AnimatorControllerParameterType.Float);
                    fx.AddParameter("APS_HandHandle_R_Stretch", AnimatorControllerParameterType.Float);
                    fx.AddParameter("APS_HideBody", AnimatorControllerParameterType.Bool);
                    maMergeAnimator.animator = fx;

                    //-----Fix Body State
                    AnimationClip anim_FixBody_Unfix = new AnimationClip() { name = "APS_FixBody_Unfix" };
                    AnimationClip anim_FixBody_PrepareFix = new AnimationClip() { name = "APS_FixBody_PrepareFix" };
                    AnimationClip anim_FixBody_Fix = new AnimationClip() { name = "APS_FixBody_Fix" };
                    AnimationClip anim_FixBody_Fix_Lock = new AnimationClip() { name = "APS_FixBody_Fix_Lock" };

                    AnimatorControllerLayer layerBody = AddAnimatorControllerLayer(fx, "APS_FixBody");
                    AnimatorState state_FixBody_Unfix = AddMotionAnimationState(layerBody.stateMachine, "Off", new Vector2(400, 100), anim_FixBody_Unfix);
                    AnimatorState state_FixBody_Prefix = AddMotionAnimationState(layerBody.stateMachine, "Prepare", new Vector2(600, 125), anim_FixBody_PrepareFix);
                    AnimatorState state_FixBody_Prefix2 = AddMotionAnimationState(layerBody.stateMachine, "Prepare2", new Vector2(600, 175), anim_FixBody_PrepareFix);
                    AnimatorState state_FixBody_Fix = AddMotionAnimationState(layerBody.stateMachine, "On", new Vector2(400, 200), anim_FixBody_Fix);
                    AnimatorState state_FixBody_Fix_Lock = AddMotionAnimationState(layerBody.stateMachine, "On_Lock", new Vector2(100, 200), anim_FixBody_Fix_Lock);

                    AddTransition(state_FixBody_Unfix, state_FixBody_Prefix, (AnimatorConditionMode.If, 1, "APS_FixBody"));
                    AddTransition(state_FixBody_Prefix, state_FixBody_Prefix2, (AnimatorConditionMode.If, 1, "APS_FixBody"));
                    AddTransition(state_FixBody_Prefix2, state_FixBody_Fix, (AnimatorConditionMode.If, 1, "APS_FixBody"));
                    AddTransition(state_FixBody_Fix, state_FixBody_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));

                    AddTransition(state_FixBody_Fix, state_FixBody_Fix_Lock, (AnimatorConditionMode.If, 1, "APS_LockHandle"));
                    AddTransition(state_FixBody_Fix_Lock, state_FixBody_Fix, (AnimatorConditionMode.IfNot, 1, "APS_LockHandle"));
                    AddTransition(state_FixBody_Fix_Lock, state_FixBody_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));


                    ParamNameResolver pNameResolver = new ParamNameResolver(descripter);
                    if (ap.SetParamatersOnFix.Count > 0)
                    {
                        VRCAvatarParameterDriver paramDriver = state_FixBody_Fix.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                        paramDriver.localOnly = true;
                        foreach (AvatarPoseSystem.ParamaterSetting pSetting in pNameResolver.Resolve(ap.SetParamatersOnFix))
                        {
                            paramDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter() { type = VRC_AvatarParameterDriver.ChangeType.Set, name = pSetting.paramaterName, value = pSetting.value });
                        }
                    }
                    if (ap.SetParamatersOnUnfix.Count > 0)
                    {
                        VRCAvatarParameterDriver paramDriver = state_FixBody_Unfix.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                        paramDriver.localOnly = true;
                        foreach (AvatarPoseSystem.ParamaterSetting pSetting in pNameResolver.Resolve(ap.SetParamatersOnUnfix))
                        {
                            paramDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter() { type = VRC_AvatarParameterDriver.ChangeType.Set, name = pSetting.paramaterName, value = pSetting.value });
                        }
                    }
                    //-----

                    //-----Fix PB State
                    AnimationClip anim_FixPB_Unfix = new AnimationClip() { name = "APS_FixPB_Unfix" };
                    AnimationClip anim_FixPB_Fix = new AnimationClip() { name = "APS_FixPB_Fix" };

                    AnimatorControllerLayer layerPB = AddAnimatorControllerLayer(fx, "APS_FixPB");
                    AnimatorState state_FixPB_Unfix = AddMotionAnimationState(layerPB.stateMachine, "Off", new Vector2(200, 100), anim_FixPB_Unfix);
                    AnimatorState state_FixPB_Fix = AddMotionAnimationState(layerPB.stateMachine, "On", new Vector2(200, 200), anim_FixPB_Fix);

                    AddTransition(state_FixPB_Unfix, state_FixPB_Fix, (AnimatorConditionMode.If, 1, "APS_FixPB"));
                    AddTransition(state_FixPB_Fix, state_FixPB_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixPB"));
                    //-----


                    //-----Hand Handle L State
                    AnimationClip anim_HandHandleL_Unfix = new AnimationClip() { name = "APS_HandHandleL_Unfix" };
                    AnimationClip anim_HandHandleL_Fix = new AnimationClip() { name = "APS_HandHandleL_Fix" };

                    AnimatorControllerLayer layerHandHandleL = AddAnimatorControllerLayer(fx, "APS_HandHandle_L");
                    AnimatorState state_HandHandleL_Unfix = AddMotionAnimationState(layerHandHandleL.stateMachine, "Unfix", new Vector2(200, 100), anim_HandHandleL_Unfix);
                    AnimatorState state_HandHandleL_Fix = AddMotionAnimationState(layerHandHandleL.stateMachine, "All", new Vector2(200, 200), anim_HandHandleL_Fix);
                    state_HandHandleL_Fix.timeParameterActive = true;
                    state_HandHandleL_Fix.timeParameter = "APS_HandHandle_L_Stretch";

                    AddTransition(state_HandHandleL_Unfix, state_HandHandleL_Fix, (AnimatorConditionMode.If, 1, "APS_FixBody"));
                    AddTransition(state_HandHandleL_Fix, state_HandHandleL_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));
                    //-----


                    //-----Hand Handle R State
                    AnimationClip anim_HandHandleR_Unfix = new AnimationClip() { name = "APS_HandHandleR_Unfix" };
                    AnimationClip anim_HandHandleR_Fix = new AnimationClip() { name = "APS_HandHandleR_Fix" };

                    AnimatorControllerLayer layerHandHandleR = AddAnimatorControllerLayer(fx, "APS_HandHandle_R");
                    AnimatorState state_HandHandleR_Unfix = AddMotionAnimationState(layerHandHandleR.stateMachine, "Unfix", new Vector2(200, 100), anim_HandHandleR_Unfix);
                    AnimatorState state_HandHandleR_Fix = AddMotionAnimationState(layerHandHandleR.stateMachine, "All", new Vector2(200, 200), anim_HandHandleR_Fix);
                    state_HandHandleR_Fix.timeParameterActive = true;
                    state_HandHandleR_Fix.timeParameter = "APS_HandHandle_R_Stretch";

                    AddTransition(state_HandHandleR_Unfix, state_HandHandleR_Fix, (AnimatorConditionMode.If, 1, "APS_FixBody"));
                    AddTransition(state_HandHandleR_Fix, state_HandHandleR_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));
                    //-----

                    FixAnimationSet fixMainBodyAnimSet = new FixAnimationSet(anim_FixBody_Unfix, anim_FixBody_Fix, anim_FixBody_Fix_Lock, anim_FixBody_PrepareFix);
                    FixAnimationSet fixMainPBAnimSet = new FixAnimationSet(anim_FixPB_Unfix, anim_FixPB_Fix);

                    FixAnimationSet fixAlterBodyAnimSet = null;
                    FixAnimationSet fixAlterPBAnimSet = null;
                    ShowAnimationSet showAlterBodyAnimSet = null;

                    if (hasAlterBody)
                    {
                        //メニューをAPSのメニューに結合する
                        Transform menuInstallTarget = alterBody.transform.Find("__MenuInstallTarget");
                        menuInstallTarget.parent = apsMenu;

                        fx.AddParameter("APS_FixAlterBody", AnimatorControllerParameterType.Bool);

                        //-----Show AlterBody State
                        AnimationClip anim_ShowAlterBody_Hide = new AnimationClip() { name = "APS_ShowAlterBody_Hide" };
                        AnimationClip anim_ShowAlterBody_Show = new AnimationClip() { name = "APS_ShowAlterBody_Unfix" };

                        AnimatorControllerLayer layerShowAlterBody = AddAnimatorControllerLayer(fx, "APS_ShowAlterBody");
                        AnimatorState state_ShowAlterBody_Hide = AddMotionAnimationState(layerShowAlterBody.stateMachine, "Hide", new Vector2(200, 100), anim_ShowAlterBody_Hide);
                        AnimatorState state_ShowAlterBody_Show = AddMotionAnimationState(layerShowAlterBody.stateMachine, "Show", new Vector2(200, 200), anim_ShowAlterBody_Show);

                        AddTransition(state_ShowAlterBody_Hide, state_ShowAlterBody_Show, (AnimatorConditionMode.If, 1, "APS_FixAlterBody"));
                        AddTransition(state_ShowAlterBody_Hide, state_ShowAlterBody_Show, (AnimatorConditionMode.If, 1, "APS_FixBody"));
                        AddTransition(state_ShowAlterBody_Show, state_ShowAlterBody_Hide, (AnimatorConditionMode.IfNot, 1, "APS_FixAlterBody"), (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));

                        showAlterBodyAnimSet = new ShowAnimationSet(anim_ShowAlterBody_Hide, anim_ShowAlterBody_Show);

                        string avatarPath = AnimationUtility.CalculateTransformPath(buildInfo.alterBodyAvatarContainer, animator.transform);
                        showAlterBodyAnimSet.AddInstantaneousCurve(avatarPath, typeof(GameObject), "m_IsActive", 0f, 1f);
                        string armaturePath = AnimationUtility.CalculateTransformPath(buildInfo.alterBodyArmatureContainer, animator.transform);
                        showAlterBodyAnimSet.AddInstantaneousCurve(armaturePath, typeof(Transform), "m_LocalScale.x", 0f, 1f);
                        showAlterBodyAnimSet.AddInstantaneousCurve(armaturePath, typeof(Transform), "m_LocalScale.y", 0f, 1f);
                        showAlterBodyAnimSet.AddInstantaneousCurve(armaturePath, typeof(Transform), "m_LocalScale.z", 0f, 1f);
                        //-----Show AlterBody State


                        //-----Fix AlterBody State
                        AnimationClip anim_FixAlterBody_Unfix = new AnimationClip() { name = "APS_FixAlterBody_Unfix" };
                        AnimationClip anim_FixAlterBody_Fix = new AnimationClip() { name = "APS_FixAlterBody_Fix" };
                        AnimationClip anim_FixAlterBody_Lock = new AnimationClip() { name = "APS_FixAlterBody_Lock" };

                        AnimatorControllerLayer layerFixAlterBody = AddAnimatorControllerLayer(fx, "APS_FixAlterBody");
                        AnimatorState state_FixAlterBody_Unfix = AddMotionAnimationState(layerFixAlterBody.stateMachine, "Off", new Vector2(400, 100), anim_FixAlterBody_Unfix);
                        AnimatorState state_FixAlterBody_Fix = AddMotionAnimationState(layerFixAlterBody.stateMachine, "On", new Vector2(400, 200), anim_FixAlterBody_Fix);
                        AnimatorState state_FixAlterBody_Lock = AddMotionAnimationState(layerFixAlterBody.stateMachine, "On_Lock", new Vector2(100, 200), anim_FixAlterBody_Lock);

                        AddTransition(state_FixAlterBody_Unfix, state_FixAlterBody_Fix, (AnimatorConditionMode.If, 1, "APS_FixAlterBody"));
                        AddTransition(state_FixAlterBody_Fix, state_FixAlterBody_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixAlterBody"));

                        AddTransition(state_FixAlterBody_Fix, state_FixAlterBody_Lock, (AnimatorConditionMode.If, 1, "APS_LockHandle"));
                        AddTransition(state_FixAlterBody_Lock, state_FixAlterBody_Fix, (AnimatorConditionMode.IfNot, 1, "APS_LockHandle"));
                        AddTransition(state_FixAlterBody_Lock, state_FixAlterBody_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixAlterBody"));

                        fixAlterBodyAnimSet = new FixAnimationSet(anim_FixAlterBody_Unfix, anim_FixAlterBody_Fix, anim_FixAlterBody_Lock);
                        //-----Fix AlterBody State


                        //-----Fix AlterBody PB State
                        AnimationClip anim_FixAlterPB_Unfix = new AnimationClip() { name = "APS_FixAlterPB_Unfix" };
                        AnimationClip anim_FixAlterPB_Fix = new AnimationClip() { name = "APS_FixAlterPB_Fix" };

                        AnimatorControllerLayer layerFixAlterPB = AddAnimatorControllerLayer(fx, "APS_FixAlterPB");
                        AnimatorState state_FixAlterPB_Unfix = AddMotionAnimationState(layerFixAlterPB.stateMachine, "Off", new Vector2(200, 100), anim_FixAlterPB_Unfix);
                        AnimatorState state_FixAlterPB_Fix = AddMotionAnimationState(layerFixAlterPB.stateMachine, "On", new Vector2(200, 200), anim_FixAlterPB_Fix);

                        AddTransition(state_FixAlterPB_Unfix, state_FixAlterPB_Fix, (AnimatorConditionMode.If, 1, "APS_FixAlterPB"));
                        AddTransition(state_FixAlterPB_Fix, state_FixAlterPB_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixAlterPB"));

                        fixAlterPBAnimSet = new FixAnimationSet(anim_FixAlterPB_Unfix, anim_FixAlterPB_Fix);
                        //-----Fix AlterBody PB State
                    }

                    GameObject propMenuRoot = apsMenu.transform.Find("Prop").gameObject;
                    Dictionary<Transform, ShowAnimationSet> showPropObjAnimSetDict = new Dictionary<Transform, ShowAnimationSet>();
                    Dictionary<Transform, ShowAnimationSet> showPropHandleAnimSetDict = new Dictionary<Transform, ShowAnimationSet>();
                    Dictionary<Transform, FixAnimationSet> fixPropAnimSetDict = new Dictionary<Transform, FixAnimationSet>();
                    if (hasPropPlacer)
                    {

                        GameObject propMenuTmp = propMenuRoot.transform.Find("Tmp").gameObject;

                        Dictionary<string, ShowAnimationSet> propNameShowObjAnimSetDict = new Dictionary<string, ShowAnimationSet>();
                        Dictionary<string, ShowAnimationSet> propNameShowHandleAnimSetDict = new Dictionary<string, ShowAnimationSet>();
                        Dictionary<string, FixAnimationSet> propNameFixAnimSetDict = new Dictionary<string, FixAnimationSet>();
                        foreach (IAvatarPoseSystemPropPlacer pp in propPlacers)
                        {
                            Transform target = pp.transform;
                            string propNameBase = pp.GetPropName();
                            if (!propNameShowObjAnimSetDict.ContainsKey(propNameBase))
                            {

                                string paramNameProp = $"APS_Prop_{propNameBase}";
                                string paramNamePropLockHandle = $"APS_Prop_{propNameBase}_LockHandle";
                                fx.AddParameter(paramNameProp, AnimatorControllerParameterType.Bool);
                                fx.AddParameter(paramNamePropLockHandle, AnimatorControllerParameterType.Bool);


                                GameObject propMenu = GameObject.Instantiate(propMenuTmp, propMenuRoot.transform);
                                propMenu.name = propNameBase;

                                GameObject propMenuEnable = propMenu.transform.Find("EnableProp").gameObject;
                                ModularAvatarMenuItem menuItemEnable = propMenuEnable.GetComponent<ModularAvatarMenuItem>();
                                menuItemEnable.Control.parameter.name = paramNameProp;

                                GameObject propMenuLockHandle = propMenu.transform.Find("LockHandle").gameObject;
                                ModularAvatarMenuItem menuItemLockHandle = propMenuLockHandle.GetComponent<ModularAvatarMenuItem>();
                                menuItemLockHandle.Control.parameter.name = paramNamePropLockHandle;


                                AnimationClip anim_ShowPropObj_Hide = new AnimationClip() { name = $"APS_ShowProp_{propNameBase}_Hide" };
                                AnimationClip anim_ShowPropObj_Show = new AnimationClip() { name = $"APS_ShowProp_{propNameBase}_Show" };

                                AnimatorControllerLayer layerShowPropObj = AddAnimatorControllerLayer(fx, $"APS_ShowProp_{propNameBase}");
                                AnimatorState state_ShowPropObj_Hide = AddMotionAnimationState(layerShowPropObj.stateMachine, "Hide", new Vector2(200, 100), anim_ShowPropObj_Hide);
                                AnimatorState state_ShowPropObj_Show = AddMotionAnimationState(layerShowPropObj.stateMachine, "Show", new Vector2(200, 200), anim_ShowPropObj_Show);

                                AddTransition(state_ShowPropObj_Hide, state_ShowPropObj_Show, (AnimatorConditionMode.If, 1, paramNameProp));
                                AddTransition(state_ShowPropObj_Show, state_ShowPropObj_Hide, (AnimatorConditionMode.IfNot, 1, paramNameProp));

                                ShowAnimationSet showPropObjNameAnimSet = new ShowAnimationSet(anim_ShowPropObj_Hide, anim_ShowPropObj_Show);
                                propNameShowObjAnimSetDict.Add(propNameBase, showPropObjNameAnimSet);


                                AnimationClip anim_ShowPropHandle_Hide = new AnimationClip() { name = $"APS_ShowProp_{propNameBase}_Hide" };
                                AnimationClip anim_ShowPropHandle_Show = new AnimationClip() { name = $"APS_ShowProp_{propNameBase}_Show" };

                                AnimatorControllerLayer layerShowPropHandle = AddAnimatorControllerLayer(fx, $"APS_ShowProp_{propNameBase}");
                                AnimatorState state_ShowPropHandle_Hide = AddMotionAnimationState(layerShowPropHandle.stateMachine, "Hide", new Vector2(200, 100), anim_ShowPropHandle_Hide);
                                AnimatorState state_ShowPropHandle_Show = AddMotionAnimationState(layerShowPropHandle.stateMachine, "Show", new Vector2(200, 200), anim_ShowPropHandle_Show);

                                AddTransition(state_ShowPropHandle_Hide, state_ShowPropHandle_Show, (AnimatorConditionMode.If, 1, paramNameProp), (AnimatorConditionMode.If, 1, "APS_FixBody"), (AnimatorConditionMode.If, 1, "APS_ShowHandle"));
                                AddTransition(state_ShowPropHandle_Show, state_ShowPropHandle_Hide, (AnimatorConditionMode.IfNot, 1, paramNameProp));
                                AddTransition(state_ShowPropHandle_Show, state_ShowPropHandle_Hide, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));
                                AddTransition(state_ShowPropHandle_Show, state_ShowPropHandle_Hide, (AnimatorConditionMode.IfNot, 1, "APS_ShowHandle"));

                                ShowAnimationSet ShowPropHandleNameAnimSet = new ShowAnimationSet(anim_ShowPropHandle_Hide, anim_ShowPropHandle_Show);
                                propNameShowHandleAnimSetDict.Add(propNameBase, ShowPropHandleNameAnimSet);


                                AnimationClip anim_FixProp_Unfix = new AnimationClip() { name = $"APS_Prop_{propNameBase}_Unfix" };
                                AnimationClip anim_FixProp_Fix = new AnimationClip() { name = $"APS_Prop_{propNameBase}_Fix" };
                                AnimationClip anim_FixProp_Lock = new AnimationClip() { name = $"APS_Prop_{propNameBase}_Lock" };


                                AnimatorControllerLayer layerFixProp = AddAnimatorControllerLayer(fx, $"APS_FixProp_{propNameBase}");
                                AnimatorState state_FixProp_Unfix = AddMotionAnimationState(layerFixProp.stateMachine, "Off", new Vector2(400, 100), anim_FixProp_Unfix);
                                AnimatorState state_FixProp_Fix = AddMotionAnimationState(layerFixProp.stateMachine, "On", new Vector2(400, 200), anim_FixProp_Fix);
                                AnimatorState state_FixProp_Lock = AddMotionAnimationState(layerFixProp.stateMachine, "On_Lock", new Vector2(100, 200), anim_FixProp_Lock);


                                AddTransition(state_FixProp_Unfix, state_FixProp_Fix, (AnimatorConditionMode.If, 1, "APS_FixBody"));
                                AddTransition(state_FixProp_Fix, state_FixProp_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));

                                AddTransition(state_FixProp_Fix, state_FixProp_Lock, (AnimatorConditionMode.IfNot, 1, paramNameProp));
                                AddTransition(state_FixProp_Fix, state_FixProp_Lock, (AnimatorConditionMode.If, 1, paramNamePropLockHandle));
                                AddTransition(state_FixProp_Lock, state_FixProp_Fix, (AnimatorConditionMode.IfNot, 1, paramNamePropLockHandle), (AnimatorConditionMode.If, 1, paramNameProp));
                                AddTransition(state_FixProp_Lock, state_FixProp_Unfix, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));

                                FixAnimationSet fixPropNameAnimationSet = new FixAnimationSet(anim_FixProp_Unfix, anim_FixProp_Fix, anim_FixProp_Lock);
                                propNameFixAnimSetDict.Add(propNameBase, fixPropNameAnimationSet);
                            }

                            ShowAnimationSet showPropObjAnimationSet = propNameShowObjAnimSetDict[propNameBase];
                            pp.toggleObjects.ToList().ForEach(o =>
                            {
                                string path = AnimationUtility.CalculateTransformPath(o.transform, animator.transform);
                                showPropObjAnimationSet.AddInstantaneousCurve(path, typeof(GameObject), "m_IsActive", 0f, 1f);
                            });
                            showPropObjAnimSetDict.Add(target, showPropObjAnimationSet);


                            ShowAnimationSet showPropHandleAnimationSet = propNameShowHandleAnimSetDict[propNameBase];
                            showPropHandleAnimSetDict.Add(target, showPropHandleAnimationSet);

                            FixAnimationSet fixAnimationSet = propNameFixAnimSetDict[propNameBase];
                            fixPropAnimSetDict.Add(target, fixAnimationSet);
                        }

                        GameObject.DestroyImmediate(propMenuTmp);

                    }
                    else
                    {
                        GameObject.DestroyImmediate(propMenuRoot);
                    }

                    AnimationClip anim_GhostBody_Hide = new AnimationClip() { name = "APS_GhostBody_Hide" };
                    AnimationClip anim_GhostBody_Show = new AnimationClip() { name = "APS_GhostBody_Show" };

                    AnimatorControllerLayer layerGhostBody = AddAnimatorControllerLayer(fx, "APS_GhostBody");
                    AnimatorState state_GhostBody_Hide = AddMotionAnimationState(layerGhostBody.stateMachine, "Hide", new Vector2(200, 100), anim_GhostBody_Hide);
                    AnimatorState state_GhostBody_Show = AddMotionAnimationState(layerGhostBody.stateMachine, "Show", new Vector2(200, 200), anim_GhostBody_Show);

                    if (hasAlterBody)
                    {
                        AddTransition(state_GhostBody_Hide, state_GhostBody_Show, (AnimatorConditionMode.If, 1, "APS_FixBody"), (AnimatorConditionMode.If, 1, "APS_FixAlterBody"), (AnimatorConditionMode.IfNot, 1, "APS_HideBody"));
                        AddTransition(state_GhostBody_Show, state_GhostBody_Hide, (AnimatorConditionMode.IfNot, 1, "APS_FixAlterBody"));
                        AddTransition(state_GhostBody_Show, state_GhostBody_Hide, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));
                        AddTransition(state_GhostBody_Show, state_GhostBody_Hide, (AnimatorConditionMode.If, 1, "APS_HideBody"));
                    }
                    else
                    {
                        AddTransition(state_GhostBody_Hide, state_GhostBody_Show, (AnimatorConditionMode.If, 1, "APS_FixBody"), (AnimatorConditionMode.IfNot, 1, "APS_HideBody"));
                        AddTransition(state_GhostBody_Show, state_GhostBody_Hide, (AnimatorConditionMode.IfNot, 1, "APS_FixBody"));
                        AddTransition(state_GhostBody_Show, state_GhostBody_Hide, (AnimatorConditionMode.If, 1, "APS_HideBody"));
                    }

                    ShowAnimationSet showGhostAnimSet = new ShowAnimationSet(anim_GhostBody_Hide, anim_GhostBody_Show);

                    Transform ghotsBody = ap.transform.Find("Ghost_Proxies/APS_Ghost_Hips_Proxy/APS_Body/BodyModel");
                    string ghostBodyPath = AnimationUtility.CalculateTransformPath(ghotsBody, animator.transform);
                    showGhostAnimSet.AddInstantaneousCurve(ghostBodyPath, typeof(GameObject), "m_IsActive", 0f, 1f);


                    tw.WriteSeparator();
                    tw.Write("+ Setup FixableBoneDict\n");

                    Dictionary<HumanBodyBones, Transform> bodyBoneDict = new Dictionary<HumanBodyBones, Transform>();
                    for (HumanBodyBones bodyBone = HumanBodyBones.Hips; bodyBone < HumanBodyBones.LastBone; bodyBone++)
                    {
                        bodyBoneDict.Add(bodyBone, animator.GetBoneTransform(bodyBone));
                    }

                    List<(Dictionary<HumanBodyBones, Transform> bodyBoneDict, bool isAlterBody)> bodyBoneDicts = new List<(Dictionary<HumanBodyBones, Transform> bodyBoneDict, bool isAlterBody)>();
                    bodyBoneDicts.Add(new(bodyBoneDict, false));
                    if (hasAlterBody && alterBody.createHandle)
                    {
                        bodyBoneDicts.Add(new(buildInfo.alterBodyBoneDict, true));
                    }


                    Dictionary<Transform, FixableBoneInfo> mainBodyFixableBoneDict = new Dictionary<Transform, FixableBoneInfo>();
                    Dictionary<Transform, FixableBoneInfo> alterBodyFixableBoneDict = new Dictionary<Transform, FixableBoneInfo>();
                    bodyBoneDicts.ForEach(bd =>
                    {
                        bool isAlterBody = bd.isAlterBody;
                        foreach (HumanBodyBones bodyBone in bd.bodyBoneDict.Keys)
                        {
                            GameObject objOrg = bd.bodyBoneDict[bodyBone]?.gameObject;
                            switch (bodyBone)
                            {
                                case HumanBodyBones.LeftToes:
                                case HumanBodyBones.RightToes:
                                    //つま先のRigが設定されてない場合は、それらしいボーンを探して割り当てる
                                    if (objOrg == null)
                                    {
                                        Transform trFoot = bd.bodyBoneDict[bodyBone - 14]; //LeftFoot or RightFoot
                                        if (trFoot != null)
                                        {
                                            Transform trToe = FindEndBone(trFoot, "toe");
                                            if (trToe == null)
                                            {
                                                trToe = FindEndBone(trFoot, "_end");
                                            }
                                            objOrg = trToe?.gameObject;
                                        }
                                    }
                                    break;
                                case HumanBodyBones.LeftEye:
                                    //目はVRCAvatarDescriptorから取得
                                    if (!isAlterBody && descripter.customEyeLookSettings.leftEye != null)
                                    {
                                        objOrg = descripter.customEyeLookSettings.leftEye.gameObject;
                                    }
                                    break;
                                case HumanBodyBones.RightEye:
                                    //目はVRCAvatarDescriptorから取得
                                    if (!isAlterBody && descripter.customEyeLookSettings.rightEye != null)
                                    {
                                        objOrg = descripter.customEyeLookSettings.rightEye.gameObject;
                                    }
                                    break;
                                case HumanBodyBones.Jaw:
                                    //顎のRigがおかしいケースがあるため、怪しい場合は無視する
                                    //一旦名前にjawが含まれてないかで判断する
                                    if (objOrg != null && !objOrg.name.Contains("jaw", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string body = "顎のRigに設定されているボーン(" + objOrg.name + ")が名前に「jaw」が含まれていないため無視します。この警告が表示されていて、顔周辺の表示異常がある場合はZeroFactoryに問い合わせください。";
                                        body += "\n - " + AnimationUtility.CalculateTransformPath(objOrg.transform, animator.transform);
                                        LogWarning(body, objOrg);
                                        objOrg = null;
                                    }
                                    break;
                            }

                            if (objOrg != null)
                            {

                                if (Math.Abs(objOrg.transform.lossyScale.x - objOrg.transform.lossyScale.y) > 0.01
                                    || Math.Abs(objOrg.transform.lossyScale.y - objOrg.transform.lossyScale.z) > 0.01
                                    || Math.Abs(objOrg.transform.lossyScale.z - objOrg.transform.lossyScale.x) > 0.01)
                                {
                                    string body = "スケールが不均等になっています。表示が歪んだり固定できていないように見える可能性があります。このオブジェクトまたは親のオブジェクトのスケール設定を確認してください。";
                                    body += "\n - " + AnimationUtility.CalculateTransformPath(objOrg.transform, animator.transform);
                                    LogWarning(body, objOrg);
                                }

                                BoneSet boneSet = (isAlterBody ? alterBodyRootBoneGroup : mainBodyRootBoneGroup).GetBoneSet(objOrg.transform);

                                float handleSize = 0.01f;
                                bool isDualHandle = true;
                                if (bodyBone == HumanBodyBones.LeftEye
                                    || bodyBone == HumanBodyBones.RightEye
                                    || (bodyBone >= HumanBodyBones.LeftThumbProximal && bodyBone <= HumanBodyBones.RightLittleDistal)) //FingerBones
                                {
                                    handleSize = 0.005f;
                                    isDualHandle = false;
                                }

                                bool isPhysBoneSplitRoot = false;
                                if (bodyBone == HumanBodyBones.Hips
                                    || bodyBone == HumanBodyBones.Neck
                                    || bodyBone == HumanBodyBones.LeftShoulder
                                    || bodyBone == HumanBodyBones.RightShoulder
                                    || bodyBone == HumanBodyBones.LeftHand
                                    || bodyBone == HumanBodyBones.RightHand
                                    || bodyBone == HumanBodyBones.LeftUpperLeg
                                    || bodyBone == HumanBodyBones.RightUpperLeg
                                    || bodyBone == HumanBodyBones.Hips)
                                {
                                    isPhysBoneSplitRoot = true;
                                }

                                Dictionary<Transform, FixableBoneInfo> fixableBoneInfo = isAlterBody ? alterBodyFixableBoneDict : mainBodyFixableBoneDict;
                                fixableBoneInfo.Add(boneSet.original, CreateBoneInfo(boneSet, bodyBone, handleSize, isDualHandle, false, isPhysBoneSplitRoot, Vector3.zero, Vector3.zero));

                            }
                        }

                    });

                    if (hasExBone)
                    {

                        tw.WriteSeparator();
                        tw.Write("+ Setup FixableBoneDict ExtraBone\n");

                        foreach (IAvatarPoseSystemExtraBone extraBone in extraBones)
                        {
                            List<Transform> physBoneSplitRoots = new List<Transform>();
                            Transform rootTransform = extraBone.GetAffectedRootTransform();
                            foreach (Transform affectedTransform in extraBone.GetAffectedTransforms())
                            {
                                BoneSet boneSet = mainBodyRootBoneGroup.GetBoneSet(affectedTransform);
                                if (boneSet == null)
                                {
                                    //ModularAvatarMergeArmatureでマージされるボーンに追加ボーンが設定されている場合があるのでチェックする
                                    //この場合、マージ先のボーンを対象に追加ボーンを作成するようにする
                                    ModularAvatarMergeArmature maMA = GetNearestComponent<ModularAvatarMergeArmature>(affectedTransform);
                                    if (maMA != null)
                                    {
                                        string relPath = AnimationUtility.CalculateTransformPath(affectedTransform, maMA.transform);
                                        Transform mergedTransform = maMA.mergeTargetObject.transform.Find(relPath);
                                        if (mergedTransform != null)
                                        {
                                            boneSet = mainBodyRootBoneGroup.GetBoneSet(mergedTransform);
                                        }
                                    }
                                }

                                if (boneSet == null)
                                {
                                    LogWarning("適用できない追加ボーン設定がありました:" + AnimationUtility.CalculateTransformPath(affectedTransform, root.transform), affectedTransform);
                                    continue;
                                }

                                //既にある場合は無視する
                                if (mainBodyFixableBoneDict.ContainsKey(boneSet.original)) continue;

                                //全てのPBを見てEXBoneを見ている物は置き換える必要がある。
                                //TODO:中途半端に影響がある場合はどうするか
                                foreach (VRCPhysBone pb in root.GetComponentsInChildren<VRCPhysBone>(true))
                                {
                                    Transform pbRootTransform = pb.rootTransform != null ? pb.rootTransform : pb.transform;
                                    if (pbRootTransform == boneSet.original)
                                    {
                                        VRCPhysBone pb1 = CopyComponent(pb.gameObject, pb);
                                        pb1.rootTransform = boneSet.clone;
                                        for (int i = 0; i < pb1.ignoreTransforms.Count; i++)
                                        {
                                            BoneSet ignoreBoneSet = mainBodyRootBoneGroup.GetBoneSet(pb1.ignoreTransforms[i]);
                                            if (ignoreBoneSet != null)
                                            {
                                                pb1.ignoreTransforms[i] = ignoreBoneSet.clone;
                                            }
                                        }
                                        UnityEngine.Object.DestroyImmediate(pb);
                                    }

                                }

                                //PhysBoneが8階層までしか付けられないので、区切るところを計算する
                                bool isPhysBoneSplitRoot = false;
                                Transform tmpTransform = boneSet.original;
                                if (tmpTransform == rootTransform)
                                {
                                    isPhysBoneSplitRoot = true;
                                }
                                else
                                {
                                    int pbCount = 0;
                                    while (!physBoneSplitRoots.Contains(tmpTransform))
                                    {
                                        pbCount += extraBone.isDualHandle ? 2 : 1;
                                        tmpTransform = tmpTransform.parent;
                                    }
                                    isPhysBoneSplitRoot = pbCount >= 8;
                                }
                                if (isPhysBoneSplitRoot)
                                {
                                    physBoneSplitRoots.Add(boneSet.original);
                                }
                                mainBodyFixableBoneDict.Add(boneSet.original, CreateBoneInfo(boneSet, HumanBodyBones.LastBone, extraBone.handleSize, extraBone.isDualHandle, false, isPhysBoneSplitRoot, Vector3.zero, Vector3.zero));
                            }
                        }
                    }


                    if (hasPropPlacer)
                    {
                        tw.WriteSeparator();
                        tw.Write("+ Setup FixableBoneDict PropPlacer\n");

                        foreach (IAvatarPoseSystemPropPlacer item in propPlacers)
                        {
                            Transform targetTransform = item.GetAffectedTransform();
                            if (targetTransform == null) continue;

                            BoneSet boneSet = mainBodyRootBoneGroup.GetBoneSet(targetTransform);
                            mainBodyFixableBoneDict.Add(boneSet.original, CreateBoneInfo(boneSet, HumanBodyBones.LastBone, item.handleSize, true, true, true, item.positionOffset, item.rotationOffset));
                        }
                    }

                    foreach (Component c in root.GetComponentsInChildren<Component>(true))
                    {
                        string cName = c.GetType().FullName;
                        switch (cName)
                        {
                            case "Narazaka.VRChat.FloorAdjuster.FloorAdjuster":
                                LogDebug("Find FloorAdjuster");
                                ConvertTransform(c, objHipsOrg.transform, objHipsClone.transform);
                                break;
                        }
                    }

                    tw.WriteSeparator();
                    tw.Write("+ Convert Physbones\n");

                    Dictionary<VRCPhysBoneBase, VRCPhysBoneBase> dictPhysBoneConvert = new Dictionary<VRCPhysBoneBase, VRCPhysBoneBase>();
                    List<GameObject> fixPhysBoneObjects = new List<GameObject>();
                    foreach (VRCPhysBone pb in root.GetComponentsInChildren<VRCPhysBone>(true))
                    {
                        if (buildInfo.isAPSParts(pb.transform)) continue;

                        GameObject objPB = pb.transform.Find("APS_PB")?.gameObject;
                        if (objPB == null)
                        {
                            objPB = new GameObject() { name = "APS_PB" };
                            objPB.transform.parent = pb.transform;
                            if (!IsUnfixPhysBone(pb, ap, unfixObjectsFromPaths))
                            {
                                fixPhysBoneObjects.Add(objPB);
                            }
                        }
                        VRCPhysBone pb1 = CopyComponent(objPB, pb);
                        pb1.resetWhenDisabled = false;
                        if (pb1.rootTransform == null)
                        {
                            pb1.rootTransform = pb.gameObject.transform;
                        }
                        if (objPB.transform.IsChildOf(pb1.rootTransform))
                        {
                            pb1.ignoreTransforms.Add(objPB.transform);
                        }

                        dictPhysBoneConvert.Add(pb, pb1);
                    }

                    tw.WriteSeparator();
                    tw.Write("+ Convert MergePhysBones\n");

                    foreach (Component mpb in root.GetComponentsInChildren<Component>(true).Where(c => c.GetType().FullName == "Anatawa12.AvatarOptimizer.MergePhysBone"))
                    {

                        tw.WriteProperties(mpb);

                        Type mergePhysBoneType = mpb.GetType();
                        FieldInfo componentsSetField = mergePhysBoneType.GetField("componentsSet");
                        object componentsSet = componentsSetField.GetValue(mpb);

                        Type vrcPhysBoneBaseSetType = componentsSet.GetType();
                        MethodInfo getAsListMethod = vrcPhysBoneBaseSetType.GetMethod("GetAsList");
                        MethodInfo setValueNonPrefabMethod = vrcPhysBoneBaseSetType.GetMethod("SetValueNonPrefab", new Type[] { typeof(List<VRCPhysBoneBase>) });
                        List<VRCPhysBoneBase> currentList = (List<VRCPhysBoneBase>)getAsListMethod.Invoke(componentsSet, null);

                        for (int i = 0; i < currentList.Count; i++)
                        {
                            if (dictPhysBoneConvert.ContainsKey(currentList[i]))
                            {
                                currentList[i] = dictPhysBoneConvert[currentList[i]];
                            }
                        }

                        if (IsUnfixPhysBone(mpb, ap, unfixObjectsFromPaths))
                        {
                            setValueNonPrefabMethod.Invoke(componentsSet, new object[] { currentList });
                            continue;
                        }

                        GameObject objMPB = mpb.transform.Find("APS_PB")?.gameObject;
                        if (objMPB == null)
                        {
                            objMPB = new GameObject() { name = "APS_PB" };
                            objMPB.transform.parent = mpb.transform;
                            fixPhysBoneObjects.Add(objMPB);
                        }
                        //object mpb1 = objMPB.AddComponent(mergePhysBoneType);
                        object mpb1 = CopyComponent(objMPB, mpb);
                        object componentsSet1 = componentsSetField.GetValue(mpb1);

                        setValueNonPrefabMethod.Invoke(componentsSet1, new object[] { currentList });

                        UnityEngine.Object.DestroyImmediate(mpb);
                    }

                    foreach (VRCPhysBoneBase pb in dictPhysBoneConvert.Keys)
                    {
                        UnityEngine.Object.DestroyImmediate(pb);
                    }

                    foreach (VRCConstraintBase con in root.GetComponentsInChildren<VRCConstraintBase>(true))
                    {
                        //VRCConstraintBase.ReEvaluatePhysBoneOrderでMissingReferenceExceptionが出ることの対応
                        //_rootChildPhysBonesをnullにしてReEvaluatePhysBoneOrderの中で再取得させる
                        FieldInfo fieldInfo = typeof(VRCConstraintBase).GetField("_rootChildPhysBones", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(con, null);
                        }
                    }

                    foreach (VRCHeadChop hc in root.GetComponentsInChildren<VRCHeadChop>(true))
                    {
                        if (IsUnfixPhysBone(hc, ap, unfixObjectsFromPaths)) continue;

                        string hcPath = AnimationUtility.CalculateTransformPath(hc.transform, animator.transform);
                        fixMainBodyAnimSet.AddInstantaneousCurve(hcPath, typeof(VRCHeadChop), "m_Enabled", 1f, 0f, 0f);
                    }

                    tw.WriteSeparator();
                    tw.Write("+ Convert ResolveUnfixObjects\n");

                    foreach (GameObject obj in ap.UnfixObjects.Where(x => x != null))
                    {
                        ResolveUnfixObject(obj, mainBodyFixableBoneDict);
                    }

                    foreach (GameObject obj in unfixObjectsFromPaths.Where(x => x != null))
                    {
                        ResolveUnfixObject(obj, mainBodyFixableBoneDict);
                    }


                    //Constraintのターゲットをcloneに移動する

                    tw.WriteSeparator();
                    tw.Write("+ Move Constraint Target Original to Clone\n");

                    foreach (FixableBoneInfo boneInfo in mainBodyFixableBoneDict.Values)
                    {
                        List<Component> constraints = new List<Component>();
                        constraints.AddRange(boneInfo.original.GetComponents(typeof(IConstraint)));
                        constraints.AddRange(boneInfo.original.GetComponents(typeof(VRCConstraintBase)));
                        constraints.ForEach(con =>
                        {
                            tw.WriteEmptyLine();
                            tw.WriteProperties(con);

                            Component con1 = CopyComponent(boneInfo.clone.gameObject, con);
                            ConvertConstraintSourceTransform(con1, sourceTransform =>
                            {
                                BoneSet boneSet = mainBodyRootBoneGroup.GetBoneSet(sourceTransform);
                                return boneSet != null ? boneSet.clone : sourceTransform;
                            });

                            tw.WriteProperties(con1);
                            tw.Write($"  ->Move");

                            UnityEngine.Object.DestroyImmediate(con);
                        });

                        root.transform.GetComponentsInChildren(typeof(VRCConstraintBase)).ToList().ForEach(con =>
                        {
                            VRCConstraintBase constraint = (VRCConstraintBase)con;
                            if (constraint.TargetTransform == boneInfo.original)
                            {
                                tw.WriteEmptyLine();
                                tw.WriteProperties(con);

                                constraint.TargetTransform = boneInfo.clone;

                                tw.WriteProperties(con);
                                tw.Write($"  ->Move Target");
                            }
                        });
                    }


                    tw.WriteSeparator();
                    tw.Write("+ Setup FixableBones\n");

                    foreach (FixableBoneInfo mainBones in mainBodyFixableBoneDict.Values)
                    {
                        tw.WriteProperties(mainBones);

                        switch (mainBones.bodyBone)
                        {
                            case HumanBodyBones.LeftEye:
                                descripter.customEyeLookSettings.leftEye = mainBones.clone;
                                break;
                            case HumanBodyBones.RightEye:
                                descripter.customEyeLookSettings.rightEye = mainBones.clone;
                                break;
                        }

                        //Collider設定がCustomの場合？にコライダー位置がおかしくなる対応
                        switch (mainBones.bodyBone)
                        {
                            case HumanBodyBones.Head:
                                descripter.collider_head.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.Chest:
                                descripter.collider_torso.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.LeftToes:
                                descripter.collider_footL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.RightToes:
                                descripter.collider_footR.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.LeftHand:
                                descripter.collider_handL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.RightHand:
                                descripter.collider_handR.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.LeftIndexDistal:
                                descripter.collider_fingerIndexL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.LeftMiddleDistal:
                                descripter.collider_fingerMiddleL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.LeftRingDistal:
                                descripter.collider_fingerRingL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.LeftLittleDistal:
                                descripter.collider_fingerLittleL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.RightIndexDistal:
                                descripter.collider_fingerIndexL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.RightMiddleDistal:
                                descripter.collider_fingerMiddleL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.RightRingDistal:
                                descripter.collider_fingerRingL.transform = mainBones.clone;
                                break;
                            case HumanBodyBones.RightLittleDistal:
                                descripter.collider_fingerLittleL.transform = mainBones.clone;
                                break;
                        }


                        FixAnimationSet fixAnimSet = fixMainBodyAnimSet;

                        GameObject objPropHandle = null;
                        if (mainBones.isPropHandle)
                        {
                            objPropHandle = GameObject.Instantiate(objPropHandleOrg, objPropHandleOrg.transform.parent);
                            objPropHandle.name = objPropHandleOrg.name + "$" + Math.Abs(objPropHandle.GetInstanceID()).ToString("X");

                            FixAnimationSet fixPropAnimSet = fixPropAnimSetDict[mainBones.original];

                            ShowAnimationSet showPropObjAnimSet = showPropObjAnimSetDict[mainBones.original];

                            ShowAnimationSet showPropHandleAnimSet = showPropHandleAnimSetDict[mainBones.original];

                            Transform tfHandleMesh = objPropHandle.transform.Find("HandleMesh");
                            string handleMeshPath = AnimationUtility.CalculateTransformPath(tfHandleMesh, animator.transform);
                            showPropHandleAnimSet.AddInstantaneousCurve(handleMeshPath, typeof(GameObject), "m_IsActive", 0f, 1f);

                            Transform tfMoveHandleMesh = objPropHandle.transform.Find("MoveHandleMesh");
                            string moveHandleMeshPath = AnimationUtility.CalculateTransformPath(tfMoveHandleMesh, animator.transform);
                            showPropHandleAnimSet.AddInstantaneousCurve(moveHandleMeshPath, typeof(GameObject), "m_IsActive", 0f, 1f);

                            List<SkinnedMeshRenderer> propHandleMeshes = new List<SkinnedMeshRenderer>();
                            propHandleMeshes.AddRange(objPropHandle.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true));
                            propHandleMeshes.ForEach(m =>
                            {
                                string meshPath = AnimationUtility.CalculateTransformPath(m.transform, animator.transform);
                                fixPropAnimSet.AddInstantaneousCurve(meshPath, typeof(SkinnedMeshRenderer), "material._IsGrayScale", 0f, 0f, 1f);
                            });


                            fixAnimSet = fixPropAnimSet;

                        }

                        string originalPath = AnimationUtility.CalculateTransformPath(mainBones.original, animator.transform);
                        string fixPath = AnimationUtility.CalculateTransformPath(mainBones.fix, animator.transform);
                        switch (mainBones.bodyBone)
                        {
                            case HumanBodyBones.Hips:
                            case HumanBodyBones.LeftShoulder:
                            case HumanBodyBones.RightShoulder:
                            case HumanBodyBones.LeftUpperLeg:
                            case HumanBodyBones.RightUpperLeg:
                            case HumanBodyBones.LeftLowerLeg:
                            case HumanBodyBones.RightLowerLeg:
                            case HumanBodyBones.LeftUpperArm:
                            case HumanBodyBones.RightUpperArm:
                            case HumanBodyBones.LeftLowerArm:
                            case HumanBodyBones.RightLowerArm:
                            case HumanBodyBones.LeftHand:
                            case HumanBodyBones.RightHand:
                            case HumanBodyBones.LeftFoot:
                            case HumanBodyBones.RightFoot:
                            case HumanBodyBones.Spine:
                            case HumanBodyBones.Chest:
                            case HumanBodyBones.Neck:
                            case HumanBodyBones.Head:
                            case >= HumanBodyBones.LeftThumbProximal and <= HumanBodyBones.RightLittleDistal: //FingerBones
                            case HumanBodyBones.LastBone:
                                Vector3 posEnd = calcMainHandleEndPos(mainBones, mainBodyFixableBoneDict);
                                if (posEnd != Vector3.zero)
                                {

                                    GameObject objPBContainer = new GameObject { name = mainBones.fix.name + "_Main_Wrap" };
                                    objPBContainer.name += "$" + Math.Abs(objPBContainer.GetInstanceID()).ToString("X");
                                    objPBContainer.transform.parent = mainBones.fix.parent;
                                    objPBContainer.transform.position = mainBones.fix.position;
                                    objPBContainer.transform.rotation = mainBones.fix.rotation;

                                    if (mainBones.isPhysBoneSplitRoot)
                                    {
                                        //PhysBone8階層制限にかからないように、適度に切ってParentConstraintで繋ぐ
                                        GameObject objPBContainerSrc = new GameObject { name = mainBones.fix.name + "_Main_Src" };
                                        objPBContainerSrc.name += "$" + Math.Abs(objPBContainerSrc.GetInstanceID()).ToString("X");
                                        objPBContainerSrc.transform.parent = mainBones.fix.parent;
                                        objPBContainerSrc.transform.position = mainBones.fix.position;
                                        objPBContainerSrc.transform.rotation = mainBones.fix.rotation;
                                        AddParentConstraint(objPBContainerSrc, true, 1f, new VRCConstraintSource() { SourceTransform = mainBones.clone, Weight = 1f });

                                        string pcPathSrc = AnimationUtility.CalculateTransformPath(objPBContainerSrc.transform, animator.transform);
                                        fixAnimSet.AddInstantaneousCurve(pcPathSrc, typeof(VRCParentConstraint), "m_Enabled", 1f, 0f, 0f);

                                        objPBContainer.transform.parent = objFixRoot.transform;
                                        AddParentConstraint(objPBContainer, true, 1f, new VRCConstraintSource() { SourceTransform = objPBContainerSrc.transform, Weight = 1f });
                                    }
                                    else
                                    {
                                        switch (mainBones.bodyBone)
                                        {
                                            case HumanBodyBones.LeftEye:
                                            case HumanBodyBones.RightEye:
                                                AddRotationConstraint(objPBContainer, true, 1f, new VRCConstraintSource() { SourceTransform = mainBones.clone, Weight = 1f });

                                                string pcPathEye = AnimationUtility.CalculateTransformPath(objPBContainer.transform, animator.transform);
                                                fixAnimSet.AddInstantaneousCurve(pcPathEye, typeof(VRCRotationConstraint), "m_Enabled", 1f, 0f, 0f);
                                                break;
                                            default:
                                                AddParentConstraint(objPBContainer, true, 1f, new VRCConstraintSource() { SourceTransform = mainBones.clone, Weight = 1f });

                                                string pcPath = AnimationUtility.CalculateTransformPath(objPBContainer.transform, animator.transform);
                                                fixAnimSet.AddInstantaneousCurve(pcPath, typeof(VRCParentConstraint), "m_Enabled", 1f, 0f, 0f);
                                                break;
                                        }
                                    }

                                    GameObject objPBMainRoot = null;
                                    if (mainBones.isPropHandle)
                                    {
                                        objPBMainRoot = objPropHandle.transform.Find("Armature/Bone_Root.002").gameObject;
                                    }
                                    else
                                    {
                                        objPBMainRoot = (mainBones.isBody() ? bodyHandleMan : exHandleMan).GetNextTransform().gameObject;
                                    }

                                    objPBMainRoot.name = "Handle_" + mainBones.bodyBone.ToString() + "_Main";
                                    objPBMainRoot.transform.localScale = new Vector3(mainBones.handleSize, mainBones.handleSize, mainBones.handleSize);
                                    objPBMainRoot.transform.parent = objPBContainer.transform;
                                    objPBMainRoot.transform.position = mainBones.fix.position;
                                    objPBMainRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, posEnd - objPBMainRoot.transform.position);

                                    GameObject objPBMainEnd = objPBMainRoot.transform.GetChild(0).gameObject;
                                    objPBMainEnd.name = "Handle_" + mainBones.bodyBone.ToString() + "_Main_End";
                                    objPBMainEnd.transform.position = posEnd;

                                    mainBones.fix.parent = objPBMainRoot.transform;
                                    if (mainBones.bodyBone == HumanBodyBones.LeftEye || mainBones.bodyBone == HumanBodyBones.RightEye)
                                    {
                                        mainBones.fix.parent = objPBContainer.transform.parent;
                                        mainBones.fix.localPosition = Vector3.zero;
                                    }

                                    VRCPhysBone pbHandleMain = AddHandlePhysBone(objPBMainRoot, mainBones.fix, isHingeHandle(mainBones.bodyBone));

                                    Vector3 posEnd2 = calcSubHandleEndPos(mainBones);
                                    if (posEnd2 != Vector3.zero)
                                    {
                                        GameObject objPBSubRoot = null;
                                        if (mainBones.isPropHandle)
                                        {
                                            objPBSubRoot = objPropHandle.transform.Find("Armature/Bone_Root.001").gameObject;
                                        }
                                        else
                                        {
                                            objPBSubRoot = (mainBones.isBody() ? bodyHandleMan : exHandleMan).GetNextTransform().gameObject;
                                        }
                                        objPBSubRoot.name = "Handle_" + mainBones.bodyBone.ToString() + "_Sub";
                                        objPBSubRoot.transform.localScale = new Vector3(mainBones.handleSize, mainBones.handleSize, mainBones.handleSize);
                                        objPBSubRoot.transform.parent = objPBContainer.transform;
                                        objPBSubRoot.transform.position = mainBones.fix.position;
                                        objPBSubRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, posEnd2 - objPBSubRoot.transform.position);

                                        GameObject objPBSubEnd = objPBSubRoot.transform.GetChild(0).gameObject;
                                        objPBSubEnd.name = "Handle_" + mainBones.bodyBone.ToString() + "_Sub_End";
                                        objPBSubEnd.transform.position = posEnd2;

                                        objPBMainRoot.transform.parent = objPBSubRoot.transform;

                                        VRCPhysBone pbHandleSub = AddHandlePhysBone(objPBSubRoot, objPBMainRoot.transform, false);

                                        switch (mainBones.bodyBone)
                                        {
                                            case HumanBodyBones.Hips:
                                                GameObject objHipsHandleRoot = objHipsHandle.transform.Find("HipsHandle_Root").gameObject;
                                                GameObject objHipsHandlePB = objHipsHandleRoot.transform.Find("Bone_Root.000/Bone_Root.001").gameObject;
                                                GameObject objHipsHandleEnd = objHipsHandleRoot.transform.Find("Bone_Root.000/Bone_Root.001/Bone_Root.002/Bone_End.002/HipsHandle_End/HipsHandle_End.001").gameObject;

                                                objHipsHandleRoot.transform.parent = objPBContainer.transform;
                                                objPBSubRoot.transform.parent = objHipsHandleEnd.transform;

                                                VRCPhysBone pbHipsHandle = objHipsHandlePB.GetComponent<VRCPhysBone>();
                                                pbHipsHandle.ignoreTransforms.Add(objPBSubRoot.transform);

                                                string pbHipsHandlePath = AnimationUtility.CalculateTransformPath(objHipsHandlePB.transform, animator.transform);
                                                fixAnimSet.AddInstantaneousCurve(pbHipsHandlePath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                                fixAnimSet.AddInstantaneousCurve(pbHipsHandlePath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);
                                                break;

                                            case HumanBodyBones.LeftLowerArm:
                                                GameObject objHandHandleLRoot = objHandHandleL.transform.Find("HandHandle_L_Root").gameObject;
                                                GameObject objHandHandleLPB = objHandHandleLRoot.transform.Find("Bone_Root").gameObject;

                                                objHandHandleLRoot.transform.parent = mainBones.fix;

                                                string pbHandLPath = AnimationUtility.CalculateTransformPath(objHandHandleLPB.transform, animator.transform);
                                                fixAnimSet.AddInstantaneousCurve(pbHandLPath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                                fixAnimSet.AddInstantaneousCurve(pbHandLPath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);
                                                break;

                                            case HumanBodyBones.RightLowerArm:

                                                GameObject objHandHandleRRoot = objHandHandleR.transform.Find("HandHandle_R_Root").gameObject;
                                                GameObject objHandHandleRPB = objHandHandleRRoot.transform.Find("Bone_Root").gameObject;

                                                objHandHandleRRoot.transform.parent = mainBones.fix;

                                                string pbHandRPath = AnimationUtility.CalculateTransformPath(objHandHandleRPB.transform, animator.transform);
                                                fixAnimSet.AddInstantaneousCurve(pbHandRPath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                                fixAnimSet.AddInstantaneousCurve(pbHandRPath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);
                                                break;

                                        }

                                        if (mainBones.isPropHandle)
                                        {
                                            Vector3 handlePos = mainBones.fix.position + mainBones.positionOffset;
                                            Quaternion totalRotation = mainBones.fix.rotation * Quaternion.Euler(mainBones.rotationOffset);
                                            Vector3 posMoveHandleStart = handlePos + totalRotation * new Vector3(0, -0.1f, -0.1f);

                                            GameObject objMoveRotSrc = new GameObject("Handle_" + mainBones.bodyBone.ToString() + "_Move_Rot_Src");
                                            objMoveRotSrc.transform.parent = objPBContainer.transform;
                                            objMoveRotSrc.transform.localPosition = Vector3.zero;
                                            objMoveRotSrc.transform.position = posMoveHandleStart;
                                            objMoveRotSrc.transform.rotation = Quaternion.FromToRotation(Vector3.up, mainBones.fix.position - objMoveRotSrc.transform.position);

                                            GameObject objPBMoveRoot = objPropHandle.transform.Find("Armature/Bone_Root.000").gameObject;
                                            objPBMoveRoot.name = "Handle_" + mainBones.bodyBone.ToString() + "_Move";
                                            objPBMoveRoot.transform.localScale = new Vector3(mainBones.handleSize, mainBones.handleSize, mainBones.handleSize);
                                            objPBMoveRoot.transform.parent = objMoveRotSrc.transform;
                                            objPBMoveRoot.transform.localPosition = Vector3.zero;
                                            objPBMoveRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, handlePos - objPBMoveRoot.transform.position);

                                            GameObject objPBMoveEnd = objPBMoveRoot.transform.GetChild(0).gameObject;
                                            objPBMoveEnd.name = "Handle_" + mainBones.bodyBone.ToString() + "_Move_End";
                                            objPBMoveEnd.transform.position = handlePos;

                                            GameObject objMoveRotDest = new GameObject("Handle_" + mainBones.bodyBone.ToString() + "_Move_Rot_Dest");
                                            objMoveRotDest.transform.parent = objPBMoveEnd.transform;
                                            objMoveRotDest.transform.localPosition = Vector3.zero;
                                            objMoveRotDest.transform.rotation = objMoveRotSrc.transform.rotation;
                                            AddRotationConstraint(objMoveRotDest, true, 1f, new VRCConstraintSource() { SourceTransform = objMoveRotSrc.transform, Weight = 1f });

                                            objPBSubRoot.transform.parent = objMoveRotDest.transform;
                                            Vector3 tmpFixPos = mainBones.fix.position;
                                            objPBSubRoot.transform.localPosition = Vector3.zero;
                                            mainBones.fix.position = tmpFixPos;

                                            VRCPhysBone pbHandleMove = AddHandlePhysBone(objPBMoveRoot, objMoveRotDest.transform, false);
                                            pbHandleMove.maxStretch = 100f;
                                            pbHandleMove.maxSquish = 1f;

                                            string pbMovePath = AnimationUtility.CalculateTransformPath(objPBMoveRoot.transform, animator.transform);
                                            fixAnimSet.AddInstantaneousCurve(pbMovePath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                            fixAnimSet.AddInstantaneousCurve(pbMovePath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);
                                        }

                                        string pbSubPath = AnimationUtility.CalculateTransformPath(objPBSubRoot.transform, animator.transform);
                                        fixAnimSet.AddInstantaneousCurve(pbSubPath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                        fixAnimSet.AddInstantaneousCurve(pbSubPath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);
                                    }

                                    string pbMainPath = AnimationUtility.CalculateTransformPath(objPBMainRoot.transform, animator.transform);
                                    fixAnimSet.AddInstantaneousCurve(pbMainPath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                    fixAnimSet.AddInstantaneousCurve(pbMainPath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);

                                    GameObject objConst = new GameObject();
                                    objConst.name = mainBones.original.name + "_Const";
                                    if (mainBones.isBody())
                                    {
                                        objConst.transform.parent = mainBones.clone;
                                        objConst.transform.localPosition = Vector3.zero;
                                        objConst.transform.localRotation = quaternion.identity;
                                    }
                                    else
                                    {
                                        objConst.transform.parent = mainBones.original.parent;
                                        objConst.transform.position = mainBones.original.position;
                                        objConst.transform.rotation = mainBones.original.rotation;
                                    }

                                    ModularAvatarBoneProxy maBP = mainBones.original.gameObject.AddComponent<ModularAvatarBoneProxy>();
                                    maBP.target = objConst.transform;

                                    if (mainBones.bodyBone == HumanBodyBones.LeftHand)
                                    {
                                        string conPath = AnimationUtility.CalculateTransformPath(objPBContainer.transform, animator.transform);
                                        anim_HandHandleL_Unfix.SetCurve(conPath, typeof(Transform), "m_LocalScale.x", AnimationCurve.Constant(0f, 0f, objPBContainer.transform.localScale.x));
                                        anim_HandHandleL_Unfix.SetCurve(conPath, typeof(Transform), "m_LocalScale.y", AnimationCurve.Constant(0f, 0f, objPBContainer.transform.localScale.y));
                                        anim_HandHandleL_Unfix.SetCurve(conPath, typeof(Transform), "m_LocalScale.z", AnimationCurve.Constant(0f, 0f, objPBContainer.transform.localScale.z));
                                        anim_HandHandleL_Fix.SetCurve(conPath, typeof(Transform), "m_LocalScale.x", new AnimationCurve(new Keyframe[] {
                                            new Keyframe(0, objPBContainer.transform.localScale.x), new Keyframe(1f, objPBContainer.transform.localScale.x * 3) }));
                                        anim_HandHandleL_Fix.SetCurve(conPath, typeof(Transform), "m_LocalScale.y", new AnimationCurve(new Keyframe[] {
                                            new Keyframe(0, objPBContainer.transform.localScale.y), new Keyframe(1f, objPBContainer.transform.localScale.y * 3) }));
                                        anim_HandHandleL_Fix.SetCurve(conPath, typeof(Transform), "m_LocalScale.z", new AnimationCurve(new Keyframe[] {
                                            new Keyframe(0, objPBContainer.transform.localScale.z), new Keyframe(1f, objPBContainer.transform.localScale.z * 3) }));
                                    }

                                    if (mainBones.bodyBone == HumanBodyBones.RightHand)
                                    {
                                        string conPath = AnimationUtility.CalculateTransformPath(objPBContainer.transform, animator.transform);
                                        anim_HandHandleR_Unfix.SetCurve(conPath, typeof(Transform), "m_LocalScale.x", AnimationCurve.Constant(0f, 0f, objPBContainer.transform.localScale.x));
                                        anim_HandHandleR_Unfix.SetCurve(conPath, typeof(Transform), "m_LocalScale.y", AnimationCurve.Constant(0f, 0f, objPBContainer.transform.localScale.y));
                                        anim_HandHandleR_Unfix.SetCurve(conPath, typeof(Transform), "m_LocalScale.z", AnimationCurve.Constant(0f, 0f, objPBContainer.transform.localScale.z));
                                        anim_HandHandleR_Fix.SetCurve(conPath, typeof(Transform), "m_LocalScale.x", new AnimationCurve(new Keyframe[] {
                                            new Keyframe(0, objPBContainer.transform.localScale.x), new Keyframe(1f, objPBContainer.transform.localScale.x * 3) }));
                                        anim_HandHandleR_Fix.SetCurve(conPath, typeof(Transform), "m_LocalScale.y", new AnimationCurve(new Keyframe[] {
                                            new Keyframe(0, objPBContainer.transform.localScale.y), new Keyframe(1f, objPBContainer.transform.localScale.y * 3) }));
                                        anim_HandHandleR_Fix.SetCurve(conPath, typeof(Transform), "m_LocalScale.z", new AnimationCurve(new Keyframe[] {
                                            new Keyframe(0, objPBContainer.transform.localScale.z), new Keyframe(1f, objPBContainer.transform.localScale.z * 3) }));
                                    }


                                    if (mainBones.bodyBone == HumanBodyBones.Head)
                                    {
                                        VRCHeadChop hc = mainBones.clone.gameObject.AddComponent<VRCHeadChop>();
                                        hc.targetBones = new VRCHeadChop.HeadChopBone[] { new VRCHeadChop.HeadChopBone() { transform = mainBones.clone, scaleFactor = 1f } };
                                        hc.globalScaleFactor = 0f;
                                        //Animationを自動マージさせられないのであらかじめ移動先のパスに変換して作成する
                                        string hcPath = "[APS_TEMP_PATH]" + AnimationUtility.CalculateTransformPath(hc.transform, animator.transform).Replace(objHipsClone.name + "/", objHipsOrg.name + "/"); ;
                                        fixAnimSet.AddInstantaneousCurve(hcPath, typeof(VRCHeadChop), "globalScaleFactor", 0f, 1f, 1f, 1f);
                                    }

                                    //Animationを自動マージさせられないのであらかじめ移動先のパスに変換して作成する
                                    string parentPath = AnimationUtility.CalculateTransformPath(objConst.transform, animator.transform).Replace(objHipsClone.name + "/", objHipsOrg.name + "/");

                                    if (mainBones.isBody())
                                    {
                                        AddParentConstraint(objConst, true, 0f, new VRCConstraintSource() { SourceTransform = mainBones.fix, Weight = 1f });
                                        fixAnimSet.AddInstantaneousCurve(parentPath, typeof(VRCParentConstraint), "GlobalWeight", 0f, 1f, 1f);
                                    }
                                    else
                                    {
                                        AddParentConstraint(objConst, true, 1f,
                                            new VRCConstraintSource() { SourceTransform = mainBones.clone, Weight = 1f },
                                            new VRCConstraintSource() { SourceTransform = mainBones.fix, Weight = 0f });
                                        fixAnimSet.AddInstantaneousCurve(parentPath, typeof(VRCParentConstraint), "Sources.source0.Weight", 1f, 0f, 0f);
                                        fixAnimSet.AddInstantaneousCurve(parentPath, typeof(VRCParentConstraint), "Sources.source1.Weight", 0f, 1f, 1f);
                                    }
                                }
                                break;
                            case HumanBodyBones.LeftEye:
                            case HumanBodyBones.RightEye:

                                if (ap.UnhandleEyes)
                                {
                                    VRCRotationConstraint rcon = AddRotationConstraint(mainBones.original.gameObject, true, 1f, new VRCConstraintSource() { SourceTransform = mainBones.clone, Weight = 1f });
                                    rcon.SolveInLocalSpace = true;
                                }

                                List<Type> constraintTypesEye = new List<Type>
                                {
                                        typeof(AimConstraint), typeof(LookAtConstraint),
                                        typeof(VRCAimConstraint), typeof(VRCLookAtConstraint)
                                };
                                constraintTypesEye.ForEach(t =>
                                {
                                    mainBones.clone.GetComponents(t).ToList().ForEach(con =>
                                    {
                                        Component con1 = CopyComponent(mainBones.original.gameObject, con);
                                        ConvertConstraintSourceTransform(con1, sourceTransform =>
                                        {
                                            if (!sourceTransform.IsChildOf(objHipsClone.transform)) return sourceTransform;

                                            string relPath = AnimationUtility.CalculateTransformPath(sourceTransform, objHipsClone.transform);
                                            return objHipsOrg.transform.Find(relPath);
                                        });
                                        UnityEngine.Object.DestroyImmediate(con);
                                    });
                                });

                                if (!ap.UnhandleEyes)
                                {
                                    Vector3 posEndEye = mainBones.fix.position + new Vector3(0, 0, 0.05f);

                                    GameObject objPBEyeRoot = (mainBones.isBody() ? bodyHandleMan : exHandleMan).GetNextTransform().gameObject;
                                    objPBEyeRoot.name = "Handle_" + mainBones.bodyBone.ToString() + "_Main";
                                    objPBEyeRoot.transform.localScale = new Vector3(mainBones.handleSize, mainBones.handleSize, mainBones.handleSize);
                                    objPBEyeRoot.transform.parent = mainBones.fix.parent;
                                    objPBEyeRoot.transform.position = mainBones.fix.position;
                                    objPBEyeRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, posEndEye - objPBEyeRoot.transform.position);

                                    GameObject objPBEyeEnd = objPBEyeRoot.transform.GetChild(0).gameObject;
                                    objPBEyeEnd.name = "Handle_" + mainBones.bodyBone.ToString() + "_Main_End";
                                    objPBEyeEnd.transform.position = posEndEye;

                                    GameObject objFixedSourceEye = new GameObject();
                                    objFixedSourceEye.name = "FixedSource_" + mainBones.bodyBone.ToString();
                                    objFixedSourceEye.transform.parent = objPBEyeRoot.transform;
                                    objFixedSourceEye.transform.position = mainBones.fix.position;
                                    objFixedSourceEye.transform.rotation = mainBones.fix.rotation;

                                    VRCPhysBone pbHandleEye = objPBEyeRoot.AddComponent<VRCPhysBone>();
                                    pbHandleEye.version = VRCPhysBoneBase.Version.Version_1_1;
                                    pbHandleEye.enabled = false;
                                    pbHandleEye.allowCollision = VRCPhysBoneBase.AdvancedBool.False;
                                    pbHandleEye.immobile = 1f;
                                    pbHandleEye.radius = 1f;
                                    pbHandleEye.resetWhenDisabled = false;
                                    pbHandleEye.pull = 1f;
                                    pbHandleEye.spring = 0f;
                                    pbHandleEye.stiffness = 0f;
                                    pbHandleEye.ignoreTransforms.Add(objFixedSourceEye.transform);


                                    string pbEyePath = AnimationUtility.CalculateTransformPath(objPBEyeRoot.transform, animator.transform);
                                    fixAnimSet.AddInstantaneousCurve(pbEyePath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);

                                    VRCRotationConstraint rcon = AddRotationConstraint(mainBones.original.gameObject, true, 1f,
                                        new VRCConstraintSource() { SourceTransform = mainBones.clone, Weight = 1f },
                                        new VRCConstraintSource() { SourceTransform = objFixedSourceEye.transform, Weight = 0f });

                                    fixAnimSet.AddInstantaneousCurve(originalPath, typeof(VRCRotationConstraint), "Sources.source0.Weight", 1f, 0f, 0f);
                                    fixAnimSet.AddInstantaneousCurve(originalPath, typeof(VRCRotationConstraint), "Sources.source1.Weight", 0f, 1f, 1f);
                                }
                                break;
                            default:
                                AddParentConstraint(mainBones.fix.gameObject, true, 1f, new VRCConstraintSource() { SourceTransform = mainBones.clone, Weight = 1f });

                                fixAnimSet.AddInstantaneousCurve(fixPath, typeof(VRCParentConstraint), "m_Enabled", 1f, 0f, 0f);

                                ModularAvatarBoneProxy maBP1 = mainBones.original.gameObject.AddComponent<ModularAvatarBoneProxy>();
                                maBP1.target = mainBones.clone;

                                AddParentConstraint(mainBones.original.gameObject, true, 0f, new VRCConstraintSource() { SourceTransform = mainBones.fix, Weight = 1f });

                                fixAnimSet.AddInstantaneousCurve(originalPath, typeof(VRCParentConstraint), "GlobalWeight", 0f, 1f, 1f);

                                break;
                        }

                        if (mainBones.isBody())
                        {
                            Transform bodyTarget = mainBones.clone;

                            if (hasAlterBody)
                            {
                                Transform alterBone;
                                if (buildInfo.alterBodyBoneDict.TryGetValue(mainBones.bodyBone, out alterBone) && alterBone != null)
                                {
                                    GameObject convMainAlter = new GameObject { name = alterBone.name + "_Conv_MainAlter" };
                                    convMainAlter.transform.parent = bodyTarget;
                                    convMainAlter.transform.localPosition = Vector3.zero;
                                    convMainAlter.transform.rotation = alterBone.transform.rotation;

                                    if (!alterBody.createHandle)
                                    {
                                        if (mainBones.bodyBone == HumanBodyBones.Hips)
                                        {
                                            VRCParentConstraint con = AddParentConstraint(alterBone.gameObject, true, 1f, new VRCConstraintSource() { SourceTransform = convMainAlter.transform, Weight = 1f });
                                            string conPath = AnimationUtility.CalculateTransformPath(con.transform, animator.transform);
                                            fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCParentConstraint), "m_Enabled", 1f, 0f, 0f);
                                        }
                                        else
                                        {
                                            VRCRotationConstraint con = AddRotationConstraint(alterBone.gameObject, true, 1f, new VRCConstraintSource() { SourceTransform = convMainAlter.transform, Weight = 1f });
                                            string conPath = AnimationUtility.CalculateTransformPath(con.transform, animator.transform);
                                            fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCRotationConstraint), "m_Enabled", 1f, 0f, 0f);
                                        }

                                        if (mainBones.bodyBone == HumanBodyBones.Head)
                                        {
                                            VRCHeadChop hc = alterBone.gameObject.AddComponent<VRCHeadChop>();
                                            hc.globalScaleFactor = 0f;
                                            hc.targetBones = new VRCHeadChop.HeadChopBone[] { new VRCHeadChop.HeadChopBone() { transform = alterBone, scaleFactor = 1f } };

                                            string hcPath = AnimationUtility.CalculateTransformPath(hc.transform, animator.transform);
                                            fixAlterBodyAnimSet.AddInstantaneousCurve(hcPath, typeof(VRCHeadChop), "globalScaleFactor", 0f, 1f, 1f);
                                        }
                                    }
                                    else
                                    {
                                        FixableBoneInfo alterBones = alterBodyFixableBoneDict[alterBone];
                                        tw.WriteProperties(alterBones);

                                        Vector3 posEnd = calcMainHandleEndPos(alterBones, alterBodyFixableBoneDict);
                                        if (posEnd != Vector3.zero)
                                        {

                                            GameObject objPBContainer = new GameObject { name = alterBones.fix.name + "_Alter_Wrap" };
                                            objPBContainer.name += "$" + Math.Abs(objPBContainer.GetInstanceID()).ToString("X");
                                            objPBContainer.transform.parent = alterBones.fix.parent;
                                            objPBContainer.transform.position = alterBones.fix.position;
                                            objPBContainer.transform.rotation = alterBones.fix.rotation;

                                            if (alterBones.isPhysBoneSplitRoot)
                                            {
                                                //PhysBone8階層制限にかからないように、適度に切ってParentConstraintで繋ぐ
                                                GameObject objPBContainerSrc = new GameObject { name = alterBones.fix.name + "_Alter_Src" };
                                                objPBContainerSrc.name += "$" + Math.Abs(objPBContainerSrc.GetInstanceID()).ToString("X");
                                                objPBContainerSrc.transform.parent = alterBones.fix.parent;
                                                objPBContainerSrc.transform.position = alterBones.fix.position;
                                                objPBContainerSrc.transform.rotation = alterBones.fix.rotation;

                                                if (alterBones.bodyBone == HumanBodyBones.Hips)
                                                {
                                                    VRCParentConstraint con = AddParentConstraint(objPBContainerSrc, true, 1f, new VRCConstraintSource() { SourceTransform = convMainAlter.transform, Weight = 1f });
                                                    string conPath = AnimationUtility.CalculateTransformPath(con.transform, animator.transform);
                                                    fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCParentConstraint), "m_Enabled", 1f, 0f, 0f);
                                                }
                                                else
                                                {
                                                    VRCRotationConstraint con = AddRotationConstraint(objPBContainerSrc, true, 1f, new VRCConstraintSource() { SourceTransform = convMainAlter.transform, Weight = 1f });
                                                    string conPath = AnimationUtility.CalculateTransformPath(con.transform, animator.transform);
                                                    fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCRotationConstraint), "m_Enabled", 1f, 0f, 0f);
                                                }

                                                objPBContainer.transform.parent = objFixRoot.transform;
                                                AddParentConstraint(objPBContainer, true, 1f, new VRCConstraintSource() { SourceTransform = objPBContainerSrc.transform, Weight = 1f });
                                            }
                                            else
                                            {
                                                VRCRotationConstraint con = AddRotationConstraint(objPBContainer, true, 1f, new VRCConstraintSource() { SourceTransform = convMainAlter.transform, Weight = 1f });
                                                string conPath = AnimationUtility.CalculateTransformPath(con.transform, animator.transform);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCRotationConstraint), "m_Enabled", 1f, 0f, 0f);
                                            }

                                            GameObject objPBMainRoot = alterBodyHandleMan.GetNextTransform().gameObject;
                                            objPBMainRoot.name = "Handle_" + alterBones.bodyBone.ToString() + "_Main";
                                            objPBMainRoot.transform.localScale = new Vector3(alterBones.handleSize, alterBones.handleSize, alterBones.handleSize);
                                            objPBMainRoot.transform.parent = objPBContainer.transform;
                                            objPBMainRoot.transform.position = alterBones.fix.position;
                                            objPBMainRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, posEnd - objPBMainRoot.transform.position);

                                            GameObject objPBMainEnd = objPBMainRoot.transform.GetChild(0).gameObject;
                                            objPBMainEnd.name = "Handle_" + alterBones.bodyBone.ToString() + "_Main_End";
                                            objPBMainEnd.transform.position = posEnd;

                                            alterBones.fix.parent = objPBMainRoot.transform;

                                            VRCPhysBone pbHandleMain = AddHandlePhysBone(objPBMainRoot, alterBones.fix, isHingeHandle(alterBones.bodyBone));

                                            Vector3 posEnd2 = calcSubHandleEndPos(alterBones);
                                            if (posEnd2 != Vector3.zero)
                                            {
                                                GameObject objPBSubRoot = alterBodyHandleMan.GetNextTransform().gameObject;
                                                objPBSubRoot.name = "Handle_" + alterBones.bodyBone.ToString() + "_Sub";
                                                objPBSubRoot.transform.localScale = new Vector3(alterBones.handleSize, alterBones.handleSize, alterBones.handleSize);
                                                objPBSubRoot.transform.parent = objPBContainer.transform;
                                                objPBSubRoot.transform.position = alterBones.fix.position;
                                                objPBSubRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, posEnd2 - objPBSubRoot.transform.position);

                                                GameObject objPBSubEnd = objPBSubRoot.transform.GetChild(0).gameObject;
                                                objPBSubEnd.name = "Handle_" + alterBones.bodyBone.ToString() + "_Sub_End";
                                                objPBSubEnd.transform.position = posEnd2;

                                                objPBMainRoot.transform.parent = objPBSubRoot.transform;

                                                VRCPhysBone pbHandleSub = AddHandlePhysBone(objPBSubRoot, objPBMainRoot.transform, false);

                                                switch (alterBones.bodyBone)
                                                {
                                                    case HumanBodyBones.Hips:
                                                        GameObject objHipsHandleRoot = objAlterHipsHandle.transform.Find("HipsHandle_Root").gameObject;
                                                        GameObject objHipsHandlePB = objHipsHandleRoot.transform.Find("Bone_Root.000/Bone_Root.001").gameObject;
                                                        GameObject objHipsHandleEnd = objHipsHandleRoot.transform.Find("Bone_Root.000/Bone_Root.001/Bone_Root.002/Bone_End.002/HipsHandle_End/HipsHandle_End.001").gameObject;

                                                        objHipsHandleRoot.transform.parent = objPBContainer.transform;
                                                        objPBSubRoot.transform.parent = objHipsHandleEnd.transform;

                                                        VRCPhysBone pbHipsHandle = objHipsHandlePB.GetComponent<VRCPhysBone>();
                                                        pbHipsHandle.ignoreTransforms.Add(objPBSubRoot.transform);

                                                        string pbHipsHandlePath = AnimationUtility.CalculateTransformPath(objHipsHandlePB.transform, animator.transform);
                                                        fixAlterBodyAnimSet.AddInstantaneousCurve(pbHipsHandlePath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                                        fixAlterBodyAnimSet.AddInstantaneousCurve(pbHipsHandlePath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);
                                                        break;
                                                }

                                                string pbSubPath = AnimationUtility.CalculateTransformPath(objPBSubRoot.transform, animator.transform);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(pbSubPath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(pbSubPath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);
                                            }

                                            string pbMainPath = AnimationUtility.CalculateTransformPath(objPBMainRoot.transform, animator.transform);
                                            fixAlterBodyAnimSet.AddInstantaneousCurve(pbMainPath, typeof(VRCPhysBone), "m_Enabled", 0f, 1f, 1f);
                                            fixAlterBodyAnimSet.AddInstantaneousCurve(pbMainPath, typeof(VRCPhysBone), "allowGrabbing", float.Epsilon, float.Epsilon, 0f);


                                            if (mainBones.bodyBone == HumanBodyBones.Hips)
                                            {
                                                VRCParentConstraint con = AddParentConstraint(alterBones.original.gameObject, true, 1f,
                                                    new VRCConstraintSource() { SourceTransform = convMainAlter.transform, Weight = 1f },
                                                    new VRCConstraintSource() { SourceTransform = alterBones.fix, Weight = 0f });
                                                string conPath = AnimationUtility.CalculateTransformPath(con.transform, animator.transform);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCParentConstraint), "Sources.source0.Weight", 1f, 0f, 0f);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCParentConstraint), "Sources.source1.Weight", 0f, 1f, 1f);
                                            }
                                            else
                                            {
                                                VRCRotationConstraint con = AddRotationConstraint(alterBones.original.gameObject, true, 1f,
                                                    new VRCConstraintSource() { SourceTransform = convMainAlter.transform, Weight = 1f },
                                                    new VRCConstraintSource() { SourceTransform = alterBones.fix, Weight = 0f });
                                                string conPath = AnimationUtility.CalculateTransformPath(con.transform, animator.transform);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCRotationConstraint), "Sources.source0.Weight", 1f, 0f, 0f);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(conPath, typeof(VRCRotationConstraint), "Sources.source1.Weight", 0f, 1f, 1f);
                                            }

                                            if (mainBones.bodyBone == HumanBodyBones.Head)
                                            {
                                                VRCHeadChop hc = alterBone.gameObject.AddComponent<VRCHeadChop>();
                                                hc.globalScaleFactor = 0f;
                                                hc.targetBones = new VRCHeadChop.HeadChopBone[] { new VRCHeadChop.HeadChopBone() { transform = alterBone, scaleFactor = 1f } };

                                                string hcPath = AnimationUtility.CalculateTransformPath(hc.transform, animator.transform);
                                                fixAlterBodyAnimSet.AddInstantaneousCurve(hcPath, typeof(VRCHeadChop), "globalScaleFactor", 0f, 1f, 1f);
                                            }
                                        }
                                    }

                                }
                            }

                            foreach (float size in dictSphereSize.Keys)
                            {
                                List<HumanBodyBones> boneList = dictSphereSize[size];
                                if (boneList.Contains(mainBones.bodyBone))
                                {
                                    GameObject apSBodyBone = bodyMan.GetNextTransform().gameObject;
                                    apSBodyBone.name = "APS_Body_" + mainBones.bodyBone.ToString();
                                    apSBodyBone.transform.parent = bodyTarget;
                                    apSBodyBone.transform.localPosition = Vector3.zero;
                                    SetAbsoluteScale(apSBodyBone, new Vector3(size, size, size));
                                    break;
                                }
                            }
                        }

                        tw.Write($"    ->done\n");

                    }

                    //HumanoidAnimationをHackするため、最後に名前を変える
                    objHipsClone.name = objHipsOrg.name;

                    tw.WriteSeparator();
                    tw.Write("+ Convert VRCConstraint Source (Solve In Local Space)\n");

                    //Solve In Local Space対応
                    //組み換えによってLocal Spaceが変わってしまっているのでなんとかする
                    Dictionary<Transform, Transform> localSpaceSolverDict = new Dictionary<Transform, Transform>();
                    foreach (VRCConstraintBase c in root.GetComponentsInChildren<VRCConstraintBase>(true).Where(c => c.SolveInLocalSpace))
                    {
                        tw.WriteProperties(c);
                        tw.WriteEmptyLine();

                        ConvertConstraintSourceTransform(c, sourceTransform =>
                        {

                            if (!mainBodyFixableBoneDict.ContainsKey(sourceTransform)) return sourceTransform;

                            Transform solverTransform = null;
                            if (!localSpaceSolverDict.TryGetValue(sourceTransform, out solverTransform))
                            {
                                GameObject localSpaceSolver = new GameObject("APS_LocalSpaceSolver_" + sourceTransform.name);
                                localSpaceSolver.transform.SetParent(sourceTransform.parent, false);
                                AddParentConstraint(localSpaceSolver, true, 1f, new VRCConstraintSource() { SourceTransform = sourceTransform, Weight = 1f });

                                solverTransform = localSpaceSolver.transform;
                                localSpaceSolverDict.Add(sourceTransform, solverTransform);

                            }

                            tw.Write($"  Convert VRCConstraint(SolveInLocalSpace) Source: {sourceTransform} -> {solverTransform}");
                            return solverTransform;
                        });

                        tw.WriteEmptyLine();
                    }

                    tw.WriteSeparator();
                    tw.Write("+ Set HandleColor\n");

                    List<SkinnedMeshRenderer> mainHandleMeshes = new List<SkinnedMeshRenderer>();
                    mainHandleMeshes.AddRange(ap.transform.Find("Body_Proxies").GetComponentsInChildren<SkinnedMeshRenderer>(true));
                    mainHandleMeshes.ForEach(m =>
                    {
                        string meshPath = AnimationUtility.CalculateTransformPath(m.transform, animator.transform);
                        fixMainBodyAnimSet.AddInstantaneousCurve(meshPath, typeof(SkinnedMeshRenderer), "material._IsGrayScale", 0f, 0f, 1f);
                    });

                    if (hasAlterBody && alterBody.createHandle)
                    {
                        List<SkinnedMeshRenderer> alterHandleMeshes = new List<SkinnedMeshRenderer>();
                        alterHandleMeshes.AddRange(alterBody.transform.Find("Body_Proxies").GetComponentsInChildren<SkinnedMeshRenderer>(true));
                        alterHandleMeshes.ForEach(m =>
                        {
                            string meshPath = AnimationUtility.CalculateTransformPath(m.transform, animator.transform);
                            fixAlterBodyAnimSet.AddInstantaneousCurve(meshPath, typeof(SkinnedMeshRenderer), "material._IsGrayScale", 0f, 0f, 1f);
                        });
                    }

                    tw.WriteSeparator();
                    tw.Write("+ Convert AnimationClip\n");

                    //アニメーションの制御を移動先に適用。
                    List<RuntimeAnimatorController> fxControllers = new List<RuntimeAnimatorController>();
                    fxControllers.AddRange(descripter.baseAnimationLayers
                        .Where(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null)
                        .Select(item => item.animatorController));
                    fxControllers.AddRange(root.GetComponentsInChildren<ModularAvatarMergeAnimator>(true)
                        .Where(item => item.layerType == VRCAvatarDescriptor.AnimLayerType.FX && item.animator != null && !buildInfo.isAPSParts(item.transform))
                        .Select(item => item.animator));

                    Dictionary<string, HashSet<AnimationClip>> animationClipGroups = fxControllers
                        .SelectMany(controller => controller.animationClips)
                        .GroupBy(clip => AssetDatabase.GetAssetPath(clip))
                        .ToDictionary(group => group.Key, group => new HashSet<AnimationClip>(group));

                    List<Type> convTypes = new List<Type>
                    {
                        typeof(Transform),
                        typeof(ParentConstraint), typeof(PositionConstraint), typeof(RotationConstraint),
                        typeof(AimConstraint), typeof(LookAtConstraint),
                        typeof(VRCParentConstraint), typeof(VRCPositionConstraint), typeof(VRCRotationConstraint),
                        typeof(VRCAimConstraint), typeof(VRCLookAtConstraint)
                    };

                    foreach (KeyValuePair<string, HashSet<AnimationClip>> group in animationClipGroups)
                    {
                        string clipPath = group.Key;
                        tw.Write($"AnimationClipPath: {clipPath}");

                        foreach (AnimationClip clip in group.Value)
                        {
                            bool isTempClip = ctx.AssetSaver.IsTemporaryAsset(clip);
                            tw.Write($" AnimationClip ({(!isTempClip ? "Persistent" : "Temporary")}): {clip.name}");

                            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
                            foreach (EditorCurveBinding binding in bindings)
                            {
                                tw.WriteProperties(binding);

                                //過去バージョンで作った不正なBindingがあれば消す
                                if (!isTempClip && (binding.path.EndsWith("/APS_PB") || binding.path.Contains("[APS_TEMP_PATH]")))
                                {
                                    tw.Write($"  ->Remove Remaining Incorrect Binding");

                                    AnimationUtility.SetEditorCurve(clip, binding, null);
                                    continue;
                                }

                                string newPath = binding.path;

                                //PhysBoneは制御用にAPS_PBに移したので、アニメーションのパスを合わせる
                                if (binding.type == typeof(VRCPhysBone))
                                {
                                    tw.Write($"  ->is PhysBone Binding Type");

                                    newPath = newPath + "/APS_PB";
                                }
                                else if (convTypes.Contains(binding.type))
                                {
                                    tw.Write($"  ->is Convert Binding Type");

                                    Transform target = root.transform.Find(newPath);
                                    tw.WriteNamedValue("BindingTransform", target);

                                    if (target != null && mainBodyFixableBoneDict.ContainsKey(target) && !mainBodyFixableBoneDict[target].isBody())
                                    {
                                        BoneSet rootBoneSet = mainBodyRootBoneGroup.GetRootBoneSet(target);
                                        if (rootBoneSet != null)
                                        {
                                            BoneSet boneSet = rootBoneSet.GetRelativeBoneSet(target);

                                            bool isArmatureBone = rootBoneSet.original == mainBodyRootBoneGroup.armatureRootBoneSet.original;
                                            newPath = (isArmatureBone ? "[APS_TEMP_PATH]" : "") + AnimationUtility.CalculateTransformPath(boneSet.clone, root.transform);
                                        }
                                    }
                                }

                                if (binding.path != newPath)
                                {
                                    EditorCurveBinding newBinding = new EditorCurveBinding();
                                    newBinding.path = newPath;
                                    newBinding.type = binding.type;
                                    newBinding.propertyName = binding.propertyName;

                                    tw.Write($"  Convert {binding.type.Name} Binding: {binding.propertyName} - {binding.path} -> {newBinding.path}");
                                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                                    AnimationUtility.SetEditorCurve(clip, binding, null);
                                    AnimationUtility.SetEditorCurve(clip, newBinding, curve);

                                    if (!isTempClip)
                                    {
                                        restorationEditorCurveSetters.Add(new EditorCurveSetter() { clip = clip, binding = newBinding, curve = curve });
                                    }
                                }

                                tw.WriteEmptyLine();
                            }
                        }
                    }

                    foreach (GameObject objPB in fixPhysBoneObjects)
                    {
                        //先の処理で組み替えとかしているのでここでアニメーション作成
                        string pbPath = AnimationUtility.CalculateTransformPath(objPB.transform, animator.transform);
                        if (hasAlterBody && objPB.transform.IsChildOf(buildInfo.alterBodyAvatarContainer))
                        {
                            fixAlterPBAnimSet.AddInstantaneousCurve(pbPath, typeof(GameObject), "m_IsActive", 1f, 0f);
                        }
                        else
                        {
                            fixMainPBAnimSet.AddInstantaneousCurve(pbPath, typeof(GameObject), "m_IsActive", 1f, 0f);
                        }
                    }

                    bodyMan.Close();
                    bodyHandleMan.Close();
                    alterBodyHandleMan.Close();
                    exHandleMan.Close();
                    //movehHandleMan.Close();

                    //AvatarOptimizerに未登録コンポーネントとして検知される前に消す
                    root.GetComponentsInChildren<Component>(true)
                        .Where(c => ApsTypes.Any(t => t.IsAssignableFrom(c.GetType())))
                        .ToList().ForEach(c => GameObject.DestroyImmediate(c));

                    tw.WriteSeparator();
                    tw.Write("AvatarPoseSystem Trace Info End -Generating");
                    tw.Close();
                }
                catch (Exception ex)
                {
                    tw.WriteSeparator();
                    tw.Write(ex.ToString());
                    tw.WriteSeparator();
                    tw.Write("AvatarPoseSystem Trace Info Abend -Generating");
                    tw.Close();
                    throw new InvalidOperationException("GeneratingフェーズでAvatarPoseSystemの処理に失敗しました。詳細はトレースログを確認してください。", ex);
                }
            });

            InPhase(BuildPhase.Transforming)
            .AfterPlugin("nadena.dev.modular-avatar")
            .Run("AvatarPoseSystem", ctx =>
            {
                if (restorationEditorCurveSetters.Count > 0)
                {
                    //Generating Phaseで編集した一時アセットでないAnimationClipを戻す
                    restorationEditorCurveSetters.ForEach(item => AnimationUtility.SetEditorCurve(item.clip, item.binding, item.curve));
                    restorationEditorCurveSetters.Clear();
                }

                if (!apsBuildCache.ContainsKey(ctx)) return;
                APSBuildInfo buildInfo = apsBuildCache[ctx];

                GameObject root = ctx.AvatarRootObject;
                VRCAvatarDescriptor descripter = root.GetComponent<VRCAvatarDescriptor>();
                Animator animator = root.GetComponent<Animator>();

                if (buildInfo.enableTrace)
                {
                    string zeroFactoryLogsPath = Path.Combine(Application.dataPath, "ZeroFactory", "AvatarPoseSystem", "__trace");
                    string twPath = Path.Combine(zeroFactoryLogsPath, $"AvatarPoseSystem_Trace_{ctx.AvatarRootObject.name}.log");
                    tw.Open(twPath, root.transform, true);
                    tw.Write($"AvatarPoseSystem Trace Info: v{Version} -Transforming");
                    tw.WriteSeparator();
                }

                try
                {
                    if (buildInfo.alterBodyTransform != null)
                    {
                        tw.WriteSeparator();
                        tw.Write("+ Check AlterBody RootBone\n");

                        buildInfo.alterBodyTransform.GetComponentsInChildren<SkinnedMeshRenderer>()
                            .ToList()
                            .ForEach(comp =>
                            {
                                if (comp.rootBone == null)
                                {
                                    tw.Write($"  ->RootBone Not Set: {AnimationUtility.CalculateTransformPath(comp.transform, root.transform)}");
                                    comp.rootBone = buildInfo.alterBodyBoneDict[HumanBodyBones.Hips];
                                }
                            });
                    }

                    tw.WriteSeparator();
                    tw.Write("+ Fix Animation Path\n");

                    List<RuntimeAnimatorController> fxControllers = new List<RuntimeAnimatorController>();
                    fxControllers.AddRange(descripter.baseAnimationLayers
                        .Where(item => item.type == VRCAvatarDescriptor.AnimLayerType.FX && item.animatorController != null)
                        .Select(item => item.animatorController));

                    Dictionary<string, HashSet<AnimationClip>> animationClipGroups = fxControllers
                        .SelectMany(controller => controller.animationClips)
                        .GroupBy(clip => AssetDatabase.GetAssetPath(clip))
                        .ToDictionary(group => group.Key, group => new HashSet<AnimationClip>(group));

                    foreach (KeyValuePair<string, HashSet<AnimationClip>> group in animationClipGroups)
                    {
                        string clipPath = group.Key;
                        tw.Write($"AnimationClipPath: {clipPath}");

                        foreach (AnimationClip clip in group.Value)
                        {
                            bool isTempClip = ctx.AssetSaver.IsTemporaryAsset(clip);
                            tw.Write($" AnimationClip ({(!isTempClip ? "Persistent" : "Temporary")}): {clip.name}");

                            if (!isTempClip) continue;

                            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
                            foreach (EditorCurveBinding binding in bindings)
                            {
                                tw.WriteProperties(binding);

                                string newPath = binding.path;


                                if (Regex.IsMatch(newPath, "/Toe_Helper_[LR]/"))
                                {
                                    // VVVVVV Toe Addon 仮対応 VVVVVV
                                    tw.Write($"  ->is Toe Addon Binding");
                                    newPath = Regex.Replace(newPath, "/([^/]+)/(Toe_Helper_[LR])/", "/$1/$1/$2/");
                                    // AAAAAA Toe Addon 仮対応 AAAAAA
                                }

                                if (newPath.Contains("[APS_TEMP_PATH]"))
                                {
                                    tw.Write($"  ->is Temp Path Binding");
                                    newPath = newPath.Replace("[APS_TEMP_PATH]", "");
                                }

                                if (binding.path != newPath)
                                {
                                    EditorCurveBinding newBinding = new EditorCurveBinding();

                                    newBinding.path = newPath;
                                    newBinding.type = binding.type;
                                    newBinding.propertyName = binding.propertyName;

                                    tw.Write($"  Convert {binding.type.Name} Binding: {binding.propertyName} - {binding.path} -> {newBinding.path}");
                                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                                    AnimationUtility.SetEditorCurve(clip, binding, null);
                                    AnimationUtility.SetEditorCurve(clip, newBinding, curve);
                                }

                                tw.WriteEmptyLine();
                            }
                        }
                    }

                    tw.WriteSeparator();
                    tw.Write("AvatarPoseSystem Trace Info End -Transforming");
                    tw.Close();
                }
                catch (Exception ex)
                {
                    tw.WriteSeparator();
                    tw.Write(ex.ToString());
                    tw.WriteSeparator();
                    tw.Write("AvatarPoseSystem Trace Info Abend -Transforming");
                    tw.Close();
                    throw new InvalidOperationException("TransformingフェーズでAvatarPoseSystemの処理に失敗しました。詳細はトレースログを確認してください。", ex);
                }
            });
        }

        private void LogDebug(string msg, UnityEngine.Object content = null, params (string, object)[] infos)
        {
            Log(LogType.Log, msg, content, infos);
        }

        private void LogWarning(string msg, UnityEngine.Object content = null, params (string, object)[] infos)
        {
            Log(LogType.Warning, msg, content, infos);
        }

        private void LogError(string msg, UnityEngine.Object content = null, params (string, object)[] infos)
        {
            Log(LogType.Error, msg, content, infos);
        }

        private void Log(LogType logType, string msg, UnityEngine.Object content = null, params (string, object)[] infos)
        {
            string formatedMsg = FormatLog(msg, infos);
            Debug.LogFormat(logType, LogOption.None, content, formatedMsg);
            tw?.Write("[" + logType.ToString() + "]: " + formatedMsg);
        }

        private string FormatLog(string msg, (string, object)[] infos)
        {
            string log = DisplayName + "[ver:" + Version + "]::" + msg;
            if (infos.Length > 0)
            {
                log += "\n" + String.Join("\n", infos.Select(kv => kv.Item1 + " = " + kv.Item2));
            }
            return log;
        }

        private bool IsUnfixObject(GameObject obj, AvatarPoseSystem ap)
        {
            for (var current = obj.transform; current != null; current = current.parent)
            {
                if (ap.UnfixObjects.Contains(current.gameObject))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsUnfixPhysBone(Component pb, AvatarPoseSystem ap, List<GameObject> listUnfixObjectFromPath)
        {
            GameObject obj = pb.gameObject;
            if (ap.UnfixPhysBones.Contains(obj)
                || ap.UnfixPhysBonesWithChildren.Contains(obj)
                || ap.UnfixObjects.Contains(obj)
                || listUnfixObjectFromPath.Contains(obj))
            {
                return true;
            }

            while (true)
            {
                obj = obj.transform.parent?.gameObject;
                if (obj == null) break;

                if (ap.gameObject == obj
                    || ap.UnfixPhysBonesWithChildren.Contains(obj)
                    || ap.UnfixObjects.Contains(obj)
                    || listUnfixObjectFromPath.Contains(obj))
                {
                    return true;
                }
            }

            return false;
        }

        private void ResolveUnfixObject(GameObject obj, Dictionary<Transform, FixableBoneInfo> fixableBoneDict)
        {

            tw.WriteNamedValue("ResolveUnfixObject obj", obj);

            if (fixableBoneDict.ContainsKey(obj.transform.parent))
            {
                tw.Write(" -> Contains in fixableBoneDict");
                tw.WriteProperties(fixableBoneDict[obj.transform.parent]);
                ModularAvatarBoneProxy maBP = obj.transform.gameObject.AddComponent<ModularAvatarBoneProxy>();
                maBP.target = fixableBoneDict[obj.transform.parent].clone;
            }

            List<Component> constraints = new List<Component>();
            constraints.AddRange(obj.GetComponents(typeof(IConstraint)));
            constraints.AddRange(obj.GetComponents(typeof(VRCConstraintBase)));
            constraints.ForEach(con =>
            {
                ConvertConstraintSourceTransform(con, sourceTransform =>
                {
                    if (!fixableBoneDict.ContainsKey(sourceTransform)) return sourceTransform;

                    return fixableBoneDict[sourceTransform].clone;
                });
            });


            foreach (ModularAvatarBoneProxy c in obj.GetComponentsInChildren<ModularAvatarBoneProxy>(true))
            {
                if (c.target != null && fixableBoneDict.ContainsKey(c.target))
                {
                    c.target = fixableBoneDict[c.target].clone;
                }
            }
        }

        private Transform FindEndBone(Transform parent, string name)
        {
            foreach (Transform item in parent)
            {
                if (item.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return item;
                }
            }

            return null;
        }

        private Boolean IsUsedObject(GameObject obj, IEnumerable<GameObject> useObjectss)
        {
            if (useObjectss.Contains(obj))
            {
                return true;
            }
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                if (IsUsedObject(obj.transform.GetChild(i).gameObject, useObjectss))
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject CloneTransform(GameObject src, string name = null)
        {
            GameObject dest = new GameObject();
            dest.name = name != null ? name : src.name;
            dest.transform.position = src.transform.position;
            dest.transform.rotation = src.transform.rotation;
            dest.transform.localScale = src.transform.lossyScale;
            for (int i = 0; i < src.transform.childCount; i++)
            {
                CloneTransform(src.transform.GetChild(i).gameObject).transform.parent = dest.transform;
            }

            return dest;

        }
        private void CopyTransform(Transform target, Transform source)
        {
            target.position = source.position;
            target.rotation = source.rotation;
            target.localScale = new Vector3(source.lossyScale.x / target.parent.lossyScale.x, source.lossyScale.y / target.parent.lossyScale.y, source.lossyScale.z / target.parent.lossyScale.z);
        }

        private VRCParentConstraint AddParentConstraint(GameObject target, Boolean enabled, float weight, params VRCConstraintSource[] sources)
        {
            VRCParentConstraint c = AddConstraint<VRCParentConstraint>(target, false, weight, sources);
            c.PositionAtRest = Vector3.zero;
            c.RotationAtRest = Vector3.zero;
            c.Locked = true;
            c.enabled = enabled;
            return c;
        }

        private VRCRotationConstraint AddRotationConstraint(GameObject target, Boolean enabled, float weight, params VRCConstraintSource[] sources)
        {
            VRCRotationConstraint c = AddConstraint<VRCRotationConstraint>(target, false, weight, sources);
            c.RotationAtRest = Vector3.zero;
            c.Locked = true;
            c.enabled = enabled;
            return c;
        }

        private VRCPositionConstraint AddPositionConstraint(GameObject target, Boolean enabled, float weight, params VRCConstraintSource[] sources)
        {
            VRCPositionConstraint c = AddConstraint<VRCPositionConstraint>(target, false, weight, sources);
            c.PositionAtRest = Vector3.zero;
            c.Locked = true;
            c.enabled = enabled;
            return c;
        }

        private VRCScaleConstraint AddScaleConstraint(GameObject target, Boolean enabled, float weight, params VRCConstraintSource[] sources)
        {
            VRCScaleConstraint c = AddConstraint<VRCScaleConstraint>(target, false, weight, sources);
            c.ScaleAtRest = Vector3.one;
            c.Locked = true;
            c.enabled = enabled;
            return c;
        }

        private static T AddConstraint<T>(GameObject target, Boolean enabled, float weight, params VRCConstraintSource[] sources) where T : VRCConstraintBase
        {
            T c = target.AddComponent<T>();
            c.enabled = enabled;
            c.GlobalWeight = weight;
            foreach (var source in sources)
            {
                c.Sources.Add(source);
            }
            c.Locked = false;
            c.IsActive = true;

            return c;
        }

        private T CopyComponent<T>(GameObject obj, T src) where T : Component
        {
            Component dest = obj.AddComponent(src.GetType());
            string originalName = obj.name; // GameObjectの元の名前を保持
            // EditorUtility.CopySerialized を使用することで、
            // ParentConstraint.constraintActive のようなネイティブ側の状態も
            // 含めてコンポーネントの値をコピーします。
            EditorUtility.CopySerialized(src, dest);
            obj.name = originalName; // GameObjectの名前を元に戻す

            //個別のクラスへの対応
            if (src is IConstraint)
            {
                IConstraint constraintSrc = (IConstraint)src;
                IConstraint constraintDest = (IConstraint)dest;
                List<ConstraintSource> srcs = new List<ConstraintSource>();
                constraintSrc.GetSources(srcs);
                constraintDest.SetSources(srcs);
            }

            return dest as T;
        }

        private void ConvertTransform(Component c, Transform src, Transform dest)
        {
            Type type = c.GetType();

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.IsStatic || field.IsNotSerialized) continue;
                if (field.FieldType != typeof(Transform)) continue;

                Transform trSrc = (Transform)field.GetValue(c);
                //if (Util.IsSelfOrAncestor(trSrc, src))
                if (trSrc.IsChildOf(src))
                {
                    string trPath = AnimationUtility.CalculateTransformPath(trSrc, src);
                    Transform trDest = trPath == "" ? dest : dest.Find(trPath);
                    field.SetValue(c, trDest);
                }
            }

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;
                if (prop.DeclaringType != typeof(Transform)) continue;

                Transform trSrc = (Transform)prop.GetValue(c);
                if (trSrc.IsChildOf(src))
                {
                    string trPath = AnimationUtility.CalculateTransformPath(trSrc, src);
                    Transform trDest = trPath == "" ? dest : dest.Find(trPath);
                    prop.SetValue(c, trDest);
                }
            }
        }

        private void SetAbsoluteScale(GameObject obj, Vector3 scale)
        {
            Vector3 parentLossyScale = obj.transform.parent.lossyScale;
            obj.transform.localScale = new Vector3(scale.x / parentLossyScale.x, scale.y / parentLossyScale.y, scale.z / parentLossyScale.z);
        }

        private FixableBoneInfo CreateBoneInfo(BoneSet boneSet, HumanBodyBones bodyBone, float handleSize, bool isDualHandle, bool isMovableHandle, bool isPhysBoneSplitRoot, Vector3 positionOffset, Vector3 rotationOffset)
        {
            return new FixableBoneInfo()
            {
                bodyBone = bodyBone,
                original = boneSet.original,
                clone = boneSet.clone,
                fix = boneSet.fix,
                handleSize = handleSize,
                isDualHandle = isDualHandle,
                isPhysBoneSplitRoot = isPhysBoneSplitRoot,
                isPropHandle = isMovableHandle,
                positionOffset = positionOffset,
                rotationOffset = rotationOffset,
            };
        }

        private AnimatorControllerLayer AddAnimatorControllerLayer(AnimatorController controller, string name)
        {

            AnimatorStateMachine stateMachine = new AnimatorStateMachine()
            {
                name = name,
                hideFlags = HideFlags.HideInHierarchy
            };

            AnimatorControllerLayer layer = new AnimatorControllerLayer()
            {
                name = stateMachine.name,
                defaultWeight = 1,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);

            return layer;
        }

        private AnimatorState AddMotionAnimationState(AnimatorStateMachine stateMachine, string name, Vector3 position, Motion motion)
        {
            AnimatorState state = stateMachine.AddState(name, position);
            state.motion = motion;
            state.writeDefaultValues = false;
            state.speed = 1;

            return state;
        }

        private AnimatorStateTransition AddTransition(UnityEditor.Animations.AnimatorState from, UnityEditor.Animations.AnimatorState to, params (AnimatorConditionMode mode, float threshold, string parameter)[] conditions)
        {
            return AddTransition(from, to, 0, conditions);
        }

        private AnimatorStateTransition AddTransition(UnityEditor.Animations.AnimatorState from, UnityEditor.Animations.AnimatorState to, float exitTime, params (AnimatorConditionMode mode, float threshold, string parameter)[] conditions)
        {
            AnimatorStateTransition tran = from.AddTransition(to);
            tran.hasExitTime = exitTime > 0;
            tran.exitTime = exitTime;
            tran.duration = 0;
            foreach (var cond in conditions)
            {
                tran.AddCondition(cond.mode, cond.threshold, cond.parameter);
            }

            return tran;
        }

        private AnimatorStateTransition AddTransitionFromAniState(AnimatorStateMachine stateMachine, UnityEditor.Animations.AnimatorState to, params (AnimatorConditionMode mode, float threshold, string parameter)[] conditions)
        {
            AnimatorStateTransition tran = stateMachine.AddAnyStateTransition(to);
            tran.hasExitTime = false;
            tran.exitTime = 0;
            tran.duration = 0;
            tran.canTransitionToSelf = false;
            foreach (var cond in conditions)
            {
                tran.AddCondition(cond.mode, cond.threshold, cond.parameter);
            }
            return tran;
        }

        private List<Transform> FindMergeArmatureDifferenceTransforms(Transform mergeSrcTransform, Transform mergeDestTransform, string prefix, string suffix)
        {
            //差分のTransformのListを再帰して作成する
            List<Transform> differenceTransforms = new List<Transform>();
            if (mergeDestTransform == null)
            {
                differenceTransforms.Add(mergeSrcTransform);
            }
            else
            {
                for (int i = 0; i < mergeSrcTransform.transform.childCount; i++)
                {
                    Transform mergeSrcChildTransform = mergeSrcTransform.GetChild(i);
                    string targetObjectName = RemovePrefixAndSuffix(mergeSrcChildTransform.name, prefix, suffix);
                    Transform mergeDestChildTransform = mergeDestTransform.Find(targetObjectName);
                    differenceTransforms.AddRange(FindMergeArmatureDifferenceTransforms(mergeSrcChildTransform, mergeDestChildTransform, prefix, suffix));
                }
            }
            return differenceTransforms;
        }

        private string RemovePrefixAndSuffix(string path, string prefix, string suffix)
        {
            if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
            {
                return path;
            }

            string pattern = $"^{Regex.Escape(prefix)}(.*){Regex.Escape(suffix)}$";
            Regex regex = new Regex(pattern);

            return string.Join("/", path.Split('/').Select(part =>
            {
                Match match = regex.Match(part);
                return match.Success ? match.Groups[1].Value : part;
            }));
        }

        private T GetNearestComponent<T>(Transform target) where T : Component
        {
            if (target == null) return null;
            T c = target.GetComponent<T>();
            if (c != null) return c;
            return GetNearestComponent<T>(target.parent);
        }

        private Vector3 calcMainHandleEndPos(FixableBoneInfo bones, Dictionary<Transform, FixableBoneInfo> fixableBoneDict)
        {

            if (bones.isPropHandle)
            {
                Quaternion totalRotation = bones.fix.rotation * Quaternion.Euler(bones.rotationOffset);
                return bones.fix.position + totalRotation * new Vector3(0, 0, 0.1f);
            }

            Vector3 posEnd = Vector3.zero;
            Vector3 posChild = Vector3.zero;
            if (bones.fix.childCount > 0)
            {
                posChild = bones.fix.GetChild(0).position;
            }

            FixableBoneInfo endBoneInfo = null;
            switch (bones.bodyBone)
            {
                case HumanBodyBones.Hips:
                    endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == HumanBodyBones.Spine);
                    posEnd = endBoneInfo != null ? endBoneInfo.fix.position : posChild;
                    break;
                case HumanBodyBones.LeftShoulder:
                case HumanBodyBones.RightShoulder:
                case HumanBodyBones.LeftUpperLeg:
                case HumanBodyBones.RightUpperLeg:
                case HumanBodyBones.LeftLowerLeg:
                case HumanBodyBones.RightLowerLeg:
                case HumanBodyBones.LeftUpperArm:
                case HumanBodyBones.RightUpperArm:
                case HumanBodyBones.LeftLowerArm:
                case HumanBodyBones.RightLowerArm:
                    endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == bones.bodyBone + 2);
                    posEnd = endBoneInfo != null ? endBoneInfo.fix.position : posChild;
                    break;
                case HumanBodyBones.LeftHand:
                    endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == HumanBodyBones.LeftMiddleProximal);
                    posEnd = endBoneInfo != null ? endBoneInfo.fix.position : posChild;
                    break;
                case HumanBodyBones.RightHand:
                    endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == HumanBodyBones.RightMiddleProximal);
                    posEnd = endBoneInfo != null ? endBoneInfo.fix.position : posChild;
                    break;
                case HumanBodyBones.LeftFoot:
                    endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == HumanBodyBones.LeftToes);
                    posEnd = endBoneInfo != null ? endBoneInfo.fix.position : bones.fix.position + new Vector3(0f, -0.05f, 0.1f);
                    break;
                case HumanBodyBones.RightFoot:
                    endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == HumanBodyBones.RightToes);
                    posEnd = endBoneInfo != null ? endBoneInfo.fix.position : bones.fix.position + new Vector3(0f, -0.05f, 0.1f);
                    break;
                case HumanBodyBones.Spine:
                case HumanBodyBones.Chest:
                case HumanBodyBones.Neck:
                    endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == bones.bodyBone + 1);
                    posEnd = endBoneInfo != null ? endBoneInfo.fix.position : posChild;
                    break;
                case HumanBodyBones.Head:
                    posEnd = bones.fix.position + new Vector3(0f, 0.15f, 0f);
                    break;
                case HumanBodyBones.LeftEye:
                case HumanBodyBones.RightEye:
                    posEnd = bones.fix.position + new Vector3(0, 0, 0.05f);
                    break;
                case >= HumanBodyBones.LeftThumbProximal and <= HumanBodyBones.RightLittleDistal: //FingerBones
                    if (bones.bodyBone == HumanBodyBones.LeftThumbDistal || bones.bodyBone == HumanBodyBones.RightThumbDistal
                    || bones.bodyBone == HumanBodyBones.LeftIndexDistal || bones.bodyBone == HumanBodyBones.RightIndexDistal
                    || bones.bodyBone == HumanBodyBones.LeftMiddleDistal || bones.bodyBone == HumanBodyBones.RightMiddleDistal
                    || bones.bodyBone == HumanBodyBones.LeftRingDistal || bones.bodyBone == HumanBodyBones.RightRingDistal
                    || bones.bodyBone == HumanBodyBones.LeftLittleDistal || bones.bodyBone == HumanBodyBones.RightLittleDistal)
                    {
                        //指先
                        Transform tfFixDistalEnd = FindEndBone(bones.fix, "_end");
                        endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == bones.bodyBone - 1);
                        posEnd = tfFixDistalEnd != null ?
                            tfFixDistalEnd.position : bones.fix.position + (bones.fix.position - endBoneInfo.fix.position);
                    }
                    else
                    {
                        //指先以外
                        endBoneInfo = fixableBoneDict.Values.FirstOrDefault(b => b.bodyBone == bones.bodyBone + 1);
                        posEnd = endBoneInfo != null ? endBoneInfo.fix.position : posChild;
                    }
                    break;
                case HumanBodyBones.LastBone:
                    if (posChild != Vector3.zero)
                    {
                        posEnd = posChild;
                    }
                    else
                    {
                        if (bones.fix.localPosition != Vector3.zero)
                        {
                            posEnd = bones.fix.position + (bones.fix.position - bones.fix.parent.position);
                        }
                        else
                        {
                            posEnd = bones.fix.position + new Vector3(0f, 0.1f, 0f);
                        }
                    }
                    break;
            }

            return posEnd;
        }

        private Vector3 calcSubHandleEndPos(FixableBoneInfo bones)
        {
            Vector3 posEnd2 = Vector3.zero;
            if (bones.isPropHandle)
            {
                Quaternion totalRotation = bones.fix.rotation * Quaternion.Euler(bones.rotationOffset);
                posEnd2 = bones.fix.position + totalRotation * new Vector3(0, 0.05f, 0);
                return posEnd2;
            }

            if (bones.isDualHandle)
            {
                switch (bones.bodyBone)
                {
                    case HumanBodyBones.LeftFoot:
                    case HumanBodyBones.RightFoot:
                        posEnd2 = bones.fix.position + new Vector3(0f, -0.1f, 0f);
                        break;
                    default:
                        posEnd2 = bones.fix.position + new Vector3(0, 0, 0.1f);
                        break;
                }
            }

            return posEnd2;
        }

        private bool isHingeHandle(HumanBodyBones bodyBone)
        {
            switch (bodyBone)
            {
                case HumanBodyBones.LeftIndexIntermediate:
                case HumanBodyBones.LeftMiddleIntermediate:
                case HumanBodyBones.LeftRingIntermediate:
                case HumanBodyBones.LeftLittleIntermediate:
                case HumanBodyBones.RightIndexIntermediate:
                case HumanBodyBones.RightMiddleIntermediate:
                case HumanBodyBones.RightRingIntermediate:
                case HumanBodyBones.RightLittleIntermediate:
                case HumanBodyBones.LeftIndexDistal:
                case HumanBodyBones.LeftMiddleDistal:
                case HumanBodyBones.LeftRingDistal:
                case HumanBodyBones.LeftLittleDistal:
                case HumanBodyBones.RightIndexDistal:
                case HumanBodyBones.RightMiddleDistal:
                case HumanBodyBones.RightRingDistal:
                case HumanBodyBones.RightLittleDistal:
                    return true;
            }

            return false;
        }

        private VRCPhysBone AddHandlePhysBone(GameObject targetObject, Transform ignoreTransforms, bool isHinge)
        {
            VRCPhysBone handlePhysBone = targetObject.AddComponent<VRCPhysBone>();
            handlePhysBone.version = VRCPhysBoneBase.Version.Version_1_1;
            handlePhysBone.enabled = false;
            handlePhysBone.allowCollision = VRCPhysBoneBase.AdvancedBool.False;
            handlePhysBone.immobile = 1f;
            handlePhysBone.radius = 1f;
            //handlePhysBone.resetWhenDisabled = false;
            handlePhysBone.resetWhenDisabled = true;
            handlePhysBone.pull = 1f;
            handlePhysBone.spring = 0f;
            handlePhysBone.stiffness = 0f;
            handlePhysBone.ignoreTransforms.Add(ignoreTransforms);

            if (isHinge)
            {
                handlePhysBone.limitType = VRCPhysBoneBase.LimitType.Hinge;
                handlePhysBone.maxAngleX = 180f;
                handlePhysBone.limitRotation = new Vector3(0f, 90f, 0f);
            }

            return handlePhysBone;
        }

        private void ConvertConstraintSourceTransform(Component con, Func<Transform, Transform> f)
        {
            if (con is IConstraint)
            {
                IConstraint c = con as IConstraint;
                for (int i = 0; i < c.sourceCount; i++)
                {
                    ConstraintSource src = c.GetSource(i);
                    if (src.sourceTransform != null)
                    {
                        src.sourceTransform = f(src.sourceTransform);
                        c.SetSource(i, src); //反映するには再設定が必要
                    }
                }
            }
            if (con is VRCConstraintBase)
            {
                VRCConstraintBase c = con as VRCConstraintBase;
                for (int i = 0; i < c.Sources.Count; i++)
                {
                    VRCConstraintSource src = c.Sources[i];
                    if (src.SourceTransform != null)
                    {
                        src.SourceTransform = f(src.SourceTransform);
                        c.Sources[i] = src; //反映するには再設定が必要
                    }
                }
            }
        }

        private class APSBuildInfo
        {
            public bool enableTrace = false;
            public Transform apsTransform = null;
            public Transform alterBodyTransform = null;
            public Transform alterBodyAvatarContainer = null;
            public Transform alterBodyArmatureContainer = null;
            public Dictionary<HumanBodyBones, Transform> alterBodyBoneDict = new Dictionary<HumanBodyBones, Transform>();

            public bool isAPSParts(Transform target)
            {
                if (target.IsChildOf(apsTransform)) return true;
                if (alterBodyTransform != null && target.IsChildOf(alterBodyTransform) && !target.IsChildOf(alterBodyAvatarContainer)) return true;

                return false;
            }

        }

        private class FixableBoneInfo
        {
            public HumanBodyBones bodyBone = HumanBodyBones.LastBone;
            public Transform original;
            public Transform clone;
            public Transform fix;
            public float handleSize = 0.01f;
            public bool isDualHandle = false;
            public bool isPhysBoneSplitRoot = false;
            public bool isPropHandle = false;
            public Vector3 positionOffset = Vector3.zero;
            public Vector3 rotationOffset = Vector3.zero;

            public bool isBody()
            {
                return bodyBone != HumanBodyBones.LastBone;
            }
        }

        private class BoneSet
        {
            public Transform original;
            public Transform clone;
            public Transform fix;

            public BoneSet GetRelativeBoneSet(Transform t)
            {
                if (original == t) return this;
                string relPath = AnimationUtility.CalculateTransformPath(t, original);
                return new BoneSet() { original = t, clone = clone?.Find(relPath), fix = fix?.Find(relPath) };
            }
        }

        private class RootBoneSetGroup
        {
            public BoneSet armatureRootBoneSet = null;
            public Dictionary<Transform, BoneSet> boneProxyRootBoneSets = new Dictionary<Transform, BoneSet>();
            public Dictionary<Transform, BoneSet> mergeArmatureRootBoneSets = new Dictionary<Transform, BoneSet>();
            public BoneSet GetRootBoneSet(Transform targetTransform)
            {
                Transform tmpTransform = targetTransform;
                while (tmpTransform != null)
                {
                    if (armatureRootBoneSet != null && armatureRootBoneSet.original == tmpTransform) return armatureRootBoneSet;
                    if (boneProxyRootBoneSets.ContainsKey(tmpTransform)) return boneProxyRootBoneSets[tmpTransform];
                    if (mergeArmatureRootBoneSets.ContainsKey(tmpTransform)) return mergeArmatureRootBoneSets[tmpTransform];
                    tmpTransform = tmpTransform.parent;
                }

                return null;
            }
            public BoneSet GetBoneSet(Transform t)
            {
                return GetRootBoneSet(t)?.GetRelativeBoneSet(t);
            }
        }

        private class IndexedTransformManager
        {
            public Transform rootTransform;
            public Transform meshTransform;
            public string bonePathFormat;
            public string meshPath = "";

            private GameObject tmpObj;
            private int idx = 0;

            public Transform GetNextTransform()
            {
                if (tmpObj == null)
                {
                    tmpObj = GameObject.Instantiate(rootTransform.gameObject);
                }
                if (meshTransform == null)
                {
                    meshTransform = rootTransform.Find(meshPath);
                }

                Transform t = rootTransform.Find(String.Format(bonePathFormat, idx));
                if (t == null)
                {
                    //最後まで取ったら複製する
                    idx = 0;
                    Transform newRootTransform = GameObject.Instantiate(tmpObj).transform;
                    newRootTransform.parent = rootTransform.parent;

                    Transform newMeshTransform = newRootTransform.Find(meshPath);
                    newMeshTransform.parent = meshTransform;
                    newMeshTransform.gameObject.SetActive(true);

                    meshTransform = newMeshTransform;
                    rootTransform = newRootTransform;
                    t = rootTransform.Find(String.Format(bonePathFormat, idx));
                }
                idx++;
                return t;
            }

            public void Close()
            {
                if (tmpObj != null)
                {
                    GameObject.DestroyImmediate(tmpObj);
                }
            }
        }

        private class FixAnimationSet
        {
            private AnimationClip unfix;
            private AnimationClip fix;
            private AnimationClip fixLock;
            private AnimationClip prefix;
            public FixAnimationSet(AnimationClip unfix, AnimationClip fix, AnimationClip fixLock = null, AnimationClip prefix = null)
            {
                this.unfix = unfix;
                this.fix = fix;
                this.fixLock = fixLock;
                this.prefix = prefix;
            }

            public void AddInstantaneousCurve(string relativePath, Type type, string propertyName, float unfixValue, float fixValue, float? fixLockValue = null, float? prefixValue = null)
            {
                unfix.SetCurve(relativePath, type, propertyName, AnimationCurve.Constant(0f, 0f, unfixValue));
                fix.SetCurve(relativePath, type, propertyName, AnimationCurve.Constant(0f, 0f, fixValue));
                if (fixLockValue != null) fixLock.SetCurve(relativePath, type, propertyName, AnimationCurve.Constant(0f, 0f, fixLockValue.Value));
                if (prefixValue != null) prefix.SetCurve(relativePath, type, propertyName, AnimationCurve.Constant(0f, 0f, prefixValue.Value));
            }
        }

        private class ShowAnimationSet
        {
            private AnimationClip hide;
            private AnimationClip show;
            public ShowAnimationSet(AnimationClip hide, AnimationClip show)
            {
                this.hide = hide;
                this.show = show;
            }


            public void AddInstantaneousCurve(string relativePath, Type type, string propertyName, float hideValue, float showValue)
            {
                hide.SetCurve(relativePath, type, propertyName, AnimationCurve.Constant(0f, 0f, hideValue));
                show.SetCurve(relativePath, type, propertyName, AnimationCurve.Constant(0f, 0f, showValue));
            }
        }

        private class ParamNameResolver
        {
            private List<string> paramNames = new List<string>();
            public ParamNameResolver(VRCAvatarDescriptor descripter)
            {

                if (descripter.expressionParameters != null)
                {
                    paramNames.AddRange(descripter.expressionParameters.parameters.Select(p => p.name));
                }
                foreach (ModularAvatarParameters maP in descripter.GetComponentsInChildren<ModularAvatarParameters>(true))
                {
                    paramNames.AddRange(maP.parameters.Select(p => p.nameOrPrefix));
                }
            }

            public List<AvatarPoseSystem.ParamaterSetting> Resolve(IEnumerable<AvatarPoseSystem.ParamaterSetting> pSettings)
            {
                List<AvatarPoseSystem.ParamaterSetting> resolvedParamaterSettings = new List<AvatarPoseSystem.ParamaterSetting>();
                foreach (AvatarPoseSystem.ParamaterSetting pSetting in pSettings)
                {
                    if (pSetting.isPrefix)
                    {
                        resolvedParamaterSettings.AddRange(paramNames.FindAll(p => p.StartsWith(pSetting.paramaterName)).Select(p => new AvatarPoseSystem.ParamaterSetting()
                        {
                            paramaterName = p,
                            value = pSetting.value
                        }));
                    }
                    else
                    {
                        resolvedParamaterSettings.Add(pSetting);
                    }
                }

                return resolvedParamaterSettings;
            }
        }

        private class EditorCurveSetter
        {
            public AnimationClip clip;
            public EditorCurveBinding binding;
            public AnimationCurve curve;

            public void Set()
            {
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

        }

        private class Util
        {
            public static bool IsSelfOrAncestor(Transform maybeSelfOrDescendant, Transform maybeSelfOrAncestor)
            {
                if (maybeSelfOrDescendant == null) return false;
                if (maybeSelfOrDescendant == maybeSelfOrAncestor) return true;
                return IsSelfOrAncestor(maybeSelfOrDescendant.parent, maybeSelfOrAncestor);
            }
        }

        private class TraceInfoWritter : IDisposable
        {
            private StreamWriter _writer = null;
            private string _filePath = null;

            private Transform _rootTransform = null;

            public bool isOpen()
            {
                return _writer != null;
            }

            public void SetRootTransform(Transform rootTransform)
            {
                _rootTransform = rootTransform;
            }

            public void Open(string filePath, Transform rootTransform, bool append = false)
            {
                if (isOpen()) Close();
                // ファイルパスからディレクトリ部分を取得
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                _filePath = filePath;
                _rootTransform = rootTransform;
                _writer = new StreamWriter(_filePath, append, System.Text.Encoding.UTF8);
            }

            public void Write(string content)
            {
                if (!isOpen()) return;
                _writer.WriteLine(content);
            }

            public void WriteSeparator(string separator = "--------------------------------------------------")
            {
                if (!isOpen()) return;
                Write(separator);
            }

            public void WriteEmptyLine()
            {
                if (!isOpen()) return;
                Write("");
            }

            public void Close()
            {
                _writer?.Flush();
                _writer?.Close();
                _writer = null;
            }

            public void Dispose()
            {
                Close();
            }

            /// <summary>
            /// 指定されたTransformの子要素から階層構造をツリー形式の文字列で取得します。
            /// </summary>
            public void WriteHierarchy(Transform targetTransform)
            {
                if (!isOpen()) return;
                if (targetTransform == null)
                {
                    Write("(null transform)");
                    return;
                }
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(targetTransform.name);
                for (int i = 0; i < targetTransform.childCount; i++)
                {
                    AppendTransform(sb, targetTransform.GetChild(i), "", i == targetTransform.childCount - 1);
                }
                Write(sb.ToString());
            }

            private void AppendTransform(System.Text.StringBuilder sb, Transform transform, string linePrefix, bool isLastInSiblings)
            {
                var components = transform.GetComponents<Component>().Where(c => !(c is Transform)).Select(c => c.GetType().Name);
                string componentString = components.Any() ? $" ({string.Join(", ", components)})" : "";
                sb.AppendLine(linePrefix + (isLastInSiblings ? "`-- " : "|-- ") + transform.name + componentString);
                string childLinePrefix = linePrefix + (isLastInSiblings ? "    " : "|   ");
                for (int i = 0; i < transform.childCount; i++)
                {
                    AppendTransform(sb, transform.GetChild(i), childLinePrefix, i == transform.childCount - 1);
                }
            }

            private string GetHierarchyPath(Transform target)
            {
                if (target == null) return "";

                string path = target.name;
                while (target.parent != null)
                {
                    path = target.parent.name + "/" + path;
                    target = target.parent;
                }
                return path;
            }

            private bool isNull<T>(T obj) where T : class
            {
                var unityObj = obj as UnityEngine.Object;
                return !object.ReferenceEquals(unityObj, null) ? unityObj == null : obj == null;
            }

            private string GetValueString(object value)
            {
                if (isNull(value))
                {
                    return "null";
                }
                else if (value is string s)
                {
                    return $"\"{s}\"";
                }
                else if (value is GameObject go)
                {
                    return $"{GetHierarchyPath(go.transform)} ({go.GetType().FullName})";
                }
                else if (value is Component c)
                {
                    return $"{GetHierarchyPath(c.transform)} ({c.GetType().FullName})";
                }
                else if (value is System.Collections.IEnumerable enumerable)
                {
                    var elements = new List<string>();
                    foreach (object item in enumerable)
                    {
                        elements.Add(GetValueString(item));
                    }

                    if (elements.Count < 2)
                    {
                        return $"[{string.Join(", ", elements)}]";
                    }
                    else
                    {
                        return $"[\n   {string.Join(",\n   ", elements)}]";
                    }
                }
                else
                {
                    return value.ToString();
                }
            }

            static List<string> ignoreProps = new List<string>() { };

            public void WriteNamedValue(string name, object value)
            {
                if (!isOpen()) return;
                Write($"  {name}: {GetValueString(value)}");
            }

            public void WriteProperties(object obj)
            {
                if (!isOpen()) return;
                if (obj == null) return;

                Write($"{obj.GetType().Name}: {GetValueString(obj)}");
                obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(prop => prop.CanRead && !ignoreProps.Contains(prop.Name))
                    .ToList().ForEach(prop =>
                    {
                        try
                        {
                            WriteNamedValue(prop.Name, prop.GetValue(obj));
                        }
                        catch (TargetInvocationException) { }
                    });
                obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(field => !ignoreProps.Contains(field.Name))
                    .ToList().ForEach(field =>
                    {
                        try
                        {
                            WriteNamedValue(field.Name, field.GetValue(obj));
                        }
                        catch (TargetInvocationException) { }
                    });
            }
        }
    }

}
#endif