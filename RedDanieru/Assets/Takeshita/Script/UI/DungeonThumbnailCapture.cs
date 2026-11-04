using UnityEngine;
using System;

public class DungeonThumbnailCapture : MonoBehaviour
{
    [SerializeField]
    private Camera thumbnailCamera;

    [SerializeField]
    private Light thumbnailLight;

    [SerializeField]
    private RenderTexture renderTexture;

    private void Start()
    {
        thumbnailCamera.enabled = false;

        if (thumbnailLight != null)
        {
            thumbnailLight.enabled = false;
        }
    }

    public string CaptureBase64()
    {
        thumbnailCamera.enabled = true;

        if (thumbnailLight != null)
        {
            thumbnailLight.enabled = true;
        }

        RenderTexture currentRT =
            RenderTexture.active;

        RenderTexture.active =
            renderTexture;

        thumbnailCamera.targetTexture =
            renderTexture;

        thumbnailCamera.Render();

        Texture2D tex =
            new Texture2D(
                renderTexture.width,
                renderTexture.height,
                TextureFormat.RGB24,
                false
            );

        tex.ReadPixels(
            new Rect(
                0,
                0,
                renderTexture.width,
                renderTexture.height
            ),
            0,
            0
        );

        tex.Apply();

        byte[] png = tex.EncodeToPNG();

        string base64 =
            Convert.ToBase64String(png);

        RenderTexture.active = currentRT;

        Destroy(tex);

        // 撮影後OFF
        thumbnailCamera.enabled = false;

        if (thumbnailLight != null)
        {
            thumbnailLight.enabled = false;
        }

        return base64;
    }
}