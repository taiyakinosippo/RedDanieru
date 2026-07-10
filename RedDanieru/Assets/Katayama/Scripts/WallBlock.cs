using UnityEngine;

public class WallBlock : MonoBehaviour
{
    // マップ上のグリッド座標
    public Vector3Int GridPosition;

    // レンダラー
    private Renderer rend;

    // この壁専用のマテリアル
    private Material materialInstance;

    // 元の色
    private Color defaultColor;

    void Awake()
    {
        // Rendererを取得
        rend = GetComponent<Renderer>();

        // マテリアルを複製して他の壁へ影響しないようにする
        materialInstance = rend.material;

        // 元の色を保存
        defaultColor = materialInstance.color;
    }

    /// 壁を選択状態にする
    public void Select()
    {
        // 黄色に変更
        materialInstance.color = Color.yellow;
    }

    /// 壁の選択状態を解除する
    public void Deselect()
    {
        // 元の色に戻す
        materialInstance.color = defaultColor;
    }
}