using UnityEngine;
using Player;

public class LeglessSticker : StickerBase
{
    private float saveSpeed;  //保存する速度

    private PlayerStatus playerStatus;

    //ステッカーが敵に貼られたときの処理
    public override void OnEnemyApply()
    {
        //ステッカーによる移動停止を解除
        enemyScript.isStickerStopped = true;
    }

    //ステッカーが敵から剥がれたときの処理
    public override void OnEnemyRemove()
    {
        //ステッカーによる移動停止を解除
        enemyScript.isStickerStopped = false;
    }

    public virtual void OnTrapApply() 
    {
    
    }
    
    public virtual void OnTrapRemove() 
    { 
    
    }

    //Stickerがプレイヤーから貼られたとき
    public override void OnPlayerApply()
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
    public override void OnPlayerRemove()
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
