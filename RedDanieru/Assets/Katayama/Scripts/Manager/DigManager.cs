using UnityEngine;

public class DigManager : MonoBehaviour
{
    // メインカメラ
    public Camera mainCamera;

    // マップ管理
    public MapManager mapManager;

    // 現在選択中の壁
    private WallBlock currentWall;

    // 最後に掘った壁
    private WallBlock lastDigWall;

    // 前フレームのマウス座標
    private Vector3 lastMousePosition;

    // 保存パネル
    [SerializeField] private GameObject savePanel;

    // Undo管理
    [SerializeField] private UndoManager undoManager;

    // 編集中か
    private bool isEditing = false;


    void Update()
    {
        // 保存パネルが開いている間は掘削しない
        if (savePanel.activeSelf)
            return;

        if (EditModeManager.Instance.CurrentMode != EditMode.Dig)
            return;

        // マウスカーソル下の壁を選択
        HighlightWall();


        // クリックした瞬間
        if (Input.GetMouseButtonDown(0))
        {
            Dig();
            lastDigWall = currentWall;
        }


        // ドラッグ中
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition != lastMousePosition)
            {
                if (currentWall != null && currentWall != lastDigWall)
                {
                    Dig();
                    lastDigWall = currentWall;
                }
            }
        }


        // マウス座標を保存
        lastMousePosition = Input.mousePosition;


        // 離したらリセット
        if (Input.GetMouseButtonUp(0))
        {
            if (isEditing)
            {
                undoManager.EndEdit();
                isEditing = false;
            }

            lastDigWall = null;
        }
    }


    /// <summary>
    /// マウスカーソルが乗っている壁を選択状態にする
    /// </summary>
    void HighlightWall()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            WallBlock wall = hit.collider.GetComponent<WallBlock>();

            if (wall != currentWall)
            {
                if (currentWall != null)
                {
                    currentWall.Deselect();
                }

                currentWall = wall;

                if (currentWall != null)
                {
                    currentWall.Select();
                }
            }
        }
        else
        {
            if (currentWall != null)
            {
                currentWall.Deselect();
                currentWall = null;
            }
        }
    }


    /// <summary>
    /// 選択中の壁を掘る
    /// </summary>
    void Dig()
    {
        if (currentWall == null)
            return;


        // 最初に掘る瞬間だけ保存
        if (!isEditing)
        {
            undoManager.BeginEdit();
            isEditing = true;
        }


        mapManager.Dig(currentWall.GridPosition);

        currentWall = null;
    }
}