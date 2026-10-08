using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class GameOverManager : MonoBehaviour
{
    [Header("ゲームオーバーUI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("GameOverカメラ")]
    [SerializeField] private GameObject gameOverCameraPrefab;

    [Header("GameOverカメラ設定")]
    [SerializeField]
    private Vector3 cameraPosition =
        new Vector3(16f, 30f, 16f);

    [SerializeField]
    private Vector3 cameraRotation =
        new Vector3(90f, 0f, 0f);

    [SerializeField] private string titleSceneName = "Title";

    private bool isGameOver = false;

    private bool gameOverShown;

    public bool IsGameOver => isGameOver;

    // プレイヤーが毎フレーム参照するので、探さずに済むようにしておく
    public static GameOverManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        ResetGameOverState();
    }

    private void Update()
    {
        if (!GameModeManager.IsMultiplayer)
            return;

        if (gameOverShown)
            return;

        // 途中で抜けた人は数えず、残っている全員が死んだらゲームオーバー
        if (NetworkGameState.Instance != null &&
            NetworkGameState.Instance.AreAllPlayersDead())
        {
            gameOverShown = true;
            GameOver();
        }
    }

    public void ResetGameOverState()
    {
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        if (BGMManager_Takeshita.Instance != null)
        {
            BGMManager_Takeshita.Instance.PlayNormalBGM();
        }

        isGameOver = true;

        Time.timeScale = 0f;
      

        // 全員の画面でそれぞれ判定しているので、自分の画面のプレイヤーを消すだけでよい
        GameObject[] players =
        GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }

    //    PlayerInput[] inputs =
    //FindObjectsOfType<PlayerInput>();

    //    foreach (PlayerInput input in inputs)
    //    {
    //        input.enabled = false;
    //    }

        SpawnGameOverCamera();

        if (!GameModeManager.IsMultiplayer)
        {
            GameObject player= GameObject.FindWithTag("Player");

            if (player != null)
            {
                Destroy(player);
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("GAME OVER");
    }

    private void SpawnGameOverCamera()
    {
        if (gameOverCameraPrefab == null)
            return;

        GameObject cameraObject =
            Instantiate(gameOverCameraPrefab);

        Camera gameOverCamera =
            cameraObject.GetComponentInChildren<Camera>();

        if (gameOverCamera == null)
        {
            Destroy(cameraObject);
            return;
        }

        Camera[] cameras =
            FindObjectsOfType<Camera>();

        foreach (Camera cam in cameras)
        {
            if (cam != gameOverCamera)
            {
                cam.gameObject.SetActive(false);
            }
        }

        MapManager mapManager =
            FindObjectOfType<MapManager>();

        if (mapManager != null)
        {
            float centerX =
                (mapManager.width - 1) / 2f;

            float centerZ =
                (mapManager.depth - 1) / 2f;

            gameOverCamera.transform.position =
                new Vector3(
                    centerX,
                    30f,
                    centerZ
                );
        }
        else
        {
            gameOverCamera.transform.position =
                cameraPosition;
        }

        gameOverCamera.transform.rotation =
            Quaternion.Euler(cameraRotation);

        gameOverCamera.gameObject.SetActive(true);

    }

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
}