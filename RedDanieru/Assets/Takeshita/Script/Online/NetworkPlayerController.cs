using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using Player;

public class NetworkAuthorityController : NetworkBehaviour
{
    private PlayerInput playerInput;
    private PlayerActor playerActor;
    private Player.PlayerCamera playerCamera;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerActor = GetComponent<PlayerActor>();
        playerCamera = GetComponent<Player.PlayerCamera>();
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
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

            return;
        }

        // 自分のカメラを登録
        if (playerCamera != null &&
            playerCamera.currentCamera != null)
        {
            LocalCameraManager.CameraTransform =
                playerCamera.currentCamera.transform;
        }
    }
}