using UnityEngine;

public class PlaceManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private UndoManager undoManager;
    [SerializeField] private GameObject savePanel;

    private FloorBlock lastPlaceFloor;

    private bool isEditing = false;

    void Update()
    {
        if (savePanel.activeSelf)
            return;

        if (EditModeManager.Instance.CurrentMode != EditMode.Place)
            return;


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


    void Place()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;


        FloorBlock floor = hit.collider.GetComponent<FloorBlock>();

        if (floor == null)
            return;


        // ìØÇ∂èÍèäÇÕñ≥éã
        if (floor == lastPlaceFloor)
            return;


        // ç≈èâÇÃ1âÒÇæÇØï€ë∂
        if (!isEditing)
        {
            undoManager.BeginEdit();
            isEditing = true;
        }


        lastPlaceFloor = floor;


        mapManager.PlaceObject(
            floor.GridPosition,
            ObjectPaletteManager.Instance.CurrentObject);
    }
}