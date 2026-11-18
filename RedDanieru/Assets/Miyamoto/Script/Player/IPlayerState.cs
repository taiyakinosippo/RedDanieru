using UnityEngine;

public interface IPlayerState 
{
    //ステートに入ったときに呼ばれる
    void Enter();

    //毎フレーム呼ばれる
    void Update();

    //ステートから抜けるときに呼ばれる
    void Exit();
}
