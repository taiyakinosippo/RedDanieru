using UnityEngine;

public class PoisonSticker : StickerBase
{
    private int poisonDamage = 50;  //毒ダメージ量
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

    public override void OnWallApply()
    {
        // Resourcesから床Prefabを取得
        GameObject floorPrefab = Resources.Load<GameObject>("Ground_Mines_D");

        if (floorPrefab == null)
        {
            Debug.LogError("Ground_Mines_Dが見つかりません。Resourcesフォルダを確認してください。");
            return;
        }

        // 壁の位置から真下に1マス移動
        Vector3 spawnPosition = transform.position + Vector3.down * 0.5f;

        // 床を生成
        Instantiate(
            floorPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // 壁を消す
        gameObject.SetActive(false);
    }

    private void Update()
    {
        //ステッカーが敵に貼られている場合
        if (gameObject.CompareTag("Enemy") || gameObject.CompareTag("Player"))
        {
            PoisonEnemyDamage();
        }
    }

    //毒ダメージを与える処理
    private void PoisonEnemyDamage()
    {
        if (poisonTimer <= 0.0f)
        {
            //毒ダメージを与える
            if (enemyScript != null)
                enemyScript.AddEnemySlipDamage(-poisonDamage);
            else if (playerStatus != null)
                playerStatus.Damage(poisonDamage);

            poisonTimer = poisonInterval;
        }
        else
        {
            //タイマーを減らす
            poisonTimer -= Time.deltaTime;
        }
    }
}
