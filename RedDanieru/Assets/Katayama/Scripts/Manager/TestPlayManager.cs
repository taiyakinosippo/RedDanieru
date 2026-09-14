using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

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

    [Header("カメラ")]
    [SerializeField] private Camera editCamera;

    [SerializeField] private Camera playerCamera;

    private GameObject playerInstance;

    private TestPlayMode currentMode =
        TestPlayMode.Edit;

    private bool testPlayCleared = false;

    //==================================================
    // テストプレイ開始時の敵位置保存
    //==================================================

    private class EnemyTransformData
    {
        public GameObject enemy;
        public Vector3 position;
        public Quaternion rotation;
    }

    private List<EnemyTransformData> enemyPositions =
        new List<EnemyTransformData>();

    //==================================================
    // プロパティ
    //==================================================

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
        // 敵の位置を保存
        //==================================================

        SaveEnemyPositions();

        //==================================================
        // NavMesh生成
        //==================================================

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
        // 敵のNavMeshAgentを有効化
        //==================================================

        if (mapManager != null)
        {
            mapManager.EnableEnemyMovement();
        }

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
            "テストプレイ開始時の敵位置を保存しました : "
            + enemyPositions.Count
            + "体"
        );
    }

    //==================================================
    // 敵の位置を復元
    //==================================================

    private void RestoreEnemyPositions()
    {
        foreach (
            EnemyTransformData data
            in enemyPositions
        )
        {
            if (data.enemy == null)
                continue;

            NavMeshAgent[] agents =
                data.enemy.GetComponentsInChildren<
                    NavMeshAgent
                >();

            foreach (NavMeshAgent agent in agents)
            {
                if (agent.enabled)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
            }

            data.enemy.transform.SetPositionAndRotation(
                data.position,
                data.rotation
            );

            foreach (NavMeshAgent agent in agents)
            {
                if (agent.enabled)
                {
                    agent.Warp(data.position);
                }
            }
        }

        enemyPositions.Clear();

        Debug.Log(
            "敵の位置をテストプレイ開始時の状態に戻しました。"
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
    // 制作画面に戻る
    //==================================================

    public void ReturnToEdit()
    {
        //==================================================
        // 敵の位置を元に戻す
        //==================================================

        if (currentMode == TestPlayMode.TestPlay)
        {
            RestoreEnemyPositions();
        }

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
        // 敵のNavMeshAgentを停止
        //==================================================

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        foreach (GameObject enemy in enemies)
        {
            NavMeshAgent[] agents =
                enemy.GetComponentsInChildren<
                    NavMeshAgent
                >();

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