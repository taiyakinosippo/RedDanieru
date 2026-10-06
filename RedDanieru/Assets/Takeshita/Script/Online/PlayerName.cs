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
            RPC_SetName(UserData.UserName);
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