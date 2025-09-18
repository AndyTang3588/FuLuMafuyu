using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using static VRC.SDKBase.VRC_AnimatorTrackingControl;
using VRC.SDK3.Avatars.ScriptableObjects;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;

public class UdekumiAutoEditor : EditorWindow
{
    [MenuItem("PLink/UdekumiAutoEditor")]

    public static void ShowWindow()
	{
		EditorWindow.GetWindow<UdekumiAutoEditor>(true, "UdekumiAutoEditor");
    }

    private VRCAvatarDescriptor avatar;
    private AnimationClip standingAnimation;
	private void OnGUI()
	{
		//自动化导入双臂交叉和牵手手机制
        GUILayout.Label("本Editor是用于自动导入双臂交叉和牵手手机制的工具。");
        GUILayout.Label("请按照以下步骤执行。");
        GUILayout.Label("　1. Avatar的栏中输入要添加双臂交叉和牵手手机制的Avatar（必选）");
        GUILayout.Label("　2. standing pose的栏中输入站立姿势（可选）");
        GUILayout.Label("　   ※如果不设置，则使用默认姿势");
        GUILayout.Label("　3. 点击「Add双臂交叉过渡到Base Layer」按钮");
        EditorGUILayout.Space();
        avatar = EditorGUILayout.ObjectField("Avatar", avatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
        standingAnimation = EditorGUILayout.ObjectField("standing pose", standingAnimation, typeof(AnimationClip), true) as AnimationClip;
        if (standingAnimation == null)
        {
            standingAnimation = AssetDatabase.LoadAssetAtPath("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/ProxyAnim/proxy_stand_still.anim", typeof(AnimationClip)) as AnimationClip;
        } 
        if (GUILayout.Button("Add双臂交叉过渡到Base Layer"))
        {
	        AddBaseLayerState();
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //可爱姿势对应用（import package版）
        GUILayout.Label("＊可爱姿势（通过 import package 导入）利用的场合下，请点击下面的按钮", EditorStyles.boldLabel);
        if (GUILayout.Button("Add双臂交叉过渡到可爱姿势（import package版）"))
        {
	        AddKawaiiPoseState_ip();
        }
        EditorGUILayout.Space();

        //可爱姿势对应用（通过 VCC 导入）
        GUILayout.Label("＊可爱姿势（通过 VCC 导入）利用的场合下，请点击下面的按钮", EditorStyles.boldLabel);
        if (GUILayout.Button("Add双臂交叉过渡到可爱姿势（通过 VCC 导入）"))
        {
	        AddKawaiiPoseState_vcc();
        }
        EditorGUILayout.Space();

        //GoGo Loco 对应用（通过 GoGo Loco 导入）
        GUILayout.Label("＊GoGo Loco （通过 GoGo Loco 导入）利用的场合下，请点击下面的按钮", EditorStyles.boldLabel);
        if (GUILayout.Button("Add双臂交叉过渡到GoGo Loco"))
        {
	        AddGoGoLocoState();
        }
    }

    private void SetTracking(AnimatorState state, TrackingType type) {
        var tracking = state.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
        Undo.RegisterCreatedObjectUndo(tracking, "VRCAnimatorTrackingControl Created");
        Undo.RecordObject(tracking, "VRCAnimatorTrackingControl Configured");
        tracking.trackingHead = TrackingType.Tracking;
        tracking.trackingLeftHand = TrackingType.Tracking;
        tracking.trackingRightHand = type;
        tracking.trackingHip = TrackingType.Tracking;
        tracking.trackingLeftFoot = TrackingType.Tracking;
        tracking.trackingRightFoot = TrackingType.Tracking;
        tracking.trackingLeftFingers = TrackingType.Tracking;
        tracking.trackingRightFingers = type;
        tracking.trackingEyes = TrackingType.Tracking;
        tracking.trackingMouth = TrackingType.Tracking;
        EditorUtility.SetDirty(tracking);
    }

    private void AddBaseLayerState()
    {   
        AnimatorController animatorController = avatar.baseAnimationLayers[0].animatorController as AnimatorController;
        VRCExpressionParameters expressionParameter = avatar.expressionParameters as VRCExpressionParameters;
        int sameStateCount1 = 0;
        int stateNum1 = 0;
        for (int stateCount = 0; stateCount < animatorController.layers[0].stateMachine.states.Length; stateCount++)
        {
            if (animatorController.layers[0].stateMachine.states[stateCount].state.name == "双臂交叉") 
            {
                sameStateCount1++;
                stateNum1 = stateCount;
            }
        }
        
        if (sameStateCount1 != 0)
        {
            if (sameStateCount1 != 0)
            {
                animatorController.layers[0].stateMachine.RemoveState(animatorController.layers[0].stateMachine.states[stateNum1].state);
            }
            for (int stateCount = 0; stateCount < animatorController.layers[0].stateMachine.states.Length; stateCount++)
            {
                if (animatorController.layers[0].stateMachine.states[stateCount].state.name == "取消双臂交叉") 
                {
                    animatorController.layers[0].stateMachine.RemoveState(animatorController.layers[0].stateMachine.states[stateCount].state);
                }
            }
        } else
        {
            VRCExpressionParameters newExpressionParameter = CreateInstance<VRCExpressionParameters>();
            int exParaLength = expressionParameter.parameters.Length;
            newExpressionParameter.parameters = new ExpressionParameter[exParaLength + 2];
            for (int paramCount = 0; paramCount < exParaLength; paramCount++)
            {
                newExpressionParameter.parameters[paramCount] = expressionParameter.parameters[paramCount];
            }
            newExpressionParameter.parameters[exParaLength] = new ExpressionParameter();
            newExpressionParameter.parameters[exParaLength].name = "双臂交叉ON";
            newExpressionParameter.parameters[exParaLength].valueType = ExpressionParameters.ValueType.Bool;

            newExpressionParameter.parameters[exParaLength + 1] = new ExpressionParameter();
            newExpressionParameter.parameters[exParaLength + 1].name = "牵手ON";
            newExpressionParameter.parameters[exParaLength + 1].valueType = ExpressionParameters.ValueType.Bool;
            expressionParameter.parameters = newExpressionParameter.parameters;

            animatorController.AddParameter("双臂交叉ON", AnimatorControllerParameterType.Bool);
            animatorController.AddParameter("牵手ON", AnimatorControllerParameterType.Bool);
        }
        AnimatorControllerLayer baselayer = animatorController.layers[0];
        var defaultstate = baselayer.stateMachine.defaultState;
        var newstate = baselayer.stateMachine.AddState("双臂交叉");
        newstate.motion = standingAnimation;
        newstate.writeDefaultValues = false;

        var newstateOff = baselayer.stateMachine.AddState("取消双臂交叉");
        newstateOff.motion = standingAnimation;
        newstateOff.writeDefaultValues = false;

        //VRCAnimatorTrackingControl 设置
        SetTracking(newstate, TrackingType.Animation);
        SetTracking(newstateOff, TrackingType.Tracking);

        //条件1: 双臂交叉参数开启
        var transitionNewstate1 = baselayer.stateMachine.AddAnyStateTransition(newstate);
        transitionNewstate1.hasExitTime = false;
        transitionNewstate1.duration = 0;
        transitionNewstate1.AddCondition(AnimatorConditionMode.If, 0, "双臂交叉ON");
        //条件2: 牵手参数开启
        var transitionNewstate2 = baselayer.stateMachine.AddAnyStateTransition(newstate);
        transitionNewstate2.hasExitTime = false;
        transitionNewstate2.duration = 0;
        transitionNewstate2.AddCondition(AnimatorConditionMode.If, 0, "牵手ON");
        //条件3: 双臂交叉参数OFF & 牵手参数OFF
        var transitionNewstate3 = newstate.AddTransition(newstateOff);
        transitionNewstate3.hasExitTime = false;
        transitionNewstate3.duration = 0;
        transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "双臂交叉ON");
        transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "牵手ON");
        //条件4: 追踪解除后（自动过渡）
        var transitionNewstate4 = newstateOff.AddTransition(defaultstate);
        transitionNewstate4.hasExitTime = true;
        transitionNewstate4.exitTime = 0;
        transitionNewstate4.duration = 0;
    }

