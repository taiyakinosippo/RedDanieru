using UnityEngine;

public class PlaceManager : MonoBehaviour
{
    // メインカメラ
    [Header("参照")]
    [SerializeField] private Camera mainCamera;

    // マップ管理
    [SerializeField] private MapManager mapManager;

    // 保存パネル
    [SerializeField] private GameObject savePanel;

    void Update()
    {
        // 保存パネル表示中は配置しない
        if (savePanel.activeSelf)
        {
            return;
        }

        // マウス左クリックで配置
        if (Input.GetMouseButtonDown(0))
        {
            Place();
        }
    }

    /// <summary>
    /// オブジェクトを配置する
    /// </summary>
    void Place()
    {
        // マウス位置からレイを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // レイがオブジェクトに当たったか判定
        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            return;
        }

        // 床を取得
        FloorBlock floor = hit.collider.GetComponent<FloorBlock>();

        // 床以外は配置しない
        if (floor == null)
        {
            return;
        }

        // オブジェクトを配置
        mapManager.PlaceObject(floor.GridPosition);
    }
}