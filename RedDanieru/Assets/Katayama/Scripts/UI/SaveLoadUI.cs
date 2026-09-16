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
        // 両方押せる状態に戻す
        ResetButtons();

        // Load一覧が開いていたら閉じる
        if (reEditUI != null)
        {
            reEditUI.CloseLoadList();
        }
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
