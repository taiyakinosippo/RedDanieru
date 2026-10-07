using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToTitleConfirm : MonoBehaviour
{
    [Header("確認ダイアログ")]
    [SerializeField] private GameObject confirmPanel;

    [Header("タイトルシーン名")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("セーブ管理")]
    [SerializeField] private SaveManager saveManager;

    private void Start()
    {
        if (saveManager == null)
        {
            saveManager =
                FindObjectOfType<SaveManager>();
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    //==================================================
    // Titleボタン
    //==================================================

    public void OpenConfirm()
    {
        // 未保存の変更がある場合だけ確認画面を表示
        if (
            saveManager != null &&
            saveManager.HasUnsavedChanges()
        )
        {
            if (confirmPanel != null)
            {
                confirmPanel.SetActive(true);
            }

            return;
        }

        // 変更がない場合はそのままタイトルへ
        GoToTitle();
    }


    //==================================================
    // 「はい」ボタン
    //==================================================

    public void Yes()
    {
        GoToTitle();
    }


    //==================================================
    // 「いいえ」ボタン
    //==================================================

    public void No()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }


    //==================================================
    // タイトルへ
    //==================================================

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
            SceneManager.LoadScene(
                titleSceneName
            );
        }
    }
}