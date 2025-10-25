using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using wataameya.motchiri_shader;

// Copyright (c) 2023 wataameya

namespace wataameya.motchiri_shader.editor
{
    [Serializable]
    public class CreateMotchiriShaderPreset : ScriptableObject
    {
        [MenuItem("Assets/Create/MotchiriShaderPreset", false)]
        static void Create()
        {
            string[] path_selection = Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.TopLevel)
                .Select(x => AssetDatabase.GetAssetPath(x)).Where(x => AssetDatabase.IsValidFolder(x)).ToArray();
            if(path_selection.Length==0) return;
            int count = Selection.GetFiltered<MotchiriShaderPreset>(SelectionMode.DeepAssets).Count();
            string path = path_selection[0] + "/" + count + ".asset";

            MotchiriShaderPreset preset = CreateInstance<MotchiriShaderPreset>();

            EditorUtility.SetDirty(preset);
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}