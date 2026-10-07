using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using Player;

/// <summary>
/// プレイヤーごとの「自分が操作するキャラか、他の人のキャラか」の切り替え
/// 他の人のキャラは入力・カメラ・HUDを止め、NetworkTransform等で同期された動きを表示するだけにする
/// </summary>
public class NetworkAuthorityController : NetworkBehaviour
{
    private static readonly List<NetworkAuthorityController> activePlayers =
        new List<NetworkAuthorityController>();

    // ルームにいるプレイヤー（途中で抜けた人は含まない）
    public static IReadOnlyList<NetworkAuthorityController> ActivePlayers => activePlayers;

    // この画面で操作しているプレイヤー
    public static NetworkAuthorityController Local { get; private set; }

    [Networked] public NetworkBool IsDead { get; set; }

    // アニメーションのトリガー（攻撃・ステッカー・回避）を他の人の画面に送るための履歴
    // トリガーは一瞬で消えるので、回数を数えて「前回から増えた分」を再生する（取りこぼさない）
    private const int TriggerBufferSize = 8;

    [Networked] private int AnimTriggerCount { get; set; }

    [Networked, Capacity(TriggerBufferSize)]
    private NetworkArray<int> AnimTriggerHashes => default;

    private int playedTriggerCount;
    private Animator animator;

    private PlayerInput playerInput;
    private PlayerActor playerActor;
    private Player.PlayerCamera playerCamera;
    private PlayerStatus playerStatus;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        activePlayers.Clear();
        Local = null;
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerActor = GetComponent<PlayerActor>();
        playerCamera = GetComponent<Player.PlayerCamera>();
        playerStatus = GetComponent<PlayerStatus>();
    }

    public override void Spawned()
    {
        activePlayers.Add(this);

        animator = GetComponent<Animator>();

        // 入ってくる前のトリガーは再生しない
        playedTriggerCount = AnimTriggerCount;

        if (!HasStateAuthority)
        {
            DisableRemoteOnlyComponents();
            return;
        }

        Local = this;

        // ダメージ表示などは自分のキャラのUI（自分のカメラ基準）を使う
        UIManager uiManager = GetComponentInChildren<UIManager>(true);

        if (uiManager != null)
        {
            UIManager.SetLocal(uiManager);
        }

        // 自分のカメラを登録
        if (playerCamera != null &&
            playerCamera.currentCamera != null)
        {
            LocalCameraManager.CameraTransform =
                playerCamera.currentCamera.transform;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        activePlayers.Remove(this);

        if (Local == this)
        {
            Local = null;
        }
    }

    private void DisableRemoteOnlyComponents()
    {
        if (playerInput != null)
            playerInput.enabled = false;

        if (playerActor != null)
            playerActor.enabled = false;

        if (playerCamera != null)
        {
            playerCamera.DisableCamera();
            playerCamera.enabled = false;
        }

        // 他の人のHPバー（画面全体に出るUI）が自分の画面に重ならないようにする
        // HPバー本体はPlayerUIの兄弟（PlayerUI/Hp）にあるので、HUDのCanvasごと止める
        PlayerUI playerUI = GetComponentInChildren<PlayerUI>(true);

        if (playerUI != null)
        {
            Canvas hud = playerUI.GetComponentInParent<Canvas>(true);

            GameObject hudRoot = hud != null ? hud.gameObject : playerUI.gameObject;

            hudRoot.SetActive(false);
        }

        // ステッカー欄の選択（マウスホイール/数字キー）を他の人のキャラで受けない
        StickerInteractor stickerInteractor = GetComponent<StickerInteractor>();

        if (stickerInteractor != null)
        {
            stickerInteractor.enabled = false;
        }
    }

    /// <summary>
    /// このキャラを自分が操作しているか（ソロでは常にtrue）
    /// </summary>
    public static bool IsLocallyControlled(GameObject player)
    {
        NetworkObject networkObject = player.GetComponentInParent<NetworkObject>();

        if (networkObject == null || !networkObject.IsValid)
            return true;

        return networkObject.HasStateAuthority;
    }

    //==================================================
    // アニメーションのトリガー
    //==================================================

    /// <summary>
    /// 自分のキャラで再生したトリガーを他の人の画面にも送る
    /// </summary>
    public void SendAnimatorTrigger(int triggerHash)
    {
        if (!HasStateAuthority)
            return;

        AnimTriggerHashes.Set(AnimTriggerCount % TriggerBufferSize, triggerHash);
        AnimTriggerCount++;
    }

    public override void Render()
    {
        if (HasStateAuthority || animator == null)
            return;

        int count = AnimTriggerCount;

        // 大きく遅れたときは古いものを飛ばす
        if (count - playedTriggerCount > TriggerBufferSize)
        {
            playedTriggerCount = count - TriggerBufferSize;
        }

        while (playedTriggerCount < count)
        {
            animator.SetTrigger(AnimTriggerHashes[playedTriggerCount % TriggerBufferSize]);
            playedTriggerCount++;
        }
    }

    //==================================================
    // ダメージ
    //==================================================

    /// <summary>
    /// 他の人のキャラに当たった攻撃を、そのキャラを操作している人に届ける
    /// </summary>
    public void SendDamageToOwner(int damage)
    {
        RPC_TakeDamage(damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_TakeDamage(int damage)
    {
        if (playerStatus != null)
        {
            playerStatus.Damage(damage);
        }
    }

    //==================================================
    // 死亡
    //==================================================

    public void MarkDead()
    {
        if (HasStateAuthority)
        {
            IsDead = true;
        }
    }
}
