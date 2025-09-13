#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using SuzuFactory.Alterith.Localization;
using UnityEditor.SceneManagement;

namespace SuzuFactory.Alterith
{
    [CustomEditor(typeof(Alterith))]
    public class AlterithEditor : Editor
    {
        private static bool showSource = true;
        private static bool showDestination = true;
        private static bool showSettings = false;
        private static bool showAdvanced = false;

        private Texture2D headerTexture;
        private string latestVersion = null;
        private bool isCheckingVersion = false;

        private void OnEnable()
        {
            string texturePath = AlterithUtil.GetScriptRelativePath(GetType(), "..", "Textures", "Alterith.png");
            headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            FetchLatestVersion();
        }

        private void FetchLatestVersion()
        {
            if (isCheckingVersion)
            {
                return;
            }

            isCheckingVersion = true;

            Task.Run(() =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string content = client.DownloadString("https://planaria.github.io/AlterithDoc/release_notes/");

                        Regex versionRegex = new Regex(@"v(\d+\.\d+\.\d+)", RegexOptions.IgnoreCase);
                        MatchCollection matches = versionRegex.Matches(content);

                        if (matches.Count > 0)
                        {
                            Match lastMatch = matches[matches.Count - 1];
                            string newVersion = lastMatch.Groups[1].Value;

                            EditorApplication.delayCall += () =>
                            {
                                latestVersion = newVersion;
                                isCheckingVersion = false;
                                Repaint();
                            };
                        }
                        else
                        {
                            EditorApplication.delayCall += () =>
                            {
                                isCheckingVersion = false;
                                Repaint();
                            };
                        }
                    }
                }
                catch (System.Exception)
                {
                    EditorApplication.delayCall += () =>
                    {
                        isCheckingVersion = false;
                        Repaint();
                    };
                }
            });
        }

        public override void OnInspectorGUI()
        {
            var t = target as Alterith;

            if (t == null)
            {
                return;
            }

            if (headerTexture != null)
            {
                float aspectRatio = (float)headerTexture.width / headerTexture.height;
                float width = EditorGUIUtility.currentViewWidth;
                float height = width / aspectRatio;

                Rect rect = GUILayoutUtility.GetRect(width, height);
                GUI.DrawTexture(rect, headerTexture, ScaleMode.ScaleToFit);

                EditorGUILayout.Space(5);
            }

            {
                GUIStyle linkStyle = new GUIStyle(GUI.skin.label);
                linkStyle.normal.textColor = new Color(0.2f, 0.5f, 0.9f);
                linkStyle.hover.textColor = new Color(0.3f, 0.6f, 1.0f);
                linkStyle.fontStyle = FontStyle.Bold;

                if (GUILayout.Button("Document: https://planaria.github.io/AlterithDoc/", linkStyle))
                {
                    Application.OpenURL("https://planaria.github.io/AlterithDoc/");
                }
            }

            EditorGUILayout.Space(10);

            var selectedLanguageIndex = LanguageManager.Instance.GetCurrentLanguageIndex();

            serializedObject.Update();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(LanguageManager.Instance.GetString("language"), GUILayout.Width(120));

                string[] languageNames = LanguageManager.Instance.GetAvailableLanguageNames();
                int newLanguageIndex = EditorGUILayout.Popup(selectedLanguageIndex, languageNames);

                if (newLanguageIndex != selectedLanguageIndex)
                {
                    string[] languageCodes = LanguageManager.Instance.GetAvailableLanguageCodes();
                    LanguageManager.Instance.SetLanguage(languageCodes[newLanguageIndex]);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                showDestination = EditorGUILayout.Foldout(showDestination, LanguageManager.Instance.GetString("destination"), true);

                if (showDestination)
                {
                    GUILayout.BeginHorizontal();

                    {
                        GUILayout.Space(20);
                        GUILayout.BeginVertical();

                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.destinationAvatar)), new GUIContent(LanguageManager.Instance.GetString("avatar")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.destinationAvatarBodies)), new GUIContent(LanguageManager.Instance.GetString("body_meshes")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.destinationClothing)), new GUIContent(LanguageManager.Instance.GetString("clothing")));

                            EditorGUILayout.Space();

                            SerializedProperty transferModeProperty = serializedObject.FindProperty(nameof(Alterith.transferBoneWeightsMode));

                            string[] transferWeightsModeOptions = new string[]
                            {
                                LanguageManager.Instance.GetString("transfer_weights_mode_everything"),
                                LanguageManager.Instance.GetString("transfer_weights_mode_nothing"),
                                LanguageManager.Instance.GetString("transfer_weights_mode_only_specified"),
                                LanguageManager.Instance.GetString("transfer_weights_mode_exclude_specified")
                            };

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent(LanguageManager.Instance.GetString("transfer_weights")), GUILayout.Width(EditorGUIUtility.labelWidth));
                            int transferModeIndex = EditorGUILayout.Popup(transferModeProperty.enumValueIndex, transferWeightsModeOptions);
                            if (transferModeIndex != transferModeProperty.enumValueIndex)
                            {
                                transferModeProperty.enumValueIndex = transferModeIndex;
                            }
                            EditorGUILayout.EndHorizontal();

                            TransferBoneWeightsMode transferMode = (TransferBoneWeightsMode)transferModeProperty.enumValueIndex;

                            switch (transferMode)
                            {
                                case TransferBoneWeightsMode.OnlySpecified:
                                case TransferBoneWeightsMode.ExcludeSpecified:
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.destinationTransferBoneWeightsClothings)), new GUIContent(LanguageManager.Instance.GetString("transfer_weights_target_meshes")));
                                    break;
                                default:
                                    break;
                            }

                            EditorGUILayout.Space();

                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.destinationExcludedClothings)), new GUIContent(LanguageManager.Instance.GetString("exclude_meshes")));
                        }

                        EditorGUILayout.Space();

                        DrawClothTransformApplier(t);

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);

                showSource = EditorGUILayout.Foldout(showSource, LanguageManager.Instance.GetString("source"), true);

                if (showSource)
                {
                    GUILayout.BeginHorizontal();

                    {
                        GUILayout.Space(20);
                        GUILayout.BeginVertical();

                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.sourceAvatar)), new GUIContent(LanguageManager.Instance.GetString("avatar")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.sourceAvatarBodies)), new GUIContent(LanguageManager.Instance.GetString("body_meshes")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.sourceClothing)), new GUIContent(LanguageManager.Instance.GetString("clothing")));
                        }

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);

                showSettings = EditorGUILayout.Foldout(showSettings, LanguageManager.Instance.GetString("settings"), true);

                if (showSettings)
                {
                    GUILayout.BeginHorizontal();

                    {
                        GUILayout.Space(20);
                        GUILayout.BeginVertical();

                        {
                            SerializedProperty fittingRangeProp = serializedObject.FindProperty(nameof(Alterith.fittingRange));
                            EditorGUILayout.PropertyField(fittingRangeProp, new GUIContent(LanguageManager.Instance.GetString("fitting_range")));
                            fittingRangeProp.floatValue = Mathf.Max(0.0f, fittingRangeProp.floatValue);

                            SerializedProperty minimumMarginProp = serializedObject.FindProperty(nameof(Alterith.minimumMargin));
                            EditorGUILayout.PropertyField(minimumMarginProp, new GUIContent(LanguageManager.Instance.GetString("fitting_minimum_margin")));
                            minimumMarginProp.floatValue = minimumMarginProp.floatValue;

                            SerializedProperty marginScaleProp = serializedObject.FindProperty(nameof(Alterith.marginScale));
                            EditorGUILayout.PropertyField(marginScaleProp, new GUIContent(LanguageManager.Instance.GetString("fitting_margin_scale")));
                            marginScaleProp.floatValue = marginScaleProp.floatValue;

                            EditorGUILayout.Space();

                            SerializedProperty transferDistanceProp = serializedObject.FindProperty(nameof(Alterith.transferBoneWeightsDistance));
                            EditorGUILayout.PropertyField(transferDistanceProp, new GUIContent(LanguageManager.Instance.GetString("weight_transfer_distance")));
                            transferDistanceProp.floatValue = Mathf.Max(0.0f, transferDistanceProp.floatValue);

                            SerializedProperty transferWidthProp = serializedObject.FindProperty(nameof(Alterith.transferBoneWeightsWidth));
                            EditorGUILayout.PropertyField(transferWidthProp, new GUIContent(LanguageManager.Instance.GetString("weight_transfer_width")));
                            transferWidthProp.floatValue = Mathf.Max(0.0f, transferWidthProp.floatValue);
                        }

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);

                showAdvanced = EditorGUILayout.Foldout(showAdvanced, LanguageManager.Instance.GetString("advanced"), true);

                if (showAdvanced)
                {
                    GUILayout.BeginHorizontal();

                    {
                        GUILayout.Space(20);
                        GUILayout.BeginVertical();

                        {
                            SerializedProperty fittingIterationsProp = serializedObject.FindProperty(nameof(Alterith.fittingNumIterations));
                            EditorGUILayout.PropertyField(fittingIterationsProp, new GUIContent(LanguageManager.Instance.GetString("fitting_iterations")));
                            fittingIterationsProp.intValue = Mathf.Clamp(fittingIterationsProp.intValue, 0, 1000);

                            EditorGUI.BeginDisabledGroup(fittingIterationsProp.intValue == 0);

                            {

                                SerializedProperty fittingNeighborsProp = serializedObject.FindProperty(nameof(Alterith.fittingNumNearestNeighbors));
                                EditorGUILayout.PropertyField(fittingNeighborsProp, new GUIContent(LanguageManager.Instance.GetString("fitting_neighbors")));
                                fittingNeighborsProp.intValue = Mathf.Clamp(fittingNeighborsProp.intValue, 1, 1000);

                                EditorGUI.BeginDisabledGroup(fittingIterationsProp.intValue < 2);

                                {
                                    SerializedProperty smoothRatioProp = serializedObject.FindProperty(nameof(Alterith.smoothRatio));
                                    EditorGUILayout.Slider(smoothRatioProp, 0.0f, 1.0f, new GUIContent(LanguageManager.Instance.GetString("smooth_ratio")));

                                    SerializedProperty smoothSamplesProp = serializedObject.FindProperty(nameof(Alterith.smoothNumSamples));
                                    EditorGUILayout.PropertyField(smoothSamplesProp, new GUIContent(LanguageManager.Instance.GetString("smooth_samples")));
                                    smoothSamplesProp.intValue = Mathf.Clamp(smoothSamplesProp.intValue, 1, 10000000);

                                    SerializedProperty smoothNeighborsProp = serializedObject.FindProperty(nameof(Alterith.smoothNumNearestNeighbors));
                                    EditorGUILayout.PropertyField(smoothNeighborsProp, new GUIContent(LanguageManager.Instance.GetString("smooth_neighbors")));
                                    smoothNeighborsProp.intValue = Mathf.Clamp(smoothNeighborsProp.intValue, 1, 1000);
                                }

                                EditorGUI.EndDisabledGroup();

                            }

                            EditorGUI.EndDisabledGroup();

                            SerializedProperty transferIterationsProp = serializedObject.FindProperty(nameof(Alterith.transferBoneWeightsNumIterations));
                            EditorGUILayout.PropertyField(transferIterationsProp, new GUIContent(LanguageManager.Instance.GetString("weight_transfer_iterations")));
                            transferIterationsProp.intValue = Mathf.Clamp(transferIterationsProp.intValue, 1, 1000);


                            SerializedProperty transferSamplesProp = serializedObject.FindProperty(nameof(Alterith.transferBoneWeightsNumSamples));
                            EditorGUILayout.PropertyField(transferSamplesProp, new GUIContent(LanguageManager.Instance.GetString("weight_transfer_samples")));
                            transferSamplesProp.intValue = Mathf.Clamp(transferSamplesProp.intValue, 1, 10000000);

                            SerializedProperty transferNeighborsProp = serializedObject.FindProperty(nameof(Alterith.transferBoneWeightsNumNearestNeighbors));
                            EditorGUILayout.PropertyField(transferNeighborsProp, new GUIContent(LanguageManager.Instance.GetString("weight_transfer_neighbors")));
                            transferNeighborsProp.intValue = Mathf.Clamp(transferNeighborsProp.intValue, 1, 1000);

                            SerializedProperty separateLeftRightProp = serializedObject.FindProperty(nameof(Alterith.separateLeftRight));
                            EditorGUILayout.PropertyField(separateLeftRightProp, new GUIContent(LanguageManager.Instance.GetString("separate_left_right")));

                            EditorGUI.BeginDisabledGroup(!separateLeftRightProp.boolValue);

                            {
                                SerializedProperty bonePositionThresholdProp = serializedObject.FindProperty(nameof(Alterith.bonePositionThreshold));
                                EditorGUILayout.PropertyField(bonePositionThresholdProp, new GUIContent(LanguageManager.Instance.GetString("bone_position_threshold")));
                                bonePositionThresholdProp.floatValue = Mathf.Max(0.0f, bonePositionThresholdProp.floatValue);
                            }

                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.Space();

                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.ignoreHandShape)), new GUIContent(LanguageManager.Instance.GetString("ignore_hand_detail")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.ignoreFootShape)), new GUIContent(LanguageManager.Instance.GetString("ignore_foot_detail")));

                            EditorGUILayout.Space();

                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.referenceArmDistance)), new GUIContent(LanguageManager.Instance.GetString("reference_arm_distance")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.referenceLegAngle)), new GUIContent(LanguageManager.Instance.GetString("reference_leg_angle")));

                            EditorGUILayout.Space();

                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.inactiveOriginalClothing)), new GUIContent(LanguageManager.Instance.GetString("inactive_original_clothing")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.makeOriginalClothingEditorOnly)), new GUIContent(LanguageManager.Instance.GetString("make_original_clothing_editor_only")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.makeConvertedClothingUntagged)), new GUIContent(LanguageManager.Instance.GetString("make_converted_clothing_untagged")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.deleteOldConvertedClothing)), new GUIContent(LanguageManager.Instance.GetString("delete_old_converted_clothing")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.preserveZeroBlendShapes)), new GUIContent(LanguageManager.Instance.GetString("preserve_zero_blendshapes")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.copyBonesToConvertedClothing)), new GUIContent(LanguageManager.Instance.GetString("copy_bones_to_converted_clothing")));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Alterith.addOriginalMeshAsBlendShape)), new GUIContent(LanguageManager.Instance.GetString("add_original_mesh_as_blendshape")));

                            EditorGUILayout.Space();

                            EditorGUILayout.TextField(LanguageManager.Instance.GetString("parameters"), GetParametersString());
                        }

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);

                var errors = t.ValidateInputs();
                var clothTransformApplierErrors = GetClothTransformApplierErrors(t);

                EditorGUI.BeginDisabledGroup(errors.Length > 0 || clothTransformApplierErrors.Length > 0);

                if (GUILayout.Button(LanguageManager.Instance.GetString("convert"), GUILayout.Height(50)))
                {
                    ExecuteClothTransformApplier(t);
                    t.Convert();
                }

                EditorGUI.EndDisabledGroup();

                foreach (var error in errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }

                GUILayout.Space(10);

                if (GUILayout.Button(LanguageManager.Instance.GetString("cleanup_unused_meshes")))
                {
                    CleanupUnusedMeshAssets();
                }

                if (latestVersion != null)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Current Version: v" + Alterith.Version);
                    EditorGUILayout.LabelField("Latest Version: v" + latestVersion);
                }

                if (check.changed)
                {
                    EditorUtility.SetDirty(t);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CleanupUnusedMeshAssets()
        {
            if (!EditorUtility.DisplayDialog(
                LanguageManager.Instance.GetString("cleanup_title"),
                LanguageManager.Instance.GetString("cleanup_confirm_save_message"),
                LanguageManager.Instance.GetString("confirm_ok"),
                LanguageManager.Instance.GetString("confirm_cancel")))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                return;
            }

            if (!EditorSceneManager.SaveOpenScenes())
            {
                return;
            }

            AssetDatabase.SaveAssets();

            if (AssetDatabase.IsValidFolder("Assets/ConvertedClothings"))
            {
                var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
                var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

                var allReferencingAssetPaths = new List<string>();

                foreach (string guid in sceneGuids)
                {
                    allReferencingAssetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
                }

                foreach (string guid in prefabGuids)
                {
                    allReferencingAssetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
                }

                EditorUtility.DisplayProgressBar("Checking Assets", "Finding unused mesh assets...", 0f);

                var dependencies = new HashSet<string>();

                try
                {
                    for (int assetIndex = 0; assetIndex < allReferencingAssetPaths.Count; ++assetIndex)
                    {
                        string assetPath = allReferencingAssetPaths[assetIndex];

                        EditorUtility.DisplayProgressBar("Checking Assets",
                            $"Checking asset {assetIndex + 1}/{allReferencingAssetPaths.Count}: {System.IO.Path.GetFileName(assetPath)}",
                            (float)assetIndex / allReferencingAssetPaths.Count);

                        dependencies.UnionWith(AssetDatabase.GetDependencies(assetPath, true));
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }

                string allDependencies = string.Join(",", dependencies);

                var meshPaths = AssetDatabase.FindAssets("t:Mesh", new[] { "Assets/ConvertedClothings" })
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .ToArray();

                var unusedMeshPaths = meshPaths
                    .Where(path => !dependencies.Contains(path))
                    .ToArray();

                if (unusedMeshPaths.Length == 0)
                {
                    EditorUtility.DisplayDialog(
                        LanguageManager.Instance.GetString("cleanup_title"),
                        LanguageManager.Instance.GetString("cleanup_no_unused_meshes"),
                        "OK");
                    return;
                }

                bool confirm = EditorUtility.DisplayDialog(
                    LanguageManager.Instance.GetString("cleanup_title"),
                    string.Format(LanguageManager.Instance.GetString("cleanup_confirm"), meshPaths.Length, unusedMeshPaths.Length),
                    LanguageManager.Instance.GetString("confirm_ok"),
                    LanguageManager.Instance.GetString("confirm_cancel"));

                if (!confirm)
                {
                    return;
                }

                AssetDatabase.MoveAssetsToTrash(unusedMeshPaths, new List<string>());

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    LanguageManager.Instance.GetString("cleanup_title"),
                    LanguageManager.Instance.GetString("cleanup_success"),
                    "OK");
            }
            else
            {
            }
        }

        private string[] GetClothTransformApplierErrors(Alterith t)
        {
            try
            {
                var applierType = GetGetClothTransformApplierType();

                if (applierType == null)
                {
                    return new string[0];
                }

                var checkMethod = applierType.GetMethod("CheckCTASettings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (checkMethod == null)
                {
                    return new string[0];
                }

                var applier = GetClothTransformApplier(t.destinationAvatar);

                if (applier == null)
                {
                    return new string[0];
                }

                var args = new object[] { applier, t.clothTransformApplierTargetIndex };
                return checkMethod.Invoke(null, args) as string[];
            }
            catch (System.Exception)
            {
                return new string[] { "Error while checking ClothTransformApplier settings." };
            }
        }

        private System.Type GetGetClothTransformApplierType()
        {
            return System.Type.GetType("WataOfuton.Tools.ClothTransformApplier.ClothTransformApplier, ClothTransformApplier");
        }

        private Object GetClothTransformApplier(Transform t)
        {
            if (t == null)
            {
                return null;
            }

            var applierType = GetGetClothTransformApplierType();

            if (applierType == null)
            {
                return null;
            }

            return t.GetComponentInChildren(applierType);
        }

        private System.Reflection.MethodInfo GetApplyClothWithAlterithMethod()
        {
            var applierType = GetGetClothTransformApplierType();

            if (applierType == null)
            {
                return null;
            }

            return applierType.GetMethod("ApplyClothWithAlterith", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        }

        private System.Reflection.MethodInfo GetDrawPresetWithAlterithMethod()
        {
            var applierEditorType = System.Type.GetType("WataOfuton.Tools.ClothTransformApplier.Editor.ClothTransformApplierEditor, ClothTransformApplier.Editor");

            if (applierEditorType == null)
            {
                return null;
            }

            return applierEditorType.GetMethod("DrawPresetWithAlterith", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        }

        private void DrawClothTransformApplier(Alterith t)
        {
            var applier = GetClothTransformApplier(t.destinationAvatar);

            if (applier == null)
            {
                return;
            }

            var drawPreset = GetDrawPresetWithAlterithMethod();

            if (drawPreset == null)
            {
                return;
            }

            var args = new object[] { applier, t.clothTransformApplierTargetIndex };
            drawPreset.Invoke(null, args);
            t.clothTransformApplierTargetIndex = (int)args[1];
        }

        private void ExecuteClothTransformApplier(Alterith t)
        {
            var clothTransformApplier = GetClothTransformApplier(t.destinationAvatar);

            if (clothTransformApplier == null)
            {
                return;
            }

            var applyMethod = GetApplyClothWithAlterithMethod();

            if (applyMethod == null)
            {
                return;
            }

            var args = new object[] { clothTransformApplier, t.clothTransformApplierTargetIndex, new Transform[] { t.destinationClothing } };
            applyMethod.Invoke(null, args);
        }

        private string GetParametersString()
        {
            var t = target as Alterith;

            if (t == null)
            {
                return "No Alterith target selected.";
            }

            string result = "";

            result += "{";
            result += "\"version\":\"" + Alterith.Version + "\"";
            result += ",\"sourceAvatar\": " + GetPath(t.sourceAvatar);
            result += ",\"sourceAvatarBody\": " + GetNames(t.sourceAvatarBodies);
            result += ",\"sourceClothing\": " + GetPath(t.sourceClothing);
            result += ",\"destinationAvatar\": " + GetPath(t.destinationAvatar);
            result += ",\"destinationAvatarBody\": " + GetNames(t.destinationAvatarBodies);
            result += ",\"destinationClothing\": " + GetPath(t.destinationClothing);
            result += ",\"destinationExcludedClothings\": " + GetNames(t.destinationExcludedClothings);
            result += ",\"transferBoneWeightsMode\": \"" + t.transferBoneWeightsMode.ToString() + "\"";
            result += ",\"destinationTransferBoneWeightsClothings\": " + GetNames(t.destinationTransferBoneWeightsClothings);
            result += ",\"fittingNumIterations\": " + t.fittingNumIterations;
            result += ",\"fittingRange\": " + t.fittingRange;
            result += ",\"fittingNumNearestNeighbors\": " + t.fittingNumNearestNeighbors;
            result += ",\"minimumMargin\": " + t.minimumMargin;
            result += ",\"marginScale\": " + t.marginScale;
            result += ",\"transferBoneWeightsNumIterations\": " + t.transferBoneWeightsNumIterations;
            result += ",\"transferBoneWeightsDistance\": " + t.transferBoneWeightsDistance;
            result += ",\"transferBoneWeightsWidth\": " + t.transferBoneWeightsWidth;
            result += ",\"transferBoneWeightsNumSamples\": " + t.transferBoneWeightsNumSamples;
            result += ",\"transferBoneWeightsNumNearestNeighbors\": " + t.transferBoneWeightsNumNearestNeighbors;
            result += ",\"smoothRatio\": " + t.smoothRatio;
            result += ",\"smoothNumSamples\": " + t.smoothNumSamples;
            result += ",\"smoothNumNearestNeighbors\": " + t.smoothNumNearestNeighbors;
            result += ",\"separateLeftRight\": " + t.separateLeftRight;
            result += ",\"bonePositionThreshold\": " + t.bonePositionThreshold;
            result += ",\"ignoreHandShape\": " + t.ignoreHandShape;
            result += ",\"ignoreFootShape\": " + t.ignoreFootShape;
            result += ",\"referenceArmDistance\": " + t.referenceArmDistance;
            result += ",\"referenceLegAngle\": " + t.referenceLegAngle;
            result += ",\"inactiveOriginalClothing\": " + t.inactiveOriginalClothing;
            result += ",\"makeOriginalClothingEditorOnly\": " + t.makeOriginalClothingEditorOnly;
            result += ",\"makeConvertedClothingUntagged\": " + t.makeConvertedClothingUntagged;
            result += ",\"deleteOldConvertedClothing\": " + t.deleteOldConvertedClothing;
            result += ",\"preserveZeroBlendShapes\": " + t.preserveZeroBlendShapes;
            result += ",\"copyBonesToConvertedClothing\": " + t.copyBonesToConvertedClothing;
            result += "}";

            return result;
        }

        private static string GetPath(Transform obj)
        {
            if (obj == null)
            {
                return "null";
            }

            string result = "";

            while (obj != null)
            {
                if (result.Length > 0)
                {
                    result = "/" + result;
                }

                result = obj.name + result;

                obj = obj.parent;
            }

            return "\"" + result + "\"";
        }

        private static string GetNames(Transform[] objs)
        {
            if (objs == null)
            {
                return "null";
            }

            return "[" + string.Join(", ", objs.Select(o => GetPath(o))) + "]";
        }

        [MenuItem("GameObject/Alterith", false, 0)]
        public static void AddAlterith()
        {
            string prefabPath = AlterithUtil.GetScriptRelativePath(typeof(AlterithEditor), "..", "Alterith.prefab");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.SetParent(Selection.activeTransform, false);
            Selection.activeTransform = go.transform;
            Undo.RegisterCreatedObjectUndo(go, "Add Alterith");
        }
    }
}

#endif
