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

        if (content == null)
        {
            Debug.LogError(
                "Contentが設定されていません。"
            );

            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError(
                "ButtonPrefabが設定されていません。"
            );

            return;
        }

        // 読み込み用Canvasを最前面にする
        if (loadCanvas == null)
        {
            loadCanvas =
                scrollView.GetComponentInParent<Canvas>();
        }

        if (loadCanvas != null)
        {
            loadCanvas.overrideSorting = true;
            loadCanvas.sortingOrder = 100;
        }
        else
        {
            Debug.LogError(
                "読み込み用Canvasが見つかりません。"
            );
        }

        IsSelectingDungeon = true;

        // ScrollViewを表示
        scrollView.SetActive(true);

        // 同じCanvas内でも最前面にする
        scrollView.transform.SetAsLastSibling();

        // ダンジョン一覧を作成
        CreateButtonList();

        Debug.Log(
            "Loadリストを表示しました。"
        );
    }

    //==================================================
    // ダンジョン一覧作成
    //==================================================

    private void CreateButtonList()
    {
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

        buttonObject.SetActive(true);

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

            Destroy(buttonObject);

            return;
        }

        button.interactable = true;

        string selectedDungeon =
            dungeonName;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            () =>
            {
                Debug.Log(
                    "ダンジョンボタンをクリック : " +
                    selectedDungeon
                );

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

        // 読み込み一覧を閉じる
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