    private void AddKawaiiPoseState_ip()
    {   
        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath("Assets/UniSakiStudio/PosingSystem/PosingSystem_Locomotion.controller", typeof(AnimatorController)) as AnimatorController;  
        int locomotionNum = 0;
        int trackingNum = 0;
        int sameLayerCount = 0;
        for (int layerCount = 0; layerCount < animatorController.layers.Length; layerCount++)
        {
            if (animatorController.layers[layerCount].name == "USSPS_Locomotion") 
            {
                sameLayerCount++;
                locomotionNum = layerCount;
            }
            if (animatorController.layers[layerCount].name == "USSPS_Tracking Control") trackingNum = layerCount;
        }
        int sameStateCount = 0;
        int duringActionNum = 0;
        int stateNum = 0;
        if (sameLayerCount != 0)
        {
            for (int stateCount = 0; stateCount < animatorController.layers[locomotionNum].stateMachine.states.Length; stateCount++)
            {
                if (animatorController.layers[locomotionNum].stateMachine.states[stateCount].state.name == "双臂交叉") 
                {
                    sameStateCount++;
                    stateNum = stateCount;
                }
            }

            for (int stateCount = 0; stateCount < animatorController.layers[trackingNum].stateMachine.states.Length; stateCount++)
            {
                if (animatorController.layers[trackingNum].stateMachine.states[stateCount].state.name == "During Action") duringActionNum = stateCount;
            }
        }
        if (sameStateCount == 0)
        {
            animatorController.AddParameter("双臂交叉ON", AnimatorControllerParameterType.Bool);
            animatorController.AddParameter("牵手ON", AnimatorControllerParameterType.Bool);
            AnimatorControllerLayer locomotionlayer = animatorController.layers[locomotionNum];
            AnimatorControllerLayer trackinglayer = animatorController.layers[trackingNum];
            var newstate = locomotionlayer.stateMachine.AddState("双臂交叉");
            newstate.motion = standingAnimation;
            newstate.writeDefaultValues = false;

            //VRCAnimatorTrackingControl 设置
            SetTracking(newstate, TrackingType.Animation);

            //条件1: 双臂交叉参数开启
            var transitionNewstate1 = locomotionlayer.stateMachine.AddAnyStateTransition(newstate);
            transitionNewstate1.hasExitTime = false;
            transitionNewstate1.duration = 0;
            transitionNewstate1.AddCondition(AnimatorConditionMode.If, 0, "双臂交叉ON");
            //条件2: 牵手参数开启
            var transitionNewstate2 = locomotionlayer.stateMachine.AddAnyStateTransition(newstate);
            transitionNewstate2.hasExitTime = false;
            transitionNewstate2.duration = 0;
            transitionNewstate2.AddCondition(AnimatorConditionMode.If, 0, "牵手ON");
            //条件3: 腕组参数OFF & 牵手参数OFF
            var transitionNewstate3 = newstate.AddExitTransition();
            transitionNewstate3.hasExitTime = false;
            transitionNewstate3.duration = 0;
            transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "双臂交叉ON");
            transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "牵手ON");

