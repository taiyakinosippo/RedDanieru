using UnityEngine;
using System.Collections;

public class StickerBase : MonoBehaviour
{
    protected EnemyBase enemyScript;

    public virtual void OnEnemyApply()
    {
        //ステッカーが敵に貼られたときの処理
    }

    public virtual void OnEnemyUpdate() 
    { 

    }

    public virtual void OnEnemyRemove()
    {
        //ステッカーが敵から剥がれたときの処理
    }

    public virtual void OnTrapApply()
    {
        //ステッカーが物に貼られたときの処理
    }

    public virtual void OnTrapUpdate()
    {

    }

    public virtual void OnTrapRemove()
    {
        //ステッカーが物から剥がれたときの処理
    }

    //敵自分のスクリプトを取得する
    public void GetEnemyScript() 
    {
        enemyScript = GetComponent<EnemyBase>();
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
