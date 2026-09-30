using UnityEngine;

public class HardeningSticker : StickerBase
{
    private int DefenseUpAmount = 100;  //防御力上昇量
    private bool isHardening = false;  //硬化中かどうかのフラグ
    private float hardeningDuration = 5.0f;  //硬化時間
    private float hardeningTimer = 0.0f;  //硬化時間のタイマー
    private int DamageCounter = 0;  //ダメージを受けた回数のカウンター

    public override void OnDamageHit()
    {
        //ダメージを受けた回数をカウント
        //DamageCounter++;
    }

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //stickerState.isSpecialMove = true;  //特殊行動ON
    }

    public override void OnEnemyUpdate()
    {
        //硬化中の処理
        if (isHardening)
        {
            //硬化中の処理をここに追加
        }
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //防御力を元に戻す
        //enemyScript.AddEnemyDefense(-DefenseUpAmount);
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
