using UnityEngine;

/// <summary>
/// 現在選択中の配置オブジェクトを管理
/// </summary>
public class ObjectPaletteManager : MonoBehaviour
{
    // シングルトン
    public static ObjectPaletteManager Instance { get; private set; }

    // 現在選択中のオブジェクト
    public PlaceObjectType CurrentObject { get; private set; }

    private void Awake()
    {
        // シングルトン設定
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 初期選択
        CurrentObject = PlaceObjectType.Chest;
    }

    /// <summary>
    /// 配置するオブジェクトを変更する
    /// </summary>
    public void SelectObject(int type)
    {
        CurrentObject = (PlaceObjectType)type;

        Debug.Log($"選択中 : {CurrentObject}");
    }
}