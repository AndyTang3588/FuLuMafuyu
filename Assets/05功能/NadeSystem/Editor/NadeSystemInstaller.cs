using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using System.IO;
using VRC.SDK3A.Editor.Elements;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.Constraint.Components;
using System;
using UnityEditorInternal;
using UnityEngine.PlayerLoop;
using RnwNadesystem;
using System.Runtime.InteropServices;

public class NadeSystemInstaller : EditorWindow
{
    
    private const string _nadeSystemGUID = "491c3f399da5d064d9966982ddf0d191";
    private const string _nadeShadowGUID = "fd1d0e8cc6fc6f646ad9f24b156a31ac";
    private const string _dummyLightGUID = "c46c6e537bbb1a140957ad83f15c5afb";

    private VRCAvatarDescriptor avatar;
    private float contactRadius = 0.14f;
    private float headOffsetY = 0.035f;
    private bool installNadeShader_Hands = true;
    private bool installNadeShader_Head = false;

    private float delayStart;
    private float delayTime = 0.5f;

    private string[] langOptions = new string[] {"English", "Japanese", "Korean"};
    private static int langIndex = 0;


    [MenuItem("RedNightWorks/NadeSystemInstaller")]
    public static void ShowWindow()
    {
        var systemlang = Application.systemLanguage;
        if(systemlang == SystemLanguage.Japanese) langIndex = 1;
        else if(systemlang == SystemLanguage.Korean) langIndex = 2;
        else langIndex = 0;
        GetWindow<NadeSystemInstaller>(true, "Nade System Installer");
    }

    private void OnGUI()
    {
        langIndex = EditorGUILayout.Popup("Language", langIndex, langOptions);
        EditorGUILayout.Space();

        avatar = EditorGUILayout.ObjectField(Localize.avatar[langIndex], avatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField(Localize.contactParameters[langIndex]);
            contactRadius = EditorGUILayout.FloatField(Localize.contactRadius[langIndex], contactRadius);
            headOffsetY = EditorGUILayout.FloatField(Localize.contactOffsetY[langIndex], headOffsetY);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField(Localize.shadowShaderInstall[langIndex]);
            installNadeShader_Hands = EditorGUILayout.Toggle(Localize.installHands[langIndex], installNadeShader_Hands);
            installNadeShader_Head = EditorGUILayout.Toggle(Localize.installHead[langIndex], installNadeShader_Head);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (GUILayout.Button("Setup!")){
            NadeSystemInstall();
        }
    }

    private GameObject GetPrefab(string guid)
    {
        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
        GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if(prefabObj == null){
            throw new IOException("Nadesystem is broken!");
        }
        return prefabObj;
    }

    private void NadeSystemInstall(){
        if(avatar != null){
            if(contactRadius < 0.01 | contactRadius > 1) contactRadius = 0.14f;

            //ヘッドポジション取得
            Vector3 avatarHeadPosition = avatar.collider_head.position;
            Vector3 avatarHeadBonePosition = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position;
            Vector3 avatarHeadCenter = avatarHeadPosition + avatarHeadBonePosition;

            //撫でシステムのプレハブを取得
            GameObject nadeSystemPrefab = GetPrefab(_nadeSystemGUID);

            //既存撫でシステムを削除
            var nadeExist = avatar.transform.Find("NadeSystem");
            if(nadeExist != null){
                DestroyImmediate(nadeExist.gameObject);
            }

            //撫でシステムインスタンス化
            GameObject nadeSystemObj = (GameObject)PrefabUtility.InstantiatePrefab(nadeSystemPrefab, avatar.transform);
            //nadeSystemObj.transform.position = avatar.ViewPosition;

            //オブジェクト取得
            var rxHeadMainObj = nadeSystemObj.transform.Find("RxHeadMain");
            if(rxHeadMainObj == null){
                throw new IOException("Nadesystem is broken!");
            }
            var headSystemObj = nadeSystemObj.transform.Find("HeadSystem");
            if(headSystemObj == null){
                throw new IOException("Nadesystem is broken!");
            }
            var rightHandSystemObj = nadeSystemObj.transform.Find("RightHandSystem");
            if(rightHandSystemObj == null){
                throw new IOException("Nadesystem is broken!");
            }
            var leftHandSystemObj = nadeSystemObj.transform.Find("LeftHandSystem");
            if(leftHandSystemObj == null){
                throw new IOException("Nadesystem is broken!");
            }


            //座標調整
            var rxHeadMainContact = rxHeadMainObj.GetComponent<VRCContactReceiver>();
            rxHeadMainContact.radius = contactRadius;
            rxHeadMainObj.transform.position = avatarHeadCenter + new Vector3(0f, headOffsetY, 0f);
            headSystemObj.transform.position = avatar.ViewPosition + avatar.transform.position;

            //撫でシェーダー配置
            GameObject nadeShadowPrefab = GetPrefab(_nadeShadowGUID);
            if(installNadeShader_Hands)
            {
                PrefabUtility.InstantiatePrefab(nadeShadowPrefab, rightHandSystemObj.transform);
                PrefabUtility.InstantiatePrefab(nadeShadowPrefab, leftHandSystemObj.transform);
            }
            if(installNadeShader_Head)
            {
                PrefabUtility.InstantiatePrefab(nadeShadowPrefab, headSystemObj.transform);
            }
            if(installNadeShader_Hands || installNadeShader_Head)
            {
                var dummyLightPrefab = GetPrefab(_dummyLightGUID);
                PrefabUtility.InstantiatePrefab(dummyLightPrefab, nadeSystemObj.transform);
            }

            //HeadSystemのParentConstraintのオフセット更新を待つ
            headSystemObj.gameObject.SetActive(true);
            delayStart = (float)EditorApplication.timeSinceStartup;
            EditorApplication.update += DelayUpdate;

            Debug.Log(avatar.gameObject.name + ": Nadesystem install compleate!");
        }
        else
        {
            EditorUtility.DisplayDialog(Localize.notSelectAvatarTitle[langIndex], Localize.notSelectAvatarMsg[langIndex], "OK");
        }
    }

    private void DelayUpdate()
    {
        if(EditorApplication.timeSinceStartup >= delayStart + delayTime)
        {
            EditorApplication.update -= DelayUpdate;

            var nadeSystemObj = avatar.transform.Find("NadeSystem");
            var headSystemObj = nadeSystemObj.transform.Find("HeadSystem");

            headSystemObj.gameObject.SetActive(false);
        }
    }
}