
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalSaveUI : MonoBehaviour
{
    [Header("保存先選択パネル")]
    [SerializeField] private GameObject savePanel;

    [Header("保存先ボタンPrefab")]
    [SerializeField] private GameObject saveSlotPrefab;

    [Header("保存先ボタンの親")]
    [SerializeField] private Transform saveSlotParent;

    [Header("空きボタンの数")]
    [SerializeField] private int emptySlotCount = 5;

    [Header("名前を付けて保存パネル")]
    [SerializeField] private GameObject nameSavePanel;

    [Header("ダンジョン名入力欄")]
    [SerializeField] private TMP_InputField dungeonNameInput;

    [Header("上書き確認パネル")]
    [SerializeField] private GameObject overwritePanel;

    [Header("上書き確認テキスト")]
    [SerializeField] private TMP_Text overwriteText;

    [Header("保存管理")]
    [SerializeField] private SaveManager saveManager;

    [Header("セーブ＆ロード画面")]
    [SerializeField] private SaveLoadUI saveLoadUI;

    [Header("ボタンの色")]
    [SerializeField] private Color normalButtonColor = Color.white;

    [SerializeField]
    private Color highlightedButtonColor =
        new Color(0.6f, 0.8f, 1f, 1f);

    private string overwriteDungeonName;

    private void Start()
    {
        CloseAllPanels();
    }

    //==================================================
    // ボタンの色を設定
    //==================================================

    private void SetButtonColors(Button button)
    {
        if (button == null)
            return;

        ColorBlock colors = button.colors;
        colors.normalColor = normalButtonColor;
        colors.highlightedColor = highlightedButtonColor;
        button.colors = colors;

        button.transition = Selectable.Transition.ColorTint;
    }

    //==================================================
    // 保存画面を開く
    //==================================================

    public void OpenSavePanel()
    {
        if (savePanel == null ||
            saveManager == null ||
            saveSlotParent == null ||
            saveSlotPrefab == null)
        {
            Debug.LogError("LocalSaveUIのInspector設定を確認してください。");
            return;
        }

        savePanel.SetActive(true);
        savePanel.transform.SetAsLastSibling();

        ClearSaveSlotButtons();

        CreateEmptyButtons();
        CreateSavedDungeonButtons();

        if (nameSavePanel != null)
            nameSavePanel.SetActive(false);

        if (overwritePanel != null)
            overwritePanel.SetActive(false);

        Debug.Log("保存先選択画面を開きました。");
    }

    //==================================================
    // 保存済みダンジョンボタン
    //==================================================

    private void CreateSavedDungeonButtons()
    {
        string[] dungeonNames = saveManager.GetSavedDungeonNames();

        if (dungeonNames == null)
            return;

        foreach (string dungeonName in dungeonNames)
        {
            if (!string.IsNullOrEmpty(dungeonName))
                CreateSavedDungeonButton(dungeonName);
        }
    }

    private void CreateSavedDungeonButton(string dungeonName)
    {
        GameObject buttonObject =
            Instantiate(saveSlotPrefab, saveSlotParent);

        buttonObject.transform.localScale = Vector3.one;
        buttonObject.transform.localRotation = Quaternion.identity;

        TMP_Text text =
            buttonObject.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
        {
            text.text = dungeonName;
            text.gameObject.SetActive(true);
            text.enabled = true;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button != null)
        {
            SetButtonColors(button);

            button.onClick.AddListener(() =>
            {
                SelectSavedDungeon(dungeonName);
            });
        }
        else
        {
            Debug.LogError("保存先ボタンPrefabにButtonがありません。");
        }
    }

    //==================================================
    // 新規保存ボタン
    //==================================================

    private void CreateEmptyButtons()
    {
        for (int i = 0; i < emptySlotCount; i++)
            CreateEmptyButton();
    }

    private void CreateEmptyButton()
    {
        GameObject buttonObject =
            Instantiate(saveSlotPrefab, saveSlotParent);

        buttonObject.transform.localScale = Vector3.one;
        buttonObject.transform.localRotation = Quaternion.identity;

        TMP_Text text =
            buttonObject.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
        {
            text.text = "新規保存";
            text.gameObject.SetActive(true);
            text.enabled = true;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button != null)
        {
            SetButtonColors(button);
            button.onClick.AddListener(SelectEmptySlot);
        }
        else
        {
            Debug.LogError("保存先ボタンPrefabにButtonがありません。");
        }
    }

    //==================================================
    // 保存済みダンジョンを選択
    //==================================================

    private void SelectSavedDungeon(string dungeonName)
    {
        overwriteDungeonName = dungeonName;

        if (overwriteText != null)
        {
            overwriteText.text =
                "「" + dungeonName + "」にはすでにデータが存在しています。\n" +
                "現在のデータを上書きして保存しますか？";
        }

        if (savePanel != null)
            savePanel.SetActive(false);

        if (overwritePanel != null)
            overwritePanel.SetActive(true);
    }

    //==================================================
    // 新規保存を選択
    //==================================================

    private void SelectEmptySlot()
    {
        if (dungeonNameInput == null)
        {
            Debug.LogError("ダンジョン名入力欄が設定されていません。");
            return;
        }

        dungeonNameInput.text = "";

        if (savePanel != null)
            savePanel.SetActive(false);

        if (nameSavePanel != null)
            nameSavePanel.SetActive(true);
    }

    //==================================================
    // 新規保存
    //==================================================

    public void SaveNewDungeon()
    {
        if (saveManager == null || dungeonNameInput == null)
        {
            Debug.LogError("SaveManagerまたは入力欄が設定されていません。");
            return;
        }

        string dungeonName = dungeonNameInput.text.Trim();

        if (string.IsNullOrEmpty(dungeonName))
        {
            Debug.LogWarning("ダンジョン名を入力してください。");
            return;
        }

        if (!saveManager.Save(dungeonName))
            return;

        Debug.Log("新規保存しました : " + dungeonName);
        CloseAfterSuccess();
    }

    //==================================================
    // 上書き保存
    //==================================================

    public void ConfirmOverwrite()
    {
        if (saveManager == null)
        {
            Debug.LogError("SaveManagerが設定されていません。");
            return;
        }

        if (string.IsNullOrEmpty(overwriteDungeonName))
        {
            Debug.LogError("上書き対象が選択されていません。");
            return;
        }

        if (!saveManager.Overwrite(overwriteDungeonName))
            return;

        Debug.Log("上書き保存しました : " + overwriteDungeonName);

        overwriteDungeonName = "";
        CloseAfterSuccess();
    }

    //==================================================
    // 保存成功後に画面を閉じる
    //==================================================

    private void CloseAfterSuccess()
    {
        if (saveLoadUI != null)
        {
            saveLoadUI.CloseSaveLoad();
        }
        else
        {
            Return();
            Debug.LogWarning(
                "SaveLoadUIが未設定のため、保存用パネルのみ閉じました。");
        }
    }

    //==================================================
    // 上書きをキャンセル
    //==================================================

    public void CancelOverwrite()
    {
        overwriteDungeonName = "";

        if (overwritePanel != null)
            overwritePanel.SetActive(false);

        OpenSavePanel();
    }

    //==================================================
    // 名前入力画面から戻る
    //==================================================

    public void ReturnFromNameSave()
    {
        if (nameSavePanel != null)
            nameSavePanel.SetActive(false);

        OpenSavePanel();
    }

    //==================================================
    // 保存用パネルを閉じる
    //==================================================

    public void Return()
    {
        CloseAllPanels();
    }

    private void ClearSaveSlotButtons()
    {
        if (saveSlotParent == null)
            return;

        for (int i = saveSlotParent.childCount - 1; i >= 0; i--)
            Destroy(saveSlotParent.GetChild(i).gameObject);
    }

    private void CloseAllPanels()
    {
        if (savePanel != null)
            savePanel.SetActive(false);

        if (nameSavePanel != null)
            nameSavePanel.SetActive(false);

        if (overwritePanel != null)
            overwritePanel.SetActive(false);
    }
}
