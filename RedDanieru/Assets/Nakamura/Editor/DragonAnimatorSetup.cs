using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

//ドラゴン（DragonUsurper）のアニメーターを作り、Dragon.prefab につなぐ
//メニュー「Tools/Dragon/アニメーターを作り直す」で何度でも作り直せる
public static class DragonAnimatorSetup
{
    private const string ControllerPath = "Assets/Nakamura/Animator/Dragon.controller";
    private const string DragonPrefabPath = "Assets/Nakamura/Prefab/Dragon.prefab";
    private const string StickerDisplayPrefabPath = "Assets/Nakamura/Prefab/StickerDisplay.prefab";
    private const string FireBreathPrefabPath = "Assets/Hun0FX/FireBreathFan/FX_Fire_06_Fan.prefab";
    private const string ClipFolder = "Assets/Nakamura/Model/FourEvilDragonsPBR/Animations/DragonUsurper/";

    //踏みつけ：attackHand（前に約8m飛び込んで跳ぶ）から前後左右の移動を消して、その場で跳ぶようにしたもの
    private const string StompInPlacePath = "Assets/Nakamura/Animator/Dragon_StompInPlace.anim";
    private const float StompTrimStart = 0.4f;   //ここまではほとんど動かないので切る
    private const float StompTrimEnd = 2.45f;    //ここから後ろへ跳んで戻るので切る
    private const string RootBonePath = "Root";  //前に飛び込む動きが入っている骨

    [MenuItem("Tools/Dragon/アニメーターを作り直す")]
    public static void Setup()
    {
        AnimatorController controller = BuildController();
        if (controller == null)
            return;

        SetupDragonPrefab(controller);
        SetupStickerDisplay();

        AssetDatabase.SaveAssets();
        Debug.Log("DragonAnimatorSetup: ドラゴンのアニメーターを作り、Dragon.prefab と StickerDisplay.prefab につなぎました。");
    }

    //-----アニメーター-----

