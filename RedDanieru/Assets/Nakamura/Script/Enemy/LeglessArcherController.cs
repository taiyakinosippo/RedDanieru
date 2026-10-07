using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LeglessArcherController : EnemyBase
{
    [SerializeField] private float escapeDistance = 3f;  //プレイヤーから逃げる距離  
    [SerializeField] private GameObject arrowPrefab;  //矢のプレハブ

    private Queue<GameObject> waitingArrows;  //オブジェクトプール用の待機中の矢のキューリスト
    private int poolCount = 3;  //とりあえずこの数ずつ用意しておく
    private List<GameObject> activeArrows = new();  //現在アクティブな矢のリスト
    
    private bool shooted = false;  //矢を撃ったかどうかのフラグ
    private GameObject targetPlayer;  // 現在狙っているプレイヤ
    private Vector3 lastPlayerPosition;  //最後にプレイヤーを見た位置
    private bool isTrackingLastPosition = false;  //最後に見た位置を追跡中か

    public override void Awake()
    {
        stickerState = GetComponent<StickerState>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        //オブジェクトプールの初期化
        waitingArrows = new Queue<GameObject>();
        for (int i = 0; i < poolCount; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab);  //矢を生成
            arrow.SetActive(false);  //非アクティブ化
            waitingArrows.Enqueue(arrow);  //キューに追加
        }
    }

    public override void EnemyMove()
    {
        //プレイヤーを探索する
        Collider[] searchHits = Physics.OverlapSphere(transform.position, enemySearchArea, playerLayer);

        if (searchHits.Length > 0)
        {
            player = searchHits[0].transform;

            //プレイヤーが見えるかどうかを判定
            if (CanSeePlayer(player))
            {
                //最後に見た位置を更新
                lastPlayerPosition = player.position;
                trackingTimer = enemyTrackingTime;
                isTrackingLastPosition = false;

                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                //プレイヤーが近すぎる場合は逃げる
                if (distanceToPlayer < escapeDistance * transform.localScale.x)
                {
                    //プレイヤーから逃げる
                    Vector3 escapeDirection = (transform.position - player.position).normalized;  //逃げる方向を計算
                    Vector3 escapePosition = transform.position + escapeDirection * escapeDistance;  //逃げる位置を計算
                    agent.isStopped = false;
                    agent.SetDestination(escapePosition);
                }
                else if (distanceToPlayer <= enemyAttackArea * transform.localScale.x)  //攻撃範囲内
                {
                    //その場で停止
                    agent.isStopped = true;
                    agent.ResetPath();

                    //攻撃クールタイムが終了していれば攻撃
                    if (attackCoolTimer <= 0f)
                    {
                        AttackSelect();
                        //currentState = enemyState.Attack;
                    }
                }
                else
                {
                    //攻撃範囲外なら追跡
                    currentState = enemyState.Move;
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }

                return;
            }

            //プレイヤーが見えない場合
            if (!isTrackingLastPosition)
            {
                //最後に見た場所へ移動
                agent.SetDestination(lastPlayerPosition);
                isTrackingLastPosition = true;
            }

            trackingTimer -= Time.deltaTime;

            if (trackingTimer <= 0f)
            {
                currentState = enemyState.Idle;
                agent.isStopped = true;
                agent.ResetPath();

                //specialCoolTimer = specialInterval;
                isTrackingLastPosition = false;
            }
            else
            {
                currentState = enemyState.Move;
                agent.isStopped = false;
            }

            return;
        }

        //プレイヤーが索敵範囲から外れた場合
        if (trackingTimer > 0f)
        {
            if (!isTrackingLastPosition)
            {
                //最後に見た場所へ移動
                agent.SetDestination(lastPlayerPosition);
                isTrackingLastPosition = true;
            }

            trackingTimer -= Time.deltaTime;
            currentState = enemyState.Move;
            agent.isStopped = false;
        }
        else
        {
            currentState = enemyState.Idle;
            agent.isStopped = true;
            agent.ResetPath();

            //specialCoolTimer = specialInterval;
            isTrackingLastPosition = false;
        }
    }

    public override void Attack()
    {
        //攻撃モーション中はプレイヤーの方向を向く
        if (attackTimer > 0f)
        {
            if (player != null && CanSeePlayer(player))
            {
                //最後に見た位置を更新
                lastPlayerPosition = player.position;

                LookAtPlayer();
            }
        }

        //矢を出す
        if (attackTimer <= enemyAttackEndTime && !shooted)
        {
            AttackEffect();
            shooted = true;
        }

        //攻撃モーションを終了する
        if (attackTimer <= 0f)
        {
            //クールタイムセット
            attackCoolTimer = enemyAttackCoolTime;
            attackTimer = enemyAttackStartTime + enemyAttackEndTime;
            //specialCoolTimer = specialInterval;
            shooted = false;

            currentState = enemyState.Idle;
        }

        //攻撃推移
        attackTimer -= Time.deltaTime;
    }

    private void LookAtPlayer()
    {
        Collider[] attackHits = Physics.OverlapSphere(transform.position, enemyAttackArea, playerLayer);

        targetPlayer = null;

        //一番近いプレイヤーを取得
        foreach (Collider hit in attackHits)
        {
            //プレイヤーが見えるかどうかを判定
            if (!CanSeePlayer(hit.transform))
                continue;

            if (targetPlayer == null)
            {
                //暫定一位
                targetPlayer = hit.gameObject;
            }
            else
            {
                float currentDistance = Vector3.Distance(transform.position, targetPlayer.transform.position);  //王者の距離
                float newDistance = Vector3.Distance(transform.position, hit.transform.position);  //挑戦者の距離

                //挑戦者の方が近ければ王者を交代
                if (newDistance < currentDistance)
                {
                    targetPlayer = hit.gameObject;
                }
            }
        }

        if (targetPlayer == null)
            return;

        //プレイヤーの方向
        Vector3 direction = targetPlayer.transform.position - transform.position;

        //上下には向かない
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        //プレイヤーの方向を向くための回転
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        //徐々に回転
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180f * Time.deltaTime);
    }

    public override void AttackEffect()
    {
        //矢を生成して最後に見た位置へ向ける
        GameObject arrow = GetArrowFromPool();

        //矢の生成位置を調整（敵の前方1m、上方1m）
        Vector3 arrowSpawnPosition = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
        arrow.transform.position = arrowSpawnPosition;

        //最後に見た位置へ向ける
        Vector3 targetPosition = lastPlayerPosition + Vector3.up * 1.0f;  //矢の高さを調整
        Vector3 direction = targetPosition - arrowSpawnPosition;  //矢の方向を計算

        if (direction.sqrMagnitude > 0.001f)
        {
            arrow.transform.rotation = Quaternion.LookRotation(direction);
        }

        //矢の攻撃力とかを設定
        arrow.GetComponent<EnemyArrow>().Setting(enemyPower, this);
        arrow.SetActive(true);

        //マルチでは他の人の画面にも矢を飛ばす（見た目だけ。当たり判定はこの画面の矢で行う）
        NetworkGameState.NotifyArrowFired(this, arrow.transform.position, arrow.transform.rotation);
    }

    //マルチで他の人の画面に出す矢（ダメージは与えない）
    public void FireVisualArrow(Vector3 position, Quaternion rotation)
    {
        GameObject arrow = GetArrowFromPool();

        arrow.transform.SetPositionAndRotation(position, rotation);

        arrow.GetComponent<EnemyArrow>().Setting(0, this, true);
        arrow.SetActive(true);
    }

    private GameObject GetArrowFromPool()
    {
        //オブジェクトプールから矢を取得
        if (waitingArrows.Count > 0)
        {
            GameObject arrow = waitingArrows.Dequeue();
            activeArrows.Add(arrow);
            return arrow;
        }
        //オブジェクトプールに矢がない場合は新しく生成
        GameObject newArrow = Instantiate(arrowPrefab);
        activeArrows.Add(newArrow);
        return newArrow;
    }

    public void ReturnArrowToPool(GameObject arrow)
    {
        //矢をオブジェクトプールに戻す
        if (waitingArrows.Count < poolCount)
        {
            arrow.SetActive(false);
            waitingArrows.Enqueue(arrow);
            activeArrows.Remove(arrow);
            return;
        }
        //オブジェクトプールがいっぱいの場合は矢を破棄
        Destroy(arrow);
        activeArrows.Remove(arrow);
    }
}
