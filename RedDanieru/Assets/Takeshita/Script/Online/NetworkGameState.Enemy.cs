using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// 同期する敵1体分の状態
/// </summary>
public struct EnemyNetState : INetworkStruct
{
    public int Id;
    public Vector3 Position;
    public float Yaw;
    public Vector3 Scale;
    public int Hp;
    public NetworkBool Alive;
    public byte Sticker;
}

/// <summary>
/// 敵とステッカーの同期
///
/// 敵はマップ読み込み時に各PCで生成される（NetworkObjectではない）ので、
///   ・マスタークライアントの敵だけAIを動かす
///   ・位置/向き/大きさ/HP/貼られているステッカーを全員に配り、他の人の画面ではそれを再現する
///   ・ダメージやステッカーの貼り剥がしはRPCでマスタークライアントにお願いする
/// 敵の対応付けはマップ上のマス座標から作るID（NetworkSyncId）で行う
/// </summary>
public partial class NetworkGameState
{
    private const int MaxSyncedEnemies = 96;

    [Networked, Capacity(MaxSyncedEnemies)]
    private NetworkArray<EnemyNetState> EnemyStates => default;

    [Networked] private int EnemyStateCount { get; set; }

    // マスタークライアントが書き込む順番（IDの昇順）
    private readonly List<int> authoritySlots = new List<int>();

    private MapManager mapManager;
    private int slotsMapRevision = -1;
    private int mapRevisionSeenFrame = -1;

    // この画面で敵のAIを動かしているか（マスタークライアントが変わったら切り替える）
    private bool? enemiesSimulatedHere;

    private void SpawnedEnemySync()
    {
        mapManager = FindObjectOfType<MapManager>();
        enemiesSimulatedHere = null;
    }

    private void DespawnedEnemySync()
    {
        // ルームを抜けたら、残っている敵は普通に動くように戻す
        SetEnemySimulation(true);
    }

    //==================================================
    // マスタークライアント : 敵の状態を書き込む
    //==================================================

    private void WriteEnemyStates()
    {
        RebuildSlotsIfMapChanged();

        int count = Mathf.Min(authoritySlots.Count, MaxSyncedEnemies);

        for (int i = 0; i < count; i++)
        {
            int id = authoritySlots[i];

            EnemyNetState state = EnemyStates.Get(i);
            state.Id = id;

            if (TryGetEnemy(id, out EnemyBase enemy))
            {
                Transform t = enemy.transform;

                state.Position = t.position;
                state.Yaw = t.eulerAngles.y;
                state.Scale = t.localScale;
                state.Hp = Mathf.CeilToInt(enemy.CurrentHp);
                state.Alive = true;

                StickerState sticker = enemy.GetComponent<StickerState>();
                state.Sticker = (byte)(sticker != null ? sticker.currentSticker : Sticker.None);
            }
            else
            {
                // 倒された（Destroyされた）敵
                state.Alive = false;
            }

            EnemyStates.Set(i, state);
        }

        EnemyStateCount = count;
    }

    private void RebuildSlotsIfMapChanged()
    {
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();

            if (mapManager == null)
                return;
        }

        int revision = mapManager.MapRevision;

        if (revision == slotsMapRevision)
            return;

        // 読み直し前の敵はフレームの最後にDestroyされるので、1フレーム待ってから数える
        if (mapRevisionSeenFrame < 0)
        {
            mapRevisionSeenFrame = Time.frameCount;
            return;
        }

        if (Time.frameCount == mapRevisionSeenFrame)
            return;

        authoritySlots.Clear();

        foreach (KeyValuePair<int, NetworkSyncId> pair in NetworkSyncId.All)
        {
            if (pair.Value != null && pair.Value.GetComponent<EnemyBase>() != null)
            {
                authoritySlots.Add(pair.Key);
            }
        }

        authoritySlots.Sort();

        if (authoritySlots.Count > MaxSyncedEnemies)
        {
            Debug.LogWarning($"敵が多すぎるため {MaxSyncedEnemies} 体までしか同期しません（{authoritySlots.Count}体）");
        }

        slotsMapRevision = revision;
        mapRevisionSeenFrame = -1;

