using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using VRC.SDK3.Avatars.Components;

namespace wataameya.motchiri_shader.ndmf.editor
{
    public sealed class motchiri_shader_MA_Create : EditorWindow
    {
        private const string _MenuPath = "GameObject/wataameya/MotchiriShader";
        private const int ContextMenuPriority = 25;
       

       [MenuItem(_MenuPath,true,ContextMenuPriority)]
       public static bool ValidateApplytoAvatar() => Selection.gameObjects.Any(ValidateCore);
       
       [MenuItem(_MenuPath,false,ContextMenuPriority)]
        public static void ApplytoAvatar()
        {
            List<GameObject> objectToCreated = new List<GameObject>();
            foreach (var x in Selection.gameObjects)
            {
                if (!ValidateCore(x))
                    continue;

                var prefab = GeneratePrefab(x.transform);

                objectToCreated.Add(prefab);
            }
            if (objectToCreated.Count == 0)
                return;

            EditorGUIUtility.PingObject(objectToCreated[0]);
            Selection.objects = objectToCreated.ToArray();
        }
        private static bool ValidateCore(GameObject obj) => obj!=null && obj.GetComponent<VRCAvatarDescriptor>() != null  && obj.GetComponentInChildren<motchiri_shader_MA>() == null;
        private static GameObject GeneratePrefab(Transform parent = null)
        {
            const string PrefabGUID = "667abf373c350aa41aaceec3db159294";
            var prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(PrefabGUID));
            var prefab = PrefabUtility.InstantiatePrefab(prefabObj, parent) as GameObject;
            Undo.RegisterCreatedObjectUndo(prefab, "Apply MotchiriShader");

            return prefab;
        }
    }
}