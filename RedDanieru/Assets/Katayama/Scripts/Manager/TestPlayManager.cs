using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class TestPlayManager : MonoBehaviour
{
    public enum TestPlayMode
    {
        Edit,
        TestPlay,
        ClearCheck
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

    [Header("クリアチェック用UI")]
    [SerializeField] private GameObject clearCheckUI;

    [Header("ESCメニュー")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("カメラ")]
    [SerializeField] private Camera editCamera;
    [SerializeField] private Camera playerCamera;

    private GameObject playerInstance;

    private TestPlayMode currentMode =
        TestPlayMode.Edit;

    private bool clearCheckCleared = false;
    private string clearCheckDungeonName;

    // 編集モードへ戻っている最中か
    private bool isReturningToEdit = false;

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

    public bool IsClearCheck =>
        currentMode == TestPlayMode.ClearCheck;

    public bool IsPlaying =>
        currentMode == TestPlayMode.TestPlay ||
        currentMode == TestPlayMode.ClearCheck;

    public bool IsClearCheckCleared =>
        clearCheckCleared;

    public string ClearCheckDungeonName =>
        clearCheckDungeonName;

    public bool IsReturningToEdit =>
        isReturningToEdit;

    private void Start()
    {
        ReturnToEdit();
    }

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
        StartPlayMode(TestPlayMode.TestPlay);

        Debug.Log("テストプレイ開始");
    }

    //==================================================
    // クリアチェック開始
    //==================================================

    public void StartClearCheck(string dungeonName)
    {
        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "クリアチェックするダンジョン名がありません。"
            );

            return;
        }

        clearCheckDungeonName = dungeonName;
        clearCheckCleared = false;

        StartPlayMode(TestPlayMode.ClearCheck);

        Debug.Log(
            "クリアチェック開始 : " +
            dungeonName
        );
    }

    //==================================================
    // プレイモード開始
    //==================================================

    private void StartPlayMode(TestPlayMode mode)
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

        currentMode = mode;

        GoalClear goalClear =
            FindObjectOfType<GoalClear>();

        if (goalClear != null)
            goalClear.ResetClearState();

        SaveEnemyPositions();

        if (mapManager != null)
        {
            mapManager.BuildNavigation();
        }
        else
        {
            Debug.LogError(
                "TestPlayManagerにMapManagerが設定されていません。"
            );
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
            mapManager.EnableEnemyMovement();

        if (mapCreateUI != null)
            mapCreateUI.SetActive(false);

        if (otherEditUI != null)
            otherEditUI.SetActive(false);

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(
                mode == TestPlayMode.TestPlay
            );
        }

        if (clearCheckUI != null)
        {
            clearCheckUI.SetActive(
                mode == TestPlayMode.ClearCheck
            );
        }

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (editCamera != null)
            editCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    //==================================================
    // クリアチェック成功
    //==================================================

    public void ClearCheckSuccess()
    {
        if (!IsClearCheck)
            return;

        if (clearCheckCleared)
            return;

        clearCheckCleared = true;

        SaveManager saveManager =
            FindObjectOfType<SaveManager>();

        if (saveManager != null)
        {
            saveManager.SetTestPlayCleared();
        }
        else
        {
            Debug.LogError(
                "SaveManagerが見つかりません。"
            );
        }

        // クリアチェックUIにクリア済みを通知
        ClearCheckUI clearCheckUI =
            FindObjectOfType<ClearCheckUI>();

        if (clearCheckUI != null)
        {
            clearCheckUI.SetCleared(
                clearCheckDungeonName
            );
        }

        Debug.Log(
            "クリアチェック成功 : " +
            clearCheckDungeonName
        );

        // 編集モードへ戻る
        ReturnToEdit();
    }

    //==================================================
    // 敵の位置を保存
    //==================================================

    private void SaveEnemyPositions()
    {
        enemyPositions.Clear();

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

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
                    agent.Warp(data.position);
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

        pauseMenuUI.SetActive(!isPaused);

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

        if (currentMode == TestPlayMode.TestPlay ||
            currentMode == TestPlayMode.ClearCheck)
        {
            RestoreEnemyPositions();
        }

        currentMode = TestPlayMode.Edit;

        if (playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

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
            mapCreateUI.SetActive(true);

        if (otherEditUI != null)
            otherEditUI.SetActive(true);

        if (testPlayUI != null)
            testPlayUI.SetActive(false);

        if (clearCheckUI != null)
            clearCheckUI.SetActive(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

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
            playerCamera.gameObject.SetActive(false);

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