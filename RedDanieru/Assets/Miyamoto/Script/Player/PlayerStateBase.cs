using UnityEngine;

public abstract class PlayerStateBase : IPlayerState
{
    protected PlayerStateMachine stateMachine;

    // これが連絡先をセットするコンストラクタ（初期化メソッド）
    public PlayerStateBase(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
