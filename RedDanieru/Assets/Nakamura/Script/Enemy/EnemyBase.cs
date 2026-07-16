using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    protected enum enemyState
    {
        Idle,
        Move,
        Attack,
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

    protected enemyState currentState;  //現在の状態
    protected float currentHp;  //現在のHP
    protected float enemyRotationSpeed = 5.0f;  //敵の回転速度
    private Vector3 moveDirection;  //敵の移動方向
    protected float attackCoolTimer;  //現在の攻撃クールタイム
    protected float attackTimer;  //現在の攻撃開始タイマー

    protected StickerState stickerState;
    protected Rigidbody rb;
    [SerializeField] protected LayerMask playerLayer;
    protected NavMeshAgent agent;

    void Start()
    {
        stickerState = GetComponent<StickerState>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        currentHp = enemyHP;
        attackCoolTimer = enemyAttackCoolTime;
        attackTimer = enemyAttackStartTime + enemyAttackEndTime;
    }

    protected void Update()
    {
        switch (currentState)
        {
            case enemyState.Idle:
            case enemyState.Move:
                NormalMove();
                break;
            case enemyState.Attack:
                Attack();
                break;
            case enemyState.Damage:
                //ダメージ処理
                break;
            case enemyState.Dead:
                //死亡処理
                break;
        }


        Debug.Log("Enemy is in " + currentState);

        //if (stickerState.CurrentSticker == Sticker.None)
        //{
        //    NormalMove();
        //}

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

    public virtual void NormalMove()
    {
        Collider[] searchHits = Physics.OverlapSphere(transform.position, enemySearchArea, playerLayer);

        //プレイヤーが見つかった場合の処理
        if (searchHits.Length > 0)
        {
            Transform player = searchHits[0].transform;

            //プレイヤーの方向を向く
            //Vector3 direction = (player.position - transform.position).normalized;
            //direction.y = 0;  //水平方向のみ回転
            //if (direction != Vector3.zero)
            //{
            //    Quaternion lookRotation = Quaternion.LookRotation(direction);
            //    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * enemyRotationSpeed);
            //}

            //プレイヤーとの距離が攻撃範囲内の場合、攻撃する
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= enemyAttackArea)
            {
                //攻撃フラグを立てる
                if (attackCoolTimer <= 0f)
                {
                    currentState = enemyState.Attack;
                    agent.isStopped = true;
                }
            }
            else
            {
                //プレイヤーに向かって移動
                //moveDirection = direction;
                currentState = enemyState.Move;
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            currentState = enemyState.Idle;
        }
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
            agent.isStopped = false;
            currentState = enemyState.Idle;
        }
        else if (attackTimer <= enemyAttackEndTime)  //攻撃判定開始
        {
            //攻撃判定を出す
            Collider[] attackHits = Physics.OverlapSphere(transform.position, enemyAttackArea, playerLayer);
            foreach (Collider hit in attackHits)
            {
                //プレイヤーにダメージを与える処理
                Debug.Log("撃たれた。攻撃を受けている！");
            }
        }
        //攻撃推移
        attackTimer -= Time.deltaTime;
    }
}
