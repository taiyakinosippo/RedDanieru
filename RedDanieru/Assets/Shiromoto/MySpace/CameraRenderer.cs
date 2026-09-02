using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraRenderer : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    private UniversalAdditionalCameraData cameraData;

    // Renderer 0 = 通常
    // Renderer 1 = 暗闇

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
    }

    //明るみ
    public void Bright()
    {
        cameraData.SetRenderer(0);
    }

    //暗み
    public void Dark()
    {
        cameraData.SetRenderer(1);
    }
}