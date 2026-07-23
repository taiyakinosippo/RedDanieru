using UnityEngine;

public class PlaceManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private UndoManager undoManager;
    [SerializeField] private GameObject savePanel;

    private FloorBlock currentFloor;
    private FloorBlock lastPlaceFloor;

    private bool isEditing = false;

    void Update()
    {
        if (savePanel.activeSelf)
            return;

        if (EditModeManager.Instance.CurrentMode != EditMode.Place)
        {
            ClearSelection();
            return;
        }

        HighlightFloor();

        if (Input.GetMouseButton(0))
        {
            Place();
        }

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
                    currentFloor.Deselect();

                currentFloor = floor;

                if (currentFloor != null)
                    currentFloor.Select();
            }
        }
        else
        {
            ClearSelection();
        }
    }

    /// <summary>
    /// オブジェクト配置
    /// </summary>
    private void Place()
    {
        if (currentFloor == null)
            return;

        // 同じ場所は無視
        if (currentFloor == lastPlaceFloor)
            return;

        // 最初の1回だけ保存
        if (!isEditing)
        {
            undoManager.BeginEdit();
            isEditing = true;
        }

        lastPlaceFloor = currentFloor;

        mapManager.PlaceObject(
            currentFloor.GridPosition,
            ObjectPaletteManager.Instance.CurrentObject);
    }

    /// <summary>
    /// 選択解除
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