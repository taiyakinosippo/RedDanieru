using UnityEngine;

public class NameBillboard : MonoBehaviour
{
    private void LateUpdate()
    {
        if (LocalCameraManager.CameraTransform == null)
            return;

        Transform cam =
            LocalCameraManager.CameraTransform;

        transform.LookAt(
            transform.position +
            cam.forward,
            cam.up
        );
    }
}