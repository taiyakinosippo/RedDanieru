using UnityEngine;

//突進ステッカーの敵が壁やプレイヤーにぶつかってスタンしたとき、頭の上で星を回す
//星のプレハブ（StickerEffectManager に設定）を敵の頭の上に置き、スタンが終わったら消す
public class StunStarEffect : MonoBehaviour
{
    //Status Effects FREE の星は「足元から2mの高さに頭がある」前提で作られているので、その分だけ下げて置く
    private const float PrefabHeadHeight = 2.0f;

    //消すときに残りの星が消えるまで待つ時間
    private const float FadeOutTime = 1.0f;

    private EnemyBase enemy;
    private Collider body;
    private float height;
    private float scale = 1.0f;
    private float endTime;
    private bool followStunState;
    private bool stopping;

    /// <summary>
    /// 敵の頭の上に星を出す（duration 秒たつか、スタンが解けたら消える）
    /// </summary>
    public static void Show(EnemyBase enemy, float duration)
    {
        StickerEffectManager manager = StickerEffectManager.Instance;

        if (enemy == null || manager == null)
            return;

        float height = manager.StunStarHeight;
        Vector3 position = HeadPosition(enemy, enemy.GetComponent<Collider>(), height, 1.0f);

        //ぶつかった瞬間に星がはじける
        if (manager.StunStarHitEffect != null)
            Instantiate(manager.StunStarHitEffect, position, Quaternion.identity);

        //スタンの間、頭の上で星が回る
        if (manager.StunStarEffect == null)
            return;

        GameObject stars = Instantiate(manager.StunStarEffect, position, Quaternion.identity);

        //星を大きくする（親の大きさが星にも効くように、全部の層を Hierarchy にする）
        float scale = Mathf.Max(0.1f, manager.StunStarScale);
        stars.transform.localScale = Vector3.one * scale;
        foreach (ParticleSystem ps in stars.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }

        StunStarEffect effect = stars.AddComponent<StunStarEffect>();
        effect.enemy = enemy;
        effect.body = enemy.GetComponent<Collider>();
        effect.height = height;
        effect.scale = scale;
        stars.transform.position = HeadPosition(enemy, effect.body, height, scale);
        effect.endTime = Time.time + duration;

        //AIを動かしている画面ではスタンが解けたら消す
        //（他の人の画面の敵はスタン状態を持っていないので時間で消す）
        effect.followStunState = !enemy.IsNetworkProxy;
    }

    //コライダーの一番上（＝頭）から height だけ上に星の中心が来る位置
    //星は足元から PrefabHeadHeight 上に作られているので、大きくした分だけ足元を下げる
    private static Vector3 HeadPosition(EnemyBase enemy, Collider body, float height, float scale)
    {
        Vector3 top = body != null
            ? new Vector3(body.bounds.center.x, body.bounds.max.y, body.bounds.center.z)
            : enemy.transform.position + Vector3.up * PrefabHeadHeight;

        return top + Vector3.up * (height - PrefabHeadHeight * scale);
    }

    private void LateUpdate()
    {
        if (stopping)
            return;

        //敵が倒れて消えたら星も消す
        if (enemy == null)
        {
            Destroy(gameObject);
            return;
        }

        //敵についていく
        transform.position = HeadPosition(enemy, body, height, scale);

        bool stunEnded = followStunState && !enemy.IsStunned;

        if (stunEnded || Time.time >= endTime)
        {
            StopStars();
        }
    }

    //星を出すのをやめ、出ている星が消えてから削除する
    private void StopStars()
    {
        stopping = true;

        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        Destroy(gameObject, FadeOutTime);
    }
}
