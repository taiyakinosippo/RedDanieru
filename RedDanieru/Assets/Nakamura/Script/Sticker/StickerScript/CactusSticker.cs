using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CactusSticker : StickerBase
{
    private enum CactusState
    {
        SelectPosition,
        MoveToPosition,
        PlaceCactus
    }

    private GameObject cactusPrefab;  //サボテンのPrefab
    private int poolCount = 5;  //サボテンのプール数
    private float searchRadius = 5f;  //サボテン設置候補地点を探す半径
    private float arrivalDistance = 1f;  //サボテン設置候補地点に到達したとみなす距離
    private float minDistanceFromEnemy = 1.5f;  //サボテン設置候補地点が敵から近すぎないかの距離
    private float minDistanceFromTrap = 2.5f;  //サボテン設置候補地点が既存のサボテンから近すぎないかの距離
    private float minDistanceFromPlayer = 2f;  //サボテン設置候補地点がプレイヤーから近すぎないかの距離
    private float placementRadius = 0.7f;  //サボテン設置候補地点に設置する空間があるか確認するための半径
    private float navMeshSampleDistance = 2f;  //
    private Vector3 movePosition;
    private float standOffDistance = 1.0f;
    private LayerMask placementBlockerMask;  //設置を妨げるLayer
    private LayerMask playerLayer = LayerMask.GetMask("Player");

    private Queue<GameObject> waitingCuctuses;
    private List<GameObject> activeCuctuses = new();

    private NavMeshAgent agent;
    private CactusState currentState;

    private Vector3 targetPosition;

    private float needleAreaRadius = 2.0f;  //針の範囲
    private int needleGuardDamage = 10;  //針バリアのダメージ量
    private int needleDamage = 30;  //針のダメージ量
    private Dictionary<EnemyBase, float> enemyDamageCooldowns = new Dictionary<EnemyBase, float>();  //敵ごとの次回ダメージ可能時刻
    private float damageCooldown = 0.7f;  //ダメージのクールタイム（秒）
    private LayerMask enemyLayer;  //敵のレイヤー

    private void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        if (gameObject.CompareTag("Player") && Input.GetKeyDown(KeyCode.Q))
        {
            PlaceCactus(transform.position);
        }
        else if (gameObject.CompareTag("Trap") || gameObject.CompareTag("Wall"))
        {
            //トラップや壁に貼られている場合の処理
            NeedleGuard();
        }
    }

    public override void OnEnemyApply()
    {
        stickerState.isSpecialMove = true;

        agent = enemyScript.GetComponent<NavMeshAgent>();

        placementBlockerMask = LayerMask.GetMask("Player", "Enemy", "Wall", "Default");

        //サボテンのプールを作成
        CactusPoolCreate(poolCount);

        currentState = CactusState.SelectPosition;
    }

    private void CactusPoolCreate(int poolSize)
    {
        waitingCuctuses = new Queue<GameObject>();
        activeCuctuses = new List<GameObject>();

        cactusPrefab = StickerEffectManager.Instance.CactusPrefab;

        //サボテンのプールを作成
        for (int i = 0; i < poolSize; i++)
        {
            GameObject cactus = Instantiate(cactusPrefab);
            cactus.SetActive(false);
            waitingCuctuses.Enqueue(cactus);
        }
    }

    public override void OnEnemyUpdate()
    {
        //HPが25%以下なら回復
        if (enemyScript.GetEnemyCurrentHP() <= enemyScript.GetEnemyMaxHP() * 0.25f)
        {
            enemyScript.AddEnemyCurrentHP(10);
            return;
        }

        //設置上限に達した場合
        if (activeCuctuses.Count >= poolCount)
        {
            FinishSpecial();
            return;
        }

        switch (currentState)
        {
            case CactusState.SelectPosition:
                SelectPosition();
                break;

            case CactusState.MoveToPosition:
                MoveToPosition();
                break;

            case CactusState.PlaceCactus:

                //HPを15%減らす
                enemyScript.AddEnemyCurrentHP(-(int)(enemyScript.GetEnemyMaxHP() * 0.15f));

                PlaceCactus(targetPosition);

                currentState = CactusState.SelectPosition;
                break;
        }
    }

    // 設置地点の選定
    private void SelectPosition()
    {
        const int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            // 敵の現在位置を中心にランダムな候補地点を作成
            Vector2 offset =
                Random.insideUnitCircle * searchRadius;

            Vector3 randomPosition =
                transform.position +
                new Vector3(offset.x, 0f, offset.y);

            // ① NavMesh上の場所か確認
            if (!NavMesh.SamplePosition(
                randomPosition,
                out NavMeshHit hit,
                navMeshSampleDistance,
                NavMesh.AllAreas))
            {
                continue;
            }

            Vector3 candidate = hit.position;

            // ② 敵から近すぎないか確認
            if (Vector3.Distance(transform.position, candidate)
                < minDistanceFromEnemy)
            {
                continue;
            }

            // ③ 敵が歩いて行ける場所か確認
            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(candidate, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            // ④ 設置する空間があるか確認
            Collider[] colliders = Physics.OverlapSphere(
                candidate,
                placementRadius,
                placementBlockerMask
            );

            bool blocked = false;

            foreach (Collider col in colliders)
            {
                // 自分自身のColliderは無視
                if (col.transform == transform ||
                    col.transform.IsChildOf(transform))
                {
                    continue;
                }

                blocked = true;
                break;
            }

            if (blocked)
            {
                continue;
            }

            // ⑤ 既存のサボテンから一定距離離れているか確認
            bool nearTrap = false;

            foreach (GameObject cactus in activeCuctuses)
            {
                if (cactus == null || !cactus.activeInHierarchy)
                {
                    continue;
                }

                if (Vector3.Distance(
                    candidate,
                    cactus.transform.position
                ) < minDistanceFromTrap)
                {
                    nearTrap = true;
                    break;
                }
            }

            if (nearTrap)
            {
                continue;
            }

            // ⑥ プレイヤーと重ならないか確認
            // 設置候補地点の周囲にプレイヤーがいるか確認
            Collider[] players = Physics.OverlapSphere(candidate, minDistanceFromPlayer, playerLayer);

            if (players.Length > 0)
            {
                continue;
            }

            Vector3 enemyPos = transform.position;

            Vector2 enemyXZ = new Vector2(enemyPos.x, enemyPos.z);
            Vector2 candidateXZ = new Vector2(candidate.x, candidate.z);

            if (Vector2.Distance(enemyXZ, candidateXZ) < minDistanceFromEnemy)
            {
                continue;
            }

            // サボテンの設置位置
            targetPosition = candidate;

            // 敵がサボテンに重ならないよう、手前を移動先にする
            Vector3 direction = (transform.position - candidate).normalized;

            Vector3 standPosition =
                candidate + direction * standOffDistance;

            // 移動先もNavMesh上に補正
            if (!NavMesh.SamplePosition(
                standPosition,
                out NavMeshHit standHit,
                navMeshSampleDistance,
                NavMesh.AllAreas))
            {
                continue;
            }

            movePosition = standHit.position;

            // 移動先まで到達できるか確認
            if (!agent.CalculatePath(movePosition, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            agent.isStopped = false;
            agent.SetDestination(movePosition);

            currentState = CactusState.MoveToPosition;

            return;
        }

        // 30回探しても設置場所がなかった
        FinishSpecial();
    }

    // 設置地点まで移動
    private void MoveToPosition()
    {
        if (agent.pathPending)
        {
            return;
        }

        if (!agent.hasPath ||
            agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            agent.ResetPath();
            currentState = CactusState.SelectPosition;
            return;
        }

        if (agent.remainingDistance <= arrivalDistance)
        {
            agent.isStopped = true;
            agent.ResetPath();

            currentState = CactusState.PlaceCactus;
        }
    }

    //サボテン設置
    private void PlaceCactus(Vector3 position)
    {
        //オブジェクトプールからサボテンを取得
        GameObject cactus = waitingCuctuses.Dequeue();

        //サボテンの位置を設定してアクティブ化
        cactus.transform.position = position;
        cactus.SetActive(true);

        //アクティブなサボテンリストに追加
        activeCuctuses.Add(cactus);

        //サボテンのCactusスクリプトにCactusStickerを設定
        cactus.GetComponent<Cactus>().SetCactusSticker(this);
    }

    //サボテンをオブジェクトプールに戻す
    public void ReturnCactusToPool(GameObject cactus)
    {
        //サボテンをオブジェクトプールに戻す
        if (waitingCuctuses.Count < poolCount)
        {
            cactus.SetActive(false);
            waitingCuctuses.Enqueue(cactus);
            activeCuctuses.Remove(cactus);
            return;
        }
        //オブジェクトプールがいっぱいの場合はサボテンを破棄
        Destroy(cactus);
        activeCuctuses.Remove(cactus);
    }

    //初期化
    private void FinishSpecial()
    {
        agent.isStopped = true;
        agent.ResetPath();

        // EnemyBaseの特殊行動終了処理を呼び出す
        enemyScript.EndSpecial();
    }

    public override void OnEnemyRemove()
    {
        stickerState.isSpecialMove = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        //待機サボテンを破棄
        CactusReset();
    }

    private void CactusReset()
    {
        // 設置済みサボテンを破棄
        //foreach (GameObject cactus in activeCuctuses)
        //{
        //    if (cactus != null)
        //    {
        //        Destroy(cactus);
        //    }
        //}

        // 待機中サボテンを破棄
        while (waitingCuctuses != null &&
               waitingCuctuses.Count > 0)
        {
            GameObject cactus = waitingCuctuses.Dequeue();

            if (cactus != null)
            {
                Destroy(cactus);
            }
        }

        //activeCuctuses.Clear();
    }

    public override void OnPlayerApply()
    {
        CactusPoolCreate(poolCount);
    }

    public override void OnPlayerRemove()
    {
        CactusReset();
    }

    private void NeedleGuard()
    {
        //針バリアの範囲内の敵を検索
        Collider[] searchHits = Physics.OverlapSphere(transform.position, needleAreaRadius, enemyLayer);

        //敵ごとにダメージを与える処理
        foreach (Collider hit in searchHits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();

            if (enemy == null)
            {
                continue;
            }

            //敵ごとのクールタイムを確認
            if (enemyDamageCooldowns.TryGetValue(enemy, out float nextDamageTime))
            {
                if (Time.time < nextDamageTime)
                {
                    continue;
                }
            }

            //ダメージを与える
            enemy.AddEnemyCurrentHP(-needleGuardDamage);

            //この敵の次回ダメージ可能時刻を設定
            enemyDamageCooldowns[enemy] = Time.time + damageCooldown;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //トラップや壁に貼られている場合のみ処理を行う
        if (!gameObject.CompareTag("Trap") && !gameObject.CompareTag("Wall"))
            return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                //敵にダメージを与える処理
                NetworkGameState.DamageEnemy(enemy, needleDamage);
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();

            if (playerStatus != null && !playerStatus.isInvincible)
            {
                //プレイヤーにダメージを与える処理
                playerStatus.Damage(needleDamage);
            }
        }
    }
}
