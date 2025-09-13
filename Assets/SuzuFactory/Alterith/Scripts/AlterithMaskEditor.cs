#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using SuzuFactory.Alterith.Localization;

namespace SuzuFactory.Alterith
{
    [CustomEditor(typeof(AlterithMask))]
    public class AlterithMaskEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var t = target as AlterithMask;

            if (t == null)
            {
                return;
            }

            serializedObject.Update();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.specifyTargetMaterials)), new GUIContent(LanguageManager.Instance.GetString("specify_target_materials")));

                if (serializedObject.FindProperty(nameof(AlterithMask.specifyTargetMaterials)).boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.targetMaterials)), new GUIContent(LanguageManager.Instance.GetString("target_materials")));
                }

                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.specifyMaskImage)), new GUIContent(LanguageManager.Instance.GetString("specify_mask_image")));

                if (serializedObject.FindProperty(nameof(AlterithMask.specifyMaskImage)).boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.maskImage)), new GUIContent(LanguageManager.Instance.GetString("mask_image")));

                    string[] maskAreaOptions = new string[]
                    {
                        LanguageManager.Instance.GetString("mask_area_white"),
                        LanguageManager.Instance.GetString("mask_area_black")
                    };

                    SerializedProperty maskAreaProp = serializedObject.FindProperty(nameof(AlterithMask.maskArea));
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(LanguageManager.Instance.GetString("mask_area")), GUILayout.Width(EditorGUIUtility.labelWidth));
                    int maskAreaIndex = EditorGUILayout.Popup(maskAreaProp.enumValueIndex, maskAreaOptions);
                    if (maskAreaIndex != maskAreaProp.enumValueIndex)
                    {
                        maskAreaProp.enumValueIndex = maskAreaIndex;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(10);

                string[] optionalBoolOptions = new string[]
                {
                    LanguageManager.Instance.GetString("optional_bool_inherit"),
                    LanguageManager.Instance.GetString("optional_bool_enable"),
                    LanguageManager.Instance.GetString("optional_bool_disable")
                };

                SerializedProperty applyFittingProp = serializedObject.FindProperty(nameof(AlterithMask.applyFitting));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(LanguageManager.Instance.GetString("apply_fitting")), GUILayout.Width(EditorGUIUtility.labelWidth));
                int applyFittingIndex = EditorGUILayout.Popup(applyFittingProp.enumValueIndex, optionalBoolOptions);
                if (applyFittingIndex != applyFittingProp.enumValueIndex)
                {
                    applyFittingProp.enumValueIndex = applyFittingIndex;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.overrideMinimumMargin)), new GUIContent(LanguageManager.Instance.GetString("override_minimum_margin")));

                if (serializedObject.FindProperty(nameof(AlterithMask.overrideMinimumMargin)).boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.fittingMinimumMargin)), new GUIContent(LanguageManager.Instance.GetString("fitting_minimum_margin")));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.overrideMarginScale)), new GUIContent(LanguageManager.Instance.GetString("override_margin_scale")));

                if (serializedObject.FindProperty(nameof(AlterithMask.overrideMarginScale)).boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AlterithMask.fittingMarginScale)), new GUIContent(LanguageManager.Instance.GetString("fitting_margin_scale")));
                }

                EditorGUILayout.Space(10);

                SerializedProperty affectedByWeightTransferProp = serializedObject.FindProperty(nameof(AlterithMask.affectedByWeightTransfer));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(LanguageManager.Instance.GetString("weight_transfer_affected")), GUILayout.Width(EditorGUIUtility.labelWidth));
                int affectedByWeightTransferIndex = EditorGUILayout.Popup(affectedByWeightTransferProp.enumValueIndex, optionalBoolOptions);
                if (affectedByWeightTransferIndex != affectedByWeightTransferProp.enumValueIndex)
                {
                    affectedByWeightTransferProp.enumValueIndex = affectedByWeightTransferIndex;
                }
                EditorGUILayout.EndHorizontal();

                SerializedProperty affectsWeightTransferProp = serializedObject.FindProperty(nameof(AlterithMask.affectsWeightTransfer));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(LanguageManager.Instance.GetString("weight_transfer_affects")), GUILayout.Width(EditorGUIUtility.labelWidth));
                int affectsWeightTransferIndex = EditorGUILayout.Popup(affectsWeightTransferProp.enumValueIndex, optionalBoolOptions);
                if (affectsWeightTransferIndex != affectsWeightTransferProp.enumValueIndex)
                {
                    affectsWeightTransferProp.enumValueIndex = affectsWeightTransferIndex;
                }
                EditorGUILayout.EndHorizontal();

                if (check.changed)
                {
                    EditorUtility.SetDirty(t);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
