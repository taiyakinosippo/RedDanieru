using UnityEngine;

public class DigManager : MonoBehaviour
{
    // メインカメラ
    public Camera mainCamera;

    // マップ管理
    public MapManager mapManager;

    // 現在選択中の壁
    private WallBlock currentWall;

    void Update()
    {
        // マウスカーソル下の壁を選択
        HighlightWall();

        // 左クリックで壁を掘る
        if (Input.GetMouseButtonDown(0))
        {
            Dig();
        }
    }

    /// マウスカーソルが乗っている壁を選択状態にする
    void HighlightWall()
    {
        // マウス位置からレイを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // レイがオブジェクトに当たったか判定
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 当たったオブジェクトからWallBlockを取得
            WallBlock wall = hit.collider.GetComponent<WallBlock>();

            // 前回選択していた壁と違う場合
            if (wall != currentWall)
            {
                // 前回の壁の選択状態を解除
                if (currentWall != null)
                {
                    currentWall.Deselect();
                }

                // 新しい壁を選択
                currentWall = wall;

                // 壁が存在する場合は選択状態にする
                if (currentWall != null)
                {
                    currentWall.Select();
                }
            }
        }
        else
        {
            // 壁以外を見ている場合は選択解除
            if (currentWall != null)
            {
                currentWall.Deselect();
                currentWall = null;
            }
        }
    }

    /// 選択中の壁を掘る
    void Dig()
    {
        Debug.Log("Dig開始");

        if (currentWall == null)
        {
            Debug.Log("壁未選択");
            return;
        }

        Debug.Log("掘る壁 : " + currentWall.GridPosition);

        mapManager.Dig(currentWall.GridPosition);

        currentWall = null;
    }
}