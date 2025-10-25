using UnityEngine;
using VRC.SDKBase;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.Dynamics;
using wataameya.motchiri_shader.editor;
using wataameya.motchiri_shader.ndmf;

namespace wataameya.motchiri_shader.ndmf.editor
{
    [CustomEditor(typeof(motchiri_shader_MA))]
    internal sealed class motchiri_shader_MA_Editor : Editor
    {
        private static readonly string _Preset_GUID = "98267ae2680fe3c4f9b9661ec4b823cf";
        private static readonly string _LocalizeCSV_GUID = "945e8212f12bd3f44a5cd58a3b4682a6";
        private bool _initialized1 = false;
        private bool _initialized2 = false;
        private List<string> languages = new List<string>();
        private List<string> _preset_names;
        public motchiri_shader_MA motchiri;

        public override void OnInspectorGUI()
        {
            motchiri = target as motchiri_shader_MA;
            serializedObject.Update();
            Undo.RecordObject(motchiri, "motchiri");

            if (motchiri._texts.Count == 0 || motchiri._presets.Count == 0)
            {
                _initialized1 = false;
                _initialized2 = false;
            }

            Initialize();
            Localize();

            motchiri._lang = EditorGUILayout.Popup("Language", (int)motchiri._lang, languages.ToArray());

            ///メニュー
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.wordWrap = true;
            style.richText = true;

#if !USE_NDMF
            EditorGUILayout.LabelField("<color=red><size=25>" + motchiri._texts[motchiri._lang][51] + "</size></color>", style);
#endif

            EditorGUILayout.Space(5);

            DrawWebButton(motchiri._texts[motchiri._lang][52], motchiri._texts[motchiri._lang][53]);
            DrawWebButton("綿飴屋 Wataameya BOOTH", "https://wataame89.booth.pm/items/4108136");

            EditorGUILayout.Space(5);

#if !USE_NDMF
            return;
#endif

            EditorGUILayout.LabelField(motchiri._texts[motchiri._lang][2], style);

            motchiri._avatar = motchiri.transform.parent?.gameObject;

            EditorGUI.BeginChangeCheck();

            if (motchiri._avatar != null && motchiri._avatar.GetComponent<VRCAvatarDescriptor>())
            {
                EditorGUILayout.LabelField(motchiri._texts[motchiri._lang][4], style);

                motchiri._index = EditorGUILayout.Popup(motchiri._texts[motchiri._lang][5], (int)motchiri._index, _preset_names.ToArray());

                if (motchiri._index != motchiri._previndex)
                {
                    motchiri._meshRenderer[0] = CheckType<SkinnedMeshRenderer>(motchiri._avatar.transform, motchiri._presets[motchiri._index].mesh0Path);
                    motchiri._meshRenderer[1] = CheckType<SkinnedMeshRenderer>(motchiri._avatar.transform, motchiri._presets[motchiri._index].mesh1Path);
                    motchiri._meshRenderer[2] = CheckType<SkinnedMeshRenderer>(motchiri._avatar.transform, motchiri._presets[motchiri._index].mesh2Path);

                    motchiri._meshMask[0] = motchiri._presets[motchiri._index].mesh0Mask;
                    motchiri._meshMask[1] = motchiri._presets[motchiri._index].mesh1Mask;
                    motchiri._meshMask[2] = motchiri._presets[motchiri._index].mesh2Mask;

                    motchiri._meshMaterialSlot[0] = motchiri._presets[motchiri._index].mesh0MaterialSlot;
                    motchiri._meshMaterialSlot[1] = motchiri._presets[motchiri._index].mesh1MaterialSlot;
                    motchiri._meshMaterialSlot[2] = motchiri._presets[motchiri._index].mesh2MaterialSlot;

                    motchiri._meshIsTessellation[0] = motchiri._presets[motchiri._index].mesh0IsTessellation;
                    motchiri._meshIsTessellation[1] = motchiri._presets[motchiri._index].mesh1IsTessellation;
                    motchiri._meshIsTessellation[2] = motchiri._presets[motchiri._index].mesh2IsTessellation;

                    motchiri._strength = motchiri._presets[motchiri._index].strength;
                    motchiri._effect = motchiri._presets[motchiri._index].radius;
                }
                motchiri._previndex = motchiri._index;

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(motchiri._texts[motchiri._lang][10], style);
                motchiri._meshRenderer[0] = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(motchiri._texts[motchiri._lang][13], motchiri._meshRenderer[0], typeof(SkinnedMeshRenderer), true);
                motchiri._meshMaterialSlot[0] = EditorGUILayout.IntField(motchiri._texts[motchiri._lang][15], motchiri._meshMaterialSlot[0]);
                motchiri._meshIsTessellation[0] = EditorGUILayout.Toggle(motchiri._texts[motchiri._lang][16], motchiri._meshIsTessellation[0]);
                motchiri._meshMask[0] = (Texture2D)EditorGUILayout.ObjectField(motchiri._texts[motchiri._lang][14], motchiri._meshMask[0], typeof(Texture2D), true);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(motchiri._texts[motchiri._lang][11], style);
                motchiri._meshRenderer[1] = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(motchiri._texts[motchiri._lang][13], motchiri._meshRenderer[1], typeof(SkinnedMeshRenderer), true);
                motchiri._meshMaterialSlot[1] = EditorGUILayout.IntField(motchiri._texts[motchiri._lang][15], motchiri._meshMaterialSlot[1]);
                motchiri._meshIsTessellation[1] = EditorGUILayout.Toggle(motchiri._texts[motchiri._lang][16], motchiri._meshIsTessellation[1]);
                motchiri._meshMask[1] = (Texture2D)EditorGUILayout.ObjectField(motchiri._texts[motchiri._lang][14], motchiri._meshMask[1], typeof(Texture2D), true);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(motchiri._texts[motchiri._lang][12], style);
                motchiri._meshRenderer[2] = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(motchiri._texts[motchiri._lang][13], motchiri._meshRenderer[2], typeof(SkinnedMeshRenderer), true);
                motchiri._meshMaterialSlot[2] = EditorGUILayout.IntField(motchiri._texts[motchiri._lang][15], motchiri._meshMaterialSlot[2]);
                motchiri._meshIsTessellation[2] = EditorGUILayout.Toggle(motchiri._texts[motchiri._lang][16], motchiri._meshIsTessellation[2]);
                motchiri._meshMask[2] = (Texture2D)EditorGUILayout.ObjectField(motchiri._texts[motchiri._lang][14], motchiri._meshMask[2], typeof(Texture2D), true);

                EditorGUILayout.Space(10);

                motchiri._strength = EditorGUILayout.Slider(motchiri._texts[motchiri._lang][20], motchiri._strength, 0f, 1f);
                motchiri._effect = EditorGUILayout.Slider(motchiri._texts[motchiri._lang][21], motchiri._effect, 0f, 1f);
                motchiri._color = EditorGUILayout.ColorField(motchiri._texts[motchiri._lang][24], motchiri._color);
                motchiri._ao = EditorGUILayout.Slider(motchiri._texts[motchiri._lang][22], motchiri._ao, 0f, 1f);
                motchiri._blur = EditorGUILayout.Slider(motchiri._texts[motchiri._lang][23], motchiri._blur, 0f, 1f);

                EditorGUILayout.Space(10);

                motchiri._isOpen3 = EditorGUILayout.Foldout(motchiri._isOpen3, motchiri._texts[motchiri._lang][40]);
                if (motchiri._isOpen3)
                {
                    // motchiri._oldver = EditorGUILayout.Toggle(motchiri._texts[motchiri._lang][38], motchiri._oldver);
                    motchiri._depth = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][30], motchiri._depth);
                    motchiri._func1 = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][41], motchiri._func1);
                    motchiri._func2 = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][42], motchiri._func2);
                    motchiri._func3 = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][43], motchiri._func3);
                    motchiri._normal = EditorGUILayout.Toggle(motchiri._texts[motchiri._lang][32], motchiri._normal);
                    motchiri._normalFunction = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][33], motchiri._normalFunction);
                    motchiri._useEXCollider = EditorGUILayout.Toggle(motchiri._texts[motchiri._lang][34], motchiri._useEXCollider);
                    motchiri._exScalingFactor = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][35], motchiri._exScalingFactor);
                    motchiri._radius = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][36], motchiri._radius);
                    motchiri._unit = EditorGUILayout.FloatField(motchiri._texts[motchiri._lang][37], motchiri._unit);
                }
            }
            else EditorGUILayout.LabelField("<color=red>" + motchiri._texts[motchiri._lang][50] + "</color>", style);

            PrefabUtility.RecordPrefabInstancePropertyModifications(motchiri);
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
            if (target && target != trans) return target.GetComponent<Type>();
            return default;
        }

        private void Initialize()
        {
            if (_initialized1) return;
            string[] files = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(_Preset_GUID), "*.asset", SearchOption.AllDirectories);
            files = files.Where(value => !(System.Text.RegularExpressions.Regex.IsMatch(value, @"\\\d.asset"))).ToArray();

            motchiri._presets = new List<MotchiriShaderPreset>();
            _preset_names = new List<string>();

            foreach (string i in files)
            {
                MotchiriShaderPreset preset = AssetDatabase.LoadAssetAtPath<MotchiriShaderPreset>(i);
                if (!preset) continue;
                motchiri._presets.Add(preset);
                _preset_names.Add(preset.avatarName);
            }
            _initialized1 = true;
        }
        private void Localize()
        {
            if (_initialized2) return;

            motchiri._texts = new List<List<string>>();
            StreamReader sr = new StreamReader(AssetDatabase.GUIDToAssetPath(_LocalizeCSV_GUID));
            bool n = false;

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] values = line.Split(',');
                if (!n)
                {
                    motchiri._lang_number = values.Length;
                    for (int i = 0; i < motchiri._lang_number; i++)
                    {
                        motchiri._texts.Add(new List<string>());
                    }

                    n = true;
                    for (int j = 0; j < motchiri._lang_number; j++)
                    {
                        motchiri._texts[j].Add("");
                    }
                }

                for (int j = 0; j < motchiri._lang_number; j++)
                {
                    motchiri._texts[j].Add(values[j]);
                }
            }
            motchiri._texts0 = new List<string>();
            motchiri._texts1 = new List<string>();
            motchiri._texts2 = new List<string>();
            motchiri._texts3 = new List<string>();

            motchiri._texts0 = motchiri._texts[0];
            motchiri._texts1 = motchiri._texts[1];
            motchiri._texts2 = motchiri._texts[2];
            motchiri._texts3 = motchiri._texts[3];

            languages = new List<string>();
            for (int i = 0; i < motchiri._lang_number; i++)
            {
                languages.Add(motchiri._texts[i][1]);
            }
            _initialized2 = true;
        }

        private float FloatFieldCheck(string text, float fl, float min, float max)
        {
            float x = EditorGUILayout.FloatField(text, fl);
            if (x < min) x = min;
            if (max < x) x = max;
            return x;
        }
        private static void DrawWebButton(string text, string URL)
        {
            var position = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            var icon = EditorGUIUtility.IconContent("BuildSettings.Web.Small");
            icon.text = text;

            var style = new GUIStyle(EditorStyles.label) { padding = new RectOffset() };
            style.normal.textColor = style.focused.textColor;
            style.hover.textColor = style.focused.textColor;
            if (GUI.Button(position, icon, style))
            {
                Application.OpenURL(URL);
            }
        }
    }

}