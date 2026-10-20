using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReEditUI : MonoBehaviour
{
    [Header("マップ管理")]
    [SerializeField] private MapManager mapManager;

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

    public bool IsSelectingDungeon { get; private set; }

    private void Awake()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
            scrollView.SetActive(false);
    }

    //==================================================
    // 読み込み一覧を開く
    //==================================================

    public void OpenLoadList()
    {
        Debug.Log("Loadボタンが押されました。");

        if (testPlayManager != null && testPlayManager.IsTestPlay)
        {
            Debug.LogWarning("テストプレイ中は読み込みできません。");
            return;
        }

        if (scrollView == null || content == null || buttonPrefab == null)
        {
            Debug.LogError("ReEditUIのInspector設定を確認してください。");
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

    //==================================================
    // ダンジョン一覧作成
    //==================================================

    private void CreateButtonList()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        string[] files = Directory.GetFiles(
            Application.persistentDataPath, "*.json");

        Debug.Log("保存済みダンジョン数 : " + files.Length);

        foreach (string file in files)
        {
            string dungeonName =
                Path.GetFileNameWithoutExtension(file);

            CreateDungeonButton(dungeonName);
        }
    }

    //==================================================
    // ダンジョンボタン作成
    //==================================================

    private void CreateDungeonButton(string dungeonName)
    {
        GameObject buttonObject = Instantiate(buttonPrefab, content);
        buttonObject.SetActive(true);

        TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>();

        if (text != null)
            text.text = dungeonName;
        else
            Debug.LogWarning("ButtonPrefabにTMP_Textがありません。");

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("ButtonPrefabにButtonがありません。");
            Destroy(buttonObject);
            return;
        }

        button.interactable = true;
        button.onClick.RemoveAllListeners();

        string selectedDungeon = dungeonName;

        button.onClick.AddListener(() =>
        {
            LoadDungeon(selectedDungeon);
        });
    }

    //==================================================
    // ダンジョン読み込み
    //==================================================

    private void LoadDungeon(string dungeonName)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json");

        if (!File.Exists(path))
        {
            Debug.LogError("ダンジョンが見つかりません : " + dungeonName);
            return;
        }

        string json = File.ReadAllText(path);
        DungeonMapData data = JsonUtility.FromJson<DungeonMapData>(json);

        if (data == null)
        {
            Debug.LogError("ダンジョンデータの読み込みに失敗しました。");
            return;
        }

        if (mapManager == null)
        {
            Debug.LogError("MapManagerが設定されていません。");
            return;
        }

        // マップを復元
        mapManager.LoadDungeon(data, false);

        SaveManager.LastDungeonId = data.dungeonId;
        SaveManager.LastDungeonName = dungeonName;

        Debug.Log("ダンジョンを読み込みました : " + dungeonName);

        // ロード成功後、セーブ＆ロード画面全体を閉じる
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

    //==================================================
    // 読み込み一覧を閉じる
    //==================================================

    public void CloseLoadList()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
            scrollView.SetActive(false);
    }
}
