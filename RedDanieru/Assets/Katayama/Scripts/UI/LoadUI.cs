using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public class LoadUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private LoadManager loadManager;

    [SerializeField] private Transform content;

    [SerializeField] private GameObject buttonPrefab;
    
    [SerializeField] private DungeonImporter importer;

    [SerializeField]
    private FusionLauncher fusionLauncher;

    [SerializeField]
    private GameObject scrollView;

    [SerializeField]
    private DungeonUIManager dungeonUIManager;


    private DungeonListItem[] cachedDungeons;

    private void OnEnable()
    {
        StartCoroutine(GetDungeonList());
    }

    /// <summary>
    /// 保存されているダンジョン一覧を表示する
    /// </summary>
    public void CreateButtonList()
    {
        // 以前作成したボタンを削除
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 保存されているJSONファイルを取得
        string[] files = Directory.GetFiles(
            Application.persistentDataPath,
            "*.json"
        );

        foreach (string file in files)
        {
            // 拡張子を除いたダンジョン名
            string dungeonName = Path.GetFileNameWithoutExtension(file);

            // ボタン生成
            GameObject button = Instantiate(buttonPrefab, content);

            // ボタンの文字を変更
            button.GetComponentInChildren<TMP_Text>().text = dungeonName;

            // ボタンを押したときの処理
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                loadManager.Load(dungeonName);
            });
        }
    }

    private IEnumerator GetDungeonList()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        UnityWebRequest request =
            UnityWebRequest.Get(
                "http://10.219.32.66/RedDaniel/get_dungeon_names.php"
            );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        string json =
            request.downloadHandler.text;

        Debug.Log("取得データ:");
        Debug.Log(json);

        DungeonNameArray data = JsonUtility.FromJson<DungeonNameArray>(json);

        if (data == null || data.dungeons == null)
        {
            Debug.LogError("ダンジョン一覧の読み込み失敗");
            yield break;
        }

        cachedDungeons = data.dungeons;
        
        var randomDungeons = cachedDungeons.OrderBy(x => Random.value).Take(Mathf.Min(20, cachedDungeons.Length));

        foreach (var dungeon in randomDungeons)
        {
            GameObject button =
                Instantiate(buttonPrefab, content);

            DungeonButtonUI ui =
                button.GetComponent<DungeonButtonUI>();

         
            ui.stageNameText.text =
                "DUNGEON:" + dungeon.dungeonName;

            ui.creatorNameText.text =
                "CREATOR:" + dungeon.creatorName;

            string[] tags =
            {
        "EASY",
        "NORMAL",
        "HARD",
        "HELL"
    };

            ui.tagText.text =
                "#" + tags[Random.Range(0, tags.Length)];

            ui.likeCountText.text =
                "GOOD:" + Random.Range(0, 100);

            ui.clearCountText.text =
                "CLEAR:" + Random.Range(0, 20);

            string selectedDungeonId =
                dungeon.dungeonId;

            string selectedDungeonName =
                dungeon.dungeonName;

            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                ui.Toggle();

                importer.ImportDungeon(selectedDungeonId);
            });

            ui.soloButton.onClick.AddListener(() =>
            {
                RoomInfo.SelectedDungeon =
                    selectedDungeonId;

                RoomInfo.SelectedDungeonName =
                    selectedDungeonName;

                dungeonUIManager.SoloMode();
            });

            ui.multiButton.onClick.AddListener(() =>
            {
                RoomInfo.SelectedDungeon =
                    selectedDungeonId;

                RoomInfo.SelectedDungeonName =
                    selectedDungeonName;

                dungeonUIManager.MultiMode();
                dungeonUIManager.MapSelectButton();
            });

            //button.GetComponent<Button>()
            //    .onClick.AddListener(() =>
            //    {
            //        RoomInfo.SelectedDungeon =
            //            selectedDungeonId;

            //        RoomInfo.SelectedDungeonName =
            //            selectedDungeonName;

            //        importer.ImportDungeon(
            //            selectedDungeonId);

            //        scrollView.SetActive(false);

            //        if (!GameModeManager.IsMultiplayer)
            //        {
            //            dungeonUIManager.HideMatchingUI();
            //            fusionLauncher.StartSolo();
            //        }
            //        else
            //        {
            //            dungeonUIManager.MapSelectButton();
            //        }
            //    });
        }
    }

    public void RefreshButton()
    {
        if (cachedDungeons == null)
            return;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        var randomDungeons =
            cachedDungeons
            .OrderBy(x => Random.value)
            .Take(Mathf.Min(20, cachedDungeons.Length));

        foreach (var dungeon in randomDungeons)
        {
            GameObject button =
                Instantiate(buttonPrefab, content);

            DungeonButtonUI ui =
                button.GetComponent<DungeonButtonUI>();

            ui.stageNameText.text =
                "DUNGEON:" + dungeon.dungeonName;

            ui.creatorNameText.text =
                "CREATOR:" + dungeon.creatorName;

            string[] tags =
            {
            "EASY",
            "NORMAL",
            "HARD",
            "HELL"
        };

            ui.tagText.text =
                "#" + tags[Random.Range(0, tags.Length)];

            ui.likeCountText.text =
                "GOOD:" + Random.Range(0, 100);

            ui.clearCountText.text =
                "CLEAR:" + Random.Range(0, 20);

            string selectedDungeonId =
                dungeon.dungeonId;

            string selectedDungeonName =
                dungeon.dungeonName;

            //button.GetComponent<Button>()
            //    .onClick.AddListener(() =>
            //    {
            //        RoomInfo.SelectedDungeon =
            //            selectedDungeonId;

            //        RoomInfo.SelectedDungeonName =
            //            selectedDungeonName;

            //        importer.ImportDungeon(
            //            selectedDungeonId);

            //        scrollView.SetActive(false);

            //        if (!GameModeManager.IsMultiplayer)
            //        {
            //            dungeonUIManager.HideMatchingUI();
            //            fusionLauncher.StartSolo();
            //        }
            //        else
            //        {
            //            dungeonUIManager.MapSelectButton();
            //        }
            //    });
        }
    }
}