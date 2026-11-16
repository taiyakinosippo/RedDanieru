
using UnityEngine;

public class ObjectDeleteManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private UndoManager undoManager;

    // 現在選択中のオブジェクト
    private PlaceObject currentObject;

    // 最後に削除した座標
    private Vector3Int lastDeletePos =
        new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

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

            lastDeletePos = new Vector3Int(
                int.MinValue,
                int.MinValue,
                int.MinValue
            );
        }
    }

    /// <summary>
    /// マウス下のオブジェクトを選択
    /// </summary>
    private void HighlightObject()
    {
        if (mainCamera == null || mapManager == null)
        {
            ClearSelection();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            ClearSelection();
            return;
        }

        PlaceObject target = null;

        // オブジェクト本体または子Colliderから取得
        PlaceObject placeObject =
            hit.collider.GetComponentInParent<PlaceObject>();

        if (placeObject != null)
        {
            target = placeObject;
        }
        else
        {
            // 床本体または子Colliderから取得
            FloorBlock floor =
                hit.collider.GetComponentInParent<FloorBlock>();

            if (floor != null)
            {
                GameObject obj =
                    mapManager.GetPlacedObject(floor.GridPosition);

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

        // 同じオブジェクトの連続削除を防止
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

    /// <summary>
    /// 編集画面復帰時の初期化
    /// </summary>
    public void ResetDeleteState()
    {
        ClearSelection();

        lastDeletePos = new Vector3Int(
            int.MinValue,
            int.MinValue,
            int.MinValue
        );

        isEditing = false;

        Debug.Log("削除状態を初期化しました");
    }
}
