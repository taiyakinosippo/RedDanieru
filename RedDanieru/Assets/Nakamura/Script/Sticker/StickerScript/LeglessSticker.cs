using UnityEngine;
using Player;

public class LeglessSticker : StickerBase
{
    private float saveSpeed;  //保存する速度

    private PlayerStatus playerStatus;

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //敵の速度を保存
        saveSpeed = enemyScript.SetEnemyMoveSpeed(0.0f);
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //敵の速度を元に戻す
        enemyScript.AddEnemyMoveSpeed(saveSpeed);
    }

    //Stickerがプレイヤーから貼られたとき
    public override void OnTrapApply()
    {
        playerStatus =
        GetComponent<Player.PlayerStatus>();

        if (playerStatus != null)
        {
            saveSpeed = playerStatus.SetMoveSpeed(0f);

            Debug.Log("Legless発動");
        }
    }

    //Stickerがプレイヤーから剝がされたとき
    public override void OnTrapRemove()
    {
        Debug.Log("Legless解除");

        if (playerStatus == null)
        {
            playerStatus =
                GetComponent<Player.PlayerStatus>();
        }

        if (playerStatus != null)
        {
            playerStatus.SetMoveSpeed(saveSpeed);

            Debug.Log(
                $"復元後 speed={playerStatus._playerMoveSpeed}"
            );
        }
    }

}
