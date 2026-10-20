using System.Collections;
using UnityEngine;
using Player;

public class BigSticker : StickerBase
{
    private float bigScale = 3.0f;  //拡大率
    private float playerScale = 2.0f; //プレイヤー拡大率
    private float scaleTime = 1.0f;  //拡大縮小時間
    private int addPower = 100;  //攻撃力の拡大率
    private float addSearchScale = 1.0f;  //索敵範囲の拡大率
    private float addAttackScale = 1.0f;  //攻撃範囲の拡大率
    private float addAttackCoolTime = 0.5f;  //攻撃クールタイムの拡大率
    private float setSpeed = 1.0f;  //移動速度の変化量
    private float saveSpeed;  //保存する速度
    private int saveAttack; //保存する攻撃力

    private PlayerCamera playerCamera;
    private PlayerStatus playerStatus;

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //スケールを拡大
        StartCoroutine(Grow(bigScale));

        //敵の攻撃力、攻撃範囲、攻撃クールタイムを増加
        enemyScript.AddEnemyPower(addPower);
        enemyScript.AddEnemySearchArea(addSearchScale);
        enemyScript.AddEnemyAttackArea(addAttackScale);
        enemyScript.AddEnemyAttackCoolTime(addAttackCoolTime);
        saveSpeed = enemyScript.SetEnemyMoveSpeed(setSpeed);
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //スケールを元に戻す
        transform.localScale = new Vector3(1f, 1f, 1.0f);

        //敵の攻撃力、攻撃範囲、攻撃クールタイムを元に戻す
        enemyScript.AddEnemyPower(-addPower);
        enemyScript.AddEnemySearchArea(-addSearchScale);
        enemyScript.AddEnemyAttackArea(-addAttackScale);
        enemyScript.AddEnemyAttackCoolTime(-addAttackCoolTime);
        enemyScript.SetEnemyMoveSpeed(saveSpeed);

        //float scale = transform.localScale.x;  //現在の拡大率

        //while (scale >= 1.0f)
        //{
        //    scale = Mathf.Lerp(scale, 1.0f, scaleTime * Time.deltaTime);
        //    transform.localScale = new Vector3(scale, scale, scale);
        //    yield return null;
        //}
    }

    //ステッカーが物に貼られたときの処理
    public override void OnTrapApply()
    {
        //スケールを拡大
        StartCoroutine(Grow(bigScale));
    }

    //ステッカーが物から剥がれたときの処理
    public override void OnTrapRemove()
    {
        //スケールを元に戻す
        transform.localScale = new Vector3(1f, 1f, 1.0f);
    }

    public override void OnPlayerApply()
    {
        playerStatus = GetComponent<PlayerStatus>();

        if (playerStatus != null)
        {
            saveAttack =
                playerStatus.SetAttack(
                    Mathf.RoundToInt(
                        playerStatus._playerAttack * 1.5f));

            saveSpeed = playerStatus.SetMoveSpeed(
                playerStatus._playerMoveSpeed);
        }

        //スケールを拡大
        StartCoroutine(Grow(playerScale));

        playerCamera =
    GetComponent<PlayerCamera>();

        if (playerCamera != null)
        {
            playerCamera.SetCameraDistance(1.5f);
        }

        Debug.Log("Big発動");
    }

    public override void OnPlayerRemove()
    {
        //スケールを元に戻す
        transform.localScale = new Vector3(1f, 1f, 1.0f);

        if (playerStatus == null)
        {
            playerStatus =
                GetComponent<PlayerStatus>();
        }

        if (playerStatus != null)
        {
            playerStatus.SetAttack(saveAttack);
            playerStatus.SetMoveSpeed(saveSpeed);
        }

        if (playerCamera != null)
        {
            playerCamera.ResetCameraDistance();
        }

        Debug.Log("Big解除");
    }

    //徐々に拡大
    private IEnumerator Grow(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * targetScale;

        Collider col = GetComponent<Collider>();

        if (col == null)
        {
            Debug.LogWarning("BigSticker: Colliderが見つかりません");
            yield break;
        }

        //拡大開始時の底面の高さ
        float originalBottomY = col.bounds.min.y;

        float elapsed = 0f;

        while (elapsed < scaleTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleTime);

            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            //拡大によって下がった底面を元の高さまで戻す
            float diff = originalBottomY - col.bounds.min.y;

            if (diff > 0f)
            {
                transform.position += Vector3.up * diff;
            }

            yield return null;
        }

        transform.localScale = endScale;

        //最後にもう一度底面の高さを補正
        float finalDiff = originalBottomY - col.bounds.min.y;

        if (finalDiff > 0f)
        {
            transform.position += Vector3.up * finalDiff;
        }
    }
}
