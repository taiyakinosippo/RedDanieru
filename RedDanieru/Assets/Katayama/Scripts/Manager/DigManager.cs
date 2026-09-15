using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DigManager : MonoBehaviour
{
    public Camera mainCamera;
    public MapManager mapManager;

    private WallBlock[] currentWalls = new WallBlock[9];

    private Vector3Int lastDigPosition =
        new Vector3Int(
            int.MinValue,
            int.MinValue,
            int.MinValue
        );

    private Vector3 lastMousePosition;

    [SerializeField]
    private GameObject savePanel;

    [SerializeField]
    private UndoManager undoManager;

    private bool isEditing = false;

    [Header("再編集・読み込みUI")]
    [SerializeField]
    private ReEditUI reEditUI;

    private void Update()
    {
        TestPlayManager testPlayManager =
            FindObjectOfType<TestPlayManager>();

        // テストプレイ中は掘削しない
        if (
            testPlayManager != null &&
            testPlayManager.IsPlaying
        )
        {
            StopDigging();
            return;
        }

        // 編集モードへ戻る途中は掘削しない
        if (
            testPlayManager != null &&
            testPlayManager.IsReturningToEdit
        )
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

        Vector3Int gridPosition;

        if (!GetMouseGridPosition(out gridPosition))
        {
            ClearHighlight();
            return;
        }

        HighlightWalls(gridPosition);

        // マウスを押した瞬間
        if (
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            lastDigPosition = gridPosition;

            lastMousePosition =
                Mouse.current.position.ReadValue();

            Dig(gridPosition);
        }

        // マウスを押している間
        if (
            Mouse.current != null &&
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

                if (gridPosition != lastDigPosition)
                {
                    Dig(gridPosition);
                }
            }
        }

        // マウスを離した瞬間
        if (
            Mouse.current != null &&
            Mouse.current.leftButton.wasReleasedThisFrame
        )
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

    bool GetMouseGridPosition(
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

    void HighlightWalls(
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

    WallBlock GetWallBlock(
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

    void Dig(
        Vector3Int center
    )
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

        if (!isEditing)
        {
            if (undoManager != null)
            {
                undoManager.BeginEdit();
            }

            isEditing = true;
        }

        mapManager.Dig(center);

        lastDigPosition = center;

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