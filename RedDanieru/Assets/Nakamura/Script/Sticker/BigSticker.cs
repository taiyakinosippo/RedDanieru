using System.Collections;
using UnityEngine;

public class BigSticker : StickerBase
{
    private float bigScale = 3.0f; //拡大率
    private float scaleTime = 2.0f; //拡大縮小時間

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

    public override void OnEnemyApply()
    {
        //ステッカーが貼られたときの処理
        transform.localScale = new Vector3(bigScale, bigScale, bigScale);
    }

    public override void OnEnemyRemove()
    {
        transform.localScale = new Vector3(1f, 1f, 1.0f);

        //float scale = transform.localScale.x;  //現在の拡大率

        //while (scale >= 1.0f)
        //{
        //    scale = Mathf.Lerp(scale, 1.0f, scaleTime * Time.deltaTime);
        //    transform.localScale = new Vector3(scale, scale, scale);
        //    yield return null;
        //}

        //ステッカーを剥がす
        Destroy(GetComponent<StickerBase>());
    }

    public override void OnTrapApply()
    {
        OnEnemyApply();
    }

    public override void OnTrapRemove()
    {
        OnEnemyRemove();
    }
}
