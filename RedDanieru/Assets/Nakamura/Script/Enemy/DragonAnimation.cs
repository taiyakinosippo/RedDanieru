using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

//ドラゴン（DragonUsurper）のアニメーション
//噛みつき・踏みつけ・炎ブレスは、モーションの「当たる瞬間」が当たり判定の出る時間にそろうように、
//再生を少し遅らせたり速めたりする
public class DragonAnimation : EnemyAnimationBase
{
    [Header("モーション（Tools/Dragon/アニメーターを作り直す で自動で入る）")]
    [SerializeField] private AnimationClip biteClip;    //噛みつき（attackMouth）
    [SerializeField] private AnimationClip stompClip;   //踏みつけ（attackHand をその場で跳ぶようにした Dragon_StompInPlace）
    [SerializeField] private AnimationClip breathClip;  //炎ブレス（attackFlame）

    [Header("モーションの中で当たる瞬間（0=最初、1=最後）")]
    [Tooltip("噛みつきで口が閉じる瞬間")]
    [Range(0f, 1f)] public float biteHitPoint = 0.45f;
    [Tooltip("踏みつけで跳んで着地する瞬間")]
    [Range(0f, 1f)] public float stompHitPoint = 0.63f;
    [Tooltip("炎ブレスで炎を吐き始める瞬間")]
    [Range(0f, 1f)] public float breathFirePoint = 0.35f;

    [Tooltip("モーションを速めるときの上限（これより速くはしない）")]
    [SerializeField] private float maxActionSpeed = 3f;

    private Animator dragonAnimator;
    private int playId;  //攻撃モーションの予約番号（ダメージなどで割り込まれたら古い予約を捨てる）

    private static readonly int BiteHash = Animator.StringToHash("Bite");
    private static readonly int StompHash = Animator.StringToHash("Stomp");
    private static readonly int BreathHash = Animator.StringToHash("Breath");
    private static readonly int ActionSpeedHash = Animator.StringToHash("ActionSpeed");

    protected override void Awake()
    {
        base.Awake();
        dragonAnimator = GetComponentInChildren<Animator>();
    }

    //噛みつき。hitTime 秒後に口が閉じるように再生する
    public void BiteAnimation(float hitTime)
    {
        PlayTimed(BiteHash, biteClip, biteHitPoint, hitTime).Forget();
    }

    //踏みつけ。hitTime 秒後に手が地面に着くように再生する
    public void StompAnimation(float hitTime)
    {
        PlayTimed(StompHash, stompClip, stompHitPoint, hitTime).Forget();
    }

    //炎ブレス。fireTime 秒後に炎を吐き始めるように再生する
    public void BreathAnimation(float fireTime)
    {
        PlayTimed(BreathHash, breathClip, breathFirePoint, fireTime).Forget();
    }

    //攻撃モーションの予約を取り消す（攻撃が中断されたとき）
    public void CancelAttackAnimation()
    {
        playId++;

        if (dragonAnimator == null) return;

        dragonAnimator.ResetTrigger(BiteHash);
        dragonAnimator.ResetTrigger(StompHash);
        dragonAnimator.ResetTrigger(BreathHash);
    }

    private async UniTask PlayTimed(int trigger, AnimationClip clip, float hitPoint, float hitTime)
    {
        if (dragonAnimator == null) return;

        int id = ++playId;

        //当たる瞬間までの、モーション本来の時間
        float clipHitTime = clip != null ? clip.length * hitPoint : 0f;
        float speed = 1f;
        float delay = 0f;

        if (clipHitTime <= hitTime)
        {
            //間に合うので、遅らせて始める
            delay = hitTime - clipHitTime;
        }
        else if (hitTime > 0f)
        {
            //間に合わないので、速めて再生する
            speed = Mathf.Min(clipHitTime / hitTime, maxActionSpeed);
        }

        if (delay > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            //待っている間に割り込まれた
            if (id != playId || dragonAnimator == null) return;
        }

        dragonAnimator.SetFloat(ActionSpeedHash, speed);
        dragonAnimator.SetTrigger(trigger);    }

    public override void DamageAnimation()
    {
        CancelAttackAnimation();
        base.DamageAnimation();
    }

    public override void StunAnimation()
    {
        CancelAttackAnimation();

        //前のスタンの「終わり」が残っていると、すぐ抜けてしまうので戻しておく
        StunAnimationEnd(false);
        base.StunAnimation();
    }

    public override UniTask DeathAnimation()
    {
        CancelAttackAnimation();
        return base.DeathAnimation();
    }

    public override void SpecialStopAnimation()
    {
        CancelAttackAnimation();
        base.SpecialStopAnimation();
    }
}
