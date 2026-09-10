using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    protected enum enemyState
    {
        Idle,
        Move,
        Attack,
        Special,
        Damage,
        Dead
    }

    protected int enemyHP = 100;                  //敵のHP
    protected int enemyPower = 10;                //敵の攻撃力
    protected int enemyDefense = 5;               //敵の防御力
    protected float enemyMoveSpeed = 3.0f;        //敵の移動速度
    protected float enemySearchArea = 6.0f;       //敵の探索範囲
    protected float enemyTrackingTime = 3.0f;     //敵の追跡時間
    protected float enemyAttackArea = 1.5f;       //敵の攻撃範囲
    protected float enemyAttackCoolTime = 1.0f;   //攻撃モーションを終了してから次の攻撃ができるまでの時間
    protected float enemyAttackStartTime = 1.2f;  //攻撃モーションを再生してから実際に当たり判定が出るまでの時間
    protected float enemyAttackEndTime = 0.3f;    //攻撃モーションを再生してから当たり判定が消えるまでの時間

    protected enemyState currentState { get; set; }  //現在の状態
    protected float currentHp;  //現在のHP
    protected float enemyRotationSpeed = 5.0f;  //敵の回転速度
    protected float trackingTimer = 0.0f;  //現在の追跡時間
    protected float attackCoolTimer;  //現在の攻撃クールタイム
    protected float attackTimer;  //現在の攻撃開始タイマー

    private float specialCoolTimer = 0.0f;  //特殊行動のタイマー
    private float specialInterval = 4.0f;  //特殊行動のクールタイム

    protected StickerState stickerState;
    protected Rigidbody rb;
    [SerializeField] protected LayerMask playerLayer;  //プレイヤーのレイヤー
    [SerializeField] protected LayerMask obstacleLayer;  //障害物のレイヤー
    public Transform player;
    protected NavMeshAgent agent;

    void Start()
    {
        stickerState = GetComponent<StickerState>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        currentHp = enemyHP;
        attackCoolTimer = enemyAttackCoolTime;
        attackTimer = enemyAttackStartTime + enemyAttackEndTime;
        specialCoolTimer = specialInterval;
    }

    protected void Update()
    {
        //行動パターン
        switch (currentState)
        {
            case enemyState.Idle:
            case enemyState.Move:
                    EnemyMove();

                break;
            case enemyState.Attack:
                Attack();
                break;
            case enemyState.Special:
                SpecialMove();
                break;
            case enemyState.Damage:
                //ダメージ処理
                break;
            case enemyState.Dead:
                //死亡処理
                Destroy(gameObject);
                break;
        }

        //攻撃のクールダウン
        if (attackCoolTimer > 0f)
        {
            attackCoolTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        //移動処理
        //if (currentState == enemyState.Move)
        //{
        //    rb.MovePosition(
        //        rb.position + moveDirection * enemyMoveSpeed * Time.fixedDeltaTime
        //    );
        //}
    }

    public virtual void EnemyMove()
    {
        //プレイヤーを探索する
        Collider[] searchHits = Physics.OverlapSphere(transform.position, enemySearchArea, playerLayer);

        //プレイヤーが見つかった場合の処理
        if (searchHits.Length > 0)
        {
            player = searchHits[0].transform;

            if (!CanSeePlayer(player))
            {
                //追跡時間が残っている場合は追跡を続ける
                if (trackingTimer > 0f)
                {
                    //追跡時間を減らす
                    trackingTimer -= Time.deltaTime;

                    //最後にプレイヤーを確認した位置に向かって移動
                    currentState = enemyState.Move;
                    agent.isStopped = false;
                    //agent.SetDestination(lastPlayerPosition);
                }
                else
                {
                    currentState = enemyState.Idle;
                    specialCoolTimer = specialInterval;  //特殊行動タイマーリセット
                    return;
                }

                return;
            }

            //プレイヤーを確認できた位置を保存
            //lastPlayerPosition = player.position;

            //追跡時間をリセット
            trackingTimer = enemyTrackingTime;

            //プレイヤーの方向を向く
            //Vector3 direction = (player.position - transform.position).normalized;
            //direction.y = 0;  //水平方向のみ回転
            //if (direction != Vector3.zero)
            //{
            //    Quaternion lookRotation = Quaternion.LookRotation(direction);
            //    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * enemyRotationSpeed);
            //}

            //ステッカーによる特殊な行動パターン
            if (stickerState.isSpecialMove)
            {
                CheckSpecialCooldown();

                if (currentState == enemyState.Special)
                    return;
            }

            //プレイヤーとの距離が攻撃範囲内の場合、攻撃する
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= enemyAttackArea * transform.localScale.x)
            {
                //攻撃フラグを立てる
                if (attackCoolTimer <= 0f)
                {
                    currentState = enemyState.Attack;
                    agent.isStopped = true;
                }
            }
            //攻撃範囲じゃない場合は追跡する
            else
            {
                //プレイヤーに向かって移動
                //moveDirection = direction;
                currentState = enemyState.Move;
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
        //プレイヤーを見失った場合
        else if (currentState != enemyState.Special)
        {
            //追跡時間が残っている場合は追跡を続ける
            if (trackingTimer > 0f)
            {
                //追跡時間を減らす
                trackingTimer -= Time.deltaTime;

                //最後にプレイヤーを確認した位置に向かって移動
                currentState = enemyState.Move;
                agent.isStopped = false;
                //agent.SetDestination(lastPlayerPosition);
            }
            else
            {
                currentState = enemyState.Idle;
                agent.isStopped = true;
                specialCoolTimer = specialInterval;  //特殊行動タイマーリセット
                return;
            }
        }
    }

    //ステッカーによる特殊行動のクールタイム
    public virtual void CheckSpecialCooldown()
    {
        if (specialCoolTimer <= 0f)
        {
            currentState = enemyState.Special;
            agent.isStopped = true;
            agent.enabled = false;
            return;   
        }
        specialCoolTimer -= Time.deltaTime;
    }

    //攻撃処理
    public virtual void Attack()
    {
        //攻撃モーションを終了する
        if (attackTimer <= 0f)
        {
            //クールタイムセット
            attackCoolTimer = enemyAttackCoolTime;
            attackTimer = enemyAttackStartTime + enemyAttackEndTime;
            specialCoolTimer = specialInterval;
            currentState = enemyState.Idle;
        }
        else if (attackTimer <= enemyAttackEndTime)  //攻撃判定開始
        {
            //攻撃判定を出す
            AttackEffect();
        }
        //攻撃推移
        attackTimer -= Time.deltaTime;
    }

    //攻撃判定の処理
    public virtual void AttackEffect()
    {
        Collider[] attackHits = Physics.OverlapSphere(transform.position, enemyAttackArea, playerLayer);
        foreach (Collider hit in attackHits)
        {
            //プレイヤーにダメージを与える処理
            Debug.Log("撃たれた。攻撃を受けている！");
        }
    }

    //特殊行動の処理
    public virtual void SpecialMove()
    {
        if (stickerState.currentStickerScript == null)
        {
            currentState = enemyState.Idle;
        }
        stickerState.currentStickerScript.OnEnemyUpdate();
    }

    //プレイヤーが見えるかどうかを判定する関数
    public virtual bool CanSeePlayer(Transform target)
    {
        //プレイヤーと障害物の位置
        Vector3 origin = transform.position + Vector3.up;
        Vector3 targetPosition = target.position + Vector3.up;

        //プレイヤーの方向と距離を計算する
        Vector3 direction = targetPosition - origin;
        float distance = direction.magnitude;

        Debug.DrawRay(origin, direction.normalized, Color.red);

        //レイキャストで障害物があるかどうかを判定する
        //return !Physics.Raycast(origin, direction.normalized, distance, obstacleLayer);
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, obstacleLayer))
        {
            Debug.Log("視界を遮っているオブジェクト: " + hit.collider.name);
            return false;
        }

        return true;
    }

    //特殊行動終了時の初期化とか
    public virtual void EndSpecial()
    {
        currentState = enemyState.Idle;
        agent.enabled = true;
        specialCoolTimer = specialInterval;
    }

    //ダメージ処理
    public virtual void Damage(int playerPow)
    {
        Debug.Log("Enemy hit");
        currentHp -= playerPow;

        if (currentHp <= 0)
        {
            currentState = enemyState.Dead;
            //死亡処理
            Debug.Log("敵が死亡しました。");
        }
        else
        {
            currentState = enemyState.Damage;
            //ダメージ処理
            Debug.Log("敵がダメージを受けました。残りHP: " + currentHp);
        }
    }
}
