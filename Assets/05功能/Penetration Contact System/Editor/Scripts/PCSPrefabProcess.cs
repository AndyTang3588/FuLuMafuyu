using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;
using nadena.dev.modular_avatar.core;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using System.Drawing.Text;
using UnityEditor;

namespace DMCustom
{
    public class PCSPrefabProcess : MonoBehaviour
    {
        //This script is made for Installer: Modular Avatar
        public enum Installer
        {
            ModularAvatar, VRCFury
        }
        public static Installer installer = Installer.ModularAvatar;
        public static void ShowInstaller()
        {
            if (installer == Installer.ModularAvatar)
            {
                GUI.enabled = false;
                GUI.color = new Color32(50, 200, 255, 255);
                PCSPrefabProcess.installer = (PCSPrefabProcess.Installer)EditorGUILayout.EnumPopup(new GUIContent("Installer", ""), PCSPrefabProcess.installer);
                GUI.color = new Color32(255, 255, 255, 255);
                GUI.enabled = true;
            }
        }

        public static void AddGeneratedAssetToPrefab(GameObject PCS, AnimatorController controler, VRCExpressionsMenu menu, VRCExpressionParameters param, AnimatorController direct)
        {
            var modularController = PCS.AddComponent<ModularAvatarMergeAnimator>();
            modularController.animator = direct;
            modularController.pathMode = MergeAnimatorPathMode.Absolute;
            modularController.matchAvatarWriteDefaults = false;

            var modularController2 = PCS.AddComponent<ModularAvatarMergeAnimator>();
            modularController2.animator = controler;
            modularController2.pathMode = MergeAnimatorPathMode.Absolute;
            modularController2.matchAvatarWriteDefaults = true;

            var modularMenuIns= PCS.AddComponent<ModularAvatarMenuInstaller>();
            modularMenuIns.menuToAppend = menu;

            var modularParamIns = PCS.AddComponent<ModularAvatarParameters>();
            ParameterConfig[] paramConfig = new ParameterConfig[param.parameters.Length];

            //Copy VRC parameter list to Modular parameter list
            for(int i = 0; i< paramConfig.Length; i++)
            {
                paramConfig[i].nameOrPrefix = param.GetParameter(i).name;
                paramConfig[i].defaultValue = param.GetParameter(i).defaultValue;
                paramConfig[i].saved = param.GetParameter(i).saved;

                //Convert VRC ValueType to MA SyncType
                if (param.GetParameter(i).valueType == VRCExpressionParameters.ValueType.Bool)
                {
                    paramConfig[i].syncType = ParameterSyncType.Bool;
                }
                else if (param.GetParameter(i).valueType == VRCExpressionParameters.ValueType.Int)
                {
                    paramConfig[i].syncType = ParameterSyncType.Int;
                }
                else if (param.GetParameter(i).valueType == VRCExpressionParameters.ValueType.Float)
                {
                    paramConfig[i].syncType = ParameterSyncType.Float;
                }

                if (param.GetParameter(i).networkSynced == false)
                {
                    paramConfig[i].syncType = ParameterSyncType.NotSynced;
                }

                modularParamIns.parameters.Add(paramConfig[i]);
            }
        }
    }
}
