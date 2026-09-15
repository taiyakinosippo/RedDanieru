using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClearCheckUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private MapManager mapManager;
    [SerializeField] private TestPlayManager testPlayManager;

    [Header("一覧")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject scrollView;

    [Header("クリア済みボタンの色")]
    [SerializeField] private Color clearedColor = Color.yellow;

    // クリアチェック一覧を開いているか
    public bool IsSelectingDungeon { get; private set; }

    private HashSet<string> clearedDungeons =
        new HashSet<string>();

    //==================================================
    // 初期化
    //==================================================

    private void Awake()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
            scrollView.SetActive(false);

        LoadClearedDungeons();
    }

    //==================================================
    // クリアチェック一覧を開く
    //==================================================

public void OpenClearCheckList()
    {
        IsSelectingDungeon = true;

        Debug.Log("クリアチェックリストを開きます。");

        // クリア状態を最新にする
        LoadClearedDungeons();

        if (scrollView == null)
        {
            Debug.LogError(
                "ClearCheckUI : ScrollViewが設定されていません。"
            );

            IsSelectingDungeon = false;
            return;
        }

        // 一度表示する
        scrollView.SetActive(true);

        // UIの表示順を一番手前にする
        transform.SetAsLastSibling();
        scrollView.transform.SetAsLastSibling();

        // リストを作り直す
        CreateButtonList();

        // ボタン生成後もScrollViewを一番手前にする
        scrollView.transform.SetAsLastSibling();

        Debug.Log("クリアチェックリストを表示しました。");
    }


    //==================================================
    // ダンジョン一覧作成
    //==================================================

    private void CreateButtonList()
    {
        if (content == null)
        {
            Debug.LogError(
                "ClearCheckUI : Contentが設定されていません。"
            );
            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError(
                "ClearCheckUI : Button Prefabが設定されていません。"
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

        string savePath =
            Application.persistentDataPath;

        string[] files =
            Directory.GetFiles(
                savePath,
                "*.json"
            );

        Debug.Log(
            "クリアチェック可能なダンジョン数 : " +
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
                Path.GetFileNameWithoutExtension(file);

            CreateDungeonButton(dungeonName);
        }
    }

    //==================================================
    // ダンジョンボタン作成
    //==================================================

    private void CreateDungeonButton(
        string dungeonName
    )
    {
        GameObject button =
            Instantiate(
                buttonPrefab,
                content
            );

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        if (text != null)
            text.text = dungeonName;

        Button buttonComponent =
            button.GetComponent<Button>();

        if (buttonComponent == null)
        {
            Debug.LogError(
                "Button PrefabにButtonコンポーネントがありません。"
            );
            return;
        }

        // クリア済みなら黄色
        if (clearedDungeons.Contains(dungeonName))
        {
            Image image =
                buttonComponent.GetComponent<Image>();

            if (image != null)
                image.color = clearedColor;
        }

        string selectedDungeon =
            dungeonName;

        buttonComponent.onClick.AddListener(() =>
        {
            SelectDungeon(selectedDungeon);
        });
    }

    //==================================================
    // ダンジョン選択
    //==================================================

    private void SelectDungeon(
        string dungeonName
    )
    {
        Debug.Log(
            "クリアチェックするダンジョン : " +
            dungeonName
        );

        string path =
            Path.Combine(
                Application.persistentDataPath,
                dungeonName + ".json"
            );

        if (!File.Exists(path))
        {
            Debug.LogError(
                "ダンジョン保存データがありません : " +
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
                "ダンジョンデータの読み込みに失敗しました : " +
                dungeonName
            );
            return;
        }

        if (mapManager == null)
        {
            Debug.LogError(
                "ClearCheckUI : MapManagerが設定されていません。"
            );
            return;
        }

        if (testPlayManager == null)
        {
            Debug.LogError(
                "ClearCheckUI : TestPlayManagerが設定されていません。"
            );
            return;
        }

        // マップを読み込む
        mapManager.LoadDungeon(data);

        // リストを閉じる
        CloseClearCheckList();

        // クリアチェック開始
        testPlayManager.StartClearCheck(
            dungeonName
        );
    }

    //==================================================
    // クリア済み登録
    //==================================================

    public void SetCleared(
        string dungeonName
    )
    {
        if (string.IsNullOrWhiteSpace(dungeonName))
            return;

        clearedDungeons.Add(
            dungeonName
        );

        PlayerPrefs.SetInt(
            GetClearKey(dungeonName),
            1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "クリア済みとして登録 : " +
            dungeonName
        );
    }

    //==================================================
    // クリア済み読み込み
    //==================================================

    private void LoadClearedDungeons()
    {
        clearedDungeons.Clear();

        string savePath =
            Application.persistentDataPath;

        string[] files =
            Directory.GetFiles(
                savePath,
                "*.json"
            );

        foreach (string file in files)
        {
            string dungeonName =
                Path.GetFileNameWithoutExtension(file);

            if (
                PlayerPrefs.GetInt(
                    GetClearKey(dungeonName),
                    0
                ) == 1
            )
            {
                clearedDungeons.Add(
                    dungeonName
                );
            }
        }
    }

    //==================================================
    // クリア状態キー
    //==================================================

    private string GetClearKey(
        string dungeonName
    )
    {
        return "DungeonClearCheck_" +
               dungeonName;
    }

    //==================================================
    // 一覧を閉じる
    //==================================================

    public void CloseClearCheckList()
    {
        IsSelectingDungeon = false;

        if (scrollView != null)
            scrollView.SetActive(false);
    }
}