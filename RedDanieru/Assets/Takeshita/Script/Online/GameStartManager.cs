using Fusion;
using UnityEngine;

public class GameStartManager : NetworkBehaviour
{
    [Rpc(RpcSources.StateAuthority,
         RpcTargets.All)]
    public void RPC_StartGame()
    {
        DungeonUIManager ui =
            FindObjectOfType<DungeonUIManager>();

        if (ui != null)
        {
            ui.HideMatchingUI();
        }
    }
}