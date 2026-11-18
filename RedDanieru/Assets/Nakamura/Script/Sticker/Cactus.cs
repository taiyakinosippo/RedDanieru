using Player;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    private LayerMask explosionLayer;  //トラップ反応するレイヤーマスク
    private float explosionRadius = 4f;  //爆発の半径
    private int explosionPower = 500;  //爆発の力
    private CactusSticker cactusSticker;  //サボテンを置いたステッカーのスクリプト

    private void Start()
    {
        explosionLayer = LayerMask.GetMask("Player", "Enemy");  //レイヤーを設定
    }

    //サボテンのステッカーを設定する関数
    public void SetCactusSticker(CactusSticker sticker)
    {
        cactusSticker = sticker;
    }

    //爆発の処理
    private void Explosion()
    {
        //爆発エフェクトを生成
        GameObject effect = Instantiate(StickerEffectManager.Instance.ExplosionEffect, transform.position, Quaternion.identity);

        //爆発の中心をトラップの位置に設定
        Vector3 explosionPosition = transform.position;
        //爆発の範囲内のすべてのコライダーを取得
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius, explosionLayer);
        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Player"))
            {
                //プレイヤーにダメージを与える処理
                PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();

                playerStatus.Damage(explosionPower);
            }
            else if (hit.CompareTag("Enemy"))
            {
                //敵にダメージを与える処理
                EnemyBase enemyScript = hit.GetComponent<EnemyBase>();

                enemyScript.Damage(explosionPower);
            }
        }

        //オブジェクトプールに戻す処理を呼び出す
        if (cactusSticker != null)
        {
            cactusSticker.ReturnCactusToPool(gameObject);
        }
        else
        {
            //cactusStickerがnullの場合は消す
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //トラップに反応するレイヤーに入った場合
        if (((1 << other.gameObject.layer) & explosionLayer) != 0)
        {
            StickerState otherState = other.GetComponent<StickerState>();

            //サボテンのステッカーを貼っている場合は爆発しない
            if (otherState.currentSticker != Sticker.Cactus)
            {
                //爆発を起こす
                Explosion();
            }
        }
    }
}