            //tracking 层相关设置
            var duringActionState = animatorController.layers[trackingNum].stateMachine.states[duringActionNum].state;
            //条件1: 双臂交叉参数开启
            var transitionDuringActionState1 = trackinglayer.stateMachine.AddAnyStateTransition(duringActionState);
            transitionDuringActionState1.hasExitTime = false;
            transitionDuringActionState1.duration = 0;
            transitionDuringActionState1.AddCondition(AnimatorConditionMode.If, 0, "双臂交叉ON");
            //条件2: 牵手参数开启
            var transitionDuringActionState2 = trackinglayer.stateMachine.AddAnyStateTransition(duringActionState);
            transitionDuringActionState2.hasExitTime = false;
            transitionDuringActionState2.duration = 0;
            transitionDuringActionState2.AddCondition(AnimatorConditionMode.If, 0, "牵手ON");
            //条件3: 腕组参数OFF & 牵手参数OFF
            var transitionOutOfActionState = duringActionState.transitions[0];
            transitionOutOfActionState.AddCondition(AnimatorConditionMode.IfNot, 0, "双臂交叉ON");
            transitionOutOfActionState.AddCondition(AnimatorConditionMode.IfNot, 0, "牵手ON");
        } else
        {
            animatorController.layers[locomotionNum].stateMachine.states[stateNum].state.motion = standingAnimation;
        }
    }

    private void AddKawaiiPoseState_vcc()
    {   
        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath("Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller", typeof(AnimatorController)) as AnimatorController;
        int locomotionNum = 0;
        int trackingNum = 0;
        int sameLayerCount = 0;
        for (int layerCount = 0; layerCount < animatorController.layers.Length; layerCount++)
        {
            if (animatorController.layers[layerCount].name == "USSPS_Locomotion") 
            {
                sameLayerCount++;
                locomotionNum = layerCount;
            }
            if (animatorController.layers[layerCount].name == "USSPS_Tracking Control") trackingNum = layerCount;
        }
        int sameStateCount = 0;
        int duringActionNum = 0;
        int stateNum = 0;
        if (sameLayerCount != 0)
        {
            for (int stateCount = 0; stateCount < animatorController.layers[locomotionNum].stateMachine.states.Length; stateCount++)
            {
                if (animatorController.layers[locomotionNum].stateMachine.states[stateCount].state.name == "双臂交叉") 
                {
                    sameStateCount++;
                    stateNum = stateCount;
                }
            }

            for (int stateCount = 0; stateCount < animatorController.layers[trackingNum].stateMachine.states.Length; stateCount++)
            {
                if (animatorController.layers[trackingNum].stateMachine.states[stateCount].state.name == "During Action") duringActionNum = stateCount;
            }
        }
        if (sameStateCount == 0)
        {
            animatorController.AddParameter("双臂交叉ON", AnimatorControllerParameterType.Bool);
            animatorController.AddParameter("牵手ON", AnimatorControllerParameterType.Bool);
            AnimatorControllerLayer locomotionlayer = animatorController.layers[locomotionNum];
            AnimatorControllerLayer trackinglayer = animatorController.layers[trackingNum];
            var newstate = locomotionlayer.stateMachine.AddState("双臂交叉");
            newstate.motion = standingAnimation;
            newstate.writeDefaultValues = false;

            //VRCAnimatorTrackingControl 设置
            SetTracking(newstate, TrackingType.Animation);

            //条件1: 双臂交叉参数开启
            var transitionNewstate1 = locomotionlayer.stateMachine.AddAnyStateTransition(newstate);
            transitionNewstate1.hasExitTime = false;
            transitionNewstate1.duration = 0;
            transitionNewstate1.AddCondition(AnimatorConditionMode.If, 0, "双臂交叉ON");
            //条件2: 牵手参数开启
            var transitionNewstate2 = locomotionlayer.stateMachine.AddAnyStateTransition(newstate);
            transitionNewstate2.hasExitTime = false;
            transitionNewstate2.duration = 0;
            transitionNewstate2.AddCondition(AnimatorConditionMode.If, 0, "牵手ON");
            //条件3: 腕组参数OFF & 牵手参数OFF
            var transitionNewstate3 = newstate.AddExitTransition();
            transitionNewstate3.hasExitTime = false;
            transitionNewstate3.duration = 0;
            transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "双臂交叉ON");
            transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "牵手ON");

            //tracking 层相关设置
            var duringActionState = animatorController.layers[trackingNum].stateMachine.states[duringActionNum].state;
            //条件1: 双臂交叉参数开启
            var transitionDuringActionState1 = trackinglayer.stateMachine.AddAnyStateTransition(duringActionState);
            transitionDuringActionState1.hasExitTime = false;
            transitionDuringActionState1.duration = 0;
            transitionDuringActionState1.AddCondition(AnimatorConditionMode.If, 0, "双臂交叉ON");
            //条件2: 牵手参数开启
            var transitionDuringActionState2 = trackinglayer.stateMachine.AddAnyStateTransition(duringActionState);
            transitionDuringActionState2.hasExitTime = false;
            transitionDuringActionState2.duration = 0;
            transitionDuringActionState2.AddCondition(AnimatorConditionMode.If, 0, "牵手ON");
            //条件3: 腕组参数OFF & 牵手参数OFF
            var transitionOutOfActionState = duringActionState.transitions[0];
            transitionOutOfActionState.AddCondition(AnimatorConditionMode.IfNot, 0, "双臂交叉ON");
            transitionOutOfActionState.AddCondition(AnimatorConditionMode.IfNot, 0, "牵手ON");
        } else
        {
            animatorController.layers[locomotionNum].stateMachine.states[stateNum].state.motion = standingAnimation;
        }
    }

    private void AddGoGoLocoState()
    {   
        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/ControllersWD/GoLocoBaseWD.controller", typeof(AnimatorController)) as AnimatorController;
        AnimatorController av3LocoController = AssetDatabase.LoadAssetAtPath("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/Controllers/vrc_AvatarV3LocomotionLayer.controller", typeof(AnimatorController)) as AnimatorController;
        int locomotionNum = 0;
        int sameLayerCount = 0;
        for (int layerCount = 0; layerCount < animatorController.layers.Length; layerCount++)
        {
            if (animatorController.layers[layerCount].name == "Locomotion") 
            {
                locomotionNum = layerCount;
                sameLayerCount++;
            }
        }
        int sameStateCount = 0;
        int subStateMachineNum = 0;
        int gogolocoStateNum = 0;
        int av3LocoStateNum = 0;
        if (sameLayerCount != 0)
        {
            for (int subStateMachineCount = 0; subStateMachineCount < animatorController.layers[locomotionNum].stateMachine.stateMachines.Length; subStateMachineCount++)
            {
                if (animatorController.layers[locomotionNum].stateMachine.stateMachines[subStateMachineCount].stateMachine.name == "( 3-4 pt )") subStateMachineNum = subStateMachineCount;
            }

            for (int stateCount = 0; stateCount < animatorController.layers[locomotionNum].stateMachine.stateMachines[subStateMachineNum].stateMachine.states.Length; stateCount++)
            {
                if (animatorController.layers[locomotionNum].stateMachine.stateMachines[subStateMachineNum].stateMachine.states[stateCount].state.name == "双臂交叉") 
                {
                    sameStateCount++;
                    gogolocoStateNum = stateCount;
                }
            }

            for (int stateCount = 0; stateCount < av3LocoController.layers[0].stateMachine.states.Length; stateCount++)
            {
                if (av3LocoController.layers[0].stateMachine.states[stateCount].state.name == "双臂交叉") av3LocoStateNum = stateCount;
            }
        }
        if (sameStateCount == 0)
        {
            //GoGo Loco本体 BaseLayer修正
            animatorController.AddParameter("双臂交叉ON", AnimatorControllerParameterType.Bool);
            animatorController.AddParameter("牵手ON", AnimatorControllerParameterType.Bool);
            AnimatorControllerLayer locomotionlayer = animatorController.layers[locomotionNum];
            var newstate = locomotionlayer.stateMachine.stateMachines[subStateMachineNum].stateMachine.AddState("双臂交叉");
            newstate.motion = standingAnimation;
            newstate.writeDefaultValues = true;

            var newstateOff = locomotionlayer.stateMachine.stateMachines[subStateMachineNum].stateMachine.AddState("取消双臂交叉");
            newstateOff.motion = standingAnimation;
            newstateOff.writeDefaultValues = true;

            //VRCAnimatorTrackingControl 设置
            SetTracking(newstate, TrackingType.Animation);
            SetTracking(newstateOff, TrackingType.Tracking);

            //条件1: 双臂交叉参数开启
            var transitionNewstate1 = locomotionlayer.stateMachine.AddAnyStateTransition(newstate);
            transitionNewstate1.hasExitTime = false;
            transitionNewstate1.duration = 0;
            transitionNewstate1.AddCondition(AnimatorConditionMode.If, 0, "双臂交叉ON");
            //条件2: 牵手参数开启
            var transitionNewstate2 = locomotionlayer.stateMachine.AddAnyStateTransition(newstate);
            transitionNewstate2.hasExitTime = false;
            transitionNewstate2.duration = 0;
            transitionNewstate2.AddCondition(AnimatorConditionMode.If, 0, "牵手ON");
            //条件3: 腕组参数OFF & 牵手参数OFF
            var transitionNewstate3 = newstate.AddTransition(newstateOff);
            transitionNewstate3.hasExitTime = false;
            transitionNewstate3.duration = 0;
            transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "双臂交叉ON");
            transitionNewstate3.AddCondition(AnimatorConditionMode.IfNot, 0, "牵手ON");
            //条件4: 追踪解除后（自动过渡）
            var transitionNewstate4 = newstateOff.AddTransition(locomotionlayer.stateMachine);
            transitionNewstate4.hasExitTime = true;
            transitionNewstate4.exitTime = 1;
            transitionNewstate4.duration = 0.2f;


            //VRC AV3 sample LocomotionLayer修正
            av3LocoController.AddParameter("双臂交叉ON", AnimatorControllerParameterType.Bool);
            av3LocoController.AddParameter("牵手ON", AnimatorControllerParameterType.Bool);
            AnimatorControllerLayer baselayer = av3LocoController.layers[0];
            var defaultstate = baselayer.stateMachine.defaultState;
            var newAv3state = baselayer.stateMachine.AddState("双臂交叉");
            newAv3state.motion = standingAnimation;
            newAv3state.writeDefaultValues = false;

            //VRCAnimatorTrackingControl 设置
            SetTracking(newAv3state, TrackingType.Animation);

            //条件1: 双臂交叉参数开启
            var transitionNewAv3state1 = baselayer.stateMachine.AddAnyStateTransition(newAv3state);
            transitionNewAv3state1.hasExitTime = false;
            transitionNewAv3state1.duration = 0;
            transitionNewAv3state1.AddCondition(AnimatorConditionMode.If, 0, "双臂交叉ON");
            //条件2: 牵手参数开启
            var transitionNewAv3state2 = baselayer.stateMachine.AddAnyStateTransition(newAv3state);
            transitionNewAv3state2.hasExitTime = false;
            transitionNewAv3state2.duration = 0;
            transitionNewAv3state2.AddCondition(AnimatorConditionMode.If, 0, "牵手ON");
            //条件3: 双臂交叉参数OFF & 牵手参数OFF
            var transitionNewAv3state3 = newAv3state.AddTransition(defaultstate);
            transitionNewAv3state3.hasExitTime = false;
            transitionNewAv3state3.duration = 0;
            transitionNewAv3state3.AddCondition(AnimatorConditionMode.IfNot, 0, "双臂交叉ON");
            transitionNewAv3state3.AddCondition(AnimatorConditionMode.IfNot, 0, "牵手ON");
        } else
        {
            animatorController.layers[locomotionNum].stateMachine.stateMachines[subStateMachineNum].stateMachine.states[gogolocoStateNum].state.motion = standingAnimation;
            animatorController.layers[locomotionNum].stateMachine.stateMachines[subStateMachineNum].stateMachine.states[gogolocoStateNum + 1].state.motion = standingAnimation;
            av3LocoController.layers[0].stateMachine.states[av3LocoStateNum].state.motion = standingAnimation;
        }
    }
}