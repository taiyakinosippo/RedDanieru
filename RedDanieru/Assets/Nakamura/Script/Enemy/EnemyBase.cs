using Player;
using System.Runtime.InteropServices;
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
        Stun,
        Dead
    }

    [SerializeField] protected int enemyHP = 1000;                  //敵のHP
    [SerializeField] protected int enemyPower = 200;                //敵の攻撃力
    [SerializeField] protected int enemyDefense = 100;               //敵の防御力
    [SerializeField] protected float enemyMoveSpeed = 3.0f;        //敵の移動速度
    [SerializeField] protected float enemySearchArea = 6.0f;       //敵の探索範囲
    [SerializeField] protected float enemyTrackingTime = 3.0f;     //敵の追跡時間
    [SerializeField] protected float enemyAttackArea = 1.5f;       //敵の攻撃範囲
    [SerializeField] protected float enemyAttackCoolTime = 1.0f;   //攻撃モーションを終了してから次の攻撃ができるまでの時間
    [SerializeField] protected float enemyAttackStartTime = 1.2f;  //攻撃モーションを再生してから実際に当たり判定が出るまでの時間
    [SerializeField] protected float enemyAttackEndTime = 0.3f;    //攻撃モーションを再生してから当たり判定が消えるまでの時間
    protected float enemyDamageTime = 0.8f;        //ダメージモーションを再生してから次の行動ができるまでの時間

    protected enemyState currentState { get; set; }  //現在の状態
    [SerializeField] protected float currentHp;  //現在のHP
    protected float enemyRotationSpeed = 5.0f;  //敵の回転速度
    protected float trackingTimer = 0.0f;  //現在の追跡時間
    protected float attackCoolTimer;  //現在の攻撃クールタイム
    protected float attackTimer;  //現在の攻撃開始タイマー
    protected float damageTimer;  //現在のダメージタイマー
    protected float stunTimer;  //現在のスタンタイマー

    protected private float specialCoolTimer = 0.0f;  //特殊行動のタイマー
    protected private float specialInterval = 4.0f;  //特殊行動のクールタイム

    protected StickerState stickerState;
    protected Rigidbody rb;
    [SerializeField] protected LayerMask playerLayer;  //プレイヤーのレイヤー
    [SerializeField] protected LayerMask obstacleLayer;  //障害物のレイヤー
    public Transform player;
    protected NavMeshAgent agent;

    //-----マルチプレイ用-----
    //マルチではホスト（マスタークライアント）の画面の敵だけAIを動かし、
    //他の人の画面の敵はホストから届いた位置やHPを表示するだけにする（NetworkGameState.Enemy.cs）
    public bool IsNetworkProxy { get; private set; }
    private bool networkHpApplied;
    private bool networkDead;

    public float CurrentHp => currentHp;

    //ダメージ中・死亡中はダメージを受けない
    public bool CanTakeDamage => currentState != enemyState.Damage && currentState != enemyState.Dead;

    public virtual void Awake()
    {
        stickerState = GetComponent<StickerState>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    public virtual void Start()
    {
        //マルチで先にホストからHPが届いていたら上書きしない
        if (!networkHpApplied)
            currentHp = enemyHP;
        attackCoolTimer = enemyAttackCoolTime;
        attackTimer = enemyAttackStartTime + enemyAttackEndTime;
        //specialCoolTimer = specialInterval;
        agent.speed = enemyMoveSpeed;
    }

    protected void Update()
    {
        //行動パターン
        switch (currentState)
        {
            case enemyState.Idle:  //待機状態
            case enemyState.Move:  //移動状態
                EnemyMove();
                break;

            case enemyState.Attack:  //攻撃状態
                Attack();
                break;

            case enemyState.Special:  //特殊行動状態
                SpecialAttack();
                break;

            case enemyState.Damage:  //ダメージ状態
                if (damageTimer <= 0f)
                {
                    //ダメージモーションが終了したら通常状態に戻す
                    currentState = enemyState.Idle;
                    damageTimer = enemyDamageTime;
                }
                else
                {
                    damageTimer -= Time.deltaTime;
                }
                break;

            case enemyState.Stun:  //スタン状態
                if (stunTimer <= 0f)
                {
                    //スタンモーションが終了したら通常状態に戻す
                    currentState = enemyState.Idle;
                    stunTimer = 0f;
                }
                else
                {
                    stunTimer -= Time.deltaTime;
                }
                break;

            case enemyState.Dead:  //死亡状態
                Dead();
                break;
        }

        //攻撃のクールダウン
        if (attackCoolTimer > 0f)
        {
            attackCoolTimer -= Time.deltaTime;
        }

        //特殊行動のクールダウン
        if (specialCoolTimer > 0f)
        {
            specialCoolTimer -= Time.deltaTime;
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
                }
                else
                {
                    currentState = enemyState.Idle;
                    //specialCoolTimer = specialInterval;  //特殊行動タイマーリセット
                    return;
                }

                return;
            }

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
            //if (stickerState.isSpecialMove)
            //{
            //    CheckSpecialCooldown();

            //    if (currentState == enemyState.Special)
            //        return;
            //}

            //プレイヤーとの距離が攻撃範囲内の場合、攻撃する
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= enemyAttackArea)
            {
                //攻撃範囲内なら常に停止
                agent.isStopped = true;
                agent.ResetPath();

                //クールタイムが終わっていれば攻撃
                if (attackCoolTimer <= 0f)
                {
                    //攻撃の選択を行う（通常攻撃か特殊攻撃か）
                    AttackSelect();
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
            }
            else
            {
                currentState = enemyState.Idle;
                //specialCoolTimer = specialInterval;  //特殊行動タイマーリセット
                return;
            }
        }
    }

    //攻撃の選択を行う関数（通常攻撃か特殊攻撃か）
    public virtual void AttackSelect()
    {
        //特別攻撃用のステッカーがあるか
        if (stickerState.currentStickerScript != null && stickerState.isSpecialMove)
        {
            //特別攻撃のクールタイムが終わっているか
            if (specialCoolTimer <= 0f)
            {
                //攻撃確率
                int randomAttack = Random.Range(1, 101);

                if (randomAttack <= stickerState.currentStickerScript.attackRate)
                {
                    currentState = enemyState.Special;
                    agent.isStopped = true;
                    //agent.enabled = false;
                    return;
                }
            }
        }

        //特別攻撃にならなかった場合は通常攻撃
        currentState = enemyState.Attack;
    }

    //ステッカーによる特殊行動のクールタイム
    //public virtual void CheckSpecialCooldown()
    //{
    //    if (specialCoolTimer <= 0f)
    //    {
    //        currentState = enemyState.Special;
    //        agent.isStopped = true;
    //        agent.enabled = false;
    //        return;
    //    }
    //    specialCoolTimer -= Time.deltaTime;
    //}

    //攻撃処理
    public virtual void Attack()
    {
        //攻撃モーションを終了する
        if (attackTimer <= 0f)
        {
            //クールタイムセット
            attackCoolTimer = enemyAttackCoolTime;
            attackTimer = enemyAttackStartTime + enemyAttackEndTime;
            //specialCoolTimer = specialInterval;
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
            Debug.Log("攻撃を受けている！");
            PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();
            if (playerStatus != null && !playerStatus.isInvincible)
            {
                playerStatus.Damage(enemyPower);
            }
        }
    }

    //特殊攻撃の処理
    public virtual void SpecialAttack()
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
        //ダメージ中はダメージを受けない
        if (currentState == enemyState.Damage)
            return;
     
        Debug.Log("Enemy hit");
        //ダメージ計算
        int damage = playerPow - enemyDefense;
        if (damage > 0) currentHp -= damage;

        //UIにダメージを表示にするために値を渡す
        if (UIManager.Instance != null)
            UIManager.Instance.ShowDamage(damage, transform.position);
        if (currentHp <= 0)
        {
            //死亡処理
            currentState = enemyState.Dead;
        }
        else
        {
            //ダメージ処理
            currentState = enemyState.Damage;
            //ステッカーにダメージを感知
            if (stickerState.currentSticker != Sticker.None)
                stickerState.OnHit();
            Debug.Log("敵がダメージを受けました。残りHP: " + currentHp);
        }
    }

    //スタン開始処理
    public virtual void Stun(float stunDuration)
    {
        //スタン中はスタンを受けない
        if (currentState == enemyState.Stun)
            return;
        //スタン処理
        currentState = enemyState.Stun;
        stunTimer = stunDuration;
        Debug.Log("敵がスタンしました。スタン時間: " + stunDuration);
    }

    public virtual void Dead()
    {
        Debug.Log("敵が死亡しました。");
        Destroy(gameObject);
    }

    //-----マルチプレイ（他の人の画面の敵）-----

    //trueにするとAIを止め、ホストから届いた状態を表示するだけにする
    public void SetNetworkProxy(bool proxy)
    {
        if (IsNetworkProxy == proxy)
            return;

        IsNetworkProxy = proxy;

        //Update（AI）を止める／動かす
        enabled = !proxy;

        if (agent != null)
        {
            if (proxy)
            {
                agent.enabled = false;
            }
            else
            {
                //ホストを引き継いだので、今いる位置からAIを再開する
                agent.enabled = true;

                if (agent.isOnNavMesh)
                {
                    agent.Warp(transform.position);
                    agent.isStopped = false;
                }

                currentState = enemyState.Idle;
            }
        }

        if (rb != null && proxy)
        {
            rb.isKinematic = true;
        }

        //ステッカーの効果もホストの画面だけで動かす
        if (stickerState != null)
        {
            stickerState.SetVisualOnly(proxy);
        }
    }

    //ホストから届いた状態を反映する
    public void ApplyNetworkState(Vector3 position, float yaw, Vector3 scale, int hp, Sticker sticker)
    {
        //少し遅れて届くので滑らかに追いかける。離れすぎていたら瞬間移動
        float t = 1.0f - Mathf.Exp(-15.0f * Time.deltaTime);

        transform.position = (transform.position - position).sqrMagnitude > 9.0f
            ? position
            : Vector3.Lerp(transform.position, position, t);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, yaw, 0.0f), t);
        transform.localScale = Vector3.Lerp(transform.localScale, scale, t);

        currentHp = hp;
        networkHpApplied = true;

        if (stickerState != null && stickerState.currentSticker != sticker)
        {
            stickerState.ApplyVisual(sticker);
        }
    }

    //ホストの画面で倒された
    public void DieByNetwork(Sticker lastSticker)
    {
        if (networkDead)
            return;

        networkDead = true;

        //爆発ステッカーで倒れたときは爆発エフェクトも出す
        if (lastSticker == Sticker.Explosion &&
            StickerEffectManager.Instance != null &&
            StickerEffectManager.Instance.ExplosionEffect != null)
        {
            Instantiate(StickerEffectManager.Instance.ExplosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    //-----敵のステータスを上げる・下げる処理-----

    public void SetAttackState()
    {
        currentState = enemyState.Attack;
    }

    //敵のHPを変動させる
    public void AddEnemyCurrentHP(int value)
    {
        currentHp += value;
        if (currentHp < 0)
            currentHp = 0;

        //現在のHPが最大HPを超えないようにする
        if (currentHp > enemyHP)
            currentHp = enemyHP;

        if (currentHp <= 0)
        {
            //死亡処理
            currentState = enemyState.Dead;
        }
        else
        {
            //ダメージ処理
            currentState = enemyState.Damage;
        }
    }

    //敵の攻撃力を変動させる
    public void AddEnemyPower(int value)
    {
        enemyPower += value;
        if (enemyPower < 0)
            enemyPower = 0;
    }

    //敵の防御力を変動させる
    public void AddEnemyDefense(int value)
    {
        enemyDefense += value;
        if (enemyDefense < 0)
            enemyDefense = 0;
    }

    //敵の移動速度を変動させる
    public void AddEnemyMoveSpeed(float value)
    {
        float previousSpeed = enemyMoveSpeed;  //変更前の速度を保存

        enemyMoveSpeed += value;
        if (enemyMoveSpeed < 0)
            enemyMoveSpeed = 0;

        agent.speed = enemyMoveSpeed;  //NavMeshAgentの速度も更新
    }


    public float SetEnemyMoveSpeed(float value)
    {
        float previousSpeed = enemyMoveSpeed;  //変更前の速度を保存

        enemyMoveSpeed = value;
        if (enemyMoveSpeed < 0)
            enemyMoveSpeed = 0;

        agent.speed = enemyMoveSpeed;  //NavMeshAgentの速度も更新

        return previousSpeed;
    }

    //敵の探索範囲を変動させる
    public void AddEnemySearchArea(float value)
    {
        enemySearchArea += value;
        if (enemySearchArea < 0)
            enemySearchArea = 0;
    }

    //敵の追跡時間を変動させる
    public void AddEnemyTrackingTime(float value)
    {
        enemyTrackingTime += value;
        if (enemyTrackingTime < 0)
            enemyTrackingTime = 0;
    }

    //敵の攻撃範囲を変動させる
    public void AddEnemyAttackArea(float value)
    {
        enemyAttackArea += value;
        if (enemyAttackArea < 0)
            enemyAttackArea = 0;
    }

    //敵の攻撃クールタイムを変動させる
    public void AddEnemyAttackCoolTime(float value)
    {
        enemyAttackCoolTime += value;
        if (enemyAttackCoolTime < 0)
            enemyAttackCoolTime = 0;
    }

    //敵の攻撃開始時間を変動させる
    public void AddEnemyAttackStartTime(float value)
    {
        enemyAttackStartTime += value;
        if (enemyAttackStartTime < 0)
            enemyAttackStartTime = 0;
    }

    //敵の攻撃終了時間を変動させる
    public void AddEnemyAttackEndTime(float value)
    {
        enemyAttackEndTime += value;
        if (enemyAttackEndTime < 0)
            enemyAttackEndTime = 0;
    }
}
