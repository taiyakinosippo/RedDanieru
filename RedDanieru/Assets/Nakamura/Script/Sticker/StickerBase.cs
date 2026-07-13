using UnityEngine;
using System.Collections;

public class StickerBase : MonoBehaviour
{
    public virtual void OnEnemyApply()
    {
        //ステッカーが貼られたときの処理
    }

    public virtual void OnEnemyRemove()
    {
        //ステッカーが剥がれたときの処理
    }

    public virtual void OnTrapApply()
    {
        //ステッカーが貼られたときの処理
    }

    public virtual void OnTrapRemove()
    {
        //ステッカーが剥がれたときの処理
    }
}

//public abstract class StickerBase
//{
//    public abstract void OnEnemyApply(GameObject enemy);
//    //public abstract StickerState.State OnEnemyUpdate(GameObject enemy, StickerState.State state);
//    public abstract IEnumerator OnEnemyRemove(GameObject enemy);

//    public abstract void OnTrapApply(GameObject trap);
//    public abstract void OnTrapUpdate(GameObject trap);
//    public abstract void OnTrapRemove(GameObject trap);
//}
