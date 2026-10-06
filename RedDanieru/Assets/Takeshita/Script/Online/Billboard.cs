using UnityEngine;
using Player;

public class NameBillboard : MonoBehaviour
{
    private PlayerCamera playerCamera;

    private void Start()
    {
        playerCamera =
            GetComponentInParent<PlayerCamera>();
    }

    private void LateUpdate()
    {
        if (playerCamera == null ||
            playerCamera.currentCamera == null)
            return;

        Transform cam =
            playerCamera.currentCamera.transform;

        Vector3 dir =
            transform.position - cam.position;

        transform.rotation =
            Quaternion.LookRotation(dir);

        transform.Rotate(0f, 180f, 0f);
    }
}