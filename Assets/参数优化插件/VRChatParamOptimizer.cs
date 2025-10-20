#if UNITY_EDITOR
#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static VRC.SDKBase.VRC_AvatarParameterDriver;
public class BitParameterOptimizerWindow : EditorWindow
{
    private VRCExpressionParameters expressionParameters;
    private AnimatorController controller;
    private Vector2 scrollPosition;
    private List<ParameterInfo> parameters = new List<ParameterInfo>();
    private string newParamName = "";
    private int newParamMax = 1;
    private int selectedParam = -1;
    [MenuItem("Tools/VRChat Parameter Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<BitParameterOptimizerWindow>("Parameter Optimizer");
    }
    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("动画控制器", EditorStyles.boldLabel);
        controller = (AnimatorController)EditorGUILayout.ObjectField(controller, typeof(AnimatorController), false);
        if (controller == null)
        {
            EditorGUILayout.HelpBox("需要选择动画控制器", MessageType.Info);
            return;
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("菜单参数", EditorStyles.boldLabel);
        expressionParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(expressionParameters, typeof(VRCExpressionParameters), false);
        if (expressionParameters == null)
        {
            EditorGUILayout.HelpBox("需要选择菜单参数", MessageType.Info);
            return;
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("VRChat参数优化工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("\n此工具通过拆分int为多个bool来减少同步参数占用，不建议超过32会降低性能\n\n建议专门用一个动画控制器处理优化参数并使用MA合并到任意播放层\n\n可以直接为现有参数创建优化参数，你会收到已经存在参数的提示，如果没有其他无关参数就点击继续\n\n创建后的主参数（int）确保为不同步状态，拆分参数（bool）确保为同步状态\n", MessageType.Info);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawParameterList();
        EditorGUILayout.EndScrollView();
        DrawAddSection();
        if (GUILayout.Button("应用并创建动画层")) Generate();
    }
    void DrawParameterList()
    {
        EditorGUILayout.LabelField("优化的参数", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        if (parameters.Count == 0)
        {
            EditorGUILayout.HelpBox("没有添加任何参数", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("参数", GUILayout.Width(150));
        EditorGUILayout.LabelField("最大值", GUILayout.Width(80));
        EditorGUILayout.LabelField("占用", GUILayout.Width(50));
        EditorGUILayout.LabelField("拆分参数", GUILayout.Width(100));
        EditorGUILayout.LabelField("操作", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            int actualMax = (int)Mathf.Pow(2, param.bitCount) - 1;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(param.name, GUILayout.Width(150));
            EditorGUILayout.LabelField(actualMax.ToString(), GUILayout.Width(80));
            EditorGUILayout.LabelField(param.bitCount.ToString(), GUILayout.Width(50));
            EditorGUILayout.LabelField($"{param.name}_1 to {param.name}_{param.bitCount}", GUILayout.Width(100));
            if (GUILayout.Button("修改", GUILayout.Width(60)))
            {
                selectedParam = i;
                newParamName = param.name;
                newParamMax = param.maxValue;
            }
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                parameters.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
    void DrawAddSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("添加参数", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        if (selectedParam >= 0) EditorGUILayout.LabelField($"Editing: {parameters[selectedParam].name}", EditorStyles.boldLabel);
        newParamName = EditorGUILayout.TextField("参数", newParamName);
        newParamMax = EditorGUILayout.IntSlider("所需数量", newParamMax, 3, 128);
        int bitCount = CalculateBitCount(newParamMax);
        int actualMax = (int)Mathf.Pow(2, bitCount) - 1;
        EditorGUILayout.LabelField($"占用: {bitCount}");
        EditorGUILayout.LabelField($"最大值: {actualMax}");
        EditorGUILayout.LabelField($"拆分参数: {newParamName}_1 ~ {newParamName}_{bitCount}");
        EditorGUILayout.BeginHorizontal();
        if (selectedParam >= 0)
        {
            if (GUILayout.Button("保存"))
            {
                parameters[selectedParam] = new ParameterInfo
                {
                    name = newParamName,
                    maxValue = newParamMax,
                    bitCount = bitCount
                };
                selectedParam = -1;
            }
            if (GUILayout.Button("取消")) selectedParam = -1;
        }
        else
        {
            if (GUILayout.Button("添加参数"))
            {
                if (string.IsNullOrEmpty(newParamName))
                {
                    EditorUtility.DisplayDialog("错误", "没有参数名", "OK");
                    return;
                }
                if (parameters.Any(p => p.name == newParamName))
                {
                    EditorUtility.DisplayDialog("错误", "已存在参数", "OK");
                    return;
                }
                parameters.Add(new ParameterInfo
                {
                    name = newParamName,
                    maxValue = newParamMax,
                    bitCount = bitCount
                });
                newParamName = "";
                newParamMax = 1;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    void Generate()
    {
        if (parameters.Count < 1)
        {
            EditorUtility.DisplayDialog("提示", "没有添加参数", "OK");
            return;
        }
        try
        {
            List<string> conflicts = ControllerCheckForConflicts();
            bool overwrite = false;
            if (conflicts.Count > 0)
            {
                if (EditorUtility.DisplayDialog("已经存在参数", "以下参数已存在:\n\n" + string.Join("\n", conflicts) + "\n\n是否继续？继续将覆盖现有参数和动画层。", "继续", "取消")) overwrite = true;
                else return;
            }
            AssetDatabase.StartAssetEditing();
            ControllerAppendParameter("IsLocal", AnimatorControllerParameterType.Bool);
            foreach (var param in parameters)
            {
                if (overwrite)
                {
                    ControllerRemoveLayerIfExists($"{param.name}_Local");
                    ControllerRemoveLayerIfExists($"{param.name}_Remote");

                    ControllerRemoveParameterIfExists(param.name);
                    for (int i = 1; i <= param.bitCount; i++)
                    {
                        ControllerRemoveParameterIfExists($"{param.name}_{i}");
                    }
                }
                ControllerAppendParameter(param.name, AnimatorControllerParameterType.Int);
                for (int i = 1; i <= param.bitCount; i++)
                {
                    string boolName = $"{param.name}_{i}";
                    ControllerAppendParameter(boolName, AnimatorControllerParameterType.Bool);
                }
                ControllerCreateLocalLayer(param);
                ControllerCreateRemoteLayer(param);
            }
            ExpressionAppendParameters(overwrite);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("已完成", "动画层添加成功", "OK");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating animator layers: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
    int CalculateBitCount(int maxValue)
    {
        if (maxValue <= 1) return 1;
        return Mathf.CeilToInt(Mathf.Log(maxValue, 2));
    }
    void ExpressionAppendParameters(bool overwrite)
    {
        SerializedObject serializedObject = new SerializedObject(expressionParameters);
        SerializedProperty propParameters = serializedObject.FindProperty("parameters");
        Dictionary<string, SerializedProperty> existingParams = new Dictionary<string, SerializedProperty>();
        for (int i = 0; i < propParameters.arraySize; i++)
        {
            SerializedProperty paramProp = propParameters.GetArrayElementAtIndex(i);
            string name = paramProp.FindPropertyRelative("name").stringValue;
            existingParams[name] = paramProp;
        }
        foreach (var param in parameters)
        {
            ExpressionAppendParameter(propParameters, existingParams, new VRCExpressionParameters.Parameter
            {
                name = param.name,
                valueType = VRCExpressionParameters.ValueType.Int,
                defaultValue = 0,
                saved = false,
                networkSynced = false
            }, overwrite);
            for (int i = 1; i <= param.bitCount; i++)
            {
                ExpressionAppendParameter(propParameters, existingParams, new VRCExpressionParameters.Parameter
                {
                    name = $"{param.name}_{i}",
                    valueType = VRCExpressionParameters.ValueType.Bool,
                    defaultValue = 0,
                    saved = false,
                    networkSynced = true
                }, overwrite);
            }
        }
        int totalCost = expressionParameters.CalcTotalCost();
        if (totalCost > VRCExpressionParameters.MAX_PARAMETER_COST) EditorUtility.DisplayDialog("参数到达限制警告", $"目标参数({totalCost})超过上限({VRCExpressionParameters.MAX_PARAMETER_COST})", "OK");
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(expressionParameters);
        AssetDatabase.SaveAssetIfDirty(expressionParameters);
    }
    void ExpressionAppendParameter(SerializedProperty propParameters, Dictionary<string, SerializedProperty> existingParams, VRCExpressionParameters.Parameter newParam, bool overwrite)
    {
        if (existingParams.TryGetValue(newParam.name, out SerializedProperty existingProp))
        {
            if (overwrite)
            {
                existingProp.FindPropertyRelative("valueType").intValue = (int)newParam.valueType;
                existingProp.FindPropertyRelative("defaultValue").floatValue = newParam.defaultValue;
                existingProp.FindPropertyRelative("saved").boolValue = newParam.saved;
                existingProp.FindPropertyRelative("networkSynced").boolValue = newParam.networkSynced;
            }
        }
        else
        {
            int index = propParameters.arraySize;
            propParameters.arraySize++;
            SerializedProperty newProp = propParameters.GetArrayElementAtIndex(index);
            newProp.FindPropertyRelative("name").stringValue = newParam.name;
            newProp.FindPropertyRelative("valueType").intValue = (int)newParam.valueType;
            newProp.FindPropertyRelative("defaultValue").floatValue = newParam.defaultValue;
            newProp.FindPropertyRelative("saved").boolValue = newParam.saved;
            newProp.FindPropertyRelative("networkSynced").boolValue = newParam.networkSynced;
        }
    }
    List<string> ControllerCheckForConflicts()
    {
        List<string> conflicts = new List<string>();
        foreach (var param in parameters)
        {
            if (ControllerParameterExists(param.name)) conflicts.Add($"参数 '{param.name}' 已存在");
            for (int i = 1; i <= param.bitCount; i++)
            {
                string boolName = $"{param.name}_{i}";
                if (ControllerParameterExists(boolName)) conflicts.Add($"参数 '{boolName}' 已存在");
            }
            string localLayerName = $"{param.name}_Local";
            string remoteLayerName = $"{param.name}_Remote";
            if (ControllerLayerExists(localLayerName)) conflicts.Add($"动画层 '{localLayerName}' 已存在");
            if (ControllerLayerExists(remoteLayerName)) conflicts.Add($"动画层 '{remoteLayerName}' 已存在");
        }
        return conflicts;
    }
    void ControllerRemoveLayerIfExists(string layerName)
    {
        for (int i = 0; i < controller.layers.Length; i++)
        {
            if (controller.layers[i].name == layerName)
            {
                AnimatorStateMachine stateMachine = controller.layers[i].stateMachine;
                controller.RemoveLayer(i);
                if (stateMachine != null && AssetDatabase.Contains(stateMachine)) UnityEngine.Object.DestroyImmediate(stateMachine, true);
                return;
            }
        }
    }
    void ControllerRemoveParameterIfExists(string name)
    {
        var parametersList = controller.parameters.ToList();
        int index = parametersList.FindIndex(p => p.name == name);
        if (index >= 0)
        {
            parametersList.RemoveAt(index);
            controller.parameters = parametersList.ToArray();
        }
    }
    bool ControllerLayerExists(string layerName)
    {
        return controller.layers.Any(l => l.name == layerName);
    }
    bool ControllerParameterExists(string name)
    {
        return controller.parameters.Any(p => p.name == name);
    }
    void ControllerAppendParameter(string name, AnimatorControllerParameterType type)
    {
        var existingParam = controller.parameters.FirstOrDefault(p => p.name == name);

        if (existingParam != null)
        {
            if (existingParam.type != type)
            {
                ControllerRemoveParameterIfExists(name);
                controller.AddParameter(name, type);
            }
        }
        else controller.AddParameter(name, type);
    }

    void ControllerCreateLocalLayer(ParameterInfo param)
    {
        string layerName = $"{param.name}_Local";
        ControllerRemoveLayerIfExists(layerName);
        AnimatorStateMachine stateMachine = new AnimatorStateMachine();
        stateMachine.name = layerName + "_SM";
        AssetDatabase.AddObjectToAsset(stateMachine, AssetDatabase.GetAssetPath(controller));
        stateMachine.hideFlags = HideFlags.HideInHierarchy;
        AnimatorControllerLayer layer = new AnimatorControllerLayer
        {
            name = layerName,
            stateMachine = stateMachine,
            defaultWeight = 0f
        };
        controller.AddLayer(layer);
        stateMachine.states = new ChildAnimatorState[0];
        stateMachine.anyStateTransitions = new AnimatorStateTransition[0];
        stateMachine.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        AnimatorState defaultState = stateMachine.AddState("IsRemote");
        defaultState.writeDefaultValues = false;
        AnimatorStateTransition defaultTransition = stateMachine.AddAnyStateTransition(defaultState);
        defaultTransition.canTransitionToSelf = false;
        defaultTransition.AddCondition(AnimatorConditionMode.IfNot, 1, "IsLocal");
        int maxValue = (int)Mathf.Pow(2, param.bitCount) - 1;
        for (int value = 0; value <= maxValue; value++)
        {
            string stateName = $"{param.name} = {value}";
            AnimatorState state = stateMachine.AddState(stateName);
            state.writeDefaultValues = false;
            VRCAvatarParameterDriver driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<Parameter>();
            for (int bit = 0; bit < param.bitCount; bit++)
            {
                bool bitValue = ((value >> bit) & 1) == 1;
                driver.parameters.Add(new Parameter
                {
                    name = $"{param.name}_{bit + 1}",
                    type = ChangeType.Set,
                    value = bitValue ? 1 : 0
                });
            }
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state);
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 1, "IsLocal");
            transition.AddCondition(AnimatorConditionMode.Equals, value, param.name);
            transition.hasExitTime = false;
            transition.duration = 0;
        }
    }
    void ControllerCreateRemoteLayer(ParameterInfo param)
    {
        string layerName = $"{param.name}_Remote";
        ControllerRemoveLayerIfExists(layerName);
        AnimatorStateMachine stateMachine = new AnimatorStateMachine();
        stateMachine.name = layerName + "_SM";
        AssetDatabase.AddObjectToAsset(stateMachine, AssetDatabase.GetAssetPath(controller));
        stateMachine.hideFlags = HideFlags.HideInHierarchy;
        AnimatorControllerLayer layer = new AnimatorControllerLayer
        {
            name = layerName,
            stateMachine = stateMachine,
            defaultWeight = 0f
        };
        controller.AddLayer(layer);
        stateMachine.states = new ChildAnimatorState[0];
        stateMachine.anyStateTransitions = new AnimatorStateTransition[0];
        stateMachine.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        AnimatorState defaultState = stateMachine.AddState("IsLocal");
        defaultState.writeDefaultValues = false;
        AnimatorStateTransition defaultTransition = stateMachine.AddAnyStateTransition(defaultState);
        defaultTransition.canTransitionToSelf = false;
        defaultTransition.AddCondition(AnimatorConditionMode.If, 1, "IsLocal");
        int maxValue = (int)Mathf.Pow(2, param.bitCount) - 1;
        for (int value = 0; value <= maxValue; value++)
        {
            string stateName = $"{param.name} = {value}";
            AnimatorState state = stateMachine.AddState(stateName);
            state.writeDefaultValues = false;
            VRCAvatarParameterDriver driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<Parameter>
            {
                new Parameter
                {
                    name = param.name,
                    type = ChangeType.Set,
                    value = value
                }
            };
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state);
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.IfNot, 1, "IsLocal");
            for (int bit = 0; bit < param.bitCount; bit++)
            {
                bool bitValue = ((value >> bit) & 1) == 1;
                transition.AddCondition(bitValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, $"{param.name}_{bit + 1}");
            }
            transition.hasExitTime = false;
            transition.duration = 0;
        }
    }
    private struct ParameterInfo
    {
        public string name;
        public int maxValue;
        public int bitCount;
    }
}
#endif
#endif