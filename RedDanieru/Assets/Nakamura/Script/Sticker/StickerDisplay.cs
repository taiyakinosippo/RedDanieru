using UnityEngine;
using System;

public class StickerDisplay : MonoBehaviour
{
    //ステッカーの種類と表示するビジュアルの対応を設定するクラス
    [Serializable] private class StickerVisual
    {
        public Sticker stickerType;
        public Material visual;
    }

    [SerializeField] private StickerVisual[] stickerVisuals;  //ステッカーの種類と表示するビジュアルの対応を設定する配列

    private Renderer stickerRenderer;  //ステッカーの表示に使用するRenderer

    void Awake()
    {
        stickerRenderer = GetComponent<Renderer>();
        stickerRenderer.enabled = false;
    }

    //ステッカーを更新して表示する
    public void ShowSticker(Sticker stickerType)
    {
        stickerRenderer.enabled = true;

        foreach (var visual in stickerVisuals)
        {
            if (visual.stickerType == stickerType)
            {
                stickerRenderer.material = visual.visual;
                break;
            }
        }
    }

    //ステッカーを非表示にする
    public void HideSticker()
    {
        stickerRenderer.enabled = false;
    }
}
