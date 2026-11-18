using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DragonSticker : StickerBase
{
    private Vector3 mouthTransform; //口の位置と向き
    private float fireTimer = 0f; //炎ブレスのタイマー
    private float fireAttackStartTime = 1.3f; //炎吐き始めタイム
    private float fireAttackEndTime = 0.7f; //炎吐き出しタイム
    private float rotateSpeed = 20f; // 溜め中の回転速度
    private float fireBreathRange = 10f; //射程
    private float fireBreathAngle = 15f; //左右の半角
    private bool isFiring = false;
    private bool isFireFinished = false;
    private LayerMask playerLayer; //プレイヤーのレイヤー
    private HashSet<Transform> hitPlayers = new HashSet<Transform>();
    private NavMeshAgent agent;

    public override void OnEnemyApply()
    {
        playerLayer = LayerMask.GetMask("Player");
        agent = GetComponent<NavMeshAgent>();

        //口の位置を取得
        DragonController dragonController = enemyScript as DragonController;

        if (dragonController != null)
            mouthTransform = dragonController.headPos;
        else
            mouthTransform = transform.position + transform.forward * 0.3f + Vector3.up * 1f;
    }

    public override void OnEnemyUpdate()
    {
        //攻撃終了後
        if (isFireFinished)
            return;

        // 予備動作
        if (!isFiring)
        {
            // プレイヤーの方向を向く
            RotateToPlayer();

            fireTimer += Time.deltaTime;

            if (fireTimer >= fireAttackStartTime)
            {
                isFiring = true;
                fireTimer = 0f;
                hitPlayers.Clear();
            }

            return;
        }

        //炎を吐いている時間
        fireTimer += Time.deltaTime;

        FireBreathEffect();

        //炎を吐き終わった後の処理
        if (fireTimer >= fireAttackEndTime)
        {
            isFiring = false;
            isFireFinished = true;

            enemyScript.EndSpecial();

            //初期化
            fireTimer = 0f;
            isFireFinished = false;
        }
    }

    private void RotateToPlayer()
    {
        // 周囲のプレイヤーを検索
        Collider[] hits = Physics.OverlapSphere(transform.position, 30f, playerLayer);

        if (hits.Length == 0)
            return;

        // 最も近いプレイヤーを取得
        Transform target = null;
        float minDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                target = hit.transform;
            }
        }

        if (target == null)
            return;

        // プレイヤー方向を計算
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private void FireBreathEffect()
    {
        Vector3 origin = transform.TransformPoint(mouthTransform);
        Vector3 forward = transform.forward;

        //射程内のプレイヤー候補を取得
        Collider[] hits = Physics.OverlapSphere(
            origin,
            fireBreathRange,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            //プレイヤーの位置
            Vector3 targetPosition = hit.bounds.center;
            //口からプレイヤーへの方向
            Vector3 direction = targetPosition - origin;
            //前方への距離
            float forwardDistance = Vector3.Dot(direction, forward);

            //後方または射程外なら除外
            if (forwardDistance <= 0f || forwardDistance > fireBreathRange)
                continue;

            //前方方向への投影位置
            Vector3 projectedPosition = origin + forward * forwardDistance;
            //中心軸からの横方向の距離
            float lateralDistance = Vector3.Distance(targetPosition, projectedPosition);
            //距離に応じた扇状の幅
            float allowedWidth = forwardDistance * Mathf.Tan(fireBreathAngle * Mathf.Deg2Rad);

            //扇状の範囲外なら除外
            if (lateralDistance > allowedWidth)
                continue;

            //同じプレイヤーへの多重ダメージ防止
            Transform playerRoot = hit.transform;

            if (!hitPlayers.Add(playerRoot))
                continue;

            PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();

            if (playerStatus != null && !playerStatus.isInvincible)
            {
                Debug.Log("攻撃を受けた！");
                playerStatus.Damage(300);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        //炎を吐いている間だけ表示
        if (!isFiring)
            return;

        Vector3 origin = transform.TransformPoint(mouthTransform);
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        int segments = 32;
        int rings = 4;

        //判定範囲の色
        Gizmos.color = Color.red;

        //扇状の外周と内部の線を描画
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(
                -fireBreathAngle,
                fireBreathAngle,
                (float)i / segments
            );

            // 中心軸から左右に広がる方向
            Vector3 direction =
                Quaternion.AngleAxis(angle, up) * forward;

            // 左右の境界線
            if (i == 0 || i == segments)
            {
                Gizmos.DrawLine(
                    origin,
                    origin + direction * fireBreathRange
                );
            }

            // 扇状の外周
            if (i > 0)
            {
                float previousAngle = Mathf.Lerp(
                    -fireBreathAngle,
                    fireBreathAngle,
                    (float)(i - 1) / segments
                );

                Vector3 previousDirection =
                    Quaternion.AngleAxis(previousAngle, up) * forward;

                Gizmos.DrawLine(
                    origin + previousDirection * fireBreathRange,
                    origin + direction * fireBreathRange
                );
            }
        }

        // 内部の距離目安を描画
        for (int i = 1; i <= rings; i++)
        {
            float distance =
                fireBreathRange * i / rings;

            Vector3 previousPoint = Vector3.zero;

            for (int j = 0; j <= segments; j++)
            {
                float angle = Mathf.Lerp(
                    -fireBreathAngle,
                    fireBreathAngle,
                    (float)j / segments
                );

                Vector3 direction =
                    Quaternion.AngleAxis(angle, up) * forward;

                Vector3 point = origin + direction * distance;

                if (j > 0)
                {
                    Gizmos.DrawLine(previousPoint, point);
                }

                previousPoint = point;
            }
        }

        // 攻撃の中心軸
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(
            origin,
            origin + forward * fireBreathRange
        );

        // 発生位置
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(origin, 0.1f);
    }
}
