using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayManager : MonoBehaviour
{
    public enum TestPlayMode
    {
        Edit,
        TestPlay
    }

    [Header("プレイヤー")]
    [SerializeField] private GameObject playerPrefab;

    [Header("スタート地点")]
    [SerializeField] private Transform startPoint;

    [Header("マップクリエイトUI")]
    [SerializeField] private GameObject mapCreateUI;

    [Header("その他の編集UI")]
    [SerializeField] private GameObject otherEditUI;

    [Header("テストプレイ用UI")]
    [SerializeField] private GameObject testPlayUI;

    [Header("ESCメニュー")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("カメラ")]
    [SerializeField] private Camera editCamera;
    [SerializeField] private Camera playerCamera;

    private GameObject playerInstance;

    private TestPlayMode currentMode =
        TestPlayMode.Edit;

    private bool testPlayCleared = false;

    public bool IsTestPlay =>
        currentMode == TestPlayMode.TestPlay;

    public bool IsTestPlayCleared =>
        testPlayCleared;

    //==================================================
    // Start
    //==================================================

    private void Start()
    {
        ReturnToEdit();
    }

    //==================================================
    // Update
    //==================================================

    private void Update()
    {
        if (!IsTestPlay)
            return;

        // ESCキーでメニューを開閉
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseMenu();
        }
    }

    //==================================================
    // テストプレイ開始
    //==================================================

    public void StartTestPlay()
    {
        if (playerPrefab == null)
        {
            Debug.LogError(
                "Player Prefabが設定されていません。"
            );

            return;
        }

        if (startPoint == null)
        {
            Debug.LogError(
                "Start Pointが設定されていません。"
            );

            return;
        }

        //==================================================
        // テストプレイ状態
        //==================================================

        currentMode =
            TestPlayMode.TestPlay;

        testPlayCleared = false;

        //==================================================
        // Goalのクリア状態をリセット
        //==================================================

        GoalClear goalClear =
            FindObjectOfType<GoalClear>();

        if (goalClear != null)
        {
            goalClear.ResetClearState();
        }

        //==================================================
        // 既存Player削除
        //==================================================

        if (playerInstance != null)
        {
            Destroy(playerInstance);

            playerInstance = null;
        }

        //==================================================
        // Player生成
        //==================================================

        playerInstance =
            Instantiate(
                playerPrefab,
                startPoint.position,
                startPoint.rotation
            );

        //==================================================
        // マップクリエイトUI非表示
        //==================================================

        if (mapCreateUI != null)
        {
            mapCreateUI.SetActive(false);
        }

        //==================================================
        // その他の編集UI非表示
        //==================================================

        if (otherEditUI != null)
        {
            otherEditUI.SetActive(false);
        }

        //==================================================
        // テストプレイUI表示
        //==================================================

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(true);
        }

        //==================================================
        // ESCメニュー非表示
        //==================================================

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        //==================================================
        // 編集カメラOFF
        //==================================================

        if (editCamera != null)
        {
            editCamera.gameObject.SetActive(false);
        }

        //==================================================
        // PlayerカメラON
        //==================================================

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }

        //==================================================
        // ゲーム再開
        //==================================================

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState =
            CursorLockMode.Locked;

        Debug.Log(
            "テストプレイ開始"
        );
    }

    //==================================================
    // ESCメニュー
    //==================================================

    private void TogglePauseMenu()
    {
        if (pauseMenuUI == null)
            return;

        bool isPaused =
            pauseMenuUI.activeSelf;

        pauseMenuUI.SetActive(
            !isPaused
        );

        if (!isPaused)
        {
            // メニューを開く
            Time.timeScale = 0f;

            Cursor.visible = true;
            Cursor.lockState =
                CursorLockMode.None;
        }
        else
        {
            // メニューを閉じる
            Time.timeScale = 1f;

            Cursor.visible = false;
            Cursor.lockState =
                CursorLockMode.Locked;
        }
    }

    //==================================================
    // 制作画面に戻る
    //==================================================

    public void ReturnToEdit()
    {
        currentMode =
            TestPlayMode.Edit;

        //==================================================
        // Player削除
        //==================================================

        if (playerInstance != null)
        {
            Destroy(playerInstance);

            playerInstance = null;
        }

        //==================================================
        // マップクリエイトUI表示
        //==================================================

        if (mapCreateUI != null)
        {
            mapCreateUI.SetActive(true);
        }

        //==================================================
        // その他の編集UI表示
        //==================================================

        if (otherEditUI != null)
        {
            otherEditUI.SetActive(true);
        }

        //==================================================
        // テストプレイUI非表示
        //==================================================

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(false);
        }

        //==================================================
        // ESCメニュー非表示
        //==================================================

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        //==================================================
        // 編集カメラON
        //==================================================

        if (editCamera != null)
        {
            editCamera.gameObject.SetActive(true);

            // 上からの視点
            editCamera.transform.rotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );
        }

        //==================================================
        // PlayerカメラOFF
        //==================================================

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        //==================================================
        // ゲーム時間を戻す
        //==================================================

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState =
            CursorLockMode.None;

        Debug.Log(
            "マップクリエイトに戻りました"
        );
    }

    //==================================================
    // テストプレイクリア
    //==================================================

    public void TestPlayClear()
    {
        if (!IsTestPlay)
            return;

        testPlayCleared = true;

        Debug.Log(
            "テストプレイクリア"
        );
    }
}
