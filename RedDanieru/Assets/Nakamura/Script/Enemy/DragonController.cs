using Player;
using UnityEngine;
using UnityEngine.AI;

public class DragonController : EnemyBase
{
    private enum AttackType
    {
        Stomp,
        Biting,
        FireBreath,
        Chase
    }
    [SerializeField] private AttackType attackType;

    [SerializeField] private Vector3 headPos;  //頭の位置
    private float stompRange = 2.5f;  //踏みつけ攻撃の射程
    private float biteRange = 0.5f;  //かみつき攻撃の射程
    private float fireBreathRange = 15f;  //炎ブレスの射程
    private float stompAttackCoolTime = 2f;  //スタンプ攻撃のクールタイム
    private float stompAttackStartTime = 0.8f;  //スタンプ攻撃の開始時間
    private float stompAttackEndTime = 0.4f;  //スタンプ攻撃の終了時間
    private float bitingAttackCoolTime = 1f;  //かみつき攻撃のクールタイム
    private float bitingAttackStartTime = 1f;  //かみつき攻撃の開始時間
    private float bitingAttackEndTime = 0.2f;  //かみつき攻撃の終了時間

    public override void EnemyMove()
    {
        //プレイヤーを探索する
        Collider[] searchHits = Physics.OverlapSphere(transform.position, enemySearchArea, playerLayer);

        //プレイヤーが見つかった場合の処理
        if (searchHits.Length > 0)
        {
            //プレイヤーを取得
            player = searchHits[0].transform;

            //ステッカーによる特殊な行動パターン
            if (stickerState.isSpecialMove)
            {
                stickerState.currentStickerScript.OnEnemyUpdate();
                return;
            }

            if (!CanSeePlayer(player))
            {
                //プレイヤーを見失った場合の処理
                PlayerLostMove();

                return;
            }

            //ドラゴンの攻撃の種類を選択
            AttackSelect();

            //追跡以外の状態なら追跡しない
            if (currentState != enemyState.Move)
                return;

            //追跡時間をリセット
            trackingTimer = enemyTrackingTime + playerLostTime;

            //プレイヤー見失ったアニメーション終わり
            if (enemyAnimation != null)
                enemyAnimation.PlayerLostAnimation(false);

            //追跡中のアニメーションを再生
            if (enemyAnimation != null)
                enemyAnimation.WalkAnimation(true);

            //プレイヤーに向かって移動
            agent.CalculatePath(player.position, chasePath);

            //経路がつながっていない場合は追跡しない
            if (chasePath.status != NavMeshPathStatus.PathComplete)
            {
                agent.isStopped = true;
                agent.ResetPath();
                return;
            }

            currentState = enemyState.Move;
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        //プレイヤーを見失った場合
        else if (currentState != enemyState.Special && currentState != enemyState.Idle && currentState != enemyState.Back)
        {
            //プレイヤーを見失った場合の処理
            PlayerLostMove();
        }
        //プレイヤーが見つからない場合
        else if (currentState != enemyState.Back)
        {
            if (enemyAnimation != null)
                enemyAnimation.WalkAnimation(false);
        }
    }

    public override void AttackSelect()
    {
        //攻撃の種類を選択
        attackType = SelectAttackType();

        switch (attackType)
        {
            case AttackType.Stomp:
                currentState = enemyState.Attack;
                agent.isStopped = true;
                attackTimer = enemyAttackStartTime * stompAttackStartTime + enemyAttackEndTime * stompAttackEndTime;
                break;

            case AttackType.Biting:
                currentState = enemyState.Attack;
                agent.isStopped = true;
                attackTimer = enemyAttackStartTime * bitingAttackStartTime + enemyAttackEndTime * bitingAttackEndTime;
                break;

            case AttackType.FireBreath:
                currentState = enemyState.Special;
                agent.isStopped = true;
                //agent.enabled = false;
                break;

            case AttackType.Chase:
                currentState = enemyState.Move;
                break;
        }
    }

    private AttackType SelectAttackType()
    {
        //プレイヤーとの距離を計算
        float distance = Vector3.Distance(transform.position, player.position);
        float headDistance = Vector3.Distance(transform.TransformPoint(headPos), player.position);

        //プレイヤーが足元にいるか
        if (distance <= stompRange)
        {
            return AttackType.Stomp;
        }

        //プレイヤーが頭の範囲内にいるか
        if (headDistance <= biteRange)
        {
            //竜化ステッカーがあるか
            if (HasDragonSticker())
            {
                //40%で炎ブレス、60%でかみつき
                int random = Random.Range(0, 100);

                if (random < 40)
                    return AttackType.FireBreath;
                else
                    return AttackType.Biting;
            }

            //ステッカーがなければかみつき
            return AttackType.Biting;
        }

        //炎ブレスの射程内か
        if (distance <= fireBreathRange)
        {
            //竜化ステッカーがあるか
            if (HasDragonSticker())
            {
                return AttackType.FireBreath;
            }
            else
            {
                return AttackType.Chase;
            }
        }

        //どの攻撃範囲にも入っていなければ追跡
        return AttackType.Chase;
    }

    private bool HasDragonSticker()
    {
        //竜化ステッカーがあるかを判定
        return stickerState.currentSticker == Sticker.Dragon;
    }

    public override void Attack()
    {
        float dragonAttackCoolTime = enemyAttackCoolTime;  //攻撃のクールタイム
        float dragonAttackEndTime = enemyAttackEndTime;  //攻撃の終了時間
        Vector3 attackPosition = transform.position;  //攻撃の中心位置
        float attackArea = enemyAttackArea;  //攻撃の範囲

        //攻撃の種類によってクールタイムと開始時間、終了時間を設定
        if (attackType == AttackType.Stomp)
        {
            dragonAttackCoolTime *= stompAttackCoolTime;
            dragonAttackEndTime *= stompAttackEndTime;
            attackArea *= stompRange;  //踏みつけ攻撃の範囲を設定
        }
        else
        {
            dragonAttackCoolTime *= bitingAttackCoolTime;
            dragonAttackEndTime *= bitingAttackEndTime;
            attackPosition = transform.TransformPoint(headPos);  //頭の位置を攻撃の中心にする
            attackArea *= biteRange;  //かみつき攻撃の範囲を設定
        }

        //攻撃モーションを終了する
        if (attackTimer <= 0f)
        {
            //クールタイムセット
            attackCoolTimer = dragonAttackCoolTime;
            attackTimer = enemyAttackStartTime + enemyAttackEndTime;
            //specialCoolTimer = specialInterval;
            currentState = enemyState.Idle;
        }
        else if (attackTimer <= dragonAttackEndTime)  //攻撃判定開始
        {
            //攻撃判定を出す
            DragonAttackEffect(attackPosition, attackArea);
        }
        //攻撃推移
        attackTimer -= Time.deltaTime;
    }

    private void DragonAttackEffect(Vector3 attackPosition, float attackArea)
    {
        Collider[] attackHits = Physics.OverlapSphere(attackPosition, attackArea, playerLayer);
        foreach (Collider hit in attackHits)
        {
            //プレイヤーにダメージを与える処理
            Debug.Log("攻撃を受けている！");
            PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();
            if (playerStatus != null && !playerStatus.isInvincible)
            {
                playerStatus.Damage(enemyPower);
            }
        }
    }
}
