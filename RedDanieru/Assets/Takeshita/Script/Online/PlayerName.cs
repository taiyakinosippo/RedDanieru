using Fusion;
using TMPro;
using UnityEngine;

public static class UserData
{
    public static string UserName;
}

public class PlayerName : NetworkBehaviour
{
    [Networked]
    public NetworkString<_32> PlayerNameText { get; set; }

    [SerializeField]
    private TextMeshPro nameText;

    private void Start()
    {
        if (!GameModeManager.IsMultiplayer)
        {
            nameText.gameObject.SetActive(false);
        }
    }

    public override void Spawned()
    {
        if (!GameModeManager.IsMultiplayer)
            return;

        if (Object.HasInputAuthority)
        {
            // タイトルを通らずに始めたとき（マッチングシーンから直接再生など）は名前が空になるので代わりの名前を付ける
            string userName = string.IsNullOrWhiteSpace(UserData.UserName)
                ? $"Player{Runner.LocalPlayer.PlayerId}"
                : UserData.UserName;

            RPC_SetName(userName);
        }
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetName(string userName)
    {
        PlayerNameText = userName;
    }

    public override void Render()
    {
        if (nameText != null)
        {
            nameText.text =
                PlayerNameText.ToString();
        }
    }
}