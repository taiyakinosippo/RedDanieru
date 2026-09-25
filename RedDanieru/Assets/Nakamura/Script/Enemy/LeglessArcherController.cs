using System.Collections.Generic;
using UnityEngine;

public class LeglessArcherController : EnemyBase
{
    [SerializeField] private GameObject arrowPrefab;  //矢のプレハブ

    private Queue<GameObject> waitingArrows;  //オブジェクトプール用の待機中の矢のキューリスト
    private int poolCount = 3;  //とりあえずこの数ずつ用意しておく
    private List<GameObject> activeArrows = new();  //現在アクティブな矢のリスト
    
    private bool shooted = false;  //矢を撃ったかどうかのフラグ
    private GameObject targetPlayer;  // 現在狙っているプレイヤー

    private void Awake()
    {
        //オブジェクトプールの初期化
        waitingArrows = new Queue<GameObject>();
        for (int i = 0; i < poolCount; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab);  //矢を生成
            arrow.SetActive(false);  //非アクティブ化
            waitingArrows.Enqueue(arrow);  //キューに追加
        }
    }

    public override void Attack()
    {
        //攻撃モーション中はプレイヤーの方向を向く
        if (attackTimer > 0f)
        {
            LookAtPlayer();
        }

        //攻撃モーションを終了する
        if (attackTimer <= 0f)
        {
            //クールタイムセット
            attackCoolTimer = enemyAttackCoolTime;
            attackTimer = enemyAttackStartTime + enemyAttackEndTime;
            specialCoolTimer = specialInterval;
            shooted = false;

            currentState = enemyState.Idle;
        }
        else if (attackTimer <= enemyAttackEndTime && !shooted)
        {
            //攻撃判定を出す
            AttackEffect();
            Debug.Log("LeglessArcher Attack!");
            shooted = true;
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
        //範囲内のプレイヤーがいれば矢を撃つ
        if (targetPlayer != null)
        {
            //矢を生成してプレイヤーに向かせる
            GameObject arrow = GetArrowFromPool();

            //矢の生成位置を調整（敵の前方1m、上方1m）
            Vector3 arrowSpawnPosition = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
            arrow.transform.position = arrowSpawnPosition;

            //矢の向きをプレイヤーに向ける
            arrow.transform.rotation = transform.rotation;

            //矢の攻撃力とかを設定
            arrow.GetComponent<EnemyArrow>().Setting(enemyPower, this);
            arrow.SetActive(true);
        }
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
