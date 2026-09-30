using UnityEngine;

public class PoisonSticker : StickerBase
{
    private int poisonDamage = 10;  //毒ダメージ量
    private float poisonInterval = 1.0f;  //毒ダメージの間隔
    private float poisonTimer = 0.0f;  //毒ダメージのタイマー

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //
        
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //
        
    }

    private void Update()
    {
        //ステッカーが敵に貼られている場合
        if (enemyScript != null)
        {
            PoisonDamage();
        }
    }

    //毒ダメージを与える処理
    private void PoisonDamage()
    {
        if (poisonTimer <= 0.0f)
        {
            //毒ダメージを与える
            enemyScript.AddEnemyCurrentHP(-poisonDamage);
            poisonTimer = poisonInterval;
        }
        else
        {
            //タイマーを減らす
            poisonTimer -= Time.deltaTime;
        }
    }
}
