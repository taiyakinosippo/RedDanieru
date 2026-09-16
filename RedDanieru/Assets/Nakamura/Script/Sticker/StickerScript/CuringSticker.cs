using UnityEngine;

public class CuringSticker : StickerBase
{
    private int DefenseUpAmount = 100; //防御力上昇量

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //防御力を上昇させる
        enemyScript.AddEnemyDefense(DefenseUpAmount);
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //防御力を元に戻す
        enemyScript.AddEnemyDefense(-DefenseUpAmount);
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
