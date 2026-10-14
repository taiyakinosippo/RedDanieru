using UnityEngine;
using static Fusion.Sockets.NetConnectionMap;
using Player;

public class HardeningSticker : StickerBase
{
    private int DefenseUpAmount = 100;  //防御力上昇量
    private bool isHardening = false;  //硬化中かどうかのフラグ
    private float hardeningDuration = 5.0f;  //硬化時間
    private float hardeningTimer = 0.0f;  //硬化時間のタイマー
    private int damageCounter = 0;  //ダメージを受けた回数のカウンター

    private PlayerStatus playerStatus;

    public override void OnDamageHit()
    {
        Debug.Log("HardeningSticker OnDamageHit : " + damageCounter);
        //ダメージを受けた回数をカウント
        if (!isHardening)
        {
            damageCounter++;
        }
    }

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        stickerState.isSpecialMove = true;  //特殊行動ON
        //初期化
        hardeningTimer = hardeningDuration;
        attackRate = 100;
    }

    public override void OnEnemyUpdate()
    {
        if (damageCounter >= 3)
        {
            if (!isHardening)  //硬化発動
            {
                enemyScript.AddEnemyDefense(DefenseUpAmount);
                isHardening = true;
            }
            else  //硬化中の処理
            {
                if (hardeningTimer <= 0f)  //硬化時間終了
                {
                    enemyScript.AddEnemyDefense(-DefenseUpAmount);
                    hardeningTimer = hardeningDuration;
                    isHardening = false;
                    damageCounter = 0;
                }
                else  //まだ時間内
                {
                    hardeningTimer -= Time.deltaTime;
                }
            }
        }
        else
        {
            //ダメージを受けた回数が3回未満の場合は通常攻撃
            enemyScript.SetAttackState();
        }
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        stickerState.isSpecialMove = false;
        
        //硬化中なら防御力を元に戻す
        if (isHardening)
        {
            enemyScript.AddEnemyDefense(-DefenseUpAmount);
        }
    }

    //ステッカーが物に貼られたときの処理
    public override void OnTrapApply()
    {

    }

    //ステッカーが物から剥がれたときの処理
    public override void OnTrapRemove()
    {

    }

    //ステッカーが壁に貼られたときの処理
    public override void OnWallApply()
    {

    }

    //ステッカーが壁から剥がれたときの処理
    public override void OnWallRemove()
    {

    }

    public override void OnPlayerUpdate()
    {
        if (playerStatus == null)
            return;

        if (damageCounter >= 3)
        {
            if (!isHardening)
            {
                playerStatus.PlayerDefenseUp(DefenseUpAmount);

                isHardening = true;

                Debug.Log("硬化発動");
            }
            else
            {
                hardeningTimer -= Time.deltaTime;

                if (hardeningTimer <= 0f)
                {
                    playerStatus.PlayerDefenseUp(-DefenseUpAmount);

                    hardeningTimer = hardeningDuration;
                    isHardening = false;
                    damageCounter = 0;

                    Debug.Log("硬化終了");
                }
            }
        }
    }

    //プレイヤー用貼られたとき
    public override void OnPlayerApply()
    {
        if (stickerState == null)
        {
            stickerState =
                GetComponent<StickerState>();
        }

        playerStatus =
            GetComponent<PlayerStatus>();

        stickerState.isSpecialMove = true;

        hardeningTimer = hardeningDuration;
        damageCounter = 0;
        isHardening = false;
    }

    //プレイヤー用剥がされたとき
    public override void OnPlayerRemove()
    {
        stickerState.isSpecialMove = false;

        if (isHardening && playerStatus != null)
        {
            playerStatus.PlayerDefenseUp(
                -DefenseUpAmount);
        }

        isHardening = false;
        damageCounter = 0;
    }
}
