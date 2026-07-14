using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    // 樽の耐久値
    [Header("耐久値")]
    [SerializeField] private int hp = 1;

    // 爆発設定
    [Header("爆発設定")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int explosionDamage = 20;

    // エフェクトの位置補正
    [Header("エフェクト位置")]
    [SerializeField] private Vector3 effectOffset = Vector3.zero;

    void Update()
    {
        // デバッグ用
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Damage(1);
        }
    }

    /// ダメージを受ける
    public void Damage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Explode();
        }
    }

    /// 爆発
    private void Explode()
    {
        // 爆発エフェクト生成
        if (explosionEffect != null)
        {
            Instantiate(
                explosionEffect,
                transform.position + effectOffset,
                Quaternion.identity);
        }

        // 爆発範囲内のオブジェクトを取得
        Collider[] hitObjects = Physics.OverlapSphere(
            transform.position,
            explosionRadius);

        //foreach (Collider hit in hitObjects)
        //{
        //    // プレイヤー
        //    PlayerHealth player = hit.GetComponent<PlayerHealth>();
        //    if (player != null)
        //    {
        //        player.Damage(explosionDamage);
        //    }

        //    // 敵
        //    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
        //    if (enemy != null)
        //    {
        //        enemy.Damage(explosionDamage);
        //    }
        //}

        // 樽を削除
        Destroy(gameObject);
    }

    // 爆発範囲をSceneビューに表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius);
    }
}