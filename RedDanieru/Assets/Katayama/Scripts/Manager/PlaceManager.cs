using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlaceManager : MonoBehaviour
{
    [Header("カメラ")]
    [SerializeField] private Camera mainCamera;

    [Header("マップ")]
    [SerializeField] private MapManager mapManager;

    [Header("Undo")]
    [SerializeField] private UndoManager undoManager;

    [Header("保存パネル")]
    [SerializeField] private GameObject savePanel;

    [Header("再編集・読み込みUI")]
    [SerializeField] private ReEditUI reEditUI;

    private FloorBlock currentFloor;
    private FloorBlock lastPlaceFloor;

    private bool isEditing = false;

    private void Update()
    {
        if (mainCamera == null)
            return;

        if (mapManager == null)
            return;

        if (Mouse.current == null)
            return;

        // 読み込み一覧を開いている間は配置しない
        if (reEditUI != null &&
            reEditUI.IsSelectingDungeon)
        {
            StopPlacing();
            return;
        }

        // セーブ画面を開いている場合は配置しない
        if (savePanel != null &&
            savePanel.activeSelf)
        {
            StopPlacing();
            return;
        }

        // EditModeManagerが存在しない場合
        if (EditModeManager.Instance == null)
        {
            StopPlacing();
            return;
        }

        // 配置モード以外では処理しない
        if (EditModeManager.Instance.CurrentMode != EditMode.Place)
        {
            StopPlacing();
            return;
        }

        // UIの上では配置しない
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            StopPlacing();
            return;
        }

        HighlightFloor();

        // マウスを押した瞬間
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Place();
        }

        // マウスを押している間
        if (Mouse.current.leftButton.isPressed)
        {
            Place();
        }

        // マウスを離した瞬間
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndEdit();
        }
    }

    /// <summary>
    /// マウスカーソル下の床を選択
    /// </summary>
    private void HighlightFloor()
    {
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            FloorBlock floor =
                hit.collider.GetComponent<FloorBlock>();

            if (floor != currentFloor)
            {
                if (currentFloor != null)
                {
                    currentFloor.Deselect();
                }

                currentFloor = floor;

                if (currentFloor != null)
                {
                    currentFloor.Select();
                }
            }
        }
        else
        {
            ClearSelection();
        }
    }

    /// <summary>
    /// オブジェクトを配置
    /// </summary>
    private void Place()
    {
        if (currentFloor == null)
            return;

        if (ObjectPaletteManager.Instance == null)
            return;

        // 同じ床には連続配置しない
        if (currentFloor == lastPlaceFloor)
            return;

        // 最初の配置時だけUndo編集開始
        if (!isEditing)
        {
            if (undoManager != null)
            {
                undoManager.BeginEdit();
            }

            isEditing = true;
        }

        lastPlaceFloor = currentFloor;

        mapManager.PlaceObject(
            currentFloor.GridPosition,
            ObjectPaletteManager.Instance.CurrentObject
        );
    }

    /// <summary>
    /// 配置終了
    /// </summary>
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

        lastPlaceFloor = null;
    }

    /// <summary>
    /// 配置処理を停止
    /// </summary>
    private void StopPlacing()
    {
        ClearSelection();

        if (isEditing)
        {
            if (undoManager != null)
            {
                undoManager.EndEdit();
            }

            isEditing = false;
        }

        lastPlaceFloor = null;
    }

    /// <summary>
    /// 床の選択を解除
    /// </summary>
    private void ClearSelection()
    {
        if (currentFloor != null)
        {
            currentFloor.Deselect();
            currentFloor = null;
        }
    }
}