using UnityEngine;
using VRC.SDKBase;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using VRC.Dynamics;
using wataameya.motchiri_shader;
using System.Security.Cryptography;

namespace wataameya.motchiri_shader.ndmf
{
    public class motchiri_shader_MA : MonoBehaviour, IEditorOnly
    { 
        public GameObject _avatar;
        public int _index=0;
        public int _previndex=-1;

        public SkinnedMeshRenderer[] _meshRenderer = new SkinnedMeshRenderer[3];
        public Texture2D[] _meshMask = new Texture2D[3];
        public int[] _meshMaterialSlot = new int[3];
        public bool[] _meshIsTessellation = new bool[3];


        //Shader setting
        public float _effect=0.5f;
        public float _strength=0.5f;
        public float _ao = 0.5f;
        public float _blur = 0.5f;
        public Color _color = new Color(0.984f,0.855f,0.792f,1.000f);

        public bool _oldver = false;
        public float _depth = 0f;
        public float _func1 = 1f;
        public float _func2 = 1f;
        public float _func3 = 1f;

        public bool _normal = true;
        public float _normalFunction =0.01f;
        public bool _useEXCollider = false;
        public float _exScalingFactor=1.0f;
        public float _radius=0.75f;
        public float _unit=0.05f;

        
        public List<MotchiriShaderPreset> _presets = new List<MotchiriShaderPreset>();
        public List<List<string>> _texts = new List<List<string>>();
        public List<string> _texts0 = new List<string>();
        public List<string> _texts1 = new List<string>();
        public List<string> _texts2 = new List<string>();
        public List<string> _texts3 = new List<string>();
        public int _lang=0;
        public int _lang_number=0;
        
        public bool _isOpen1 = true;
        public bool _isOpen2 = true;
        public bool _isOpen3 = true;
    }
}