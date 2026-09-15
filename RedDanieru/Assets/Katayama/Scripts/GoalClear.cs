using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalClear : MonoBehaviour
{
    [Header("クリアUI")]
    [SerializeField]
    private GameObject clearPanel;

    [Header("Clear時に召喚するカメラ")]
    [SerializeField]
    private GameObject clearCameraPrefab;

    [Header("Clearカメラ設定")]
    [Tooltip("32×32マップ時のカメラ位置")]
    [SerializeField]
    private Vector3 clearCameraPosition =
        new Vector3(16f, 30f, 16f);

    [Tooltip("カメラ角度")]
    [SerializeField]
    private Vector3 clearCameraRotation =
        new Vector3(90f, 0f, 0f);

    [Tooltip("32×32マップ時のField of View")]
    [SerializeField]
    private float baseFieldOfView = 60f;

    [Tooltip("基準となるマップサイズ")]
    [SerializeField]
    private float baseMapSize = 31f;

    [Header("タイトルシーン")]
    [SerializeField]
    private string titleSceneName = "Title";

    private bool isCleared = false;

    private void Start()
    {
        ResetClearState();
    }

    private void Update()
    {
        if (isCleared)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ResetClearState()
    {
        isCleared = false;

        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared)
            return;

        if (!other.CompareTag("Player"))
            return;

        TestPlayManager testPlayManager =
            FindObjectOfType<TestPlayManager>();

        // クリアチェック
        if (testPlayManager != null &&
            testPlayManager.IsClearCheck)
        {
            isCleared = true;

            testPlayManager.ClearCheckSuccess();

            Debug.Log(
                "クリアチェック成功。"
            );

            return;
        }

        // テストプレイ
        if (testPlayManager != null &&
            testPlayManager.IsTestPlay)
        {
            isCleared = true;

            Debug.Log(
                "テストプレイクリア。編集モードへ戻ります。"
            );

            testPlayManager.ReturnToEdit();

            return;
        }

        // 通常ゲームプレイ
        isCleared = true;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SpawnClearCamera();

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Clear Panelが設定されていません。"
            );
        }

        Debug.Log(
            "GAME CLEAR! Clear UIを表示しました。"
        );
    }

    private void SpawnClearCamera()
    {
        if (clearCameraPrefab == null)
        {
            Debug.LogError(
                "Clear Camera Prefabが設定されていません。"
            );

            return;
        }

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

        Camera[] cameras =
            FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            if (camera != clearCamera)
            {
                camera.gameObject.SetActive(false);
            }
        }

        MapManager mapManager =
            FindObjectOfType<MapManager>();

        float scale = 1f;

        if (mapManager != null)
        {
            float mapWidth =
                mapManager.width - 1;

            float mapDepth =
                mapManager.depth - 1;

            float mapSize =
                Mathf.Max(
                    mapWidth,
                    mapDepth
                );

            scale =
                mapSize / baseMapSize;

            if (scale <= 0f)
                scale = 1f;

            float centerX =
                mapWidth / 2f;

            float centerZ =
                mapDepth / 2f;

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
            clearCamera.transform.position =
                clearCameraPosition;
        }

        clearCamera.transform.rotation =
            Quaternion.Euler(
                clearCameraRotation
            );

        clearCamera.fieldOfView =
            baseFieldOfView * scale;

        clearCamera.fieldOfView =
            Mathf.Clamp(
                clearCamera.fieldOfView,
                10f,
                120f
            );

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

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        FusionLauncher launcher =
            FindObjectOfType<FusionLauncher>();

        if (launcher != null)
        {
            launcher.ShutdownAndLoadTitle(
                titleSceneName
            );
        }
        else
        {
            SceneManager.LoadScene(
                titleSceneName
            );
        }
    }
}
