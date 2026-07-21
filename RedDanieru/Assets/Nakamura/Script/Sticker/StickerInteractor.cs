using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class StickerInteractor : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private List<GameObject> interactObjects = new List<GameObject>();  //インタラクトトリガー内にあるStickerState持ちのオブジェクトのリスト
    private Sticker holdSticker;  //保持してるステッカーのタイプ

    void Start()
    {

    }

    void Update()
    {
        if (interactObjects.Count <= 0 || !Input.GetMouseButtonDown(1))
            return;

        GameObject interactObj = GetInteractObject();  //ステッカーを貼り、剥がすオブジェクト

        StickerState target = interactObj.GetComponent<StickerState>();
        if (target == null)
            return;

        //既に貼られているなら剥がして保持
        if (target.CurrentSticker != Sticker.None)
        {
            holdSticker = target.Remove();
        }
        //何も貼られていないなら保持中のステッカーを貼る
        else if (holdSticker != Sticker.None)
        {
            target.Apply(holdSticker);
            holdSticker = Sticker.None;
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
        GameObject targetObject = null;
        float maxDot = -1.0f;  //中央にどれほど近いか

        foreach (GameObject target in interactObjects)
        {
            Vector3 dir = (target.transform.position - mainCamera.transform.position).normalized;

            float dot = Vector3.Dot(mainCamera.transform.forward, dir);  //

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
