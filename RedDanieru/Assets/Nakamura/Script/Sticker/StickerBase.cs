using UnityEngine;

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
