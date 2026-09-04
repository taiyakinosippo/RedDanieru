using System.Collections.Generic;
using UnityEngine;

public class StickerInteractor : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private List<GameObject> interactObjects = new List<GameObject>();  //インタラクトトリガー内にあるStickerState持ちのオブジェクトのリスト
    [SerializeField] private Sticker[] holdSticker;  //保持してるステッカーのタイプ
    [SerializeField] private int maxHoldCount = 3;  //保持できるステッカーの数
    private int holdIndex = 0;  //保持しているステッカーのインデックス

    void Start()
    {
        //保持できるステッカーの数を設定
        holdSticker = new Sticker[maxHoldCount];
    }

    void Update()
    {
        //保持中のステッカーのインデックスを変更
        HoldIndex();

        //右クリックでステッカーを貼る、剥がす
        if (interactObjects.Count > 0 && Input.GetMouseButtonDown(1))
        {
            ReceiptPickup();
        }
    }

    private void HoldIndex()
    {
        //数字キーで保持中のステッカーのインデックスを変更
        for (int i = 0; i < maxHoldCount && i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i + 1))
            {
                holdIndex = i;
                Debug.Log("HoldIndex: " + holdIndex);
                break;
            }
        }

        //マウスホイールで保持中のステッカーのインデックスを変更
        float scroll = Input.mouseScrollDelta.y;

        //スクロールの方向に応じてインデックスを変更
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                holdIndex = (holdIndex + 1) % maxHoldCount;
            }
            else if (scroll < 0)
            {
                holdIndex = (holdIndex - 1 + maxHoldCount) % maxHoldCount;
            }

            Debug.Log("HoldIndex: " + holdIndex);
        }
    }

    private void ReceiptPickup()
    {
        GameObject interactObj = GetInteractObject();  //ステッカーを貼り、剥がすオブジェクト
        StickerState target = interactObj.GetComponent<StickerState>();  //インタラクトしているオブジェクトのStickerStateを取得
        if (target == null)
            return;
        //既に貼られているなら剥がして保持
        if (target.CurrentSticker != Sticker.None)
        {
            //選択中のスロットが空なら剥がしたステッカーを保持
            if (holdSticker[holdIndex] == Sticker.None)
            {
                holdSticker[holdIndex] = target.Remove();
            }
            else
            {
                //選択中のスロットが埋まってたなら他の空いているスロットに剥がしたステッカーを保持
                for (int i = 0; i < maxHoldCount; i++)
                {
                    if (holdSticker[i] == Sticker.None)
                    {
                        holdSticker[i] = target.Remove();
                        return;
                    }
                }

                //保持中のステッカーが全て埋まっている場合は選択中のスロットのステッカーを剥がしたステッカーに上書き
                holdSticker[holdIndex] = target.Remove();
            }
        }
        //何も貼られていないなら保持中のステッカーを貼り、保持中のステッカーを空にする
        else if (holdSticker[holdIndex] != Sticker.None)
        {
            target.Apply(holdSticker[holdIndex]);
            holdSticker[holdIndex] = Sticker.None;
        }
        //ステッカーを持っていない
        else
        {
            Debug.Log("ステッカーを持ってないよ");
        }
    }

    //一番カメラの中央にあるオブジェクトを取得
    private GameObject GetInteractObject()
    {
        GameObject targetObject = null;  //一番カメラの中央にあるオブジェクト
        float maxDot = -1.0f;  //中央にどれほど近いか

        //インタラクト候補の中で一番カメラの中央にあるオブジェクトを取得
        foreach (GameObject target in interactObjects)
        {
            //カメラの正面方向とオブジェクトの方向の内積を計算
            Vector3 dir = (target.transform.position - mainCamera.transform.position).normalized;
            //内積を計算
            float dot = Vector3.Dot(mainCamera.transform.forward, dir);

            //比較
            if (dot > maxDot)
            {
                maxDot = dot;
                targetObject = target;
            }
        }

        return targetObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<StickerState>() != null)
        {
            //インタラクト候補入り
            interactObjects.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<StickerState>() != null)
        {
            //インタラクト候補脱落
            interactObjects.Remove(other.gameObject);
        }
    }
}
