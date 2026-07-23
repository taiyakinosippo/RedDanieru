using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

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
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }

        UnityWebRequest request =
            UnityWebRequest.Get(
                "http://localhost/RedDaniel/get_dungeon_names.php"
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

        if (data == null || data.names == null)
        {
            Debug.LogError("ダンジョン一覧の読み込み失敗");
            yield break;
        }

        foreach (string dungeonName in data.names)
        {
            GameObject button =
                Instantiate(buttonPrefab, content);

            button
                .GetComponentInChildren<TMP_Text>()
                .text = dungeonName;

            string selectedDungeon = dungeonName;

            button
 .GetComponent<Button>()
 .onClick
 .AddListener(() =>
 {
     RoomInfo.SelectedDungeon = selectedDungeon;

     importer.ImportDungeon(selectedDungeon);

     scrollView.SetActive(false);

     if (!GameModeManager.IsMultiplayer)
     {
         dungeonUIManager.HideMatchingUI();

         fusionLauncher.StartSolo();
     }
     else
     {
         dungeonUIManager.MapSelectButton();
     }
 });
        }

    }
}