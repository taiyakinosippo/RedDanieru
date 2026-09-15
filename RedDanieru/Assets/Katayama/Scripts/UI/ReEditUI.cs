using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReEditUI : MonoBehaviour
{
    [Header("マップ管理")]
    [SerializeField] private MapManager mapManager;

    [Header("再編集一覧")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject scrollView;

    [Header("編集管理")]
    [SerializeField] private TestPlayManager testPlayManager;

    [Header("クリアチェック済みボタンの色")]
    [SerializeField] private Color clearedColor = Color.yellow;

    // 再編集ダンジョン一覧を開いているか
    public bool IsSelectingDungeon { get; private set; }

    //==================================================
    // 初期化
    //==================================================

    private void Awake()
    {
        IsSelectingDungeon = false;

        // 起動時は必ず非表示
        if (scrollView != null)
        {
            scrollView.SetActive(false);
        }
    }

    //==================================================
    // 再編集一覧を開く
    //==================================================

    public void OpenReEditList()
    {
        // テストプレイ中は開かない
        if (
            testPlayManager != null &&
            testPlayManager.IsTestPlay
        )
        {
            return;
        }

        IsSelectingDungeon = true;

        if (scrollView == null)
        {
            Debug.LogError(
                "再編集用ScrollViewが設定されていません。"
            );

            return;
        }

        scrollView.SetActive(true);

        // リストを一番手前にする
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
                "再編集一覧のContentが設定されていません。"
            );

            return;
        }

        // 以前作ったボタンを削除
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(
                content.GetChild(i).gameObject
            );
        }

        // 保存場所
        string savePath =
            Application.persistentDataPath;

        // JSONファイル取得
        string[] files =
            Directory.GetFiles(
                savePath,
                "*.json"
            );

        Debug.Log(
            "再編集可能なダンジョン数 : "
            + files.Length
        );

        if (files.Length == 0)
        {
            Debug.Log(
                "保存されているダンジョンがありません。"
            );

            return;
        }

        // ボタン生成
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
        if (buttonPrefab == null)
        {
            Debug.LogError(
                "再編集用ButtonPrefabが設定されていません。"
            );

            return;
        }

        GameObject buttonObject =
            Instantiate(
                buttonPrefab,
                content
            );

        // ボタンの文字
        TMP_Text text =
            buttonObject.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = dungeonName;
        }

        // Button取得
        Button button =
            buttonObject.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError(
                "ButtonPrefabにButtonコンポーネントがありません。"
            );

            return;
        }

        //==================================================
        // クリアチェック済みなら黄色
        //==================================================

        if (IsCleared(dungeonName))
        {
            Image image =
                buttonObject.GetComponent<Image>();

            if (image != null)
            {
                image.color = clearedColor;
            }
        }

        string selectedDungeon =
            dungeonName;

        button.onClick.AddListener(
            () =>
            {
                LoadForReEdit(
                    selectedDungeon
                );
            }
        );
    }

    //==================================================
    // クリアチェック済みか確認
    //==================================================

    private bool IsCleared(
        string dungeonName
    )
    {
        string key =
            "DungeonClearCheck_" +
            dungeonName;

        return PlayerPrefs.GetInt(
            key,
            0
        ) == 1;
    }

    //==================================================
    // 再編集用読み込み
    //==================================================

    private void LoadForReEdit(
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
                "ダンジョンが見つかりません : "
                + dungeonName
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

        // マップ復元
        mapManager.LoadDungeon(data);

        // 再編集一覧を閉じる
        CloseReEditList();

        // 編集モードへ
        if (testPlayManager != null)
        {
            testPlayManager.ReturnToEdit();
        }

        Debug.Log(
            "再編集を開始しました : "
            + dungeonName
        );
    }

    //==================================================
    // 一覧を閉じる
    //==================================================

    public void CloseReEditList()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
        {
            scrollView.SetActive(false);
        }
    }
}