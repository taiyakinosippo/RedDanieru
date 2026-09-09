using UnityEngine;

public class DigManager : MonoBehaviour
{
    //==================================================
    // カメラ
    //==================================================

    public Camera mainCamera;

    //==================================================
    // マップ管理
    //==================================================

    public MapManager mapManager;

    //==================================================
    // 現在の3×3選択範囲
    //==================================================

    private WallBlock[] currentWalls =
        new WallBlock[9];

    //==================================================
    // 最後に掘った位置
    //==================================================

    private Vector3Int lastDigPosition =
        new Vector3Int(
            int.MinValue,
            int.MinValue,
            int.MinValue
        );

    //==================================================
    // 前フレームのマウス座標
    //==================================================

    private Vector3 lastMousePosition;

    //==================================================
    // 保存パネル
    //==================================================

    [SerializeField]
    private GameObject savePanel;

    //==================================================
    // Undo管理
    //==================================================

    [SerializeField]
    private UndoManager undoManager;

    //==================================================
    // 編集中か
    //==================================================

    private bool isEditing = false;


    //==================================================
    // Update
    //==================================================

    void Update()
    {
        //==================================================
        // 保存パネルが開いている間は何もしない
        //==================================================

        if (
            savePanel != null &&
            savePanel.activeSelf
        )
        {
            ClearHighlight();
            return;
        }

        //==================================================
        // 掘削モード以外
        //==================================================

        if (
            EditModeManager.Instance == null ||
            EditModeManager.Instance.CurrentMode
            != EditMode.Dig
        )
        {
            ClearHighlight();
            return;
        }

        //==================================================
        // マウス位置からグリッド位置を取得
        //==================================================

        Vector3Int mouseGridPosition;

        bool hasGridPosition =
            GetMouseGridPosition(
                out mouseGridPosition
            );

        //==================================================
        // 3×3ハイライト
        //==================================================

        if (hasGridPosition)
        {
            HighlightWalls(
                mouseGridPosition
            );
        }
        else
        {
            ClearHighlight();
        }

        //==================================================
        // クリックした瞬間
        //==================================================

        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition =
                Input.mousePosition;

            // 新しい操作なのでリセット
            lastDigPosition =
                new Vector3Int(
                    int.MinValue,
                    int.MinValue,
                    int.MinValue
                );

            if (hasGridPosition)
            {
                Dig(
                    mouseGridPosition
                );
            }
        }

        //==================================================
        // ドラッグ中
        //==================================================

        if (Input.GetMouseButton(0))
        {
            if (
                Input.mousePosition
                != lastMousePosition
            )
            {
                if (hasGridPosition)
                {
                    Dig(
                        mouseGridPosition
                    );
                }
            }
        }

        //==================================================
        // マウス座標保存
        //==================================================

        lastMousePosition =
            Input.mousePosition;

        //==================================================
        // マウスを離した
        //==================================================

        if (Input.GetMouseButtonUp(0))
        {
            if (isEditing)
            {
                undoManager.EndEdit();
                isEditing = false;
            }

            lastDigPosition =
                new Vector3Int(
                    int.MinValue,
                    int.MinValue,
                    int.MinValue
                );
        }
    }


    //==================================================
    // マウス位置からグリッド座標を取得
    //==================================================

    bool GetMouseGridPosition(
        out Vector3Int gridPosition)
    {
        gridPosition =
            new Vector3Int();

        if (mainCamera == null)
            return false;

        if (mapManager == null)
            return false;

        //==================================================
        // マウスからRayを作成
        //==================================================

        Ray ray =
            mainCamera.ScreenPointToRay(
                Input.mousePosition
            );

        //==================================================
        // マップの水平面とRayを交差させる
        //==================================================

        Plane mapPlane =
            new Plane(
                Vector3.up,
                Vector3.zero
            );

        float distance;

        if (!mapPlane.Raycast(
                ray,
                out distance))
        {
            return false;
        }

        //==================================================
        // ワールド座標を取得
        //==================================================

        Vector3 worldPosition =
            ray.GetPoint(distance);

        //==================================================
        // ワールド座標 → グリッド座標
        //==================================================

        gridPosition =
            new Vector3Int(
                Mathf.RoundToInt(
                    worldPosition.x
                ),
                0,
                Mathf.RoundToInt(
                    worldPosition.z
                )
            );

        //==================================================
        // マップ範囲確認
        //==================================================

        if (
            gridPosition.x < 0 ||
            gridPosition.x >= mapManager.width ||
            gridPosition.z < 0 ||
            gridPosition.z >= mapManager.depth
        )
        {
            return false;
        }

        return true;
    }


    //==================================================
    // 3×3の壁をハイライト
    //==================================================

    void HighlightWalls(
        Vector3Int center)
    {
        //==================================================
        // 現在の3×3と同じ場所なら何もしない
        //==================================================

        if (
            currentWalls[4] != null &&
            currentWalls[4].GridPosition == center
        )
        {
            return;
        }

        //==================================================
        // 前のハイライトを解除
        //==================================================

        ClearHighlight();

        //==================================================
        // 3×3を取得
        //==================================================

        int index = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3Int pos =
                    new Vector3Int(
                        center.x + x,
                        center.y,
                        center.z + z
                    );

                WallBlock wall =
                    GetWallBlock(pos);

                if (wall != null)
                {
                    currentWalls[index] =
                        wall;

                    wall.Select();
                }

                index++;
            }
        }
    }


    //==================================================
    // 指定位置のWallBlockを取得
    //==================================================

    WallBlock GetWallBlock(
        Vector3Int pos)
    {
        if (
            pos.x < 0 ||
            pos.x >= mapManager.width ||
            pos.y < 0 ||
            pos.y >= mapManager.height ||
            pos.z < 0 ||
            pos.z >= mapManager.depth
        )
        {
            return null;
        }

        Ray ray =
            new Ray(
                new Vector3(
                    pos.x,
                    1000f,
                    pos.z
                ),
                Vector3.down
            );

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                2000f
            );

        foreach (RaycastHit hit in hits)
        {
            WallBlock wall =
                hit.collider.GetComponent<WallBlock>();

            if (
                wall != null &&
                wall.GridPosition == pos
            )
            {
                return wall;
            }
        }

        return null;
    }


    //==================================================
    // 3×3を掘る
    //==================================================

    void Dig(
        Vector3Int center)
    {
        //==================================================
        // マップ範囲確認
        //==================================================

        if (
            center.x < 0 ||
            center.x >= mapManager.width ||
            center.z < 0 ||
            center.z >= mapManager.depth
        )
        {
            return;
        }

        //==================================================
        // 同じ場所を連続して掘らない
        //==================================================

        if (center == lastDigPosition)
        {
            return;
        }

        //==================================================
        // 最初の掘削だけUndo開始
        //==================================================

        if (!isEditing)
        {
            undoManager.BeginEdit();
            isEditing = true;
        }

        //==================================================
        // 3×3掘削
        //==================================================

        mapManager.Dig(center);

        //==================================================
        // 最後に掘った位置を保存
        //==================================================

        lastDigPosition = center;

        //==================================================
        // ハイライト解除
        //==================================================

        ClearHighlight();
    }


    //==================================================
    // ハイライト解除
    //==================================================

    void ClearHighlight()
    {
        for (
            int i = 0;
            i < currentWalls.Length;
            i++
        )
        {
            if (currentWalls[i] != null)
            {
                currentWalls[i].Deselect();
                currentWalls[i] = null;
            }
        }
    }
}
