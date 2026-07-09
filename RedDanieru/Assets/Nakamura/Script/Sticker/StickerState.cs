using UnityEngine;

public class StickerState : MonoBehaviour
{
    public Sticker CurrentSticker { get; private set; } = Sticker.None;

    private StickerBase currentStickerScript;

    private void Start()
    {
        //ゲーム開始時
        if (CurrentSticker != Sticker.None)
        {
            Apply(CurrentSticker);
        }
    }

    public void Apply(Sticker sticker)
    {
        //既に貼られていたら剥がす
        Remove();

        CurrentSticker = sticker;

        switch (sticker)
        {
            case Sticker.Big:
                //currentStickerScript = gameObject.AddComponent<>();
                break;

            case Sticker.Invisible:
                //currentStickerScript = gameObject.AddComponent<>();
                break;
        }

        currentStickerScript?.OnEnemyApply();
    }

    public Sticker Remove()
    {
        if (currentStickerScript != null)
        {
            currentStickerScript.OnEnemyRemove();
            Destroy(currentStickerScript);
        }

        Sticker old = CurrentSticker;

        CurrentSticker = Sticker.None;
        currentStickerScript = null;

        return old;
    }
}