        Debug.Log($"敵の同期対象を更新 : {authoritySlots.Count}体");
    }

    //==================================================
    // 全員 : 敵の状態を画面に反映する
    //==================================================

    private void RenderEnemyStates()
    {
        bool simulateHere = HasStateAuthority;

        if (enemiesSimulatedHere != simulateHere)
        {
            enemiesSimulatedHere = simulateHere;
            SetEnemySimulation(simulateHere);
        }

        if (simulateHere)
            return;

        // 後からマップを読み込んだ敵もAIを止める
        SetEnemySimulation(false);

        int count = Mathf.Min(EnemyStateCount, MaxSyncedEnemies);

        for (int i = 0; i < count; i++)
        {
            EnemyNetState state = EnemyStates[i];

            if (state.Id == 0 || !TryGetEnemy(state.Id, out EnemyBase enemy))
                continue;

            if (!state.Alive)
            {
                enemy.DieByNetwork((Sticker)state.Sticker);
                continue;
            }

            enemy.ApplyNetworkState(
                state.Position,
                state.Yaw,
                state.Scale,
                state.Hp,
                (Sticker)state.Sticker);
        }
    }

    private static void SetEnemySimulation(bool simulate)
    {
        foreach (NetworkSyncId syncId in NetworkSyncId.All.Values)
        {
            if (syncId != null && syncId.TryGetComponent(out EnemyBase enemy))
            {
                enemy.SetNetworkProxy(!simulate);
            }
        }
    }

    private static bool TryGetEnemy(int id, out EnemyBase enemy)
    {
        enemy = null;

        return NetworkSyncId.TryGet(id, out NetworkSyncId syncId) &&
               syncId.TryGetComponent(out enemy);
    }

    private static bool IsOnline(out NetworkGameState state)
    {
        state = Instance;

        return state != null && state.Object != null && state.Object.IsValid;
    }

    //==================================================
    // 敵へのダメージ
    //==================================================

    /// <summary>
    /// プレイヤーの攻撃が敵に当たったときはここを通す
    /// マルチではマスタークライアントの敵にダメージを入れ、結果が全員に配られる
    /// </summary>
    public static void DamageEnemy(EnemyBase enemy, int damage)
    {
        int id = NetworkSyncId.Of(enemy);

        if (!IsOnline(out NetworkGameState state) || id == 0)
        {
            enemy.Damage(damage);
            return;
        }

        state.RPC_DamageEnemy(id, damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DamageEnemy(int id, int damage)
    {
        if (!TryGetEnemy(id, out EnemyBase enemy) || !enemy.CanTakeDamage)
            return;

        // マスタークライアントの画面ではここでダメージ表示も出る
        enemy.Damage(damage);

        RPC_ShowEnemyDamage(id, damage);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowEnemyDamage(int id, int damage)
    {
        if (HasStateAuthority)
            return;

        if (TryGetEnemy(id, out EnemyBase enemy) && UIManager.Instance != null)
        {
            UIManager.Instance.ShowDamage(damage, enemy.transform.position);
        }
    }

    //==================================================
    // 敵の矢（見た目だけ他の人の画面にも出す）
    //==================================================

    public static void NotifyArrowFired(EnemyBase archer, Vector3 position, Quaternion rotation)
    {
        int id = NetworkSyncId.Of(archer);

        if (!IsOnline(out NetworkGameState state) || !state.HasStateAuthority || id == 0)
            return;

        state.RPC_ArrowFired(id, position, rotation);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ArrowFired(int id, Vector3 position, Quaternion rotation)
    {
        if (HasStateAuthority)
            return;

        if (TryGetEnemy(id, out EnemyBase enemy) &&
            enemy is LeglessArcherController archer)
        {
            archer.FireVisualArrow(position, rotation);
        }
    }

    //==================================================
    // ステッカーの貼り剥がし
    //==================================================

    /// <summary>
    /// マルチのときはステッカーを剥がす処理をマスタークライアントにお願いする
    /// 戻り値がtrueなら呼び出し側では何もしない（結果は後で届く）
    /// </summary>
    public static bool RequestPeelSticker(StickerState target, int slot)
    {
        int id = NetworkSyncId.Of(target);

        if (!IsOnline(out NetworkGameState state) || id == 0)
            return false;

        state.RPC_RequestPeel(id, slot);
        return true;
    }

    /// <summary>
    /// マルチのときはステッカーを貼る処理をマスタークライアントにお願いする
    /// 失敗したらステッカーは手元に戻ってくる
    /// </summary>
    public static bool RequestApplySticker(StickerState target, Sticker sticker, int slot)
    {
        int id = NetworkSyncId.Of(target);

        if (!IsOnline(out NetworkGameState state) || id == 0)
            return false;

        state.RPC_RequestApply(id, (byte)sticker, slot);
        return true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPeel(int id, int slot, RpcInfo info = default)
    {
        Sticker removed = Sticker.None;

        if (NetworkSyncId.TryGet(id, out NetworkSyncId syncId) &&
            syncId.TryGetComponent(out StickerState target) &&
            target.currentSticker != Sticker.None)
        {
            removed = target.Remove();

            RPC_StickerChanged(id, (byte)Sticker.None);
        }

        // 先に他の人が剥がしていたら None が返る
        RPC_StickerResult(GetRequester(info), slot, (byte)removed);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestApply(int id, byte sticker, int slot, RpcInfo info = default)
    {
        if (NetworkSyncId.TryGet(id, out NetworkSyncId syncId) &&
            syncId.TryGetComponent(out StickerState target) &&
            target.currentSticker == Sticker.None)
        {
            target.Apply((Sticker)sticker);

            RPC_StickerChanged(id, sticker);
            return;
        }

        // 先に他の人が貼っていたら手元に戻す
        RPC_StickerResult(GetRequester(info), slot, sticker);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StickerChanged(int id, byte sticker)
    {
        if (HasStateAuthority)
            return;

        if (!NetworkSyncId.TryGet(id, out NetworkSyncId syncId) ||
            !syncId.TryGetComponent(out StickerState target))
            return;

        if (syncId.TryGetComponent(out EnemyBase _))
        {
            // 敵の効果はマスタークライアントで動くので、見た目だけ変える
            target.ApplyVisual((Sticker)sticker);
        }
        else if ((Sticker)sticker == Sticker.None)
        {
            target.Remove();
        }
        else
        {
            target.Apply((Sticker)sticker);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StickerResult([RpcTarget] PlayerRef player, int slot, byte sticker)
    {
        NetworkAuthorityController local = NetworkAuthorityController.Local;

        if (local == null)
            return;

        StickerInteractor interactor = local.GetComponent<StickerInteractor>();

        if (interactor != null)
        {
            interactor.ReceiveSticker(slot, (Sticker)sticker);
        }
    }

    private PlayerRef GetRequester(RpcInfo info)
    {
        return info.Source.IsRealPlayer ? info.Source : Runner.LocalPlayer;
    }
}
