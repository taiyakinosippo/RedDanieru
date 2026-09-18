using System.Collections;
using UnityEngine;

public class BigSticker : StickerBase
{
    private float bigScale = 3.0f;  //拡大率
    private float scaleTime = 2.0f;  //拡大縮小時間
    private int addPower = 100;  //現在の拡大率
    private float addAttackScale = 1.0f;  //現在の拡大率
    private float addAttackCoolTime = 0.5f;  //現在の拡大率

    //public override StickerState.State OnEnemyUpdate(GameObject enemy, StickerState.State state)
    //{
    //    switch (state)
    //    {
    //        case StickerState.State.Growing:
    //            //徐々に拡大
    //            float scale = Mathf.MoveTowards(enemy.transform.localScale.x, bigScale, scaleTime * Time.deltaTime);
    //            enemy.transform.localScale = new Vector3(scale, scale, scale);

    //            if (enemy.transform.localScale.x >= bigScale) 
    //                return StickerState.State.Active; //拡大率を超えたら処理を終了

    //            break;
    //        case StickerState.State.Shrinking:
    //            // 縮小処理
    //            scale = Mathf.MoveTowards(enemy.transform.localScale.x, 1.0f, scaleTime * Time.deltaTime);
    //            enemy.transform.localScale = new Vector3(scale, scale, scale);

    //            if (enemy.transform.localScale.x <= 1.0f)
    //                return StickerState.State.None; //縮小率を下回ったら処理を終了
    //            break;
    //    }

    //    return state;
    //}

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //スケールを拡大
        transform.localScale = new Vector3(bigScale, bigScale, bigScale);

        //敵の攻撃力、攻撃範囲、攻撃クールタイムを増加
        enemyScript.AddEnemyPower(addPower);
        enemyScript.AddEnemyAttackArea(addAttackScale);
        enemyScript.AddEnemyAttackCoolTime(addAttackCoolTime);
    }
    
    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //スケールを元に戻す
        transform.localScale = new Vector3(1f, 1f, 1.0f);

        //敵の攻撃力、攻撃範囲、攻撃クールタイムを元に戻す
        enemyScript.AddEnemyPower(-addPower);
        enemyScript.AddEnemyAttackArea(-addAttackScale);
        enemyScript.AddEnemyAttackCoolTime(-addAttackCoolTime);

        //float scale = transform.localScale.x;  //現在の拡大率

        //while (scale >= 1.0f)
        //{
        //    scale = Mathf.Lerp(scale, 1.0f, scaleTime * Time.deltaTime);
        //    transform.localScale = new Vector3(scale, scale, scale);
        //    yield return null;
        //}
    }

    //ステッカーが物に貼られたときの処理
    public override void OnTrapApply()
    {
        //スケールを拡大
        transform.localScale = new Vector3(bigScale, bigScale, bigScale);
    }

    //ステッカーが物から剥がれたときの処理
    public override void OnTrapRemove()
    {
        //スケールを元に戻す
        transform.localScale = new Vector3(1f, 1f, 1.0f);
    }
}
