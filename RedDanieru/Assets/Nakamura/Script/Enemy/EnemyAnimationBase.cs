using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

public class EnemyAnimationBase : MonoBehaviour
{
    private Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public virtual void WalkAnimation(bool isMoving)
    {
        if (animator == null) return;

        animator.SetBool("Walk", isMoving);
    }

    public virtual async UniTask AttackAnimation(float duration)
    {
        if (animator == null) return;

        await UniTask.Delay(TimeSpan.FromSeconds(duration));

        animator.SetTrigger("Attack");
    }

    public virtual void PlayerLostAnimation(bool isPlayerLost)
    {
        if (animator == null) return;

        animator.SetBool("PlayerLost", isPlayerLost);
    }

    public virtual void DamageAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger("Damage");
    }

    public virtual void StunAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger("Stun");
    }

    public virtual void StunAnimationEnd(bool isStunEnd)
    {
        if (animator == null) return;

        animator.SetBool("StunEnd", isStunEnd);
    }

    public virtual async UniTask DeathAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger("Death");

        //死亡ステートに遷移するまで待機
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));

        //死亡アニメーションが終了するまで待機
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
    }

    public virtual void ChargeAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger("Charge");
    }

    public virtual void ChargeDashAnimation(bool isDashing)
    {
        if (animator == null) return;

        animator.SetBool("ChargeDash", isDashing);
    }

    public virtual void SpecialStopAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger("SpecialStop");
    }
}
