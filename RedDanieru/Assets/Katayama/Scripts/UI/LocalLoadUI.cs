using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalLoadUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private LoadManager loadManager;

    [SerializeField] private Transform content;

    [SerializeField] private GameObject buttonPrefab;

    [SerializeField] private GameObject scrollView;

    [SerializeField] private DungeonUIManager dungeonUIManager;

    [SerializeField] private FusionLauncher fusionLauncher;

    private void OnEnable()
    {
        CreateButtonList();
    }

    /// <summary>
    /// ローカルに保存されているダンジョン一覧を表示
    /// </summary>
    public void CreateButtonList()
    {
        // 以前作成したボタンを削除
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // ローカル保存先
        string savePath =
            Application.persistentDataPath;

        Debug.Log(
            "ダンジョン保存先 : " + savePath
        );

        // JSONファイルを取得
        string[] files =
            Directory.GetFiles(
                savePath,
                "*.json"
            );

        Debug.Log(
            "ローカルダンジョン数 : "
            + files.Length
        );

        // ダンジョンがない場合
        if (files.Length == 0)
        {
            Debug.Log("保存されているダンジョンがありません。");
            return;
        }

        // ボタン生成
        foreach (string file in files)
        {
            string dungeonName =
                Path.GetFileNameWithoutExtension(file);

            CreateDungeonButton(dungeonName);
        }
    }

    /// <summary>
    /// ダンジョン選択ボタンを生成
    /// </summary>
    private void CreateDungeonButton(
        string dungeonName)
    {
        GameObject button =
            Instantiate(
                buttonPrefab,
                content
            );

        // ボタンの文字
        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = dungeonName;
        }

        // Button取得
        Button buttonComponent =
            button.GetComponent<Button>();

        if (buttonComponent == null)
        {
            Debug.LogError(
                "buttonPrefabにButtonがありません。"
            );

            return;
        }

        // 変数をコピー
        string selectedDungeon =
            dungeonName;

        // クリック処理
        buttonComponent.onClick.AddListener(() =>
        {
            SelectDungeon(selectedDungeon);
        });
    }

    /// <summary>
    /// ダンジョンを選択
    /// </summary>
    private void SelectDungeon(
        string dungeonName)
    {
        Debug.Log(
            "選択したダンジョン : "
            + dungeonName
        );

        // 選択したダンジョン名を保存
        RoomInfo.SelectedDungeon =
            dungeonName;

        // ローカルファイルが存在するか確認
        if (!loadManager.Exists(dungeonName))
        {
            Debug.LogError(
                "ローカルにダンジョンがありません : "
                + dungeonName
            );

            return;
        }

        // ローカルからロード
        loadManager.Load(
            dungeonName
        );

        // ScrollViewを閉じる
        scrollView.SetActive(false);

        // =========================
        // ソロ
        // =========================

        if (!GameModeManager.IsMultiplayer)
        {
            dungeonUIManager.HideMatchingUI();

            fusionLauncher.StartSolo();
        }
        // =========================
        // マルチ
        // =========================
        else
        {
            dungeonUIManager.MapSelectButton();
        }
    }
}