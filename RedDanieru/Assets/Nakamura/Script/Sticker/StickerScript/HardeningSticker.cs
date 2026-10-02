using UnityEngine;
using static Fusion.Sockets.NetConnectionMap;

public class HardeningSticker : StickerBase
{
    private int DefenseUpAmount = 100;  //防御力上昇量
    private bool isHardening = false;  //硬化中かどうかのフラグ
    private float hardeningDuration = 5.0f;  //硬化時間
    private float hardeningTimer = 0.0f;  //硬化時間のタイマー
    private int damageCounter = 0;  //ダメージを受けた回数のカウンター

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
}
