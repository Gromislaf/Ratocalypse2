using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class EnemyAnimatorCreator
{
    const string AnimBase = "Assets/Cartoon Heroes/Male/3D/Animation Skeleton/Animations/";
    const string SavePath = "Assets/Ratocalypse/Enemy/EnemyAnimator.controller";

    [MenuItem("Ratocalypse/Create Enemy Animator")]
    static void Create()
    {
        AnimationClip clipIdle   = GetFirstClip(AnimBase + "Male_Animation@Male_Idle.FBX");
        AnimationClip clipWalk   = GetFirstClip(AnimBase + "Male_Animation@Male_Walk.FBX");
        AnimationClip clipAttack = GetFirstClip(AnimBase + "Male_Animation@Male_Attacks.FBX");
        AnimationClip clipStun   = GetFirstClip(AnimBase + "Male_Animation@Male_Damage.FBX");
        AnimationClip clipDead   = GetFirstClip(AnimBase + "Male_Animation@Male_Die.FBX");

        var controller = AnimatorController.CreateAnimatorControllerAtPath(SavePath);

        controller.AddParameter("Speed",  AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Stun",   AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Dead",   AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;

        // Locomotion BlendTree: Idle (Speed=0) → Walk (Speed=1)
        BlendTree locomotionTree;
        AnimatorState locomotionState = controller.CreateBlendTreeInController("Locomotion", out locomotionTree);
        locomotionTree.blendType = BlendTreeType.Simple1D;
        locomotionTree.blendParameter = "Speed";
        locomotionTree.useAutomaticThresholds = false;
        if (clipIdle != null)   locomotionTree.AddChild(clipIdle, 0f);
        if (clipWalk != null)   locomotionTree.AddChild(clipWalk, 1f);

        var attackState = sm.AddState("Attack", new Vector3(300, -100, 0));
        if (clipAttack != null) attackState.motion = clipAttack;

        var stunState = sm.AddState("Stun", new Vector3(300, 0, 0));
        if (clipStun != null)   stunState.motion = clipStun;

        var deadState = sm.AddState("Dead", new Vector3(300, 100, 0));
        if (clipDead != null)   deadState.motion = clipDead;

        sm.defaultState = locomotionState;

        // Any State → Dead (najwyższy priorytet — wpisany pierwszy)
        AddAnyTransition(sm, deadState,  "Dead",   transitionDuration: 0.1f);

        // Any State → Stun
        AddAnyTransition(sm, stunState,  "Stun",   transitionDuration: 0.1f);

        // Any State → Attack (Can Transition To Self OFF — nie przerywa trwającego ataku)
        AddAnyTransition(sm, attackState, "Attack", transitionDuration: 0.05f);

        // Attack → Locomotion
        var attackToLoco = attackState.AddTransition(locomotionState);
        attackToLoco.hasExitTime = true;
        attackToLoco.exitTime    = 0.9f;
        attackToLoco.duration    = 0.1f;

        // Stun → Locomotion
        var stunToLoco = stunState.AddTransition(locomotionState);
        stunToLoco.hasExitTime = true;
        stunToLoco.exitTime    = 1.0f;
        stunToLoco.duration    = 0.1f;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[EnemyAnimator] Controller zapisany: {SavePath}");
        Selection.activeObject = controller;
        EditorGUIUtility.PingObject(controller);
    }

    static void AddAnyTransition(AnimatorStateMachine sm, AnimatorState destination,
                                  string triggerName, float transitionDuration)
    {
        var t = sm.AddAnyStateTransition(destination);
        t.AddCondition(AnimatorConditionMode.If, 0, triggerName);
        t.hasExitTime         = false;
        t.duration            = transitionDuration;
        t.canTransitionToSelf = false;
    }

    static AnimationClip GetFirstClip(string fbxPath)
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        Debug.LogWarning($"[EnemyAnimator] Brak klipu w: {fbxPath}");
        return null;
    }
}
