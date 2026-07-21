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
    protected float attackCoolTimer;  //現在の攻撃クールタイム
    protected float attackTimer;  //現在の攻撃開始タイマー

    private float specialCoolTimer = 0.0f;  //特殊行動のタイマー
    private float specialInterval = 4.0f;  //特殊行動のクールタイム

    protected StickerState stickerState;
    protected Rigidbody rb;
    [SerializeField] protected LayerMask playerLayer;
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
        Collider[] searchHits = Physics.OverlapSphere(transform.position, enemySearchArea, playerLayer);

        //プレイヤーが見つかった場合の処理
        if (searchHits.Length > 0)
        {
            player = searchHits[0].transform;

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
            currentState = enemyState.Idle;
            specialCoolTimer = specialInterval;  //特殊行動タイマーリセット
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

    public virtual void AttackEffect()
    {
        Collider[] attackHits = Physics.OverlapSphere(transform.position, enemyAttackArea, playerLayer);
        foreach (Collider hit in attackHits)
        {
            //プレイヤーにダメージを与える処理
            Debug.Log("撃たれた。攻撃を受けている！");
        }
    }

    public virtual void SpecialMove()
    {
        if (stickerState.currentStickerScript == null)
        {
            currentState = enemyState.Idle;
        }
        stickerState.currentStickerScript.OnEnemyUpdate();
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
        currentHp -= playerPow;
    }
}
