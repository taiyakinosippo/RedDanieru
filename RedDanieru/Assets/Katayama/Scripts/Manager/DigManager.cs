using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DigManager : MonoBehaviour
{
    [Header("メインカメラ")]
    public Camera mainCamera;

    [Header("マップ管理")]
    public MapManager mapManager;

    private WallBlock[] currentWalls =
        new WallBlock[9];

    private Vector3Int lastDigPosition =
        new Vector3Int(
            int.MinValue,
            int.MinValue,
            int.MinValue
        );

    private Vector3 lastMousePosition;

    [Header("保存パネル")]
    [SerializeField]
    private GameObject savePanel;

    [Header("Undo")]
    [SerializeField]
    private UndoManager undoManager;

    private bool isEditing = false;

    [Header("再編集・読み込みUI")]
    [SerializeField]
    private ReEditUI reEditUI;

    //==================================================
    // Update
    //==================================================

    private void Update()
    {
        //==================================================
        // テストプレイ中は掘削しない
        //==================================================

        if (IsTestPlayScene())
        {
            StopDigging();
            return;
        }

        //==================================================
        // 読み込み一覧を開いている間は掘削しない
        //==================================================

        if (
            reEditUI != null &&
            reEditUI.IsSelectingDungeon
        )
        {
            StopDigging();
            return;
        }

        //==================================================
        // UIの上では掘削しない
        //==================================================

        if (
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject()
        )
        {
            ClearHighlight();
            return;
        }

        //==================================================
        // 保存パネル表示中は掘削しない
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
        // 掘削モード以外では掘削しない
        //==================================================

        if (
            EditModeManager.Instance == null ||
            EditModeManager.Instance.CurrentMode !=
            EditMode.Dig
        )
        {
            ClearHighlight();
            return;
        }

        //==================================================
        // マウス位置からマスを取得
        //==================================================

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

        //==================================================
        // マウスを押した瞬間
        //==================================================

        if (
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            lastMousePosition =
                Mouse.current.position.ReadValue();

            // クリックした場所を掘る
            Dig(gridPosition);
        }

        //==================================================
        // マウスを押している間
        //==================================================

        if (
            Mouse.current.leftButton.isPressed
        )
        {
            Vector2 currentMousePosition =
                Mouse.current.position.ReadValue();

            if (
                Vector2.Distance(
                    currentMousePosition,
                    lastMousePosition
                ) > 5f
            )
            {
                lastMousePosition =
                    currentMousePosition;

                if (
                    gridPosition !=
                    lastDigPosition
                )
                {
                    Dig(gridPosition);
                }
            }
        }

        //==================================================
        // マウスを離した瞬間
        //==================================================

        if (
            Mouse.current.leftButton.wasReleasedThisFrame
        )
        {
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

            isEditing = false;
        }

        lastDigPosition =
            new Vector3Int(
                int.MinValue,
                int.MinValue,
                int.MinValue
            );

        if (Mouse.current != null)
        {
            lastMousePosition =
                Mouse.current.position.ReadValue();
        }
    }

    //==================================================
    // マウス位置からマス取得
    //==================================================

    private bool GetMouseGridPosition(
        out Vector3Int gridPosition
    )
    {
        gridPosition =
            new Vector3Int();

        if (mainCamera == null)
        {
            return false;
        }

        if (mapManager == null)
        {
            return false;
        }

        if (Mouse.current == null)
        {
            return false;
        }

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

        if (
            !mapPlane.Raycast(
                ray,
                out distance
            )
        )
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
    // 壁のハイライト
    //==================================================

    private void HighlightWalls(
        Vector3Int center
    )
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

    private WallBlock GetWallBlock(
        Vector3Int pos
    )
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
    // 掘削
    //==================================================

    private void Dig(
        Vector3Int center
    )
    {
        if (mapManager == null)
        {
            return;
        }

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
        // クリック直後ではまだlastDigPositionを更新していないので
        // クリック時の1回目の掘削は正常に実行される
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

        // マップを掘る
        mapManager.Dig(center);

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