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

    [Header("ボタンの色")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.gray;

    private Image saveButtonImage;
    private Image loadButtonImage;

    private enum SelectedMenu
    {
        None,
        Save,
        Load
    }

    private SelectedMenu selectedMenu = SelectedMenu.None;

    //==================================================
    // 初期化
    //==================================================

    private void Start()
    {
        if (saveButton != null)
        {
            saveButtonImage = saveButton.GetComponent<Image>();
        }

        if (loadButton != null)
        {
            loadButtonImage = loadButton.GetComponent<Image>();
        }

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

        if (localSaveUI != null)
        {
            localSaveUI.Return();
        }

        if (reEditUI != null)
        {
            reEditUI.CloseLoadList();
        }

        // 最初はどちらも未選択
        selectedMenu = SelectedMenu.None;
        UpdateButtonState();
    }

    //==================================================
    // Saveを押す
    //==================================================

    public void SelectSave()
    {
        selectedMenu = SelectedMenu.Save;

        // ロード一覧を閉じる
        if (reEditUI != null)
        {
            reEditUI.CloseLoadList();
        }

        // セーブUIを開く
        if (localSaveUI != null)
        {
            localSaveUI.OpenSavePanel();
        }
        else
        {
            Debug.LogError("LocalSaveUIが設定されていません。");
        }

        UpdateButtonState();
    }

    //==================================================
    // Loadを押す
    //==================================================

    public void SelectLoad()
    {
        selectedMenu = SelectedMenu.Load;

        // セーブUIを閉じる
        if (localSaveUI != null)
        {
            localSaveUI.Return();
        }

        // ロード一覧を開く
        if (reEditUI != null)
        {
            reEditUI.OpenLoadList();
        }
        else
        {
            Debug.LogError("ReEditUIが設定されていません。");
        }

        UpdateButtonState();
    }

    //==================================================
    // ボタンの有効状態と色を更新
    //==================================================

    private void UpdateButtonState()
    {
        // 両方とも常に押せる
        if (saveButton != null)
        {
            saveButton.interactable = true;
        }

        if (loadButton != null)
        {
            loadButton.interactable = true;
        }

        if (saveButtonImage == null && saveButton != null)
        {
            saveButtonImage = saveButton.GetComponent<Image>();
        }

        if (loadButtonImage == null && loadButton != null)
        {
            loadButtonImage = loadButton.GetComponent<Image>();
        }

        if (saveButtonImage != null)
        {
            saveButtonImage.color =
                selectedMenu == SelectedMenu.Save
                ? selectedColor
                : normalColor;
        }

        if (loadButtonImage != null)
        {
            loadButtonImage.color =
                selectedMenu == SelectedMenu.Load
                ? selectedColor
                : normalColor;
        }
    }

    //==================================================
    // Save / Load画面から戻る
    //==================================================

    public void ReturnFromSaveLoad()
    {
        if (localSaveUI != null)
        {
            localSaveUI.Return();
        }

        if (reEditUI != null)
        {
            reEditUI.CloseLoadList();
        }

        selectedMenu = SelectedMenu.None;
        UpdateButtonState();
    }

    //==================================================
    // セーブ＆ロード画面を閉じる
    //==================================================

    public void CloseSaveLoad()
    {
        if (localSaveUI != null)
        {
            localSaveUI.Return();
        }

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

        selectedMenu = SelectedMenu.None;
        UpdateButtonState();
    }
}
