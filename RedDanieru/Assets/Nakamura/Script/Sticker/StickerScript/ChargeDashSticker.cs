using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Player;
using Fusion;

public class ChargeDashSticker : StickerBase
{
    private Rigidbody rb;
    protected NavMeshAgent agent;
    private Transform target;

    private Player.PlayerInputPriority input;

    private bool canUse = true;
    private bool isUsing = false;

    private float coolTime = 7f;
    private float coolTimer;

    private CharacterController controller;

    private enum DashState
    {
        Charge,
        Dash
    }

    private DashState state;

    private float enemyDashSpeed = 13.0f;  //敵の突進速度
    private float trapDashSpeed = 20.0f;  //物の突進速度
    private float dashTime = 0.5f;  //突進時間
    private float chargeTime = 2.0f;  //溜め時間
    private float timer = 0.0f;  //溜め時間のタイマー
    private bool chargeEffectPlayed = false;  //溜めエフェクトが再生されたかどうかのフラグ
    private Vector3 effectOffset = Vector3.zero;  //エフェクトの位置補正
    private Vector3 dashDirection;  //敵の向く方向
    private GameObject lastEnteredObject;  //最後に衝突したオブジェクト

    private Player.PlayerMovement playerMovement;

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        rb = GetComponent<Rigidbody>();
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

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        stickerState.isSpecialMove = false;
        agent.enabled = true;
    }

    //ステッカーが物に貼られたときの処理
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

    //ステッカーが物から剥がれたときの処理
    public override void OnTrapRemove()
    {
        stickerState.isSpecialMove = false;
        stickerState.currentSticker = Sticker.None;
    }

    //溜め
    private void Charge()
    {
        if (!chargeEffectPlayed)
        {
            if (NetworkGameState.Instance != null)
            {
                NetworkObject netObj =
                    GetComponent<NetworkObject>();

                if (netObj != null)
                {
                    NetworkGameState.Instance.RPC_PlayChargeEffect(
                        netObj.Id);
                }
            }
            else
            {
                Instantiate(
                    StickerEffectManager.Instance.ChargeEffect,
                    transform.position + effectOffset,
                    Quaternion.identity,
                    transform);
            }

            chargeEffectPlayed = true;
        }

        //プレイヤーの方を向く
        // 敵だけターゲット方向を向く
        if (enemyScript != null)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0;

            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 8f);
            }
        }

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            //向き保存
            dashDirection = transform.forward;
            dashDirection.y = 0;
            dashDirection.Normalize();

            //動けるようにする
            if (enemyScript != null)
            {
                CanMove();
            }

            //突進へ
            state = DashState.Dash;
            timer = dashTime;
            chargeEffectPlayed = false;
        }
    }

    //敵の突進
    private void EnemyDash()
    {
        //ここで突進
        Vector3 velocity = dashDirection * enemyDashSpeed;
        velocity.y = rb.linearVelocity.y;   //重力は維持

        rb.linearVelocity = velocity;

        //enemyScript.AttackEffect();

        //プレイヤーとは物理的にぶつからない設定（敵の上に乗らないようにするため）なので、重なりで当たりを判定する
        if (CheckDashHitPlayer())
            return;

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
        //動けるようにする
        CanMove();

        //突進（吹き飛ぶ感じ）
        Vector3 dir = dashDirection;
        dir.y = 0.2f;
        dir.Normalize();
        rb.AddForce(dir * trapDashSpeed, ForceMode.Impulse);

        //一連の行動終了（ので消える）
        stickerState.Remove();
    }

    //終了時の初期化とか
    private void EndDash()
    {
        //動かないようにする
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;

        state = DashState.Charge;
        timer = chargeTime;

        //敵スクリプト側の初期化とか
        agent.enabled = true;
        enemyScript.EndSpecial();
    }

    //Rigidbodyの制約を解除して動けるようにする
    private void CanMove()
    {
        rb.useGravity = true;
        rb.constraints &= ~RigidbodyConstraints.FreezePosition;
        rb.isKinematic = false;
    }

    private bool CheckDashHitPlayer()
    {
        Collider body = GetComponent<Collider>();

        Vector3 center = body != null ? body.bounds.center : transform.position;
        float radius = (body != null ? body.bounds.extents.x : 0.5f) + 0.3f;

        Collider[] hits = Physics.OverlapSphere(center, radius, LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player") && hit.gameObject != lastEnteredObject)
            {
                OnDashHit(hit.gameObject);
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("衝突：" + collision.gameObject.name);
        if (state != DashState.Dash || collision.gameObject.CompareTag("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("Floor") || collision.gameObject == lastEnteredObject)
            return;

        OnDashHit(collision.gameObject);
    }

    private void OnDashHit(GameObject hitObject)
    {
        //最後に衝突したオブジェクトを保存
        lastEnteredObject = hitObject;

        if (hitObject.CompareTag("Player"))
        {
            //プレイヤーに当たったらダメージ判定出す
            enemyScript.AttackEffect();

            //プレイヤーが突き飛ばされる

        }

        if (hitObject.CompareTag("Enemy"))
        {
            EnemyBase enemy =
                hitObject.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                NetworkGameState.DamageEnemy(enemy, 500);

                enemy.Stun(3f);

                EndPlayerDash();
            }
        }

        //敵だけ終了
        if (enemyScript != null)
        {
            //一連の行動終了
            EndDash();

            //6秒間スタン
            enemyScript.Stun(6f);
        }
    }

    //プレイヤーに貼られたとき
    public override void OnPlayerApply()
    {
        controller =
            GetComponent<CharacterController>();

        playerMovement = GetComponent<PlayerMovement>();

        stickerState.isSpecialMove = true;

        canUse = true;
    }

    public override void OnPlayerUpdate()
    {
        if (!canUse)
        {
            coolTimer -= Time.deltaTime;

            if (coolTimer <= 0)
            {
                canUse = true;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E) && canUse && !isUsing)
        {
            target = FindNearestEnemy();

            if (target != null)
            {
                dashDirection = transform.forward;
                dashDirection.y = 0;
                dashDirection.Normalize();

                isUsing = true;

                playerMovement.IsDashLocked = true;

                state = DashState.Charge;
                timer = chargeTime;
            }
        }

        if (!isUsing)
            return;

        switch (state)
        {
            case DashState.Charge:
                Charge();
                break;

            case DashState.Dash:
                PlayerDash();
                break;
        }
    }

    private Transform FindNearestEnemy()
    {
        EnemyBase[] enemies =
            FindObjectsOfType<EnemyBase>();

        Transform nearest = null;

        float nearestDist = Mathf.Infinity;

        foreach (EnemyBase enemy in enemies)
        {
            float dist = Vector3.Distance(
                transform.position,
                enemy.transform.position);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private void PlayerDash()
    {
        controller.Move(dashDirection *trapDashSpeed *Time.deltaTime);

        CheckDashHit();

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            EndPlayerDash();
        }
    }

    private void EndPlayerDash()
    {
        isUsing = false;

        canUse = false;

        coolTimer = coolTime;

        if (playerMovement != null)
        {
            playerMovement.IsDashLocked = false;
        }
    }

    private void CheckDashHit()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                1.0f);

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            EnemyBase enemy =
                hit.GetComponent<EnemyBase>();

            // 敵
            if (enemy != null)
            {
                NetworkGameState.DamageEnemy(enemy, 500);
                enemy.Stun(3f);

                EndPlayerDash();
                return;
            }

            // 壁
            if (hit.GetComponent<WallBlock>() != null)
            {
                EndPlayerDash();
                return;
            }

            //// Block
            //if (hit.CompareTag("Floor"))
            //{
            //    EndPlayerDash();
            //    return;
            //}
        }
    }
}