    private static AnimatorController BuildController()
    {
        AnimationClip idle = LoadClip("idle01");
        AnimationClip walk = LoadClip("Walk");
        AnimationClip lookAround = LoadClip("idle02");
        AnimationClip bite = LoadClip("attackMouth");
        AnimationClip stomp = BuildStompInPlace(LoadClip("attackHand"));
        AnimationClip breath = LoadClip("attackFlame");
        AnimationClip getHit = LoadClip("getHit");
        AnimationClip defend = LoadClip("Defend");
        AnimationClip die = LoadClip("Die");
        AnimationClip scream = LoadClip("Scream");
        AnimationClip run = LoadClip("Run");

        if (new[] { idle, walk, lookAround, bite, stomp, breath, getHit, defend, die, scream, run }.Any(c => c == null))
        {
            Debug.LogError("DragonAnimatorSetup: DragonUsurper のモーションが見つかりません: " + ClipFolder);
            return null;
        }

        //作り直すときは、同じファイルの中身を空にして使う（GUID を変えないため）
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }
        else
        {
            foreach (AnimatorControllerParameter p in controller.parameters)
                controller.RemoveParameter(p);
        }

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState s in sm.states)
            sm.RemoveState(s.state);
        foreach (AnimatorStateTransition t in sm.anyStateTransitions)
            sm.RemoveAnyStateTransition(t);

        //パラメーター（ほかの敵と同じ名前＋ドラゴン用）
        controller.AddParameter("Walk", AnimatorControllerParameterType.Bool);
        controller.AddParameter("PlayerLost", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Bite", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Stomp", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Breath", AnimatorControllerParameterType.Trigger);
        controller.AddParameter(new AnimatorControllerParameter { name = "ActionSpeed", type = AnimatorControllerParameterType.Float, defaultFloat = 1f });
        controller.AddParameter("Damage", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Stun", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("StunEnd", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Charge", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("ChargeDash", AnimatorControllerParameterType.Bool);
        controller.AddParameter("SpecialStop", AnimatorControllerParameterType.Trigger);

        //ステート
        AnimatorState idleState = AddState(sm, "Idle", idle, 0, 0);
        AnimatorState walkState = AddState(sm, "Walk", walk, 1, 0);
        AnimatorState lostState = AddState(sm, "PlayerLost", lookAround, 0, 1);
        AnimatorState biteState = AddState(sm, "Bite", bite, 3, -2);
        AnimatorState stompState = AddState(sm, "Stomp", stomp, 3, -1);
        AnimatorState breathState = AddState(sm, "Breath", breath, 3, 0);
        AnimatorState damageState = AddState(sm, "Damage", getHit, 3, 1);
        AnimatorState stunState = AddState(sm, "Stun", defend, 3, 2);
        AnimatorState deathState = AddState(sm, "Death", die, 3, 3);
        AnimatorState chargeState = AddState(sm, "Charge", scream, -2, 2);
        AnimatorState dashState = AddState(sm, "ChargeDash", run, -2, 3);
        sm.defaultState = idleState;

        //攻撃は当たる瞬間に合わせて速さを変える
        foreach (AnimatorState s in new[] { biteState, stompState, breathState })
        {
            s.speedParameterActive = true;
            s.speedParameter = "ActionSpeed";
        }

        //待機・歩き・見失い
        Transition(idleState, lostState, 0.25f, null, If("PlayerLost"));
        Transition(walkState, lostState, 0.25f, null, If("PlayerLost"));
        Transition(lostState, idleState, 0.25f, null, IfNot("PlayerLost"));
        Transition(idleState, walkState, 0.2f, null, If("Walk"));
        Transition(walkState, idleState, 0.2f, null, IfNot("Walk"));

        //攻撃（どこからでも入る。連続で出ても最初から再生し直す）
        AnyState(sm, biteState, 0.1f, true, If("Bite"));
        AnyState(sm, biteState, 0.1f, true, If("Attack"));
        AnyState(sm, stompState, 0.1f, true, If("Stomp"));
        AnyState(sm, breathState, 0.15f, true, If("Breath"));
        //待機に戻る途中でも、次の攻撃やダメージ（Any State から）が来たらすぐ切り替える
        Transition(biteState, idleState, 0.15f, 0.95f).interruptionSource = TransitionInterruptionSource.Source;
        Transition(stompState, idleState, 0.2f, 0.95f).interruptionSource = TransitionInterruptionSource.Source;
        Transition(breathState, idleState, 0.2f, 0.95f).interruptionSource = TransitionInterruptionSource.Source;
        Transition(breathState, idleState, 0.2f, null, If("SpecialStop"));

        //やられ
        AnyState(sm, damageState, 0.05f, true, If("Damage"));
        Transition(damageState, idleState, 0.2f, 0.6f);
        AnyState(sm, stunState, 0.1f, false, If("Stun"));
        Transition(stunState, idleState, 0.2f, null, If("StunEnd"));
        AnyState(sm, deathState, 0.1f, false, If("Death"));

        //突進ステッカー（溜めて吠える → 走る）
        AnyState(sm, chargeState, 0.15f, false, If("Charge"));
        Transition(chargeState, dashState, 0.15f, null, If("ChargeDash"));
        Transition(chargeState, idleState, 0.2f, null, If("SpecialStop"));
        Transition(dashState, idleState, 0.2f, null, IfNot("ChargeDash"));

        EditorUtility.SetDirty(controller);
        return controller;
    }

    //attackHand から、その場で跳ぶ踏みつけを作る
    //・StompTrimStart〜StompTrimEnd だけを切り出して 0 秒始まりにする
    //・Root の骨の前後左右（x, z）は最初の位置のまま動かさない（高さ y と回転、手足はそのまま）
    private static AnimationClip BuildStompInPlace(AnimationClip source)
    {
        if (source == null)
            return null;

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(StompInPlacePath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, StompInPlacePath);
        }

        clip.ClearCurves();
        clip.frameRate = source.frameRate;

        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);

            bool isRootSlide = binding.path == RootBonePath &&
                (binding.propertyName == "m_LocalPosition.x" || binding.propertyName == "m_LocalPosition.z");

            AnimationCurve newCurve = isRootSlide
                ? AnimationCurve.Constant(0f, StompTrimEnd - StompTrimStart, curve.Evaluate(0f))
                : Trim(curve, StompTrimStart, StompTrimEnd);

            AnimationUtility.SetEditorCurve(clip, binding, newCurve);
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorUtility.SetDirty(clip);
        return clip;
    }

    //start〜end を切り出して 0 秒始まりにする
    private static AnimationCurve Trim(AnimationCurve curve, float start, float end)
    {
        const float step = 1f / 120f;  //切り口の傾きを求める間隔
        AnimationCurve result = new AnimationCurve();

        result.AddKey(BoundaryKey(curve, start, 0f, step));

        foreach (Keyframe key in curve.keys)
        {
            if (key.time <= start + 0.0001f || key.time >= end - 0.0001f)
                continue;

            Keyframe shifted = key;
            shifted.time = key.time - start;
            result.AddKey(shifted);
        }

        result.AddKey(BoundaryKey(curve, end, end - start, step));
        return result;
    }

    private static Keyframe BoundaryKey(AnimationCurve curve, float sourceTime, float newTime, float step)
    {
        float tangent = (curve.Evaluate(sourceTime + step) - curve.Evaluate(sourceTime - step)) / (step * 2f);
        return new Keyframe(newTime, curve.Evaluate(sourceTime), tangent, tangent);
    }

    private static AnimationClip LoadClip(string fileName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ClipFolder + fileName + ".fbx")
            .OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
    }

    private static AnimatorState AddState(AnimatorStateMachine sm, string name, AnimationClip clip, int x, int y)
    {
        AnimatorState state = sm.AddState(name, new Vector3(300 + x * 220, 100 + y * 70, 0));
        state.motion = clip;
        return state;
    }

    private struct Condition
    {
        public AnimatorConditionMode mode;
        public string parameter;
    }

    private static Condition If(string parameter) => new Condition { mode = AnimatorConditionMode.If, parameter = parameter };
    private static Condition IfNot(string parameter) => new Condition { mode = AnimatorConditionMode.IfNot, parameter = parameter };

    //exitTime が null なら条件だけで移る
    private static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to, float duration, float? exitTime, params Condition[] conditions)
    {
        AnimatorStateTransition t = from.AddTransition(to);
        t.hasFixedDuration = true;
        t.duration = duration;
        t.hasExitTime = exitTime.HasValue;
        if (exitTime.HasValue)
            t.exitTime = exitTime.Value;

        foreach (Condition c in conditions)
            t.AddCondition(c.mode, 0f, c.parameter);

        return t;
    }

    private static void AnyState(AnimatorStateMachine sm, AnimatorState to, float duration, bool canTransitionToSelf, params Condition[] conditions)
    {
        AnimatorStateTransition t = sm.AddAnyStateTransition(to);
        t.hasFixedDuration = true;
        t.duration = duration;
        t.hasExitTime = false;
        t.canTransitionToSelf = canTransitionToSelf;

        foreach (Condition c in conditions)
            t.AddCondition(c.mode, 0f, c.parameter);
    }

    //-----プレハブ-----

    private static void SetupDragonPrefab(AnimatorController controller)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(DragonPrefabPath);
        try
        {
            //モデル（DragonUsurper Red）のアニメーターをオンにして、ドラゴン用のアニメーターを入れる
            Animator animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                Debug.LogError("DragonAnimatorSetup: Dragon.prefab に Animator が見つかりません。");
                return;
            }
            animator.enabled = true;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            DragonAnimation dragonAnimation = root.GetComponent<DragonAnimation>();
            if (dragonAnimation == null)
                dragonAnimation = root.AddComponent<DragonAnimation>();

            SerializedObject so = new SerializedObject(dragonAnimation);
            so.FindProperty("biteClip").objectReferenceValue = LoadClip("attackMouth");
            so.FindProperty("stompClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimationClip>(StompInPlacePath);
            so.FindProperty("breathClip").objectReferenceValue = LoadClip("attackFlame");
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, DragonPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void SetupStickerDisplay()
    {
        GameObject fireBreath = AssetDatabase.LoadAssetAtPath<GameObject>(FireBreathPrefabPath);
        if (fireBreath == null)
        {
            Debug.LogError("DragonAnimatorSetup: 炎ブレスのプレハブが見つかりません: " + FireBreathPrefabPath);
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(StickerDisplayPrefabPath);
        try
        {
            StickerEffectManager manager = root.GetComponentInChildren<StickerEffectManager>(true);
            if (manager == null)
            {
                Debug.LogError("DragonAnimatorSetup: StickerDisplay.prefab に StickerEffectManager が見つかりません。");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("fireBreathEffect").objectReferenceValue = fireBreath;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, StickerDisplayPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
