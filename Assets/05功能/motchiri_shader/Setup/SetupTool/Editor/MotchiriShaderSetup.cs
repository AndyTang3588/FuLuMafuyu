using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.Dynamics;
using wataameya.motchiri_shader.editor;

// Copyright (c) 2023 wataameya

namespace wataameya.motchiri_shader.editor
{
public class MotchiriShaderSetup : EditorWindow
{
    [MenuItem("wataameya/motchiri_shader_Setup")]
    static void Open()
    {
        var window = GetWindow<MotchiriShaderSetup>();
        window.titleContent = new GUIContent("motchiri_shader_Setup");
        window.minSize = new Vector2(420, 500);
    }
    
    private static readonly string _LocalizeCSV_GUID = "945e8212f12bd3f44a5cd58a3b4682a6";

    private int _lang=0;
    private int _lang_number=0;
    private List<List<string>> _texts = new List<List<string>>();

    private Vector2 _scrollpos = Vector2.zero;

    private int _index=0;
    private int _previndex=-1;

    private float _breast_blendshape=0f;
    private float _prevbreast_blendshape=-1f;

    private bool _initialized = false;
    // private List<MarshmallowPreset> _presets;
    private List<string> _preset_names;

    private VRCAvatarDescriptor _descriptor=null;
    private VRCAvatarDescriptor _descriptor_copy;
    private GameObject _avatar;
    private GameObject _prevavatar=null;
    private GameObject _avatar_copy;
    private GameObject _mpb;
    private GameObject _mpb_prefab = null;
    private GameObject _mpb_instance = null;
    private GameObject _Breast_L;
    private GameObject _Breast_R;

    private bool _isOpen1;
    private bool _isOpen2;
    private bool _isOpen3;
    private VRCPhysBoneColliderBase[] _PhysBone_collider= new VRCPhysBoneColliderBase[5];

    //private bool _copy=true;
    // private bool _grab=true;
    // private bool _limit_velocity=true;
    private bool _floor=true;
    private bool _writedefaults=true;
    private bool _interference=true;
    
    #if USE_MA
    private bool _modularavatar=true;
    #else
    private bool _modularavatar=false;
    #endif

    #if USE_SQUISH
    private bool _squishPB=true;
    #else
    private bool _squishPB=false;
    #endif

    private bool _interference_squishPB=false;


    private float _breast_scale=1.0f;
    private float _limit_collider_position_z=0.135f;
    private float _breast_collider_radius=0.1f;
    private float _rotation_constraint_weight=0.8f;
    private float _scale_constraint_weight=1.0f;


    private int _PhysBone_index = 2;
    private int _prevPhysBone_index = -1;
    float[,] _PhysBone_preset = new float[5,6]
    {
        {0.08f,0.00f,0.08f,0.30f,1.00f,0.20f},
        {0.10f,0.30f,0.25f,0.30f,1.00f,0.25f},
        {0.10f,0.50f,0.25f,0.30f,1.00f,0.50f},
        {0.15f,0.50f,0.30f,0.30f,1.00f,0.60f},
        {0.20f,0.50f,0.30f,0.30f,1.00f,0.75f},
    };
    

    private float _PhysBone_Pull=0.1f;
    private float _PhysBone_Momentum=0.5f;
    private float _PhysBone_Stiffness=0.25f;
    private float _PhysBone_Gravity=0.02f;
    private float _PhysBone_GravityFalloff=1f;
    private float _PhysBone_Immobile=0.5f;
    private float _PhysBone_Limit_Angle=40f;
    private float _PhysBone_Collision_Radius=4.0f;

    private float[] _PhysBone_setting = new float[4];

    private GameObject _test;


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void OnGUI()
    {
        Initialize();
        if(_texts.Count==0) Localize();
        _scrollpos = EditorGUILayout.BeginScrollView(_scrollpos);
        EditorGUIUtility.labelWidth = position.size.x/2;
        
        List<string> languages = new List<string>();

        for (int i = 0; i < _lang_number; i++)
        {
            languages.Add(_texts[i][1]);
        }

        _lang = EditorGUILayout.Popup("Language", (int)_lang, languages.ToArray());

        ///メニュー
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.wordWrap = true;
        style.richText = true;

        EditorGUILayout.LabelField($"<color=red><size=15>"+_texts[_lang][70]+"</size></color>",style);
        DrawWebButton(_texts[_lang][71], _texts[_lang][72]);
        DrawWebButton("booth", "https://wataame89.booth.pm/items/4108136");
        EditorGUILayout.EndScrollView();
    }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public string GetFullPath(GameObject obj)
	{
		return GetFullPath(obj.transform);
	}

	public string GetFullPath(Transform t)
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

    public GameObject CheckGameObject(Transform t)
    {
        if(t)
        {
            if(t.name==_avatar.name) return null;
            return t.gameObject;
        }
        return null;
    }

    public VRCPhysBoneBase CheckPhysBone(Transform t)
    {
        if(t)
        {
            if(t.name==_avatar.name) return null;
            return t.gameObject.GetComponent<VRCPhysBoneBase>();;
        }
        return null;
    }
    public VRCPhysBoneColliderBase CheckPhysBoneCollider(Transform t)
    {
        if(t)
        {
            if(t.name==_avatar.name) return null;
            return t.gameObject.GetComponent<VRCPhysBoneColliderBase>();;
        }
        return null;
    }
    
    public void Localize()
    {
        _texts =  new List<List<string>>();
        StreamReader sr = new StreamReader(AssetDatabase.GUIDToAssetPath(_LocalizeCSV_GUID));
        bool n = false;

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');
            if (!n)
            {
                _lang_number = values.Length;
                for (int i = 0; i < _lang_number; i++)
                {
                    _texts.Add(new List<string>());
                }

                n = true;
                for (int j = 0; j < _lang_number; j++)
                {
                    _texts[j].Add("");
                }
            }

            for (int j = 0; j < _lang_number; j++)
            {
                _texts[j].Add(values[j]);
            }
        }
    }

    public void Initialize()
    {
        if(_initialized) return;
        // string[] files = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(_Preset_GUID), "*.asset", SearchOption.AllDirectories);

        // _presets = new List <MarshmallowPreset>();
        // _preset_names = new List<string>();

        // foreach (string i in files)
        // {
        //     MarshmallowPreset preset = AssetDatabase.LoadAssetAtPath<MarshmallowPreset>(i);
        //     if(!preset) continue;
        //     _presets.Add(preset);
        //     _preset_names.Add(preset.avatar_name);
            
        // }
        _initialized = true;
    }

    public void WriteDefaultOff(AnimatorStateMachine statemachine)
    {
        foreach(var childstate in statemachine.states)
        {
            childstate.state.writeDefaultValues = false;
        }

        foreach(var childstatemachine in statemachine.stateMachines)
        {
            WriteDefaultOff(childstatemachine.stateMachine);
        }
    }

    public float FloatFieldCheck(string text, float fl, float min, float max)
    {
        float x = EditorGUILayout.FloatField(text, fl);
        if(x<min) x=min;
        if(max<x) x=max;
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