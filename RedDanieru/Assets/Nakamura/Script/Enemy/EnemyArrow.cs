using Player;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    public int enemyPower = 10;  //敵の攻撃力
    private LeglessArcherController leglessArcherController;  //矢を撃った敵のスクリプト

    private float moveSpeed = 20f;  //矢の移動速度
    private float fallSpeed = 1f;  //矢の落下速度
    private float firstFallSpeed = 1f;  //矢の落下初速度
    private float maxFallSpeed = 5f;  //矢の最大落下速度
    private float fallAcceleration = 0.3f;  //矢の落下加速度

    void Start()
    {
        fallSpeed = firstFallSpeed;  //矢の落下速度を初期化
    }

    void Update()
    {
        //矢の移動処理をここに追加
        ArrowMove();
    }

    //矢の移動処理
    private void ArrowMove()
    {
        //正面方向に移動しつつ、少しづつ落下
        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        transform.Translate(Vector3.down * Time.deltaTime * fallSpeed);

        //落下速度の上昇
        fallSpeed += fallAcceleration * Time.deltaTime;
        //落下速度の上限を設定
        fallSpeed = Mathf.Min(fallSpeed, maxFallSpeed);
    }

    //敵の攻撃力を設定する関数
    public void Setting(int power, LeglessArcherController controller)
    {
        enemyPower = power;
        leglessArcherController = controller;
    }

    //矢の初期化処理
    private void ResetArrow()
    {
        enemyPower = 10;  //デフォルトの攻撃力にリセット
        fallSpeed = firstFallSpeed;  //矢の落下速度を初期化
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Arrow Hit : " + other.gameObject.name);

        //プレイヤーに当たった処理
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus =
                other.GetComponentInParent<PlayerStatus>();

            if (playerStatus != null)
            {
                playerStatus.Damage(enemyPower);
            }
        }

        //敵には当たっても消さない
        if (other.CompareTag("Enemy"))
        {
            return;
        }

        //初期化
        ResetArrow();

        //矢をプールに返却
        leglessArcherController.ReturnArrowToPool(gameObject);
    }
}
