using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using TMPro;

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

    [Header("マップ管理")]
    [SerializeField] private MapManager mapManager;

    [Header("マップクリエイトUI")]
    [SerializeField] private GameObject mapCreateUI;

    [Header("その他の編集UI")]
    [SerializeField] private GameObject otherEditUI;

    [Header("テストプレイ用UI")]
    [SerializeField] private GameObject testPlayUI;

    [Header("ESCメニュー")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("投稿確認UI")]
    [SerializeField] private GameObject postConfirmUI;

    [Header("投稿設定UI")]
    [SerializeField] private GameObject postSettingUI;

    [Header("投稿設定")]
    [SerializeField] private TMP_InputField dungeonNameInputField;
    [SerializeField] private TMP_InputField creatorNameInputField;

    [Header("ダンジョン投稿")]
    [SerializeField] private DungeonUploader dungeonUploader;

    [Header("カメラ")]
    [SerializeField] private Camera editCamera;
    [SerializeField] private Camera playerCamera;

    private GameObject playerInstance;

    private TestPlayMode currentMode =
        TestPlayMode.Edit;

    private bool isReturningToEdit = false;

    private bool isCleared = false;

    private class EnemyTransformData
    {
        public GameObject enemy;
        public Vector3 position;
        public Quaternion rotation;
    }

    private List<EnemyTransformData> enemyPositions =
        new List<EnemyTransformData>();

    public bool IsTestPlay =>
        currentMode == TestPlayMode.TestPlay;

    public bool IsPlaying =>
        currentMode == TestPlayMode.TestPlay;

    public bool IsReturningToEdit =>
        isReturningToEdit;

    public bool IsCleared =>
        isCleared;

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
        if (!IsPlaying)
            return;

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
        if (mapManager == null)
        {
            Debug.LogError(
                "TestPlayManagerにMapManagerが設定されていません。"
            );

            return;
        }

        if (!mapManager.HasGoal())
        {
            Debug.LogWarning(
                "Goalが配置されていないため、テストプレイを開始できません。"
            );

            return;
        }

        isCleared = false;

        StartPlayMode();

        Debug.Log(
            "テストプレイ開始"
        );
    }

    //==================================================
    // プレイモード開始
    //==================================================

    private void StartPlayMode()
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

        isReturningToEdit = false;

        currentMode =
            TestPlayMode.TestPlay;

        GoalClear goalClear =
            FindObjectOfType<GoalClear>();

        if (goalClear != null)
        {
            goalClear.ResetClearState();
        }

        SaveEnemyPositions();

        if (mapManager != null)
        {
            mapManager.BuildNavigation();
        }

        if (playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }

        playerInstance = Instantiate(
            playerPrefab,
            startPoint.position,
            startPoint.rotation
        );

        if (mapManager != null)
        {
            mapManager.EnableEnemyMovement();
        }

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

        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(false);
        }

        if (postSettingUI != null)
        {
            postSettingUI.SetActive(false);
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (editCamera != null)
        {
            editCamera.gameObject.SetActive(false);
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState =
            CursorLockMode.Locked;
    }

    //==================================================
    // テストプレイクリア
    //==================================================

    public void PlayClear()
    {
        if (!IsTestPlay)
            return;

        if (isCleared)
            return;

        isCleared = true;

        Debug.Log(
            "テストプレイクリア"
        );

        ShowPostConfirm();
    }

    //==================================================
    // 投稿確認UI表示
    //==================================================

    private void ShowPostConfirm()
    {
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState =
            CursorLockMode.None;

        if (postConfirmUI == null)
        {
            Debug.LogError(
                "TestPlayManagerに投稿確認UIが設定されていません。"
            );

            return;
        }

        postConfirmUI.SetActive(true);

        Debug.Log(
            "投稿確認UIを表示しました。"
        );
    }

    //==================================================
    // 投稿確認「はい」
    //==================================================

    public void OpenPostSetting()
    {
        Debug.Log(
            "投稿確認の「はい」が押されました。"
        );

        if (!isCleared)
        {
            Debug.LogWarning(
                "クリア状態ではありません。"
            );

            return;
        }

        // 投稿確認UIを非表示
        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(false);
        }

        // 投稿設定UIを表示
        if (postSettingUI == null)
        {
            Debug.LogError(
                "TestPlayManagerに投稿設定UIが設定されていません。"
            );

            return;
        }

        postSettingUI.SetActive(true);

        Debug.Log(
            "投稿Panelを表示しました : " +
            postSettingUI.name
        );

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState =
            CursorLockMode.None;
    }

    //==================================================
    // 投稿しない
    //==================================================

    public void CancelPost()
    {
        if (!isCleared)
            return;

        Debug.Log(
            "ダンジョンを投稿せず編集画面へ戻ります。"
        );

        ReturnToEdit();
    }

    //==================================================
    // 投稿設定画面「戻る」
    //==================================================

    public void BackFromPostSetting()
    {
        Debug.Log("投稿設定画面から編集画面へ戻ります。");

        if (postSettingUI != null)
        {
            postSettingUI.SetActive(false);
        }

        if (postConfirmUI != null)
        {
            postConfirmUI.SetActive(false);
        }

        ReturnToEdit();
    }

    //==================================================
    // 投稿する
    //==================================================

    public void PostDungeon()
    {
        if (!isCleared)
            return;

        if (dungeonUploader == null)
        {
            Debug.LogError(
                "TestPlayManagerにDungeonUploaderが設定されていません。"
            );

            return;
        }

        if (dungeonNameInputField == null)
        {
            Debug.LogError(
                "DungeonNameInputFieldが設定されていません。"
            );

            return;
        }

        if (creatorNameInputField == null)
        {
            Debug.LogError(
                "CreatorNameInputFieldが設定されていません。"
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
                "クリエイター名を入力してください。"
            );

            return;
        }

        Debug.Log(
            "ダンジョンを投稿します : " +
            dungeonName +
            " / " +
            creatorName
        );

        // DungeonUploaderは変更しない
        dungeonUploader.UploadDungeon(
            dungeonName,
            creatorName
        );

        if (postSettingUI != null)
        {
            postSettingUI.SetActive(false);
        }

        Time.timeScale = 1f;

        ReturnToEdit();
    }

    //==================================================
    // 敵の位置を保存
    //==================================================

    private void SaveEnemyPositions()
    {
        enemyPositions.Clear();

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        foreach (GameObject enemy in enemies)
        {
            enemyPositions.Add(
                new EnemyTransformData()
                {
                    enemy = enemy,
                    position = enemy.transform.position,
                    rotation = enemy.transform.rotation
                }
            );
        }

        Debug.Log(
            "開始時の敵位置を保存しました : " +
            enemyPositions.Count +
            "体"
        );
    }

    //==================================================
    // 敵の位置を復元
    //==================================================

    private void RestoreEnemyPositions()
    {
        foreach (EnemyTransformData data in enemyPositions)
        {
            if (data.enemy == null)
                continue;

            NavMeshAgent[] agents =
                data.enemy.GetComponentsInChildren<NavMeshAgent>();

            foreach (NavMeshAgent agent in agents)
            {
                if (!agent.enabled)
                    continue;

                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            data.enemy.transform.SetPositionAndRotation(
                data.position,
                data.rotation
            );

            foreach (NavMeshAgent agent in agents)
            {
                if (agent.enabled)
                {
                    agent.Warp(
                        data.position
                    );
                }
            }
        }

        enemyPositions.Clear();

        Debug.Log(
            "敵の位置を開始時の状態に戻しました。"
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
            Time.timeScale = 0f;

            Cursor.visible = true;
            Cursor.lockState =
                CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;

            Cursor.visible = false;
            Cursor.lockState =
                CursorLockMode.Locked;
        }
    }

    //==================================================
    // 編集モードへ戻る
    //==================================================

    public void ReturnToEdit()
    {
        isReturningToEdit = true;

        if (currentMode == TestPlayMode.TestPlay)
        {
            RestoreEnemyPositions();
        }

        currentMode =
            TestPlayMode.Edit;

        isCleared = false;

        if (playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        foreach (GameObject enemy in enemies)
        {
            NavMeshAgent[] agents =
                enemy.GetComponentsInChildren<NavMeshAgent>();

            foreach (NavMeshAgent agent in agents)
            {
                if (!agent.enabled)
                    continue;

                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.enabled = false;
            }
        }

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

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (editCamera != null)
        {
            editCamera.gameObject.SetActive(true);

            editCamera.transform.rotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState =
            CursorLockMode.None;

        Debug.Log(
            "マップクリエイトに戻りました"
        );

        StartCoroutine(
            EnableEditInputNextFrame()
        );
    }

    //==================================================
    // 編集操作を次のフレームから許可
    //==================================================

    private IEnumerator EnableEditInputNextFrame()
    {
        yield return null;

        isReturningToEdit = false;
    }
}