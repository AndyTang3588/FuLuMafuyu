#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using SuzuFactory.Alterith.Localization;
using VRC.SDKBase;

#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace SuzuFactory.Alterith
{
    public enum TransferBoneWeightsMode
    {
        Everything,
        Nothing,
        OnlySpecified,
        ExcludeSpecified,
    }

    public class Alterith : MonoBehaviour, IEditorOnly
    {
        public static string Version = "1.5.6";
        public static string Postfix = " (Alterith Converted)";

        public Transform sourceAvatar = null;
        public Transform[] sourceAvatarBodies = null;
        public Transform sourceClothing = null;
        public Transform destinationAvatar = null;
        public Transform[] destinationAvatarBodies = null;
        public Transform destinationClothing = null;
        public Transform[] destinationExcludedClothings = null;
        public TransferBoneWeightsMode transferBoneWeightsMode = TransferBoneWeightsMode.Nothing;
        public Transform[] destinationTransferBoneWeightsClothings = null;

        public int fittingNumIterations = 10;
        public float fittingRange = 0.1f;
        public int fittingNumNearestNeighbors = 50;
        public float minimumMargin = 0.0f;
        public float marginScale = 1.0f;
        public int transferBoneWeightsNumIterations = 20;
        public float transferBoneWeightsDistance = 0.01f;
        public float transferBoneWeightsWidth = 0.2f;
        public int transferBoneWeightsNumSamples = 10000;
        public int transferBoneWeightsNumNearestNeighbors = 50;
        public float smoothRatio = 1.0f;
        public int smoothNumSamples = 10000;
        public int smoothNumNearestNeighbors = 50;
        public bool separateLeftRight = true;
        public float bonePositionThreshold = 0.01f;
        public bool ignoreHandShape = true;
        public bool ignoreFootShape = true;
        public float referenceArmDistance = 0.0f;
        public float referenceLegAngle = 0.0f;
        public bool inactiveOriginalClothing = true;
        public bool makeOriginalClothingEditorOnly = true;
        public bool makeConvertedClothingUntagged = true;
        public bool deleteOldConvertedClothing = true;
        public bool preserveZeroBlendShapes = false;
        public bool copyBonesToConvertedClothing = false;
        public bool addOriginalMeshAsBlendShape = false;

        public int clothTransformApplierTargetIndex = 0;

        public string[] ValidateInputs()
        {
            List<string> errors = new List<string>();

            CheckAvatar(LanguageManager.Instance.GetString("source"), sourceAvatar, errors);
            CheckAvatarBody(LanguageManager.Instance.GetString("source"), sourceAvatar, sourceAvatarBodies, errors);
            CheckClothing(LanguageManager.Instance.GetString("source"), sourceAvatar, sourceClothing, errors);

            CheckAvatar(LanguageManager.Instance.GetString("destination"), destinationAvatar, errors);
            CheckAvatarBody(LanguageManager.Instance.GetString("destination"), destinationAvatar, destinationAvatarBodies, errors);
            CheckClothing(LanguageManager.Instance.GetString("destination"), destinationAvatar, destinationClothing, errors);

            if (errors.Count == 0)
            {
                try
                {
                    GetClothingSMRTuples(sourceClothing, destinationClothing, null, destinationExcludedClothings, transferBoneWeightsMode, destinationTransferBoneWeightsClothings);
                }
                catch (InvalidOperationException ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return errors.ToArray();
        }

        private static ClothingSMRTuple[] GetClothingSMRTuples(
            Transform sourceClothing,
            Transform destinationClothingOriginal,
            Transform destinationClothingConverted,
            Transform[] destinationExcludedClothings,
            TransferBoneWeightsMode transferBoneWeightsMode,
            Transform[] destinationTransferBoneWeightsClothings)
        {
            var sourceClothingSMRs = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>().ToArray();
            var destinationClothingOriginalSMRs = destinationClothingOriginal.GetComponentsInChildren<SkinnedMeshRenderer>().ToArray();
            var destinationClothingConvertedSMRs = destinationClothingConverted?.GetComponentsInChildren<SkinnedMeshRenderer>().ToArray();

            if (sourceClothingSMRs.Length != destinationClothingOriginalSMRs.Length)
            {
                throw new InvalidOperationException(LanguageManager.Instance.GetString("error_clothing_smr_count_mismatch"));
            }

            if (destinationClothingConverted != null)
            {
                if (sourceClothingSMRs.Length != destinationClothingConvertedSMRs.Length)
                {
                    throw new InvalidOperationException(LanguageManager.Instance.GetString("error_clothing_smr_count_mismatch"));
                }
            }

            List<ClothingSMRTuple> pairs = new List<ClothingSMRTuple>();

            for (int i = 0; i < sourceClothingSMRs.Length; ++i)
            {
                var sourceClothingSMR = sourceClothingSMRs[i];
                var destinationClothingOriginalSMR = destinationClothingOriginalSMRs[i];
                var destinationClothingConvertedSMR = destinationClothingConvertedSMRs != null ? destinationClothingConvertedSMRs[i] : null;

                if (destinationExcludedClothings != null)
                {
                    foreach (var excludedClothing in destinationExcludedClothings)
                    {
                        if (excludedClothing == null)
                        {
                            continue;
                        }

                        if (!excludedClothing.IsChildOf(destinationClothingOriginal))
                        {
                            throw new InvalidOperationException(string.Format(LanguageManager.Instance.GetString("error_excluded_clothing_not_child"), excludedClothing.name));
                        }
                    }
                }

                var excluded = destinationExcludedClothings != null && destinationExcludedClothings.Any(excluded =>
                {
                    return excluded != null && destinationClothingOriginalSMR.transform.IsChildOf(excluded);
                });

                if (sourceClothingSMR.sharedMesh.vertices.Length != destinationClothingOriginalSMR.sharedMesh.vertices.Length)
                {
                    throw new InvalidOperationException(string.Format(LanguageManager.Instance.GetString("error_clothing_smr_vertex_count_mismatch"), i));
                }

                if (destinationClothingConvertedSMR != null && destinationClothingOriginalSMR.sharedMesh.vertices.Length != destinationClothingConvertedSMR.sharedMesh.vertices.Length)
                {
                    throw new InvalidOperationException(string.Format(LanguageManager.Instance.GetString("error_clothing_smr_vertex_count_mismatch"), i));
                }

                bool transferBoneWeights = false;

                switch (transferBoneWeightsMode)
                {
                    case TransferBoneWeightsMode.Everything:
                        transferBoneWeights = true;
                        break;
                    case TransferBoneWeightsMode.Nothing:
                        transferBoneWeights = false;
                        break;
                    case TransferBoneWeightsMode.OnlySpecified:
                        transferBoneWeights = IsIncluded(destinationClothingOriginal, destinationTransferBoneWeightsClothings, destinationClothingOriginalSMR.transform);
                        break;
                    case TransferBoneWeightsMode.ExcludeSpecified:
                        transferBoneWeights = !IsIncluded(destinationClothingOriginal, destinationTransferBoneWeightsClothings, destinationClothingOriginalSMR.transform);
                        break;
                }

                pairs.Add(new ClothingSMRTuple(sourceClothingSMR, destinationClothingOriginalSMR, destinationClothingConvertedSMR, excluded, transferBoneWeights));
            }

            if (pairs.Count == 0)
            {
                throw new InvalidOperationException(LanguageManager.Instance.GetString("error_no_body_smr"));
            }

            return pairs.ToArray();
        }

        private static bool IsIncluded(Transform root, Transform[] targets, Transform target)
        {
            if (targets != null)
            {
                foreach (var transferClothing in targets)
                {
                    if (transferClothing == null)
                    {
                        continue;
                    }

                    if (!transferClothing.IsChildOf(root))
                    {
                        throw new InvalidOperationException(string.Format(LanguageManager.Instance.GetString("error_transfer_clothing_not_child"), transferClothing.name));
                    }

                    if (target.IsChildOf(transferClothing))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CheckAvatar(string name, Transform avatar, List<string> errors)
        {
            if (avatar == null)
            {
                errors.Add(string.Format(LanguageManager.Instance.GetString("error_avatar_not_set"), name));
            }
            else
            {
                var animator = avatar.GetComponent<Animator>();

                if (animator == null)
                {
                    errors.Add(string.Format(LanguageManager.Instance.GetString("error_avatar_no_animator"), name));
                }
                else if (animator.avatar == null)
                {
                    errors.Add(string.Format(LanguageManager.Instance.GetString("error_avatar_no_avatar"), name));
                }
                else if (!animator.avatar.isHuman)
                {
                    errors.Add(string.Format(LanguageManager.Instance.GetString("error_avatar_not_human"), name));
                }
                else if (!animator.avatar.isValid)
                {
                    errors.Add(string.Format(LanguageManager.Instance.GetString("error_avatar_not_valid"), name));
                }
            }
        }

        private void CheckAvatarBody(string name, Transform avatar, Transform[] avatarBodies, List<string> errors)
        {
            if (avatar == null)
            {
                return;
            }

            if (avatarBodies == null || avatarBodies.Length == 0)
            {
                errors.Add(string.Format(LanguageManager.Instance.GetString("error_avatar_bodies_not_set"), name));
                return;
            }

            foreach (var avatarBody in avatarBodies)
            {
                if (avatarBody == null)
                {
                    continue;
                }

                if (!avatarBody.IsChildOf(avatar))
                {
                    errors.Add(string.Format(LanguageManager.Instance.GetString("error_body_not_child"), name));
                    return;
                }
            }

            var smrs = avatarBodies.SelectMany(body =>
            {
                if (body == null)
                {
                    return Enumerable.Empty<SkinnedMeshRenderer>();
                }

                return body.GetComponentsInChildren<SkinnedMeshRenderer>();
            }).ToArray();

            if (smrs.Length == 0)
            {
                errors.Add(string.Format(LanguageManager.Instance.GetString("error_bodies_no_smr"), name));
            }
            else
            {
                foreach (var smr in smrs)
                {
                    if (smr.sharedMesh == null)
                    {
                        errors.Add(string.Format(LanguageManager.Instance.GetString("error_body_smr_no_mesh"), name));
                    }
                }
            }
        }

        private void CheckClothing(string name, Transform avatar, Transform clothing, List<string> errors)
        {
            if (avatar == null)
            {
                return;
            }

            if (clothing == null)
            {
                errors.Add(string.Format(LanguageManager.Instance.GetString("error_clothing_not_set"), name));
            }
            else if (!clothing.IsChildOf(avatar))
            {
                errors.Add(string.Format(LanguageManager.Instance.GetString("error_clothing_not_child"), name));
            }
            else
            {
                var clothingSkinnedMeshRenderers = clothing.GetComponentsInChildren<SkinnedMeshRenderer>();

                if (clothingSkinnedMeshRenderers.Length == 0)
                {
                    errors.Add(string.Format(LanguageManager.Instance.GetString("error_clothing_no_smr"), name));
                }
                else
                {
                    foreach (var smr in clothingSkinnedMeshRenderers)
                    {
                        if (smr.sharedMesh == null)
                        {
                            errors.Add(string.Format(LanguageManager.Instance.GetString("error_clothing_smr_no_mesh"), name));
                        }
                    }
                }
            }
        }

        public void Convert()
        {
            if (ValidateInputs().Length != 0)
            {
                Debug.LogError(LanguageManager.Instance.GetString("error_conversion_failed"));
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/ConvertedClothings"))
            {
                AssetDatabase.CreateFolder("Assets", "ConvertedClothings");
            }

            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/ConvertedClothings/ConvertedClothing.asset");

            Transform convertedClothing = null;
            bool completed = false;

            try
            {
                ReportProgress("Finding SkinnedMeshRenderers", 0.0f);

                var sourceAvatarSMRs = sourceAvatarBodies.Where(body => body != null).SelectMany(body => body.GetComponentsInChildren<SkinnedMeshRenderer>()).ToArray();
                var destinationAvatarSMRs = destinationAvatarBodies.Where(body => body != null).SelectMany(body => body.GetComponentsInChildren<SkinnedMeshRenderer>()).ToArray();

                ReportProgress("Duplicating Destination Clothing", 0.05f);

                Selection.activeTransform = destinationClothing;
                Unsupported.DuplicateGameObjectsUsingPasteboard();
                convertedClothing = Selection.activeTransform;

                convertedClothing.name = destinationClothing.name + Postfix;
                convertedClothing.gameObject.SetActive(true);

                var clothingSMRTuples = GetClothingSMRTuples(sourceClothing, destinationClothing, convertedClothing, destinationExcludedClothings, transferBoneWeightsMode, destinationTransferBoneWeightsClothings);

                ReportProgress("Preparing Alterith Engine", 0.1f);

                var settings = new AlterithEngine.Settings
                {
                    FittingNumIterations = (uint)fittingNumIterations,
                    FittingRange = fittingRange,
                    FittingNumNearestNeighbors = (uint)fittingNumNearestNeighbors,
                    MinimumMargin = minimumMargin,
                    MarginScale = marginScale,
                    TransferBoneWeightsNumIterations = (uint)transferBoneWeightsNumIterations,
                    TransferBoneWeightsDistance = transferBoneWeightsDistance,
                    TransferBoneWeightsWidth = transferBoneWeightsWidth,
                    TransferBoneWeightsNumSamples = (uint)transferBoneWeightsNumSamples,
                    TransferBoneWeightsNumNearestNeighbors = (uint)transferBoneWeightsNumNearestNeighbors,
                    SmoothRatio = smoothRatio,
                    SmoothNumSamples = (uint)smoothNumSamples,
                    SmoothNumNearestNeighbors = (uint)smoothNumNearestNeighbors,
                    SeparateLeftRight = separateLeftRight,
                    BonePositionThreshold = bonePositionThreshold,
                    IgnoreHandShape = ignoreHandShape,
                    IgnoreFootShape = ignoreFootShape,
                    ReferenceArmDistance = referenceArmDistance,
                    ReferenceLegAngle = referenceLegAngle,
                    PreserveZeroBlendShapes = preserveZeroBlendShapes,
                    AddOriginalMeshAsBlendShape = addOriginalMeshAsBlendShape
                };

                using (var engine = new AlterithEngine(sourceAvatar, destinationAvatar, sourceAvatarSMRs, destinationAvatarSMRs, clothingSMRTuples, settings))
                {
                    while (true)
                    {
                        var progress = engine.GetProgress();

                        var progressRatio = Mathf.Lerp(0.1f, 1.0f, (float)progress.Progress);
                        ReportProgress("Converting Clothing", progressRatio);

                        if (progress.Completed)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(100);
                    }

                    var results = engine.GetResult();

                    if (copyBonesToConvertedClothing)
                    {
                        var boneMap = new Dictionary<Transform, Transform>();

                        results = results.Select(r =>
                        {
                            return new AlterithEngine.ResultData
                            {
                                Mesh = r.Mesh,
                                Bones = r.Bones.Select(b => CopyBone(destinationAvatar, convertedClothing, b, boneMap)).ToArray(),
                            };
                        }).ToArray();
                    }

                    for (int resultIndex = 0; resultIndex < results.Length; ++resultIndex)
                    {
                        if (resultIndex == 0)
                        {
                            AssetDatabase.CreateAsset(results[resultIndex].Mesh, assetPath);
                        }
                        else
                        {
                            AssetDatabase.AddObjectToAsset(results[resultIndex].Mesh, assetPath);
                        }
                    }

                    AssetDatabase.SaveAssets();

                    for (int resultIndex = 0; resultIndex < results.Length; ++resultIndex)
                    {
                        var smrTuple = clothingSMRTuples[resultIndex];

                        if (smrTuple.Excluded)
                        {
                            continue;
                        }

                        var dst = smrTuple.DestinationConverted;
                        dst.sharedMesh = results[resultIndex].Mesh;
                        dst.bones = results[resultIndex].Bones;
                    }

                    var group = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("Alterith Convert Clothing");

                    Undo.RegisterCreatedObjectUndo(convertedClothing.gameObject, "Add converted clothing");

                    if (makeConvertedClothingUntagged && convertedClothing.gameObject.tag == "EditorOnly")
                    {
                        convertedClothing.gameObject.tag = "Untagged";
                    }

                    Undo.RegisterFullObjectHierarchyUndo(destinationClothing, "Change original clothing");

                    if (inactiveOriginalClothing)
                    {
                        destinationClothing.gameObject.SetActive(false);
                    }

                    if (makeOriginalClothingEditorOnly && destinationClothing.gameObject.tag != "EditorOnly")
                    {
                        destinationClothing.gameObject.tag = "EditorOnly";
                    }

                    if (deleteOldConvertedClothing && destinationClothing.parent != null)
                    {
                        var destroyObjects = new List<GameObject>();

                        for (int i = 0; i < destinationClothing.parent.childCount; ++i)
                        {
                            var child = destinationClothing.parent.GetChild(i);

                            if (child != convertedClothing && child.name.EndsWith(Postfix))
                            {
                                destroyObjects.Add(child.gameObject);
                            }
                        }

                        foreach (var obj in destroyObjects)
                        {
                            Undo.DestroyObjectImmediate(obj);
                        }
                    }

                    EditorUtility.SetDirty(destinationClothing);
                    Undo.CollapseUndoOperations(group);
                }

                completed = true;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (!completed)
                {
                    if (convertedClothing != null)
                    {
                        DestroyImmediate(convertedClothing.gameObject);
                    }

                    AssetDatabase.DeleteAsset(assetPath);
                }

                EditorUtility.ClearProgressBar();
            }
        }

        private static Transform CopyBone(Transform destinationAvatar, Transform convertedClothing, Transform destinationAvatarBone, Dictionary<Transform, Transform> boneMap)
        {
            if (destinationAvatarBone == null)
            {
                return null;
            }

            if (destinationAvatarBone.IsChildOf(convertedClothing))
            {
                return destinationAvatarBone;
            }

            var result = AlterithUtil.SyncHierarchy(destinationAvatar, destinationAvatarBone, convertedClothing, boneMap);

#if MODULAR_AVATAR
            var destinationHierarchy = AlterithUtil.GetHierarchyBetween(destinationAvatar, destinationAvatarBone);
            var convertedHierarchy = AlterithUtil.GetHierarchyBetween(convertedClothing, result);

            if (destinationHierarchy.Length != convertedHierarchy.Length)
            {
                throw new InvalidOperationException("Hierarchy length mismatch between destination and converted clothing.");
            }

            if (convertedHierarchy.Length != 0)
            {
                var oldArmature = destinationHierarchy[0];
                var newArmature = convertedHierarchy[0];

                if (newArmature.GetComponent<ModularAvatarMergeArmature>() == null)
                {
                    var mergeArmature = newArmature.gameObject.AddComponent<ModularAvatarMergeArmature>();
                    mergeArmature.mergeTarget.Set(oldArmature.gameObject);
                }
            }
#endif

            return result;
        }

        private void ReportProgress(string message, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Alterith", message, progress))
            {
                throw new OperationCanceledException(LanguageManager.Instance.GetString("error_conversion_cancelled"));
            }
        }
    }
}

#endif
