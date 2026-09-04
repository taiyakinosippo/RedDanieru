using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalClear : MonoBehaviour
{
    [Header("クリアUI")]
    [SerializeField] private GameObject clearPanel;

    [Header("Clear時に召喚するカメラ")]
    [SerializeField] private GameObject clearCameraPrefab;

    [Header("Clearカメラ設定")]
    [SerializeField] private Vector3 clearCameraPosition = new Vector3(16f, 30f, 16f);
    [SerializeField] private Vector3 clearCameraRotation = new Vector3(90f, 0f, 0f);

    [Header("タイトルシーン")]
    [SerializeField] private string titleSceneName = "Title";

    private bool isCleared = false;

    private void Start()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared)
            return;

        if (!other.CompareTag("Player"))
            return;

        isCleared = true;

        // ゲーム停止
        Time.timeScale = 0f;

        // カーソル表示
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Clearカメラ召喚
        SpawnClearCamera();

        // Clear UI表示
        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        Debug.Log("GAME CLEAR!");
    }

    private void SpawnClearCamera()
    {
        if (clearCameraPrefab == null)
        {
            Debug.LogError("Clear Camera Prefabが設定されていません。");
            return;
        }

        GameObject cameraObject =
            Instantiate(clearCameraPrefab);

        Camera clearCamera =
            cameraObject.GetComponentInChildren<Camera>();

        if (clearCamera == null)
        {
            Debug.LogError("Clear Camera PrefabにCameraがありません。");
            Destroy(cameraObject);
            return;
        }

        // 現在のカメラを停止
        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            if (camera != clearCamera)
            {
                camera.gameObject.SetActive(false);
            }
        }

        // Inspectorで設定した位置・角度を使用
        clearCamera.transform.position = clearCameraPosition;
        clearCamera.transform.rotation =
            Quaternion.Euler(clearCameraRotation);

        clearCamera.gameObject.SetActive(true);
    }

    public void ReturnToTitle()
    {
        // ゲーム時間を戻す
        Time.timeScale = 1f;

        // カーソル表示
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(titleSceneName);
    }
}