using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private UndoManager undoManager;

    void Update()
    {
        // 保存パネルが開いている間は掘削しない
        if (savePanel.activeSelf)
            return;

        if (EditModeManager.Instance.CurrentMode != EditMode.Dig)
            return;

        // マウスカーソル下の壁を選択
        HighlightWall();

        // クリックした瞬間は必ず1マス掘る
        if (Input.GetMouseButtonDown(0))
        {
            Dig();
            lastDigWall = currentWall;
        }

        // クリックしたままマウスが動いたら掘る
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

        // ボタンを離したらリセット
        if (Input.GetMouseButtonUp(0))
        {
            lastDigWall = null;
        }
    }

    /// マウスカーソルが乗っている壁を選択状態にする
    void HighlightWall()
    {
        // マウス位置からレイを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // レイがオブジェクトに当たったか判定
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 当たったオブジェクトからWallBlockを取得
            WallBlock wall = hit.collider.GetComponent<WallBlock>();

            // 前回選択していた壁と違う場合
            if (wall != currentWall)
            {
                // 前回の壁の選択状態を解除
                if (currentWall != null)
                {
                    currentWall.Deselect();
                }

                // 新しい壁を選択
                currentWall = wall;

                // 壁が存在する場合は選択状態にする
                if (currentWall != null)
                {
                    currentWall.Select();
                }
            }
        }
        else
        {
            // 壁以外を見ている場合は選択解除
            if (currentWall != null)
            {
                currentWall.Deselect();
                currentWall = null;
            }
        }
    }

    /// 選択中の壁を掘る
    void Dig()
    {
        if (currentWall == null)
            return;

        // 掘る前に保存
        undoManager.SaveState();

        mapManager.Dig(currentWall.GridPosition);

        currentWall = null;
    }
}