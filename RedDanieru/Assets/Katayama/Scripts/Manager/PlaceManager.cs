using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private UndoManager undoManager;
    [SerializeField] private GameObject savePanel;

    private FloorBlock currentFloor;
    private FloorBlock lastPlaceFloor;

    private bool isEditing = false;

    private void Update()
    {
        // セーブ画面を開いている場合
        if (savePanel != null && savePanel.activeSelf)
        {
            ClearSelection();
            return;
        }

        // EditModeManagerが存在しない場合
        if (EditModeManager.Instance == null)
            return;

        // 配置モード以外では処理しない
        if (EditModeManager.Instance.CurrentMode != EditMode.Place)
        {
            ClearSelection();
            lastPlaceFloor = null;
            return;
        }

        // UIボタンの上にマウスがある場合は配置しない
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            ClearSelection();
            return;
        }

        HighlightFloor();

        // マウスを押している間は配置
        if (Input.GetMouseButton(0))
        {
            Place();
        }

        // マウスを離したら編集終了
        if (Input.GetMouseButtonUp(0))
        {
            if (isEditing)
            {
                undoManager.EndEdit();
                isEditing = false;
            }

            lastPlaceFloor = null;
        }
    }

    /// <summary>
    /// マウスカーソル下の床を選択
    /// </summary>
    private void HighlightFloor()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            FloorBlock floor = hit.collider.GetComponent<FloorBlock>();

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

        // 同じ場所には連続配置しない
        if (currentFloor == lastPlaceFloor)
            return;

        // 最初の配置時だけUndo用の編集開始
        if (!isEditing)
        {
            undoManager.BeginEdit();
            isEditing = true;
        }

        lastPlaceFloor = currentFloor;

        mapManager.PlaceObject(
            currentFloor.GridPosition,
            ObjectPaletteManager.Instance.CurrentObject
        );
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