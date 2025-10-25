using UnityEngine;
using VRC.SDKBase;
using System;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZeroFactory.AvatarPoseSystem.NDMF
{
    [AddComponentMenu("ZeroFactory/AvatarPoseSystem")]
    public class AvatarPoseSystem : MonoBehaviour, IEditorOnly
    {
        [Header("目をハンドルで調整しない（他ギミックの視線制御を使用する場合はチェックしてください）")]
        public bool UnhandleEyes = false;

        [Header("固定したくないPhysBone/HeadChopを設定してください（設定した要素のみ対象）")]
        public List<GameObject> UnfixPhysBones;

        [Header("固定したくないPhysBone/HeadChopを設定してください（設定した要素と子要素が対象）")]
        public List<GameObject> UnfixPhysBonesWithChildren;

        [Header("固定したくないオブジェクトを設定してください（VirtualLensなどはこちら）")]
        public List<GameObject> UnfixObjects;

        [Header("固定したくないオブジェクトのパスを設定してください(自動生成オブジェクト指定用)")]
        public List<string> UnfixObjectPaths;

        [Header("ポーズ固定時/解除時にパラメータを変更する設定")]
        public List<ParamaterSetting> SetParamatersOnFix;
        public List<ParamaterSetting> SetParamatersOnUnfix;

        [Header("トレース情報ファイルを作成する(バグ報告用：AvatarPoseSystem\\__trace フォルダに作成)")]
        public bool EnableTrace = false;

        [Serializable]
        public class ParamaterSetting
        {
            public string paramaterName;
            public bool isPrefix;
            public float value;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(AvatarPoseSystem.ParamaterSetting))]
    public class ParamaterSettingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                // 各要素の幅を計算
                float totalWidth = position.width;
                float nameHeaderWidth = 105f;
                float isPrefixHeaderWidth = 55f;
                float valueHeaderWidth = 40f;
                float spacing = 5f;

                float isPrefixFieldWidth = 20f;
                float inputWidth = totalWidth - nameHeaderWidth - isPrefixHeaderWidth - valueHeaderWidth - isPrefixFieldWidth - spacing - spacing;
                float nameFieldWidth = 0.8f * inputWidth;
                float valueFieldWidth = 0.2f * inputWidth;

                float currentX = position.x;
                float currentY = position.y;
                float height = EditorGUIUtility.singleLineHeight;

                // パラメータ名
                SerializedProperty paramaterNameProp = property.FindPropertyRelative(nameof(AvatarPoseSystem.ParamaterSetting.paramaterName));
                Rect nameHeaderRect = new Rect(currentX, currentY, nameHeaderWidth, height);
                EditorGUI.LabelField(nameHeaderRect, paramaterNameProp.displayName, EditorStyles.boldLabel);
                currentX += nameHeaderWidth;

                Rect nameFieldRect = new Rect(currentX, currentY, nameFieldWidth, height);
                EditorGUI.PropertyField(nameFieldRect, paramaterNameProp, GUIContent.none);
                currentX += nameFieldWidth + spacing;


                // Prefixかどうか
                SerializedProperty isPrefixProp = property.FindPropertyRelative(nameof(AvatarPoseSystem.ParamaterSetting.isPrefix));
                Rect isPrefixRect = new Rect(currentX, currentY, isPrefixHeaderWidth, height);
                EditorGUI.LabelField(isPrefixRect, isPrefixProp.displayName, EditorStyles.boldLabel);
                currentX += isPrefixHeaderWidth;

                Rect prefixFieldRect = new Rect(currentX, currentY, isPrefixFieldWidth, height);
                EditorGUI.PropertyField(prefixFieldRect, isPrefixProp, GUIContent.none);
                currentX += isPrefixFieldWidth + spacing;

                // 設定する値
                SerializedProperty valProp = property.FindPropertyRelative(nameof(AvatarPoseSystem.ParamaterSetting.value));
                Rect valueHeaderRect = new Rect(currentX, currentY, valueHeaderWidth, height);
                EditorGUI.LabelField(valueHeaderRect, valProp.displayName, EditorStyles.boldLabel);
                currentX += valueHeaderWidth;

                Rect valueFieldRect = new Rect(currentX, currentY, valueFieldWidth, height);
                EditorGUI.PropertyField(valueFieldRect, valProp, GUIContent.none);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}
