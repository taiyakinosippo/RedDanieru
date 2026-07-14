using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    int enemyHP = 100;                  //敵のHP
    int enemyPower = 10;                //敵の攻撃力
    int enemyDefense = 5;               //敵の防御力
    float enemyMoveSpeed = 2.0f;        //敵の移動速度
    float enemySearchArea = 5.0f;       //敵の探索範囲
    float enemyTrackingTime = 3.0f;     //敵の追跡時間
    float enemyAttackArea = 1.5f;       //敵の攻撃範囲
    float enemyAttackCoolTime = 1.0f;   //攻撃モーションを終了してから次の攻撃ができるまでの時間
    float enemyAttackStartTime = 0.7f;  //攻撃モーションを再生してから実際に当たり判定が出るまでの時間
    float enemyAttackEndTime = 0.3f;    //攻撃モーションを再生してから当たり判定が消えるまでの時間

    float currentHp;  //現在のHP
    StickerState stickerState;
    [SerializeField] private LayerMask playerLayer;

    void Start()
    {
        stickerState = GetComponent<StickerState>();
        currentHp = enemyHP;
    }

    void Update()
    {
        if (stickerState.CurrentSticker == Sticker.None)
        {
            NormalMove();
        }
    }

    public virtual void NormalMove()
    {
        Collider[] searchHits = Physics.OverlapSphere(transform.position, enemySearchArea, playerLayer);

        //プレイヤーが見つかった場合の処理
        if (searchHits.Length > 0)
        {
            Transform player = searchHits[0].transform;

            
        }
    }
}
