using Player;
using System.Collections.Generic;
using UnityEngine;

public class CactusSticker : StickerBase
{
    private float needleAreaRadius = 2.0f;  //針の範囲
    private int needleGuardDamage = 10;  //針バリアのダメージ量
    private int needleDamage = 30;  //針のダメージ量
    private Dictionary<EnemyBase, float> enemyDamageCooldowns = new Dictionary<EnemyBase, float>();  //敵ごとの次回ダメージ可能時刻
    private float damageCooldown = 0.7f;  //ダメージのクールタイム（秒）
    private LayerMask enemyLayer;  //敵のレイヤー

    private void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
    }

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
        //針バリアの範囲内の敵を検索
        Collider[] searchHits = Physics.OverlapSphere(transform.position, needleAreaRadius, enemyLayer);

        //敵ごとにダメージを与える処理
        foreach (Collider hit in searchHits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();

            if (enemy == null)
            {
                continue;
            }

            //敵ごとのクールタイムを確認
            if (enemyDamageCooldowns.TryGetValue(enemy, out float nextDamageTime))
            {
                if (Time.time < nextDamageTime)
                {
                    continue;
                }
            }

            //ダメージを与える
            enemy.AddEnemyCurrentHP(-needleGuardDamage);

            //この敵の次回ダメージ可能時刻を設定
            enemyDamageCooldowns[enemy] = Time.time + damageCooldown;
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
