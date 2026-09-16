using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToTitleConfirm : MonoBehaviour
{
    [Header("確認ダイアログ")]
    [SerializeField] private GameObject confirmPanel;

    [Header("タイトルシーン名")]
    [SerializeField] private string titleSceneName = "Title";

    private void Start()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    // Titleボタンから呼び出す
    public void OpenConfirm()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
    }

    // 「はい」ボタン
    public void Yes()
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

    // 「いいえ」ボタン
    public void No()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }
}