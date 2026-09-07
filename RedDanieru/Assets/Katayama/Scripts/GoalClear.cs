using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalClear : MonoBehaviour
{
    //==================================================
    // クリアUI
    //==================================================

    [Header("クリアUI")]
    [SerializeField]
    private GameObject clearPanel;

    //==================================================
    // Clearカメラ
    //==================================================

    [Header("Clear時に召喚するカメラ")]
    [SerializeField]
    private GameObject clearCameraPrefab;

    //==================================================
    // Clearカメラ設定
    //==================================================

    [Header("Clearカメラ設定")]

    [Tooltip("32×32マップ時のカメラ位置")]
    [SerializeField]
    private Vector3 clearCameraPosition =
        new Vector3(
            16f,
            30f,
            16f
        );

    [Tooltip("カメラ角度")]
    [SerializeField]
    private Vector3 clearCameraRotation =
        new Vector3(
            90f,
            0f,
            0f
        );

    [Tooltip("32×32マップ時のField of View")]
    [SerializeField]
    private float baseFieldOfView = 60f;

    [Tooltip("基準となるマップサイズ")]
    [SerializeField]
    private float baseMapSize = 31f;

    //==================================================
    // タイトルシーン
    //==================================================

    [Header("タイトルシーン")]
    [SerializeField]
    private string titleSceneName = "Title";

    //==================================================
    // 内部変数
    //==================================================

    private bool isCleared = false;

    //==================================================
    // Start
    //==================================================

    private void Start()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    //==================================================
    // Update
    //==================================================

    private void Update()
    {
        if (isCleared)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    //==================================================
    // Goalに触れた
    //==================================================

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared)
            return;

        if (!other.CompareTag("Player"))
            return;

        isCleared = true;

        //==================================================
        // ゲーム停止
        //==================================================

        Time.timeScale = 0f;

        //==================================================
        // カーソル表示
        //==================================================

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //==================================================
        // Clearカメラ生成
        //==================================================

        SpawnClearCamera();

        //==================================================
        // Clear UI表示
        //==================================================

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        Debug.Log("GAME CLEAR!");
    }

    //==================================================
    // Clearカメラ生成
    //==================================================

    private void SpawnClearCamera()
    {
        if (clearCameraPrefab == null)
        {
            Debug.LogError(
                "Clear Camera Prefabが設定されていません。"
            );

            return;
        }

        //==================================================
        // Clearカメラ生成
        //==================================================

        GameObject cameraObject =
            Instantiate(clearCameraPrefab);

        Camera clearCamera =
            cameraObject.GetComponentInChildren<Camera>();

        if (clearCamera == null)
        {
            Debug.LogError(
                "Clear Camera PrefabにCameraがありません。"
            );

            Destroy(cameraObject);

            return;
        }

        //==================================================
        // 通常カメラ停止
        //==================================================

        Camera[] cameras =
            FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            if (camera != clearCamera)
            {
                camera.gameObject.SetActive(false);
            }
        }

        //==================================================
        // MapManager取得
        //==================================================

        MapManager mapManager =
            FindObjectOfType<MapManager>();

        //==================================================
        // マップサイズに合わせる
        //==================================================

        float scale = 1f;

        if (mapManager != null)
        {
            float mapWidth =
                (mapManager.width - 1);

            float mapDepth =
                (mapManager.depth - 1);

            float mapSize =
                Mathf.Max(
                    mapWidth,
                    mapDepth
                );

            scale =
                mapSize /
                baseMapSize;

            if (scale <= 0f)
            {
                scale = 1f;
            }

            //==================================================
            // マップ中央
            //==================================================

            float centerX =
                mapWidth / 2f;

            float centerZ =
                mapDepth / 2f;

            //==================================================
            // 基準カメラの中心からのズレ
            //==================================================

            float baseCenterX =
                baseMapSize / 2f;

            float baseCenterZ =
                baseMapSize / 2f;

            float offsetX =
                clearCameraPosition.x -
                baseCenterX;

            float offsetZ =
                clearCameraPosition.z -
                baseCenterZ;

            //==================================================
            // Clearカメラ位置
            //==================================================

            float cameraX =
                centerX +
                offsetX * scale;

            float cameraY =
                clearCameraPosition.y *
                scale;

            float cameraZ =
                centerZ +
                offsetZ * scale;

            clearCamera.transform.position =
                new Vector3(
                    cameraX,
                    cameraY,
                    cameraZ
                );
        }
        else
        {
            // MapManagerが見つからない場合
            clearCamera.transform.position =
                clearCameraPosition;
        }

        //==================================================
        // カメラ角度
        //==================================================

        clearCamera.transform.rotation =
            Quaternion.Euler(
                clearCameraRotation
            );

        //==================================================
        // Field of View変更
        //==================================================

        clearCamera.fieldOfView =
            baseFieldOfView * scale;

        //==================================================
        // Field of Viewを安全な範囲に制限
        //==================================================

        clearCamera.fieldOfView =
            Mathf.Clamp(
                clearCamera.fieldOfView,
                10f,
                120f
            );

        //==================================================
        // Clearカメラ有効化
        //==================================================

        clearCamera.gameObject.SetActive(true);

        Debug.Log(
            "Clear Camera Position : " +
            clearCamera.transform.position
        );

        Debug.Log(
            "Clear Camera Field of View : " +
            clearCamera.fieldOfView
        );
    }

    //==================================================
    // タイトルへ戻る
    //==================================================

    public void ReturnToTitle()
    {
        //==================================================
        // ゲーム時間を元に戻す
        //==================================================

        Time.timeScale = 1f;

        //==================================================
        // カーソル表示
        //==================================================

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //==================================================
        // タイトルシーン
        //==================================================

        SceneManager.LoadScene(
            titleSceneName
        );
    }
}