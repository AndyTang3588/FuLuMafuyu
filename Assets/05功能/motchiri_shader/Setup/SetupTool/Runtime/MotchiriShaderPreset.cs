using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace wataameya.motchiri_shader
{
    [Serializable]
    public class MotchiriShaderPreset : ScriptableObject
    {
        public string avatarName;

        public float radius = 0.4f;
        public float strength = 0.4f;

        [Space(20)]
        public string mesh0Path;
        public Texture2D mesh0Mask;
        public int mesh0MaterialSlot = 0;
        public bool mesh0IsTessellation = true;

        [Space(20)]
        public string mesh1Path;
        public Texture2D mesh1Mask;
        public int mesh1MaterialSlot = 0;
        public bool mesh1IsTessellation = false;

        [Space(20)]
         public string mesh2Path;
        public Texture2D mesh2Mask;
        public int mesh2MaterialSlot = 0;
        public bool mesh2IsTessellation = false;
    }
}