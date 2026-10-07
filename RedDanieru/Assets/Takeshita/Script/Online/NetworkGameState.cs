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

    [Networked]
    public int StartPlayerCount { get; set; }

    private bool localStartHandled;
    private bool localClearHandled;
    private float localStartTime;

    public override void Spawned()
    {
        Instance = this;

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
        if (HasStateAuthority)
        {
            WriteEnemyStates();
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

        Debug.Log($"ゲーム開始 StartPlayerCount={StartPlayerCount}");
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

        Debug.Log($"クリア確定 Goal={info.Source}");
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

        Debug.Log(
            $"DeadPlayerCount={DeadPlayerCount}"
        );
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
}
