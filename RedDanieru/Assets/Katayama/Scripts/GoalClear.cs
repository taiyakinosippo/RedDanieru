using UnityEngine;
using UnityEngine.SceneManagement;

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

        // 他の人のキャラ（同期で動いているだけのもの）がゴールに入ったときは、
        // そのキャラを操作している人の画面で判定するので何もしない
        if (!NetworkAuthorityController.IsLocallyControlled(other.gameObject))
        {
            return;
        }

        isCleared = true;

        //==================================================
        // マルチプレイ
        //==================================================

        // [Networked]の値はState Authority（ホスト）しか書き換えられないので、
        // クリアをお願いする。確定したら全員の画面でShowClearが呼ばれる
        NetworkGameState state = NetworkGameState.Instance;

        if (state != null)
        {
            state.RequestClear();
            return;
        }

        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }

        TestPlayManager testPlayManager =
            FindObjectOfType<TestPlayManager>();

        //==================================================
        // テストプレイ中
        //==================================================

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
                "テストプレイでGoalに到達しました"
            );

            // シーン移動しない
            // TestPlayManager側で投稿確認を表示する
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
                mapSize /
                baseMapSize;

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
            Mathf.Clamp(
                baseFieldOfView * scale,
                10f,
                120f
            );

        clearCamera.gameObject.SetActive(true);
        _cursorController.ShowCursor();
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
    // クリア表示（マルチでは全員の画面でNetworkGameStateから呼ばれる）
    //==================================================

    public void ShowClear()
    {
        if (alreadyShown)
        {
            return;
        }

        isCleared = true;
        alreadyShown = true;

        // PauseUIを閉じる
        GameStopManager gameStopManager =
            FindObjectOfType<GameStopManager>();

        if (gameStopManager != null)
        {
            gameStopManager.DisablePauseMenu();
            gameStopManager.GameReturnButton();
        }

        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }

        if (BGMManager_Takeshita.Instance != null)
        {
            BGMManager_Takeshita.Instance.PlayNormalBGM();
        }

        // マルチで止めると通信で動いている物まで止まるので、ソロのときだけ止める
        if (!GameModeManager.IsMultiplayer)
        {
            Time.timeScale = 0f;
        }

        SpawnClearCamera();

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Clear Panelが設定されていません"
            );
        }

        Debug.Log(
            "GAME CLEAR! Clear UIを表示しました。"
        );
    }
}