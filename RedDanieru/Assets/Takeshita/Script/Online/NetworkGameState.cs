using System.Linq;
using Fusion;
using UnityEngine;

/// <summary>
/// ルーム全員で共有するゲーム状態（開始・クリア・死亡）
/// State Authority はマスタークライアントが持つ（Prefabの Is Master Client Object）
/// [Networked] の値は State Authority しか書き換えられないので、他の人はRPCでお願いする
/// 敵とステッカーの同期は NetworkGameState.Enemy.cs
/// </summary>
public partial class NetworkGameState : NetworkBehaviour
{
    public static NetworkGameState Instance { get; private set; }

    [Networked] public NetworkBool GameStarted { get; set; }

    [Networked] public NetworkBool IsCleared { get; set; }

    [Networked] public int DeadPlayerCount { get; set; }

    [Networked] public int StartPlayerCount { get; set; }

    [Networked] public int SharedCoin { get; private set; } = 10000;

    [Networked] public int RetryVoteCount { get; set; }

    [Networked] public int RetryCountdown { get; set; }

    [Networked] private TickTimer RetryTimer { get; set; }

    [Networked] public NetworkBool RetryCountdownStarted { get; set; }

    private bool localStartHandled;
    private bool localClearHandled;
    private float localStartTime;

    public override void Spawned()
    {
        Instance = this;

        Debug.Log(
            $"NetworkGameStateは {gameObject.name} に付いています"
        );

        SpawnedEnemySync();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }

        DespawnedEnemySync();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        WriteEnemyStates();

        if (!RetryCountdownStarted &&RetryVoteCount >= Runner.ActivePlayers.Count())
        {
            RetryCountdownStarted = true;

            RetryCountdown = 5;

            RetryTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    1f
                );
        }

        if (RetryCountdown > 0 &&RetryTimer.Expired(Runner))
        {
            RetryCountdown--;

            RetryTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    1f
                );

            if (RetryCountdown <= 0)
            {
                RPC_StartRetry();
            }
        }
    }


    public override void Render()
    {
        // RPCは後から入った人に届かないので、状態を見てそれぞれの画面で反映する
        if (GameStarted && !localStartHandled)
        {
            localStartHandled = true;
            localStartTime = Time.realtimeSinceStartup;

            FusionLauncher.Instance?.HandleGameStarted();
        }

        if (IsCleared && !localClearHandled)
        {
            localClearHandled = true;

            ShowClearLocal();
        }

        RetryButton retryButton =
      FindObjectOfType<RetryButton>();

        if (retryButton != null)
        {
            retryButton.UpdateRetryText();
        }

        RenderEnemyStates();
    }

    //==================================================
    // ゲーム開始
    //==================================================

    public void RequestStartGame()
    {
        RPC_RequestStartGame();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestStartGame()
    {
        if (GameStarted)
            return;

        StartPlayerCount = Runner.ActivePlayers.Count();
        GameStarted = true;

        // 開始後は途中参加させない（スタートの合図を受け取れず止まってしまうため）
        if (Runner.SessionInfo.IsValid)
        {
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
        }
    }

    //==================================================
    // クリア
    //==================================================

    public void RequestClear()
    {
        RPC_RequestClear();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestClear(RpcInfo info = default)
    {
        if (IsCleared)
            return;

        IsCleared = true;

    }

    private void ShowClearLocal()
    {
        GoalClear goal = FindObjectOfType<GoalClear>();

        if (goal != null)
        {
            goal.ShowClear();
        }
        else
        {
            Debug.LogWarning("GoalClearが見つからないためクリアUIを表示できません");
        }
    }

    //==================================================
    // 死亡・ゲームオーバー
    //==================================================

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerDied()
    {
        DeadPlayerCount++;
    }

    /// <summary>
    /// ルームに残っているプレイヤーが全員死んでいるか
    /// 途中で抜けた人は数えない
    /// </summary>
    public bool AreAllPlayersDead()
    {
        if (!localStartHandled)
            return false;

        // スポーン待ちの人がいる開始直後は判定しない
        if (Time.realtimeSinceStartup - localStartTime < 3f)
            return false;

        var players = NetworkAuthorityController.ActivePlayers;

        return players.Count > 0 && players.All(player => player.IsDead);
    }

    //購入量
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_BuyItem(int price)
    {
        if (SharedCoin < price)
            return;

        SharedCoin -= price;

        Debug.Log($"購入後コイン: {SharedCoin}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddRetryVote()
    {
        RetryVoteCount++;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ResetRetryVote()
    {
        RetryVoteCount = 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartRetry()
    {
        if (HasStateAuthority)
        {
            IsCleared = false;

            RetryVoteCount = 0;
            RetryCountdown = 0;
            RetryCountdownStarted = false;

            DeadPlayerCount = 0;
        }

        localClearHandled = false;
        localStartHandled = false;

        GoalClear goalClear =
            FindObjectOfType<GoalClear>();

        if (goalClear != null)
        {
            goalClear.ResetClearState();
        }

        GameOverManager gameOverManager =
            FindObjectOfType<GameOverManager>();

        if (gameOverManager != null)
        {
            gameOverManager.ResetGameOverState();
        }

        RetryButton[] buttons =
       FindObjectsOfType<RetryButton>(true);

        foreach (RetryButton button in buttons)
        {
            button.ResetRetryState();
        }

        NetworkAuthorityController[] players =
    FindObjectsOfType<NetworkAuthorityController>();

        foreach (NetworkAuthorityController player in players)
        {
            player.ResetDeadState();
        }

        FusionLauncher.Instance.DespawnLocalPlayer();

        FusionLauncher.Instance.HandleGameStarted();
    }
}
