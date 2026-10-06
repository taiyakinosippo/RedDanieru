using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class TestPlayManager : MonoBehaviour
{
    public enum TestPlayMode
    {
        Edit,
        TestPlay
    }

    [Header("シーン名")]
    [SerializeField] private string mapCreateSceneName = "Katayama_ren";
    [SerializeField] private string testPlaySceneName = "Testplay";

    [Header("プレイヤー")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform startPoint;

    [Header("マップ")]
    [SerializeField] private MapManager mapManager;

    [Header("編集UI")]
    [SerializeField] private GameObject mapCreateUI;
    [SerializeField] private GameObject otherEditUI;

    [Header("テストプレイUI")]
    [SerializeField] private GameObject testPlayUI;

    [Header("ポーズUI")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("投稿確認UI")]
    [SerializeField] private GameObject postConfirmUI;

    [Header("投稿設定UI")]
    [SerializeField] private GameObject postSettingUI;

    [Header("投稿情報")]
    [SerializeField] private TMP_InputField dungeonNameInputField;
    [SerializeField] private TMP_InputField creatorNameInputField;

    [Header("投稿")]
    [SerializeField] private DungeonUploader dungeonUploader;

    [Header("カメラ")]
    [SerializeField] private Camera editCamera;
    [SerializeField] private Camera playerCamera;

    [Header("ゴール警告UI")]
    [SerializeField] private GameObject goalWarningUI;
    [SerializeField] private TMP_Text goalWarningText;
    [SerializeField] private float goalWarningDuration = 2f;

    //==================================================
    // テストプレイ用データ
    //==================================================

    private static DungeonMapData pendingTestPlayData;

    // テストプレイ開始前の編集マップ
    private static DungeonMapData pendingEditData;

    // MapCreateSceneへ戻って編集マップを復元するか
    private static bool returnToEdit = false;

    // 初期化中か
    private bool isInitializing = false;

    // テストプレイをクリアしたか
    private bool isTestPlayCleared = false;

    //==================================================
    // プロパティ
    //==================================================

    public bool IsTestPlay
    {
        get
        {
            return string.Equals(
                SceneManager.GetActiveScene().name,
                testPlaySceneName,
                StringComparison.OrdinalIgnoreCase
            );
        }
    }

    public bool IsPlaying
    {
        get
        {
            return IsTestPlay && !isTestPlayCleared;
        }
    }

    //==================================================
    // 初期化
    //==================================================

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
        }

        if (IsTestPlay)
        {
            if (!isInitializing)
            {
                StartCoroutine(InitializeTestPlayScene());
            }
        }
        else
        {
            if (returnToEdit)
            {
                StartCoroutine(RestoreEditMap());
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //==================================================
    // シーン読み込み完了
    //==================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (string.Equals(
            scene.name,
            testPlaySceneName,
            StringComparison.OrdinalIgnoreCase))
        {
            if (!isInitializing)
            {
                StartCoroutine(
                    InitializeTestPlayScene()
                );
            }

            return;
        }

        if (string.Equals(
            scene.name,
            mapCreateSceneName,
            StringComparison.OrdinalIgnoreCase))
        {
            if (returnToEdit)
            {
                StartCoroutine(
                    RestoreEditMap()
                );
            }
        }
    }

    //==================================================
    // テストプレイ開始
    //==================================================

    public void StartTestPlay()
    {
        Debug.Log("================================");
        Debug.Log("テストプレイ開始チェック");
        Debug.Log("================================");

        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
        }

        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが見つかりません。"
            );

            return;
        }

        //==================================================
        // Goalチェック
        //==================================================

        if (!mapManager.HasGoal())
        {
            Debug.LogWarning(
                "Goalが配置されていないため、" +
                "テストプレイを開始できません。"
            );

            ShowGoalWarning();

            return;
        }

        Debug.Log(
            "Goalを確認しました。"
        );

        //==================================================
        // 現在のマップを保存
        //==================================================

        pendingEditData =
            mapManager.CreateSaveData();

        pendingTestPlayData =
            mapManager.CreateSaveData();

        returnToEdit = false;

        isTestPlayCleared = false;

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log(
            "テストプレイを開始します。"
        );

        SceneManager.LoadScene(
            testPlaySceneName
        );
    }

    //==================================================
    // テストプレイシーン初期化
    //==================================================

    private IEnumerator InitializeTestPlayScene()
    {
        isInitializing = true;

        Debug.Log("================================");
        Debug.Log("TestPlayScene初期化");
        Debug.Log("================================");

        yield return null;

        mapManager =
            FindObjectOfType<MapManager>();

        if (mapManager == null)
        {
            Debug.LogError(
                "TestPlaySceneのMapManagerが見つかりません。"
            );

            isInitializing = false;

            yield break;
        }

        //==================================================
        // テストプレイ用マップを復元
        //==================================================

        if (pendingTestPlayData == null)
        {
            Debug.LogError(
                "テストプレイ用マップデータがありません。"
            );

            isInitializing = false;

            yield break;
        }

        mapManager.LoadDungeon(
            pendingTestPlayData,
            false
        );

        yield return null;

        //==================================================
        // NavMesh作成
        //==================================================

        mapManager.BuildNavigation();

        yield return null;

        //==================================================
        // プレイヤー生成
        //==================================================

        if (playerPrefab == null)
        {
            Debug.LogError(
                "Player Prefabが設定されていません。"
            );
        }
        else if (startPoint == null)
        {
            Debug.LogError(
                "Start Pointが設定されていません。"
            );
        }
        else
        {
            GameObject player =
                Instantiate(
                    playerPrefab,
                    startPoint.position,
                    startPoint.rotation
                );

            Debug.Log(
                "Playerを生成しました : " +
                player.name
            );
        }

        //==================================================
        // 敵の移動を有効化
        //==================================================

        mapManager.EnableEnemyMovement();

        //==================================================
        // UI設定
        //==================================================

        SetupTestPlayUI();

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        isInitializing = false;

        Debug.Log(
            "TestPlayScene初期化完了"
        );
    }

    //==================================================
    // テストプレイUI設定
    //==================================================

    private void SetupTestPlayUI()
    {
        if (mapCreateUI != null)
        {
            mapCreateUI.SetActive(false);
        }

        if (otherEditUI != null)
        {
            otherEditUI.SetActive(false);
        }

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(true);
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(false);
        }

        if (postSettingUI != null)
        {
            postSettingUI.SetActive(false);
        }

        if (editCamera != null)
        {
            editCamera.gameObject.SetActive(false);
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }
    }

    //==================================================
    // Update
    //==================================================

    private void Update()
    {
        if (!IsTestPlay)
        {
            return;
        }

        if (isTestPlayCleared)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseMenu();
        }
    }

    //==================================================
    // ポーズメニュー切り替え
    //==================================================

    private void TogglePauseMenu()
    {
        if (pauseMenuUI == null)
        {
            return;
        }

        if (pauseMenuUI.activeSelf)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    //==================================================
    // ポーズメニューを開く
    //==================================================

    public void OpenPauseMenu()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //==================================================
    // ポーズメニューを閉じる
    //==================================================

    public void ClosePauseMenu()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //==================================================
    // テストプレイクリア
    //==================================================

    public void ClearTestPlay()
    {
        Debug.Log("================================");
        Debug.Log("テストプレイクリア");
        Debug.Log("================================");

        isTestPlayCleared = true;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(false);
        }

        if (postSettingUI != null)
        {
            postSettingUI.SetActive(false);
        }

        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "Post Confirm UIが設定されていません。"
            );
        }
    }

    //==================================================
    // 投稿設定を開く
    //==================================================

    public void OpenPostSetting()
    {
        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(false);
        }

        if (postSettingUI != null)
        {
            postSettingUI.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //==================================================
    // 投稿設定を閉じる
    //==================================================

    public void ClosePostSetting()
    {
        if (postSettingUI != null)
        {
            postSettingUI.SetActive(false);
        }

        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(true);
        }
    }

    //==================================================
    // 投稿しない
    //==================================================

    public void CancelPost()
    {
        Debug.Log("================================");
        Debug.Log("投稿しない");
        Debug.Log("MapCreateSceneへ戻ります");
        Debug.Log("編集マップを復元します");
        Debug.Log("================================");

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        returnToEdit = true;

        SceneManager.LoadScene(
            mapCreateSceneName
        );
    }

    //==================================================
    // 投稿する
    //==================================================

    public void PostDungeon()
    {
        if (dungeonUploader == null)
        {
            Debug.LogError(
                "DungeonUploaderが設定されていません。"
            );

            return;
        }

        if (dungeonNameInputField == null)
        {
            Debug.LogError(
                "Dungeon Name Input Fieldが設定されていません。"
            );

            return;
        }

        if (creatorNameInputField == null)
        {
            Debug.LogError(
                "Creator Name Input Fieldが設定されていません。"
            );

            return;
        }

        string dungeonName =
            dungeonNameInputField.text.Trim();

        string creatorName =
            creatorNameInputField.text.Trim();

        if (string.IsNullOrEmpty(dungeonName))
        {
            Debug.LogWarning(
                "ダンジョン名を入力してください。"
            );

            return;
        }

        if (string.IsNullOrEmpty(creatorName))
        {
            Debug.LogWarning(
                "製作者名を入力してください。"
            );

            return;
        }

        Debug.Log(
            "ダンジョンを投稿します : " +
            dungeonName
        );

        dungeonUploader.UploadDungeon(
            dungeonName,
            creatorName
        );
    }

    //==================================================
    // 編集画面へ戻る
    //==================================================

    public void ReturnToEdit()
    {
        Debug.Log("================================");
        Debug.Log("編集画面へ戻ります");
        Debug.Log("================================");

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //マップ上のオブジェクトのRigidBodyをすべて凍結
        mapManager.ObjectsAllFreeze();

        returnToEdit = true;

        SceneManager.LoadScene(
            mapCreateSceneName
        );
    }

    //==================================================
    // 編集マップを復元
    //==================================================

    private IEnumerator RestoreEditMap()
    {
        Debug.Log("================================");
        Debug.Log("編集マップ復元開始");
        Debug.Log("================================");

        yield return null;

        mapManager =
            FindObjectOfType<MapManager>();

        if (mapManager == null)
        {
            Debug.LogError(
                "MapCreateSceneのMapManagerが見つかりません。"
            );

            yield break;
        }

        if (pendingEditData == null)
        {
            Debug.LogError(
                "復元する編集マップデータがありません。"
            );

            yield break;
        }

        mapManager.LoadDungeon(
            pendingEditData,
            false
        );

        yield return null;

        if (mapCreateUI != null)
        {
            mapCreateUI.SetActive(true);
        }

        if (otherEditUI != null)
        {
            otherEditUI.SetActive(true);
        }

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(false);
        }

        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(false);
        }

        if (postSettingUI != null)
        {
            postSettingUI.SetActive(false);
        }

        if (editCamera != null)
        {
            editCamera.gameObject.SetActive(true);
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        returnToEdit = false;

        Debug.Log(
            "編集画面への復帰完了"
        );
    }

    //==================================================
    // ゴール警告表示
    //==================================================

    private void ShowGoalWarning()
    {
        if (goalWarningUI == null)
        {
            Debug.LogError(
                "Goal Warning UIが設定されていません。"
            );

            return;
        }

        if (goalWarningText != null)
        {
            goalWarningText.text =
                "ゴールが置かれていません";

            goalWarningText.color = Color.red;
        }

        goalWarningUI.SetActive(true);

        CancelInvoke(nameof(HideGoalWarning));
        Invoke(
            nameof(HideGoalWarning),
            goalWarningDuration
        );
    }

    private void HideGoalWarning()
    {
        if (goalWarningUI != null)
        {
            goalWarningUI.SetActive(false);
        }
    }
}