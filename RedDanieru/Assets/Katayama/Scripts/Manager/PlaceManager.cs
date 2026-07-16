using UnityEngine;

public class PlaceManager : MonoBehaviour
{
    // メインカメラ
    [SerializeField] private Camera mainCamera;

    // マップ管理
    [SerializeField] private MapManager mapManager;

    // 保存パネル
    [SerializeField] private GameObject savePanel;

    void Update()
    {
        // 保存画面表示中は配置しない
        if (savePanel.activeSelf)
            return;

        // 左クリックしたら配置
        if (Input.GetMouseButtonDown(0))
        {
            Place();
        }
    }

    /// <summary>
    /// 床をクリックした位置へオブジェクトを配置する
    /// </summary>
    void Place()
    {
        // マウス位置からレイを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // オブジェクトに当たらなければ終了
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // 床か判定
        FloorBlock floor = hit.collider.GetComponent<FloorBlock>();

        if (floor == null)
            return;

        // パレットで選択中のオブジェクトを配置
        mapManager.PlaceObject(
            floor.GridPosition,
            ObjectPaletteManager.Instance.CurrentObject);
    }
}