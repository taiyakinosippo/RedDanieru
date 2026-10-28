
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

public class GoalClear : MonoBehaviour
{
    [Header("クリアUI")]
    [SerializeField] private GameObject clearPanel;

    [Header("Clear時に召喚するカメラ")]
    [SerializeField] private GameObject clearCameraPrefab;

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
    [SerializeField] private float baseFieldOfView = 60f;

    [Tooltip("基準となるマップサイズ")]
    [SerializeField] private float baseMapSize = 31f;

    [Header("タイトルシーン")]
    [SerializeField] private string titleSceneName = "Title";

    private bool isCleared = false;
    private bool alreadyShown = false;

    private CursorController _cursorController;

    private GameObject spawnedClearCamera;

    //==================================================
    // 初期化
    //==================================================

    private void Start()
    {
        _cursorController = GetComponent<CursorController>();

        ResetClearState();
    }

    //==================================================
    // クリア状態リセット
    //==================================================

    public void ResetClearState()
    {
        isCleared = false;
        alreadyShown = false;

        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }

        DestroyClearCamera();

        Time.timeScale = 1f;
    }

    //==================================================
    // Goal接触
    //==================================================

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 操作中のプレイヤーだけ判定
        if (!NetworkAuthorityController.IsLocallyControlled(
            other.gameObject))
        {
            return;
        }

        // 通常のゴール接触
        ClearByGoalDestroyed();
    }

    //==================================================
    // ゴール破壊によるクリア
    //==================================================

    public void ClearByGoalDestroyed()
    {
        if (isCleared)
        {
            Debug.Log("クリア処理済み");
            return;
        }

        isCleared = true;

        Debug.Log("ゴール破壊によるクリア開始");

        //==================================================
        // マルチプレイ
        //==================================================

        NetworkGameState state =
            NetworkGameState.Instance;

        if (state != null)
        {
            Debug.Log("マルチプレイ：クリアを要求");

            state.RequestClear();
            return;
        }

        //==================================================
        // プレイヤー停止
        //==================================================

        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }

        //==================================================
        // テストプレイ判定
        //==================================================

        TestPlayManager testPlayManager =
            FindObjectOfType<TestPlayManager>();

        if (testPlayManager != null &&
            testPlayManager.IsTestPlay)
        {
            SaveManager saveManager =
                FindObjectOfType<SaveManager>();

            if (saveManager != null)
            {
                saveManager.SetTestPlayCleared();
            }

            Debug.Log(
                "テストプレイでゴールを破壊しました"
            );

            // 投稿確認など既存処理へ
            testPlayManager.ClearTestPlay();

            return;
        }

        //==================================================
        // 通常プレイ
        //==================================================

        ShowClear();
    }

    //==================================================
    // クリアカメラ生成
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

        spawnedClearCamera =
            Instantiate(clearCameraPrefab);

        Camera clearCamera =
            spawnedClearCamera.GetComponentInChildren<Camera>();

        if (clearCamera == null)
        {
            Debug.LogError(
                "Clear Camera PrefabにCameraがありません。"
            );

            Destroy(spawnedClearCamera);
            spawnedClearCamera = null;

            return;
        }

        // ほかのカメラを停止
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
                Mathf.Max(mapWidth, mapDepth);

            scale = mapSize / baseMapSize;

            if (scale <= 0f)
            {
                scale = 1f;
            }

            float centerX =
                mapWidth / 2f;

            float centerZ =
                mapDepth / 2f;

            float baseCenterX =
                baseMapSize / 2f;

            float baseCenterZ =
                baseMapSize / 2f;

            float offsetX =
                clearCameraPosition.x - baseCenterX;

            float offsetZ =
                clearCameraPosition.z - baseCenterZ;

            float cameraX =
                centerX + offsetX * scale;

            float cameraY =
                clearCameraPosition.y * scale;

            float cameraZ =
                centerZ + offsetZ * scale;

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
            Quaternion.Euler(clearCameraRotation);

        clearCamera.fieldOfView =
            Mathf.Clamp(
                baseFieldOfView * scale,
                10f,
                120f
            );

        clearCamera.gameObject.SetActive(true);

        if (_cursorController != null)
        {
            _cursorController.ShowCursor();
        }

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
    // タイトルへ
    //==================================================

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;

        FusionLauncher launcher =
            FusionLauncher.Instance;

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

    //==================================================
    // クリア表示
    //==================================================

    public void ShowClear()
    {
        Debug.Log("ShowClear開始");

        if (alreadyShown)
        {
            Debug.Log(
                "ShowClear：すでに表示済み"
            );

            return;
        }

        // 二重表示を防止
        alreadyShown = true;
        isCleared = true;

        Debug.Log("ShowClear：クリア処理開始");

        AddClearCount();

        //==================================================
        // PauseUIを閉じる
        //==================================================

        GameStopManager gameStopManager =
            FindObjectOfType<GameStopManager>();

        if (gameStopManager != null)
        {
            // PauseUIだけ閉じる
            gameStopManager.DisablePauseMenu();
        }

        //==================================================
        // プレイヤー停止
        //==================================================

        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }

        //==================================================
        // BGM復帰
        //==================================================

        if (BGMManager_Takeshita.Instance != null)
        {
            BGMManager_Takeshita.Instance.PlayNormalBGM();
        }

        //==================================================
        // ソロプレイのみ時間停止
        //==================================================

        if (!GameModeManager.IsMultiplayer)
        {
            Time.timeScale = 0f;
        }

        //==================================================
        // カメラ生成
        //==================================================

        Debug.Log("ShowClear：カメラ生成前");

        SpawnClearCamera();

        //==================================================
        // UI表示
        //==================================================

        Debug.Log("ShowClear：UI表示前");

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);

            Debug.Log("クリアUI表示実行");
        }
        else
        {
            Debug.LogError(
                "ClearPanelが未設定"
            );
        }

        //==================================================
        // 敵停止
        //==================================================

        EnemyBase[] enemies =
            FindObjectsOfType<EnemyBase>();

        foreach (EnemyBase enemy in enemies)
        {
            enemy.StopEnemy();
        }

        Debug.Log("ShowClear終了");
    }

    //==================================================
    // クリア回数加算
    //==================================================

    private void AddClearCount()
    {
        StartCoroutine(AddClearCountCoroutine());
    }

    private IEnumerator AddClearCountCoroutine()
    {
        WWWForm form = new WWWForm();

        form.AddField(
            "dungeon_id",
            RoomInfo.SelectedDungeon
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                ServerApi.Url("add_clear_count.php"),
                form
            );

        yield return request.SendWebRequest();
    }

    //==================================================
    // クリアカメラ削除
    //==================================================

    public void DestroyClearCamera()
    {
        if (spawnedClearCamera != null)
        {
            Destroy(spawnedClearCamera);
            spawnedClearCamera = null;
        }
    }
}
