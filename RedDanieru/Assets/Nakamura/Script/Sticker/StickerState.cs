using UnityEngine;

public class StickerState : MonoBehaviour
{
    public Sticker CurrentSticker { get; private set; } = Sticker.None;

    private StickerBase currentStickerScript;

    private void Start()
    {
        //ゲーム開始時に貼られているステッカーがあれば適用する
        if (CurrentSticker != Sticker.None)
        {
            Apply(CurrentSticker);
        }
    }

    private void Update()
    {
        //デバッグ用
        if (Input.GetKeyDown(KeyCode.B))
        {
            Apply(Sticker.Big);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Apply(Sticker.Invisible);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Remove();
        }
        Debug.Log(CurrentSticker.ToString());
    }

    public void Apply(Sticker sticker)
    {
        //既に貼られていたら剥がす
        Remove();

        CurrentSticker = sticker;

        switch (sticker)
        {
            case Sticker.Big:
                currentStickerScript = gameObject.AddComponent<BigSticker>();
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

//public class StickerState : MonoBehaviour
//{
//    public enum State
//    {
//        None,
//        Growing,
//        Active,
//        Shrinking
//    }

//    public State state = State.None;

//    public Sticker currentSticker { get; private set; } = Sticker.None;  //現在貼られているステッカーの種類

//    private StickerBase currentStickerScript;  //現在貼られているステッカーのスクリプト

//    private void Start()
//    {
//        //ゲーム開始時に貼られているステッカーがあれば適用する
//        if (currentSticker != Sticker.None)
//        {
//            Apply(currentSticker);
//        }
//    }

//    private void Update()
//    {
//        if (currentStickerScript != null)
//        {
//            state = currentStickerScript.OnEnemyUpdate(gameObject, state);

//            //ステッカーの状態がNoneになったら、現在のステッカーをリセット
//            if (state == State.None)
//            {
//                currentStickerScript = null;
//                currentSticker = Sticker.None;
//            }
//        }

//        //デバッグ用
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            Apply(Sticker.Big);
//        }
//        else if (Input.GetKeyDown(KeyCode.Alpha2))
//        {
//            Apply(Sticker.Invisible);
//        }
//        else if (Input.GetKeyDown(KeyCode.Backspace))
//        {
//            Remove();
//        }

//        Debug.Log(currentSticker.ToString());
//    }

//    //ステッカーを貼る
//    public void Apply(Sticker type)
//    {
//        //既に貼られていたら剥がす
//        Remove();

//        state = State.Growing;

//        //現在のステッカーの種類を更新
//        switch (type)
//        {
//            case Sticker.Big:
//                currentStickerScript = new BigSticker();
//                currentSticker = Sticker.Big;
//                break;

//            case Sticker.Invisible:
//                //currentStickerScript = new InvisibleSticker();
//                break;
//        }

//        //ステッカーの効果を適用（Start関数のようなもの）
//        //currentStickerScript?.OnEnemyApply(gameObject);
//    }

//    //ステッカーを剥がす
//    public Sticker Remove()
//    {
//        //現在のステッカーがなければ何もしない
//        if (currentStickerScript == null) return Sticker.None;

//        //ステッカーの効果を剥がす
//        //StartCoroutine(currentStickerScript.OnEnemyRemove(gameObject));

//        //現在のステッカーの種類を返す
//        Sticker old = currentSticker;

//        //現在のステッカーの種類をリセット
//        state = State.Shrinking;

//        return old;
//    }
//}
