using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReturnToTitleConfirm : MonoBehaviour
{
    [Header("確認ダイアログ")]
    [SerializeField] private GameObject confirmPanel;

    [Header("タイトルシーン名")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("セーブ管理")]
    [SerializeField] private SaveManager saveManager;

    [Header("セーブ画面")]
    [SerializeField] private SaveLoadUI saveLoadUI;

    private void Start()
    {
        if (saveManager == null)
        {
            saveManager = FindObjectOfType<SaveManager>();
        }

        if (saveLoadUI == null)
        {
            saveLoadUI = FindObjectOfType<SaveLoadUI>();
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    // タイトルへ戻るボタン
    public void OpenConfirm()
    {
        if (saveManager != null &&
            saveManager.HasUnsavedChanges())
        {
            if (confirmPanel != null)
            {
                confirmPanel.SetActive(true);
            }

            return;
        }

        GoToTitle();
    }

    // はい：セーブ画面を開く
    public void Yes()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (saveLoadUI != null)
        {
            saveLoadUI.OpenSaveLoad();
        }
        else
        {
            Debug.LogError(
                "SaveLoadUIが設定されていません。"
            );
        }
    }

    // いいえ：保存せずにタイトルへ戻る
    public void No()
    {
        GoToTitle();
    }

    // キャンセル：編集画面に戻る
    public void Cancel()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    private void GoToTitle()
    {
        Time.timeScale = 1f;

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
            SceneManager.LoadScene(titleSceneName);
        }
    }
}
