using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReEditUI : MonoBehaviour
{
    [Header("マップ管理")]
    [SerializeField] private MapManager mapManager;

    [Header("読み込み一覧")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject scrollView;

    [Header("編集管理")]
    [SerializeField] private TestPlayManager testPlayManager;

    // 読み込み一覧を開いているか
    public bool IsSelectingDungeon { get; private set; }

    private void Awake()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
        {
            scrollView.SetActive(false);
        }
    }

    //==================================================
    // 読み込み一覧を開く
    //==================================================

    public void OpenLoadList()
    {
        Debug.Log("Loadボタンが押されました。");

        if (
            testPlayManager != null &&
            testPlayManager.IsTestPlay
        )
        {
            Debug.LogWarning(
                "テストプレイ中は読み込みできません。"
            );

            return;
        }

        if (scrollView == null)
        {
            Debug.LogError(
                "読み込み用ScrollViewが設定されていません。"
            );

            return;
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
        if (content == null)
        {
            Debug.LogError(
                "読み込み一覧のContentが設定されていません。"
            );

            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError(
                "読み込み用ButtonPrefabが設定されていません。"
            );

            return;
        }

        // 以前のボタンを削除
        for (
            int i = content.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                content.GetChild(i).gameObject
            );
        }

        string savePath =
            Application.persistentDataPath;

        string[] files =
            Directory.GetFiles(
                savePath,
                "*.json"
            );

        Debug.Log(
            "保存済みダンジョン数 : " +
            files.Length
        );

        if (files.Length == 0)
        {
            Debug.Log(
                "保存されているダンジョンがありません。"
            );

            return;
        }

        foreach (string file in files)
        {
            string dungeonName =
                Path.GetFileNameWithoutExtension(
                    file
                );

            CreateDungeonButton(
                dungeonName
            );
        }
    }

    //==================================================
    // ダンジョンボタン作成
    //==================================================

    private void CreateDungeonButton(
        string dungeonName
    )
    {
        GameObject buttonObject =
            Instantiate(
                buttonPrefab,
                content
            );

        TMP_Text text =
            buttonObject.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = dungeonName;
        }
        else
        {
            Debug.LogWarning(
                "ButtonPrefabにTMP_Textがありません。"
            );
        }

        Button button =
            buttonObject.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError(
                "ButtonPrefabにButtonコンポーネントがありません。"
            );

            return;
        }

        string selectedDungeon =
            dungeonName;

        button.onClick.AddListener(
            () =>
            {
                LoadDungeon(
                    selectedDungeon
                );
            }
        );
    }

    //==================================================
    // ダンジョン読み込み
    //==================================================

    private void LoadDungeon(
        string dungeonName
    )
    {
        string path =
            Path.Combine(
                Application.persistentDataPath,
                dungeonName + ".json"
            );

        if (!File.Exists(path))
        {
            Debug.LogError(
                "ダンジョンが見つかりません : " +
                dungeonName
            );

            return;
        }

        string json =
            File.ReadAllText(path);

        DungeonMapData data =
            JsonUtility.FromJson<DungeonMapData>(
                json
            );

        if (data == null)
        {
            Debug.LogError(
                "ダンジョンデータの読み込みに失敗しました。"
            );

            return;
        }

        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return;
        }

        // マップを復元
        // 読み込み直後は敵を動かさない
        mapManager.LoadDungeon(
            data,
            false
        );

        CloseLoadList();

        // 編集モードへ戻す
        if (testPlayManager != null)
        {
            testPlayManager.ReturnToEdit();
        }

        Debug.Log(
            "ダンジョンを読み込みました : " +
            dungeonName
        );
    }

    //==================================================
    // 読み込み一覧を閉じる
    //==================================================

    public void CloseLoadList()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
        {
            scrollView.SetActive(false);
        }
    }
}