using UnityEngine;

public class ObjectDeleteManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private UndoManager undoManager;

    // 現在選択中のオブジェクト
    private PlaceObject currentObject;

    // 最後に削除した座標
    private Vector3Int lastDeletePos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    // 編集中か
    private bool isEditing = false;

    private void Update()
    {
        if (EditModeManager.Instance.CurrentMode != EditMode.Delete)
        {
            ClearSelection();
            return;
        }

        HighlightObject();

        if (Input.GetMouseButton(0))
        {
            Delete();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isEditing)
            {
                undoManager.EndEdit();
                isEditing = false;
            }

            lastDeletePos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        }
    }

    /// <summary>
    /// マウス下のオブジェクトを選択
    /// （床を指していても、そのマスのオブジェクトを取得）
    /// </summary>
    private void HighlightObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            ClearSelection();
            return;
        }

        PlaceObject target = null;

        // オブジェクトを直接指している
        PlaceObject placeObject = hit.collider.GetComponent<PlaceObject>();

        if (placeObject != null)
        {
            target = placeObject;
        }
        else
        {
            // 床を指している
            FloorBlock floor = hit.collider.GetComponent<FloorBlock>();

            if (floor != null)
            {
                GameObject obj = mapManager.GetPlacedObject(floor.GridPosition);

                if (obj != null)
                {
                    target = obj.GetComponent<PlaceObject>();
                }
            }
        }

        if (target != currentObject)
        {
            if (currentObject != null)
            {
                currentObject.Deselect();
            }

            currentObject = target;

            if (currentObject != null)
            {
                currentObject.Select();
            }
        }
    }

    /// <summary>
    /// オブジェクト削除
    /// </summary>
    private void Delete()
    {
        if (currentObject == null)
            return;

        Vector3Int pos = currentObject.GridPosition;

        // 同じ場所は連続削除しない
        if (pos == lastDeletePos)
            return;

        if (!isEditing)
        {
            undoManager.BeginEdit();
            isEditing = true;
        }

        lastDeletePos = pos;

        mapManager.DeleteObject(pos);

        ClearSelection();
    }

    /// <summary>
    /// 選択解除
    /// </summary>
    private void ClearSelection()
    {
        if (currentObject != null)
        {
            currentObject.Deselect();
            currentObject = null;
        }
    }
}