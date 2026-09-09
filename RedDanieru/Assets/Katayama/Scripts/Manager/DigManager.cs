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
        if (
            savePanel != null &&
            savePanel.activeSelf
        )
        {
            ClearHighlight();
            return;
        }

        if (
            EditModeManager.Instance == null ||
            EditModeManager.Instance.CurrentMode != EditMode.Dig
        )
        {
            ClearHighlight();
            return;
        }

        Vector3Int mouseGridPosition;

        bool hasGridPosition =
            GetMouseGridPosition(
                out mouseGridPosition
            );

        // 3×3ハイライト
        if (hasGridPosition)
        {
            HighlightWalls(mouseGridPosition);
        }
        else
        {
            ClearHighlight();
        }

        //==================================================
        // クリック開始
        //==================================================

        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition =
                Input.mousePosition;

            lastDigPosition =
                new Vector3Int(
                    int.MinValue,
                    int.MinValue,
                    int.MinValue
                );

            if (hasGridPosition)
            {
                Dig(mouseGridPosition);
            }
        }

        //==================================================
        // ドラッグ中
        //==================================================

        if (Input.GetMouseButton(0))
        {
            if (
                Input.mousePosition !=
                lastMousePosition
            )
            {
                if (hasGridPosition)
                {
                    Dig(mouseGridPosition);
                }
            }
        }

        lastMousePosition =
            Input.mousePosition;

        //==================================================
        // クリック終了
        //==================================================

        if (Input.GetMouseButtonUp(0))
        {
            if (isEditing)
            {
                if (undoManager != null)
                {
                    undoManager.EndEdit();
                }

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

        Ray ray =
            mainCamera.ScreenPointToRay(
                Input.mousePosition
            );

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

        Vector3 worldPosition =
            ray.GetPoint(distance);

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
    // 3×3の掘削対象をハイライト
    //==================================================

    void HighlightWalls(
        Vector3Int center)
    {
        if (
            currentWalls[4] != null &&
            currentWalls[4].GridPosition == center
        )
        {
            return;
        }

        ClearHighlight();

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
    // 指定位置の掘削可能なWallBlockを取得
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

        //==================================================
        // まず通常の壁・配置WallをRaycastで探す
        //==================================================

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

            if (wall != null &&
                wall.GridPosition == pos)
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
        if (
            center.x < 0 ||
            center.x >= mapManager.width ||
            center.z < 0 ||
            center.z >= mapManager.depth
        )
        {
            return;
        }

        if (center == lastDigPosition)
        {
            return;
        }

        //==================================================
        // この場所に掘削対象があるか確認
        //==================================================

        bool canDig = false;

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

                if (GetWallBlock(pos) != null)
                {
                    canDig = true;
                    break;
                }
            }

            if (canDig)
                break;
        }

        if (!canDig)
        {
            return;
        }

        //==================================================
        // 最初の掘削だけUndo開始
        //==================================================

        if (!isEditing)
        {
            if (undoManager != null)
            {
                undoManager.BeginEdit();
            }

            isEditing = true;
        }

        //==================================================
        // 3×3掘削
        //==================================================

        mapManager.Dig(center);

        lastDigPosition =
            center;

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