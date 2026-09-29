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

    [Header("保存画面を開いたときに隠すUI")]
    [SerializeField] private GameObject mapCreateUI;

    [SerializeField] private GameObject otherEditUI;

    private string overwriteDungeonName;

    //==================================================
    // 初期化
    //==================================================

    private void Start()
    {
        CloseAllPanels();
    }

    //==================================================
    // 保存画面を開く
    //==================================================

    public void OpenSavePanel()
    {
        if (savePanel == null)
        {
            Debug.LogError(
                "保存先選択パネルが設定されていません。"
            );

            return;
        }

        if (saveManager == null)
        {
            Debug.LogError(
                "SaveManagerが設定されていません。"
            );

            return;
        }

        if (saveSlotParent == null)
        {
            Debug.LogError(
                "保存先ボタンの親(Content)が設定されていません。"
            );

            return;
        }

        if (saveSlotPrefab == null)
        {
            Debug.LogError(
                "保存先ボタンPrefabが設定されていません。"
            );

            return;
        }

        //==================================================
        // マップ作成画面を隠す
        //==================================================

        if (mapCreateUI != null)
        {
            mapCreateUI.SetActive(false);
        }

        if (otherEditUI != null)
        {
            otherEditUI.SetActive(false);
        }

        //==================================================
        // 保存画面を表示
        //==================================================

        savePanel.SetActive(true);

        // 保存画面を最前面にする
        savePanel.transform.SetAsLastSibling();

        //==================================================
        // 以前のボタンを削除
        //==================================================

        ClearSaveSlotButtons();

        //==================================================
        // 保存済みダンジョンを表示
        //==================================================

        CreateSavedDungeonButtons();

        //==================================================
        // 空きボタンを表示
        //==================================================

        CreateEmptyButtons();

        //==================================================
        // 名前入力画面を閉じる
        //==================================================

        if (nameSavePanel != null)
        {
            nameSavePanel.SetActive(false);
        }

        //==================================================
        // 上書き確認画面を閉じる
        //==================================================

        if (overwritePanel != null)
        {
            overwritePanel.SetActive(false);
        }

        Debug.Log(
            "保存先選択画面を開きました。"
        );
    }

    //==================================================
    // 保存済みダンジョンボタン作成
    //==================================================

    private void CreateSavedDungeonButtons()
    {
        string[] dungeonNames =
            saveManager.GetSavedDungeonNames();

        if (dungeonNames == null)
        {
            return;
        }

        foreach (string dungeonName in dungeonNames)
        {
            if (string.IsNullOrEmpty(dungeonName))
            {
                continue;
            }

            CreateSavedDungeonButton(
                dungeonName
            );
        }
    }

    //==================================================
    // 保存済みダンジョンボタン
    //==================================================

    private void CreateSavedDungeonButton(
        string dungeonName
    )
    {
        GameObject buttonObject =
            Instantiate(
                saveSlotPrefab,
                saveSlotParent
            );

        // サイズ・回転をPrefabの状態に合わせる
        buttonObject.transform.localScale =
            Vector3.one;

        buttonObject.transform.localRotation =
            Quaternion.identity;

        //==================================================
        // ダンジョン名
        //==================================================

        TMP_Text text =
            buttonObject.GetComponentInChildren<TMP_Text>(
                true
            );

        if (text != null)
        {
            text.text =
                dungeonName;

            text.gameObject.SetActive(true);

            text.enabled =
                true;

            text.color =
                Color.black;

            text.alignment =
                TextAlignmentOptions.Center;
        }
        else
        {
            Debug.LogError(
                "SaveSlotButtonPrefabにTMP_Textがありません。"
            );
        }

        //==================================================
        // ボタン
        //==================================================

        Button button =
            buttonObject.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(
                () =>
                {
                    SelectSavedDungeon(
                        dungeonName
                    );
                }
            );
        }
        else
        {
            Debug.LogError(
                "SaveSlotButtonPrefabにButtonがありません。"
            );
        }
    }

    //==================================================
    // 空きボタン作成
    //==================================================

    private void CreateEmptyButtons()
    {
        for (
            int i = 0;
            i < emptySlotCount;
            i++
        )
        {
            CreateEmptyButton();
        }
    }

    //==================================================
    // 空きボタン
    //==================================================

    private void CreateEmptyButton()
    {
        GameObject buttonObject =
            Instantiate(
                saveSlotPrefab,
                saveSlotParent
            );

        buttonObject.transform.localScale =
            Vector3.one;

        buttonObject.transform.localRotation =
            Quaternion.identity;

        //==================================================
        // 文字を空にする
        //==================================================

        TMP_Text text =
            buttonObject.GetComponentInChildren<TMP_Text>(
                true
            );

        if (text != null)
        {
            text.text = "";
        }

        //==================================================
        // ボタン
        //==================================================

        Button button =
            buttonObject.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(
                SelectEmptySlot
            );
        }
        else
        {
            Debug.LogError(
                "SaveSlotButtonPrefabにButtonがありません。"
            );
        }
    }

    //==================================================
    // 保存済みダンジョンを選択
    //==================================================

    private void SelectSavedDungeon(
        string dungeonName
    )
    {
        overwriteDungeonName =
            dungeonName;

        if (overwriteText != null)
        {
            overwriteText.text =
                "「" +
                dungeonName +
                "」を上書きしますか？";
        }

        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

        if (overwritePanel != null)
        {
            overwritePanel.SetActive(true);
        }

        Debug.Log(
            "上書き確認 : " +
            dungeonName
        );
    }

    //==================================================
    // 空き保存先を選択
    //==================================================

    private void SelectEmptySlot()
    {
        if (dungeonNameInput == null)
        {
            Debug.LogError(
                "ダンジョン名入力欄が設定されていません。"
            );

            return;
        }

        dungeonNameInput.text = "";

        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

        if (nameSavePanel != null)
        {
            nameSavePanel.SetActive(true);
        }

        Debug.Log(
            "空き保存先を選択しました。"
        );
    }

    //==================================================
    // 新規保存
    //==================================================

    public void SaveNewDungeon()
    {
        if (saveManager == null)
        {
            Debug.LogError(
                "SaveManagerが設定されていません。"
            );

            return;
        }

        if (dungeonNameInput == null)
        {
            Debug.LogError(
                "ダンジョン名入力欄が設定されていません。"
            );

            return;
        }

        string dungeonName =
            dungeonNameInput.text.Trim();

        if (string.IsNullOrEmpty(dungeonName))
        {
            Debug.Log(
                "ダンジョン名を入力してください。"
            );

            return;
        }

        bool success =
            saveManager.Save(
                dungeonName
            );

        if (!success)
        {
            return;
        }

        Debug.Log(
            "新規保存しました : " +
            dungeonName
        );

        if (nameSavePanel != null)
        {
            nameSavePanel.SetActive(false);
        }

        OpenSavePanel();
    }

    //==================================================
    // 上書き確定
    //==================================================

    public void ConfirmOverwrite()
    {
        if (saveManager == null)
        {
            Debug.LogError(
                "SaveManagerが設定されていません。"
            );

            return;
        }

        if (string.IsNullOrEmpty(
            overwriteDungeonName))
        {
            Debug.LogError(
                "上書き対象が選択されていません。"
            );

            return;
        }

        bool success =
            saveManager.Overwrite(
                overwriteDungeonName
            );

        if (!success)
        {
            return;
        }

        Debug.Log(
            "上書き保存しました : " +
            overwriteDungeonName
        );

        overwriteDungeonName = "";

        if (overwritePanel != null)
        {
            overwritePanel.SetActive(false);
        }

        OpenSavePanel();
    }

    //==================================================
    // 上書きをキャンセル
    //==================================================

    public void CancelOverwrite()
    {
        overwriteDungeonName = "";

        if (overwritePanel != null)
        {
            overwritePanel.SetActive(false);
        }

        OpenSavePanel();
    }

    //==================================================
    // 名前入力画面から戻る
    //==================================================

    public void ReturnFromNameSave()
    {
        if (nameSavePanel != null)
        {
            nameSavePanel.SetActive(false);
        }

        OpenSavePanel();
    }

    //==================================================
    // 保存画面を閉じる
    //==================================================

    public void Return()
    {
        CloseAllPanels();

        // マップ作成画面を戻す
        if (mapCreateUI != null)
        {
            mapCreateUI.SetActive(true);
        }

        if (otherEditUI != null)
        {
            otherEditUI.SetActive(true);
        }

        Debug.Log(
            "保存画面から戻りました。"
        );
    }

    //==================================================
    // 生成したボタンを削除
    //==================================================

    private void ClearSaveSlotButtons()
    {
        if (saveSlotParent == null)
        {
            return;
        }

        for (
            int i = saveSlotParent.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                saveSlotParent.GetChild(i).gameObject
            );
        }
    }

    //==================================================
    // 全パネルを閉じる
    //==================================================

    private void CloseAllPanels()
    {
        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

        if (nameSavePanel != null)
        {
            nameSavePanel.SetActive(false);
        }

        if (overwritePanel != null)
        {
            overwritePanel.SetActive(false);
        }
    }
}