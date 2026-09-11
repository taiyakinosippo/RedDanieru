using UnityEngine;

public class TestPlayManager : MonoBehaviour
{
    public enum TestPlayMode
    {
        Edit,
        TestPlay
    }

    [Header("プレイヤープレハブ")]
    [SerializeField] private GameObject playerPrefab;

    [Header("スタート地点")]
    [SerializeField] private Transform startPoint;

    [Header("編集用UI")]
    [SerializeField] private GameObject editUI;

    [Header("テストプレイ用UI")]
    [SerializeField] private GameObject testPlayUI;

    private GameObject playerInstance;

    private TestPlayMode currentMode = TestPlayMode.Edit;

    private bool testPlayCleared = false;

    public bool IsTestPlay
    {
        get { return currentMode == TestPlayMode.TestPlay; }
    }

    public bool IsTestPlayCleared
    {
        get { return testPlayCleared; }
    }

    private void Start()
    {
        ReturnToEdit();
    }

    public void StartTestPlay()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefabが設定されていません。");
            return;
        }

        if (startPoint == null)
        {
            Debug.LogError("SpawnPointが設定されていません。");
            return;
        }

        currentMode = TestPlayMode.TestPlay;

        testPlayCleared = false;

        // 既にPlayerが存在する場合は削除
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        // SpawnPointの位置・向きにPlayerを生成
        playerInstance = Instantiate(
            playerPrefab,
            startPoint.position,
            startPoint.rotation
        );

        if (editUI != null)
        {
            editUI.SetActive(false);
        }

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(true);
        }

        Debug.Log("テストプレイ開始");
        Debug.Log("PlayerをSpawnPointに生成しました");
    }

    public void ReturnToEdit()
    {
        currentMode = TestPlayMode.Edit;

        if (playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }

        if (editUI != null)
        {
            editUI.SetActive(true);
        }

        if (testPlayUI != null)
        {
            testPlayUI.SetActive(false);
        }

        Debug.Log("編集モードに戻りました");
    }

    public void TestPlayClear()
    {
        if (!IsTestPlay)
        {
            return;
        }

        testPlayCleared = true;

        Debug.Log("テストプレイクリア");
    }
}