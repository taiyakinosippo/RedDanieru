
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReEditUI : MonoBehaviour
{
    [Header("マップ管理")]
    [SerializeField] private MapManager mapManager;

    [Header("履歴管理")]
    [SerializeField] private UndoManager undoManager;

    [Header("保存状態管理")]
    [SerializeField] private SaveManager saveManager;

    [Header("読み込み一覧")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject scrollView;

    [Header("読み込み用Canvas")]
    [SerializeField] private Canvas loadCanvas;

    [Header("編集管理")]
    [SerializeField] private TestPlayManager testPlayManager;

    [Header("保存画面")]
    [SerializeField] private LocalSaveUI localSaveUI;

    [Header("セーブ＆ロード画面")]
    [SerializeField] private SaveLoadUI saveLoadUI;

    [Header("未保存確認ダイアログ")]
    [SerializeField] private GameObject unsavedConfirmPanel;
    [SerializeField] private TMP_Text unsavedConfirmText;

    [Header("ロードボタンの色")]
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField]
    private Color highlightedButtonColor =
        new Color(0.6f, 0.8f, 1f, 1f);

    public bool IsSelectingDungeon { get; private set; }

    private string pendingDungeonName;

    private void Awake()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
            scrollView.SetActive(false);

        if (unsavedConfirmPanel != null)
            unsavedConfirmPanel.SetActive(false);
    }

    public void OpenLoadList()
    {
        Debug.Log("Loadボタンが押されました。");

        if (testPlayManager != null &&
            testPlayManager.IsTestPlay)
        {
            Debug.LogWarning("テストプレイ中は読み込みできません。");
            return;
        }

        if (scrollView == null ||
            content == null ||
            buttonPrefab == null)
        {
            Debug.LogError(
                "ReEditUIのInspector設定を確認してください。");
            return;
        }

        if (loadCanvas == null)
            loadCanvas = scrollView.GetComponentInParent<Canvas>();

        if (loadCanvas != null)
        {
            loadCanvas.overrideSorting = true;
            loadCanvas.sortingOrder = 100;
        }
        else
        {
            Debug.LogError("読み込み用Canvasが見つかりません。");
        }

        IsSelectingDungeon = true;

        scrollView.SetActive(true);
        scrollView.transform.SetAsLastSibling();

        CreateButtonList();
    }

    private void CreateButtonList()
    {
        // 以前のボタンを削除
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        string[] files = Directory.GetFiles(
            Application.persistentDataPath,
            "*.json");

        files = files
            .OrderBy(file => File.GetCreationTime(file))
            .ThenBy(file => Path.GetFileNameWithoutExtension(file))
            .ToArray();

        Debug.Log("保存済みダンジョン数 : " + files.Length);

        foreach (string file in files)
        {
            string dungeonName =
                Path.GetFileNameWithoutExtension(file);

            CreateDungeonButton(dungeonName);
        }
    }

    private void CreateDungeonButton(string dungeonName)
    {
        GameObject buttonObject =
            Instantiate(buttonPrefab, content);

        buttonObject.SetActive(true);

        // ボタン内のTextMeshProを取得
        TMP_Text[] texts =
            buttonObject.GetComponentsInChildren<TMP_Text>(true);

        if (texts.Length > 0)
        {
            // ボタン内のテキストに実際のダンジョン名を設定
            TMP_Text dungeonText = texts[0];

            dungeonText.text = dungeonName;
            dungeonText.color = Color.black;
            dungeonText.ForceMeshUpdate();

            Debug.Log(
                "ロードボタン名を設定: " + dungeonName);
        }
        else
        {
            Debug.LogError(
                "ButtonPrefab内にTextMeshProがありません。");
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError(
                "ButtonPrefabにButtonコンポーネントがありません。");

            Destroy(buttonObject);
            return;
        }

        button.interactable = true;
        button.onClick.RemoveAllListeners();

        // マウスを重ねたときの色を設定
        SetButtonColors(button);

        string selectedDungeon = dungeonName;

        button.onClick.AddListener(() =>
        {
            RequestLoadDungeon(selectedDungeon);
        });
    }

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

    private void RequestLoadDungeon(string dungeonName)
    {
        if (saveManager != null &&
            saveManager.HasUnsavedChanges())
        {
            pendingDungeonName = dungeonName;

            if (unsavedConfirmPanel == null)
            {
                Debug.LogError(
                    "未保存確認ダイアログが設定されていません。");
                return;
            }

            if (unsavedConfirmText != null)
            {
                unsavedConfirmText.text =
                    "現在のマップは保存されていません。\n" +
                    "ロードすると編集内容が失われます。\n" +
                    "ロードしますか？";
            }

            unsavedConfirmPanel.SetActive(true);
            unsavedConfirmPanel.transform.SetAsLastSibling();

            return;
        }

        LoadDungeon(dungeonName);
    }

    public void ConfirmPendingLoad()
    {
        if (string.IsNullOrEmpty(pendingDungeonName))
            return;

        string dungeonName = pendingDungeonName;
        pendingDungeonName = null;

        if (unsavedConfirmPanel != null)
            unsavedConfirmPanel.SetActive(false);

        LoadDungeon(dungeonName);
    }

    public void CancelPendingLoad()
    {
        pendingDungeonName = null;

        if (unsavedConfirmPanel != null)
            unsavedConfirmPanel.SetActive(false);
    }

    private void LoadDungeon(string dungeonName)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json");

        if (!File.Exists(path))
        {
            Debug.LogError(
                "ダンジョンが見つかりません : " + dungeonName);
            return;
        }

        try
        {
            string json = File.ReadAllText(path);

            DungeonMapData data =
                JsonUtility.FromJson<DungeonMapData>(json);

            if (data == null)
            {
                Debug.LogError(
                    "ダンジョンデータの読み込みに失敗しました。");
                return;
            }

            if (mapManager == null)
            {
                Debug.LogError(
                    "MapManagerが設定されていません。");
                return;
            }

            mapManager.LoadDungeon(data, false);

            if (undoManager != null)
            {
                undoManager.ClearHistory();
            }
            else
            {
                Debug.LogWarning(
                    "UndoManagerが未設定です。Inspectorで割り当ててください。");
            }

            if (saveManager != null)
            {
                saveManager.SetMapLoadedClean();
            }

            SaveManager.LastDungeonId = data.dungeonId;
            SaveManager.LastDungeonName = dungeonName;

            Debug.Log(
                "ダンジョンを読み込みました : " + dungeonName);

            if (saveLoadUI != null)
            {
                saveLoadUI.CloseSaveLoad();
            }
            else
            {
                CloseLoadList();

                if (localSaveUI != null)
                    localSaveUI.Return();

                Debug.LogWarning(
                    "SaveLoadUIが未設定です。Inspectorで割り当ててください。");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "ダンジョンのロードに失敗しました : " + e.Message);
        }
    }

    public void CloseLoadList()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
            scrollView.SetActive(false);

        if (unsavedConfirmPanel != null)
            unsavedConfirmPanel.SetActive(false);

        pendingDungeonName = null;
    }
}
