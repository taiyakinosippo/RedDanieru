using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    [Header("セーブ＆ロード画面")]
    [SerializeField] private GameObject saveLoadPanel;

    [Header("Saveボタン")]
    [SerializeField] private GameObject saveButtonObject;
    [SerializeField] private Button saveButton;

    [Header("Loadボタン")]
    [SerializeField] private GameObject loadButtonObject;
    [SerializeField] private Button loadButton;

    [Header("Load UI")]
    [SerializeField] private ReEditUI reEditUI;

    [Header("Local Save UI")]
    [SerializeField] private LocalSaveUI localSaveUI;

    //==================================================
    // 初期化
    //==================================================

    private void Start()
    {
        CloseSaveLoad();
    }

    //==================================================
    // セーブ＆ロード画面を開く
    //==================================================

    public void OpenSaveLoad()
    {
        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(true);
        }

        if (saveButtonObject != null)
        {
            saveButtonObject.SetActive(true);
        }

        if (loadButtonObject != null)
        {
            loadButtonObject.SetActive(true);
        }

        // LocalSaveUIを閉じる
        if (localSaveUI != null)
        {
            // LocalSaveUI側の画面を閉じる
            localSaveUI.Return();
        }

        // 最初は両方押せる
        ResetButtons();
    }

    //==================================================
    // Saveを押す
    //==================================================

    public void SelectSave()
    {
        if (saveButton != null)
        {
            saveButton.interactable = true;
        }

        if (loadButton != null)
        {
            loadButton.interactable = false;
        }

        // Load一覧を閉じる
        if (reEditUI != null)
        {
            reEditUI.CloseLoadList();
        }

        // LocalSaveUIを開く
        if (localSaveUI != null)
        {
            localSaveUI.OpenSavePanel();
        }
        else
        {
            Debug.LogError(
                "LocalSaveUIが設定されていません。"
            );
        }
    }

    //==================================================
    // Loadを押す
    //==================================================

    public void SelectLoad()
    {
        if (saveButton != null)
        {
            saveButton.interactable = false;
        }

        if (loadButton != null)
        {
            loadButton.interactable = true;
        }

        // LocalSaveUIを閉じる
        if (localSaveUI != null)
        {
            localSaveUI.Return();
        }

        // Load一覧を開く
        if (reEditUI != null)
        {
            reEditUI.OpenLoadList();
        }
    }

    //==================================================
    // Save / Load画面から戻る
    //==================================================

    public void ReturnFromSaveLoad()
    {
        // LocalSaveUIを閉じる
        if (localSaveUI != null)
        {
            localSaveUI.Return();
        }

        // Load一覧を閉じる
        if (reEditUI != null)
        {
            reEditUI.CloseLoadList();
        }

        // 両方押せる状態に戻す
        ResetButtons();
    }

    //==================================================
    // ボタン状態を初期化
    //==================================================

    private void ResetButtons()
    {
        if (saveButton != null)
        {
            saveButton.interactable = true;
        }

        if (loadButton != null)
        {
            loadButton.interactable = true;
        }
    }

    //==================================================
    // セーブ＆ロード画面を閉じる
    //==================================================

    public void CloseSaveLoad()
    {
        // LocalSaveUIを閉じる
        if (localSaveUI != null)
        {
            localSaveUI.Return();
        }

        // Load一覧を閉じる
        if (reEditUI != null)
        {
            reEditUI.CloseLoadList();
        }

        if (saveButtonObject != null)
        {
            saveButtonObject.SetActive(false);
        }

        if (loadButtonObject != null)
        {
            loadButtonObject.SetActive(false);
        }

        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(false);
        }

        ResetButtons();
    }
}