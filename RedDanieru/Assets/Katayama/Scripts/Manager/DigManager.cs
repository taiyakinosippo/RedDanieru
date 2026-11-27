
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DigManager : MonoBehaviour
{
    [Header("メインカメラ")]
    public Camera mainCamera;

    [Header("マップ管理")]
    public MapManager mapManager;

    private WallBlock[] currentWalls = new WallBlock[9];

    private Vector3Int lastDigPosition =
        new Vector3Int(
            int.MinValue,
            int.MinValue,
            int.MinValue
        );

    // ドラッグ中の前回のマス
    private Vector3Int lastDragGridPosition;

    // ドラッグ開始済みか
    private bool hasLastDragGridPosition = false;

    [Header("保存パネル")]
    [SerializeField]
    private GameObject savePanel;

    [Header("Undo")]
    [SerializeField]
    private UndoManager undoManager;

    [Header("再編集・読み込みUI")]
    [SerializeField]
    private ReEditUI reEditUI;

    [Header("配置一覧UI")]
    [SerializeField]
    private PlacementCategoryUI placementCategoryUI;

    private bool isEditing = false;

    //==================================================
    // Update
    //==================================================

    private void Update()
    {
        // テストプレイ中は掘削しない
        if (IsTestPlayScene())
        {
            StopDigging();
            return;
        }

        // 読み込み一覧を開いている間は掘削しない
        if (
            reEditUI != null &&
            reEditUI.IsSelectingDungeon
        )
        {
            StopDigging();
            return;
        }

        // UIの上では掘削しない
        if (
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject()
        )
        {
            ClearHighlight();
            return;
        }

        // 保存パネル表示中は掘削しない
        if (
            savePanel != null &&
            savePanel.activeSelf
        )
        {
            ClearHighlight();
            return;
        }

        // 掘削モード以外では掘削しない
        if (
            EditModeManager.Instance == null ||
            EditModeManager.Instance.CurrentMode != EditMode.Dig
        )
        {
            ClearHighlight();
            return;
        }

        // マウス位置からマスを取得
        Vector3Int gridPosition;

        if (!GetMouseGridPosition(out gridPosition))
        {
            ClearHighlight();
            return;
        }

        HighlightWalls(gridPosition);

        if (Mouse.current == null)
        {
            return;
        }

        // マウスを押した瞬間
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // ドラッグ開始位置を記録
            lastDragGridPosition = gridPosition;
            hasLastDragGridPosition = true;

            // クリックした場所を掘る
            Dig(gridPosition);
        }

        // マウスを押している間
        if (Mouse.current.leftButton.isPressed)
        {
            if (!hasLastDragGridPosition)
            {
                lastDragGridPosition = gridPosition;
                hasLastDragGridPosition = true;
            }

            // 前回のマスから現在のマスまで掘る
            if (gridPosition != lastDragGridPosition)
            {
                DigLine(
                    lastDragGridPosition,
                    gridPosition
                );

                // 現在のマスを次回の開始位置にする
                lastDragGridPosition = gridPosition;
            }
        }

        // マウスを離した瞬間
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            hasLastDragGridPosition = false;
            EndEdit();
        }
    }

    //==================================================
    // テストプレイシーン判定
    //==================================================

    private bool IsTestPlayScene()
    {
        string sceneName =
            UnityEngine.SceneManagement
                .SceneManager.GetActiveScene().name;

        return
            sceneName == "TestPlayScene" ||
            sceneName == "Testplay";
    }

    //==================================================
    // 編集終了
    //==================================================

    private void EndEdit()
    {
        if (isEditing)
        {
            // Undo編集を終了
            if (undoManager != null)
            {
                undoManager.EndEdit();
            }

            // NavMeshを再構築
            if (mapManager != null)
            {
                mapManager.RebuildNavigation();
            }

            isEditing = false;
        }

        lastDigPosition =
            new Vector3Int(
                int.MinValue,
                int.MinValue,
                int.MinValue
            );

        hasLastDragGridPosition = false;

        ClearHighlight();
    }

    //==================================================
    // 掘削停止
    //==================================================

    private void StopDigging()
    {
        ClearHighlight();

        if (isEditing)
        {
            if (undoManager != null)
            {
                undoManager.EndEdit();
            }

            // NavMeshを再構築
            if (mapManager != null)
            {
                mapManager.RebuildNavigation();
            }

            isEditing = false;
        }

        lastDigPosition =
            new Vector3Int(
                int.MinValue,
                int.MinValue,
                int.MinValue
            );

        hasLastDragGridPosition = false;
    }

    //==================================================
    // マウス位置からマス取得
    //==================================================

    private bool GetMouseGridPosition(
        out Vector3Int gridPosition
    )
    {
        gridPosition = new Vector3Int();

        if (mainCamera == null)
            return false;

        if (mapManager == null)
            return false;

        if (Mouse.current == null)
            return false;

        Ray ray =
            mainCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        Plane mapPlane =
            new Plane(
                Vector3.up,
                Vector3.zero
            );

        float distance;

        if (!mapPlane.Raycast(ray, out distance))
        {
            return false;
        }

        Vector3 worldPosition = ray.GetPoint(distance);

        gridPosition =
            new Vector3Int(
                Mathf.RoundToInt(worldPosition.x),
                0,
                Mathf.RoundToInt(worldPosition.z)
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
    // 壁のハイライト
    //==================================================

    private void HighlightWalls(Vector3Int center)
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

                WallBlock wall = GetWallBlock(pos);

                if (wall != null)
                {
                    currentWalls[index] = wall;
                    wall.Select();
                }

                index++;
            }
        }
    }

    //==================================================
    // 壁取得
    //==================================================

    private WallBlock GetWallBlock(Vector3Int pos)
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
            Physics.RaycastAll(ray, 2000f);

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
    // マスとマスの間を連続して掘削
    //==================================================

    private void DigLine(
        Vector3Int start,
        Vector3Int end
    )
    {
        int x = start.x;
        int z = start.z;

        int dx = Mathf.Abs(end.x - start.x);
        int dz = Mathf.Abs(end.z - start.z);

        int sx = start.x < end.x ? 1 : -1;
        int sz = start.z < end.z ? 1 : -1;

        int error = dx - dz;

        while (true)
        {
            // 通過したマスを掘る
            Dig(
                new Vector3Int(
                    x,
                    start.y,
                    z
                )
            );

            if (x == end.x && z == end.z)
            {
                break;
            }

            int error2 = error * 2;

            if (error2 > -dz)
            {
                error -= dz;
                x += sx;
            }

            if (error2 < dx)
            {
                error += dx;
                z += sz;
            }
        }
    }

    //==================================================
    // 掘削
    //==================================================

    private void Dig(Vector3Int center)
    {
        if (mapManager == null)
            return;

        if (
            center.x < 0 ||
            center.x >= mapManager.width ||
            center.z < 0 ||
            center.z >= mapManager.depth
        )
        {
            return;
        }

        // 前回掘ったマスと同じなら掘らない
        if (center == lastDigPosition)
        {
            return;
        }

        // 3×3の中に壁があるか確認
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
            {
                break;
            }
        }

        // 掘削対象の壁がなければ何もしない
        if (!canDig)
        {
            return;
        }

        // Undo開始
        if (!isEditing)
        {
            if (undoManager != null)
            {
                undoManager.BeginEdit();
            }

            isEditing = true;
        }

        //==============================================
        // マップを掘る
        //==============================================

        mapManager.Dig(center);

        //==============================================
        // 掘削対象があった場合、配置一覧を閉じる
        //==============================================

        if (placementCategoryUI != null)
        {
            placementCategoryUI.CloseAllPanelsForDig();
        }

        // 最後に掘った場所を記録
        lastDigPosition = center;

        // ハイライト解除
        ClearHighlight();
    }

    //==================================================
    // ハイライト解除
    //==================================================

    private void ClearHighlight()
    {
        for (int i = 0; i < currentWalls.Length; i++)
        {
            if (currentWalls[i] != null)
            {
                currentWalls[i].Deselect();
                currentWalls[i] = null;
            }
        }
    }
}
