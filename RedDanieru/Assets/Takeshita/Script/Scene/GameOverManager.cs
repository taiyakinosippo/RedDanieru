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

        NetworkRunner runner = FindObjectOfType<NetworkRunner>();

        if (runner == null)
            return;

        if (NetworkGameState.Instance != null)
        {
            Debug.Log(
                $"Dead={NetworkGameState.Instance.DeadPlayerCount} " +
                $"Start={NetworkGameState.Instance.StartPlayerCount}"
            );

            if (NetworkGameState.Instance.DeadPlayerCount >=
                NetworkGameState.Instance.StartPlayerCount)
            {
                gameOverShown = true;
                GameOver();
            }
        }

        if (isGameOver)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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

        isGameOver = true;
       
        //ソロの時だけ上空カメラ
       
        PlayerUI[] playerUIs =
    FindObjectsOfType<PlayerUI>(true);

        foreach (PlayerUI ui in playerUIs)
        {
            if (ui._dieText != null)
            {
                ui._dieText.enabled = false;
            }
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!GameModeManager.IsMultiplayer)
        {
            SpawnGameOverCamera();

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

        gameOverCamera.transform.position =
            cameraPosition;

        gameOverCamera.transform.rotation =
            Quaternion.Euler(cameraRotation);

        gameOverCamera.gameObject.SetActive(true);
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