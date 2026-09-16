using UnityEngine;

public class LeglessSticker : StickerBase
{
    private float saveSpeed;  //保存する速度

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //敵の速度を保存
        saveSpeed = enemyScript.AddEnemyMoveSpeed(0f);
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //敵の速度を元に戻す
        enemyScript.AddEnemyMoveSpeed(saveSpeed);
    }

    //ステッカーが物に貼られたときの処理
    public override void OnTrapApply()
    {

    }

    //ステッカーが物から剥がれたときの処理
    public override void OnTrapRemove()
    {

    }
}
