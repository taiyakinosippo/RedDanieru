using Player;
using UnityEngine;

public class CactusSticker : StickerBase
{
    private float needleAreaRadius = 2.0f; //針の範囲
    private int needleGuardDamage = 10; //針バリアのダメージ量
    private int needleDamage = 50; //針のダメージ量
    private LayerMask enemyLayer = LayerMask.GetMask("Enemy");  //敵のレイヤー

    private void Update()
    {
        if (gameObject.CompareTag("Player"))
        {
            NeedleGuard();
        }
        else if (gameObject.CompareTag("Trap") || gameObject.CompareTag("Wall"))
        {
            //トラップや壁に貼られている場合の処理
        }
    }

    public override void OnEnemyApply()
    {
        //stickerState.isSpecialMove = true;  //特殊行動ON
    }

    private void NeedleGuard()
    {
        Collider[] searchHits = Physics.OverlapSphere(transform.position, needleAreaRadius, enemyLayer);
        Debug.Log($"Enemy LayerMask: {enemyLayer.value}");

        foreach (Collider hit in searchHits)
        {
            Debug.Log($"検出したオブジェクト: {hit.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Debug.Log($"ダメージ実行: {enemy.name}");
                enemy.AddEnemyCurrentHP(-needleGuardDamage);
            }
            else
            {
                Debug.Log("EnemyBase が見つからない");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //トラップや壁に貼られている場合のみ処理を行う
        if (!gameObject.CompareTag("Trap") && !gameObject.CompareTag("Wall"))
            return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                //敵にダメージを与える処理
                NetworkGameState.DamageEnemy(enemy, needleDamage);
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();

            if (playerStatus != null && !playerStatus.isInvincible)
            {
                //プレイヤーにダメージを与える処理
                playerStatus.Damage(needleDamage);
            }
        }
    }
}
