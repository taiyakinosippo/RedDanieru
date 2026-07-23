using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ChargeDashSticker : StickerBase
{
    private EnemyBase enemyScript;
    private StickerState stickerState;
    private Rigidbody rb;
    protected NavMeshAgent agent;
    private Transform target;

    private enum DashState
    {
        Charge,
        Dash
    }

    private DashState state;

    private float enemyDashSpeed = 10.0f;  //敵の突進速度
    private float trapDashSpeed = 20.0f;  //物の突進速度
    private float dashTime = 5.0f;  //突進時間
    private float chargeTime = 2.0f;  //溜め時間
    private float timer = 0.0f;  //溜め時間のタイマー
    private Vector3 dashDirection;  //敵の向く方向

    public override void OnEnemyApply()
    {
        rb = GetComponent<Rigidbody>();
        enemyScript = GetComponent<EnemyBase>();
        stickerState = GetComponent<StickerState>();
        agent = GetComponent<NavMeshAgent>();
        stickerState.isSpecialMove = true;  //特殊行動ON

        state = DashState.Charge;
        timer = chargeTime;
    }

    public override void OnEnemyUpdate()
    {
        target = enemyScript.player;  //プレイヤー取得

        switch (state)
        {
            case DashState.Charge:
                Charge();
                break;

            case DashState.Dash:
                EnemyDash();
                break;
        }
    }

    public override void OnEnemyRemove()
    {
        stickerState.isSpecialMove = false;
        agent.enabled = true;
    }

    public override void OnTrapApply()
    {
        rb = GetComponent<Rigidbody>();
        stickerState = GetComponent<StickerState>();
        stickerState.isSpecialMove = true;  //特殊行動ON
        target = GameObject.FindGameObjectWithTag("Player").transform;  //プレイヤー取得

        state = DashState.Charge;
        timer = chargeTime;
    }

    public override void OnTrapUpdate()
    {
        switch (state)
        {
            case DashState.Charge:
                Charge();
                break;

            case DashState.Dash:
                TrapDash();
                break;
        }
    }

    public override void OnTrapRemove()
    {
        stickerState.isSpecialMove = false;
        stickerState.CurrentSticker = Sticker.None;
    }

    //溜め
    private void Charge()
    {
        //プレイヤーの方を向く
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 8f);
        }

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            //向き保存
            dashDirection = transform.forward;
            dashDirection.y = 0;
            dashDirection.Normalize();

            //突進へ
            state = DashState.Dash;
            timer = dashTime;
        }
    }

    //敵の突進
    private void EnemyDash()
    {
        //ここで突進
        Vector3 velocity = dashDirection * enemyDashSpeed;
        velocity.y = rb.linearVelocity.y;   //重力は維持
        rb.linearVelocity = velocity;

        enemyScript.AttackEffect();

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            //一連の行動終了
            EndDash();
        }
    }

    //物の突進
    private void TrapDash()
    {
        //突進（吹き飛ぶ感じ）
        Vector3 dir = dashDirection;
        dir.y = 0.2f;
        dir.Normalize();
        rb.AddForce(dir * trapDashSpeed, ForceMode.Impulse);

        //一連の行動終了（ので消える）
        OnTrapRemove();
        Destroy(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != DashState.Dash)
            return;

        //敵だけ終了
        if (enemyScript != null)
        {
            //一連の行動終了
            EndDash();
        }
    }

    //終了時の初期化とか
    private void EndDash()
    {
        rb.linearVelocity = Vector3.zero;

        state = DashState.Charge;
        timer = chargeTime;

        //敵スクリプト側の初期化とか
        agent.enabled = true;

        enemyScript.EndSpecial();
    }
}